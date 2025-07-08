namespace LiteSkinViewer3D.OpenGL;

internal static class GLSLSource {
    internal const string GlHeader = "#version 150\n";
    internal const string GlesHeader = "#version 300 es\n";

    internal const string MacosHeader = """
        #version 150
        #define Macos
    """;

    internal const string VertexShaderFXAASource = """
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
    """;

    internal const string FragmentShaderFXAASource = """
         #if defined(GL_ES)
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
    """;

    internal const string VertexShaderSource = """
        #if __VERSION__ >= 130
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
    """;

    internal const string FragmentShaderSource = """
        #if defined(GL_ES)
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
        
            // 半兰伯特光照
            float diff = dot(norm, lightDir) * 0.5 + 0.5;
            vec3 diffuse = diff * lightColor;
        
            vec3 result = ambient + diffuse;
            FragColor = COMPAT_TEXTURE(texture0, texIn) * vec4(result, 1.0);
        }
    """;
}