using System.Runtime.InteropServices;

namespace MinecraftSkinRender.OpenGL;

public partial class SkinRenderOpenGL
{
    private int _pgModel;

    private const string MacosHeader = 
@"#version 150
#define Macos
";
    
    private const string VertexShaderSource =
@"#if __VERSION__ >= 130
#define COMPAT_VARYING out
#define COMPAT_ATTRIBUTE in
#define COMPAT_TEXTURE texture
#else
#define COMPAT_VARYING varying
#define COMPAT_ATTRIBUTE attribute
#define COMPAT_TEXTURE texture2D
#endif

COMPAT_ATTRIBUTE vec3 a_position;
COMPAT_ATTRIBUTE vec2 a_texCoord;
COMPAT_ATTRIBUTE vec3 a_normal;

uniform mat4 model;
uniform mat4 projection;
uniform mat4 view;
uniform mat4 self;

COMPAT_VARYING vec3 normalIn;
COMPAT_VARYING vec2 texIn;
COMPAT_VARYING vec3 fragPosIn;

void main()
{
    texIn = a_texCoord;

    mat4 temp = view * model * self;

    fragPosIn = vec3(model * vec4(a_position, 1.0));
    normalIn = normalize(vec3(model * vec4(a_normal, 1.0)));
    gl_Position = projection * temp * vec4(a_position, 1.0);
}
";

    private const string FragmentShaderSource =
@"#if defined(GL_ES)
precision mediump float;
#endif

#ifdef Macos
#define COMPAT_VARYING in
#define COMPAT_ATTRIBUTE in
#define COMPAT_TEXTURE texture
out vec4 FragColor;
#else
#if __VERSION__ >= 130
#define COMPAT_VARYING in
#define COMPAT_ATTRIBUTE in
#define COMPAT_TEXTURE texture
out vec4 FragColor;
#else
#define COMPAT_VARYING varying
#define COMPAT_ATTRIBUTE attribute
#define COMPAT_TEXTURE texture2D
#define FragColor gl_FragColor
#endif
#endif

uniform sampler2D texture0;

COMPAT_VARYING vec3 fragPosIn;
COMPAT_VARYING vec3 normalIn;
COMPAT_VARYING vec2 texIn;

void main() {
    vec3 lightColor = vec3(1.0, 1.0, 1.0);
    float ambientStrength = 0.15;
    vec3 lightPos = vec3(0, 1, 5);
    
    vec3 ambient = ambientStrength * lightColor;
    vec3 norm = normalize(normalIn);
    vec3 lightDir = normalize(lightPos - fragPosIn);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor;
    
    vec3 result = (ambient + diffuse);
    FragColor = COMPAT_TEXTURE(texture0, texIn) * vec4(result, 1.0);
}
";

    private void InitShader()
    {
        string str;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            str = MacosHeader + VertexShaderSource;
        }
        else
        {
            str = VertexShaderSource;
        }

        var vertexShader = gl.CreateShader(gl.GL_VERTEX_SHADER);
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
            str = MacosHeader + FragmentShaderSource;
        }
        else
        {
            str = FragmentShaderSource;
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

        _pgModel = gl.CreateProgram();
        gl.AttachShader(_pgModel, vertexShader);
        gl.AttachShader(_pgModel, fragmentShader);
        gl.LinkProgram(_pgModel);
        gl.GetProgramiv(_pgModel, gl.GL_LINK_STATUS, out state);
        if (state == 0)
        {
            gl.GetProgramInfoLog(_pgModel, out var info);
            throw new Exception($"GL_LINK_PROGRAM.\n{info}");
        }

        //Delete the no longer useful individual shaders;
        gl.DetachShader(_pgModel, vertexShader);
        gl.DetachShader(_pgModel, fragmentShader);
        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);
    }
}
