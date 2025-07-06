using System.Runtime.InteropServices;

namespace MinecraftSkinRender.OpenGL;

/// <summary>
/// OpenGL的FXAA抗锯齿
/// </summary>
public partial class SkinRenderOpenGL
{
    /// <summary>
    /// FXAA
    /// </summary>
    private int _fxaaVAO;
    private int _fxaaVBO;

    /// <summary>
    /// FXAA stop loc
    /// </summary>
    private int _fxaaStep;

    private int _fxaaRenderBuffer;
    private int _fxaaTexture;
    private int _fxaaFrameBuffer;

    private int _pgFXAA;

    private readonly float[] _quadVertices =
    [
        -1.0f,  1.0f,   0.0f, 1.0f,
        -1.0f, -1.0f,   0.0f, 0.0f,
         1.0f,  1.0f,   1.0f, 1.0f,
         1.0f, -1.0f,   1.0f, 0.0f
    ];

    private const string GlesHeader = "#version 300 es\n";
    private const string GlHeader = "#version 150\n";
    
    private const string VertexShaderFXAASource =
@"
#if __VERSION__ >= 130
#define COMPAT_VARYING out
#define COMPAT_ATTRIBUTE in
#define COMPAT_TEXTURE texture
#else
#define COMPAT_VARYING varying
#define COMPAT_ATTRIBUTE attribute
#define COMPAT_TEXTURE texture2D
#endif

COMPAT_ATTRIBUTE vec4 a_position;
COMPAT_ATTRIBUTE vec2 a_texCoord;
COMPAT_VARYING vec2 v_texCoord;

void main() {
    gl_Position = a_position;
    v_texCoord = a_texCoord;
}
";

    private const string FragmentShaderFXAASource =
@"#if defined(GL_ES)
precision mediump float;
#endif

uniform sampler2D u_colorTexture; 

uniform vec2 u_texelStep;
uniform int u_showEdges;
uniform int u_fxaaOn;
uniform int u_disablePass;

uniform float u_lumaThreshold;
uniform float u_mulReduce;
uniform float u_minReduce;
uniform float u_maxSpan;

in vec2 v_texCoord;

out vec4 fragColor;

// see FXAA
// http://developer.download.nvidia.com/assets/gamedev/files/sdk/11/FXAA_WhitePaper.pdf
// http://iryoku.com/aacourse/downloads/09-FXAA-3.11-in-15-Slides.pdf
// http://horde3d.org/wiki/index.php5?title=Shading_Technique_-_FXAA

void main(void)
{
    vec3 rgbM = texture(u_colorTexture, v_texCoord).rgb;

    // Possibility to toggle FXAA on and off.
    if (u_fxaaOn == 0)
    {
    	fragColor = texture(u_colorTexture, v_texCoord);

    	return;
    }

    // Sampling neighbour texels. Offsets are adapted to OpenGL texture coordinates. 
    vec3 rgbNW = textureOffset(u_colorTexture, v_texCoord, ivec2(-1, 1)).rgb;
    vec3 rgbNE = textureOffset(u_colorTexture, v_texCoord, ivec2(1, 1)).rgb;
    vec3 rgbSW = textureOffset(u_colorTexture, v_texCoord, ivec2(-1, -1)).rgb;
    vec3 rgbSE = textureOffset(u_colorTexture, v_texCoord, ivec2(1, -1)).rgb;

    // see http://en.wikipedia.org/wiki/Grayscale
    const vec3 toLuma = vec3(0.299, 0.587, 0.114);

    // Convert from RGB to luma.
    float lumaNW = dot(rgbNW, toLuma);
    float lumaNE = dot(rgbNE, toLuma);
    float lumaSW = dot(rgbSW, toLuma);
    float lumaSE = dot(rgbSE, toLuma);
    float lumaM = dot(rgbM, toLuma);

    // Gather minimum and maximum luma.
    float lumaMin = min(lumaM, min(min(lumaNW, lumaNE), min(lumaSW, lumaSE)));
    float lumaMax = max(lumaM, max(max(lumaNW, lumaNE), max(lumaSW, lumaSE)));

    // If contrast is lower than a maximum threshold ...
    if (u_disablePass == 0 && lumaMax - lumaMin <= lumaMax * u_lumaThreshold)
    {
    	// ... do no AA and return.
    	fragColor = texture(u_colorTexture, v_texCoord);

    	return;
    }  

    // Sampling is done along the gradient.
    vec2 samplingDirection;	
    samplingDirection.x = -((lumaNW + lumaNE) - (lumaSW + lumaSE));
    samplingDirection.y =  ((lumaNW + lumaSW) - (lumaNE + lumaSE));

    // Sampling step distance depends on the luma: The brighter the sampled texels, the smaller the final sampling step direction.
    // This results, that brighter areas are less blurred/more sharper than dark areas.  
    float samplingDirectionReduce = max((lumaNW + lumaNE + lumaSW + lumaSE) * 0.25 * u_mulReduce, u_minReduce);

    // Factor for norming the sampling direction plus adding the brightness influence. 
    float minSamplingDirectionFactor = 1.0 / (min(abs(samplingDirection.x), abs(samplingDirection.y)) + samplingDirectionReduce);

    // Calculate final sampling direction vector by reducing, clamping to a range and finally adapting to the texture size. 
    samplingDirection = clamp(samplingDirection * minSamplingDirectionFactor, vec2(-u_maxSpan), vec2(u_maxSpan)) * u_texelStep;

    // Inner samples on the tab.
    vec3 rgbSampleNeg = texture(u_colorTexture, v_texCoord + samplingDirection * (1.0/3.0 - 0.5)).rgb;
    vec3 rgbSamplePos = texture(u_colorTexture, v_texCoord + samplingDirection * (2.0/3.0 - 0.5)).rgb;

    vec3 rgbTwoTab = (rgbSamplePos + rgbSampleNeg) * 0.5;  

    // Outer samples on the tab.
    vec3 rgbSampleNegOuter = texture(u_colorTexture, v_texCoord + samplingDirection * (0.0/3.0 - 0.5)).rgb;
    vec3 rgbSamplePosOuter = texture(u_colorTexture, v_texCoord + samplingDirection * (3.0/3.0 - 0.5)).rgb;

    vec3 rgbFourTab = (rgbSamplePosOuter + rgbSampleNegOuter) * 0.25 + rgbTwoTab * 0.5;   

    // Calculate luma for checking against the minimum and maximum value.
    float lumaFourTab = dot(rgbFourTab, toLuma);

    // Are outer samples of the tab beyond the edge ... 
    if (lumaFourTab < lumaMin || lumaFourTab > lumaMax)
    {
    	// ... yes, so use only two samples.
    	fragColor = vec4(rgbTwoTab, 1.0); 
    }
    else
    {
    	// ... no, so use four samples. 
    	fragColor = vec4(rgbFourTab, 1.0);
    }

    // Show edges for debug purposes.	
    if (u_showEdges != 0)
    {
    	fragColor.r = 1.0;
    }
}
";
    private void InitFXAAFrameBuffer()
    {
        _fxaaFrameBuffer = gl.GenFramebuffer();
        gl.BindFramebuffer(gl.GL_FRAMEBUFFER, _fxaaFrameBuffer);

        _fxaaTexture = gl.GenTexture();
        gl.BindTexture(gl.GL_TEXTURE_2D, _fxaaTexture);
        gl.TexParameteri(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_WRAP_S, gl.GL_CLAMP_TO_EDGE);
        gl.TexParameteri(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_WRAP_T, gl.GL_CLAMP_TO_EDGE);
        gl.TexParameteri(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_MIN_FILTER, gl.GL_LINEAR);
        gl.TexParameteri(gl.GL_TEXTURE_2D, gl.GL_TEXTURE_MAG_FILTER, gl.GL_LINEAR);
        gl.TexImage2D(gl.GL_TEXTURE_2D, 0, gl.GL_RGBA, _width, _height, 0, gl.GL_RGBA, gl.GL_UNSIGNED_BYTE, 0);
        gl.FramebufferTexture2D(gl.GL_FRAMEBUFFER, gl.GL_COLOR_ATTACHMENT0, gl.GL_TEXTURE_2D, _fxaaTexture, 0);

        _fxaaRenderBuffer = gl.GenRenderbuffer();
        gl.BindRenderbuffer(gl.GL_RENDERBUFFER, _fxaaRenderBuffer);
        gl.RenderbufferStorage(gl.GL_RENDERBUFFER,
            gl.GL_DEPTH24_STENCIL8, _width, _height);
        gl.FramebufferRenderbuffer(gl.GL_FRAMEBUFFER, gl.GL_DEPTH_STENCIL_ATTACHMENT,
            gl.GL_RENDERBUFFER, _fxaaRenderBuffer);

        if (gl.CheckFramebufferStatus(gl.GL_FRAMEBUFFER) != gl.GL_FRAMEBUFFER_COMPLETE)
        {
            throw new Exception("glCheckFramebufferStatus status != GL_FRAMEBUFFER_COMPLETE");
        }

        gl.BindTexture(gl.GL_TEXTURE_2D, 0);
        gl.BindRenderbuffer(gl.GL_RENDERBUFFER, 0);
        gl.BindFramebuffer(gl.GL_FRAMEBUFFER, 0);
    }

    private void ShaderFXAA()
    {
        string str;
        var vertexShader = gl.CreateShader(gl.GL_VERTEX_SHADER);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            str = GlHeader + VertexShaderFXAASource;
        }
        else
        {
            str = GlesHeader + VertexShaderFXAASource;
        }
        
        gl.ShaderSource(vertexShader, str);
        gl.CompileShader(vertexShader);
        gl.GetShaderiv(vertexShader, gl.GL_COMPILE_STATUS, out var state);
        if (state == 0)
        {
            gl.GetShaderInfoLog(vertexShader, out var info);
            throw new Exception($"GL_VERTEX_SHADER.\n{info}");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            str = GlHeader + FragmentShaderFXAASource;
        }
        else
        {
            str = GlesHeader + FragmentShaderFXAASource;
        }
        
        var fragmentShader = gl.CreateShader(gl.GL_FRAGMENT_SHADER);
        gl.ShaderSource(fragmentShader, str);
        gl.CompileShader(fragmentShader);
        gl.GetShaderiv(fragmentShader, gl.GL_COMPILE_STATUS, out state);
        if (state == 0)
        {
            gl.GetShaderInfoLog(fragmentShader, out var info);
            throw new Exception($"GL_FRAGMENT_SHADER.\n{info}");
        }

        _pgFXAA = gl.CreateProgram();
        gl.AttachShader(_pgFXAA, vertexShader);
        gl.AttachShader(_pgFXAA, fragmentShader);
        gl.LinkProgram(_pgFXAA);
        gl.GetProgramiv(_pgFXAA, gl.GL_LINK_STATUS, out state);
        if (state == 0)
        {
            gl.GetProgramInfoLog(_pgFXAA, out var info);
            throw new Exception($"GL_LINK_PROGRAM.\n{info}");
        }

        //Delete the no longer useful individual shaders;
        gl.DetachShader(_pgFXAA, vertexShader);
        gl.DetachShader(_pgFXAA, fragmentShader);
        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);
    }

    private unsafe void VAOFXAA()
    {
        gl.UseProgram(_pgFXAA);

        _fxaaVAO = gl.GenVertexArray();
        gl.BindVertexArray(_fxaaVAO);

        _fxaaVBO = gl.GenBuffer();
        gl.BindBuffer(gl.GL_ARRAY_BUFFER, _fxaaVBO);
        fixed (void* ptr = _quadVertices)
        {
            gl.BufferData(gl.GL_ARRAY_BUFFER, _quadVertices.Length * sizeof(float), new(ptr), gl.GL_STATIC_DRAW);
        }

        int posLoc = gl.GetAttribLocation(_pgFXAA, "a_position");
        int texLoc = gl.GetAttribLocation(_pgFXAA, "a_texCoord");

        gl.EnableVertexAttribArray(posLoc);
        gl.VertexAttribPointer(posLoc, 2, gl.GL_FLOAT, false, 4 * sizeof(float), 0);

        gl.EnableVertexAttribArray(texLoc);
        gl.VertexAttribPointer(texLoc, 2, gl.GL_FLOAT, false, 4 * sizeof(float), 2 * sizeof(float));

        gl.BindVertexArray(0);
        gl.UseProgram(0);
    }

    private void SetFXAAArg()
    {
        gl.UseProgram(_pgFXAA);
        _fxaaStep = gl.GetUniformLocation(_pgFXAA, "u_texelStep");
        var showEdges = gl.GetUniformLocation(_pgFXAA, "u_showEdges");
        var fxaaOn = gl.GetUniformLocation(_pgFXAA, "u_fxaaOn");

        var lumaThreshold = gl.GetUniformLocation(_pgFXAA, "u_lumaThreshold");
        var disablePass = gl.GetUniformLocation(_pgFXAA, "u_disablePass");
        var mulReduce = gl.GetUniformLocation(_pgFXAA, "u_mulReduce");
        var minReduce = gl.GetUniformLocation(_pgFXAA, "u_minReduce");
        var maxSpan = gl.GetUniformLocation(_pgFXAA, "u_maxSpan");

        gl.Uniform1i(disablePass, 0);
        gl.Uniform1i(showEdges, 0);
        gl.Uniform1i(fxaaOn, 1);

        gl.Uniform1f(lumaThreshold, 0.3f);
        gl.Uniform1f(mulReduce, 1.0f / 8.0f);
        gl.Uniform1f(minReduce, 1.0f / 128.0f);
        gl.Uniform1f(maxSpan, 8.0f);

        gl.UseProgram(0);
    }

    private unsafe void InitFXAA()
    {
        ShaderFXAA();
        VAOFXAA();
        SetFXAAArg();
    }

    private void DeleteFXAAFrameBuffer()
    {
        if (_fxaaFrameBuffer != 0)
        {
            gl.DeleteFramebuffer(_fxaaFrameBuffer);
            _fxaaFrameBuffer = 0;
        }
        if (_fxaaRenderBuffer != 0)
        {
            gl.DeleteRenderbuffer(_fxaaRenderBuffer);
            _fxaaRenderBuffer = 0;
        }
        if (_fxaaTexture != 0)
        {
            gl.DeleteTexture(_fxaaTexture);
            _fxaaTexture = 0;
        }
    }

    private void DeleteFXAA()
    {
        if (_fxaaVAO != 0)
        {
            gl.DeleteVertexArray(_fxaaVAO);
            _fxaaVAO = 0;
        }
        if (_fxaaVBO != 0)
        {
            gl.DeleteBuffer(_fxaaVBO);
            _fxaaVBO = 0;
        }
    }
}
