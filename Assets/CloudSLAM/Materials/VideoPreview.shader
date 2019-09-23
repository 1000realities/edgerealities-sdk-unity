//UNITY_SHADER_NO_UPGRADE
Shader "CloudSLAM/LocalVideoPreview"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _UvInfo ("UV scale", Vector) = (1.0,1.0,0.0,0.0)
        // _UvInfo.x = width scale.
        // _UvInfo.y = height scale.
        // _UvInfo.z = screen orientation (as defined in UnityEngine.ScreenOrientation)
    }
    SubShader
    {
        Pass
        {
            Zwrite Off
            Cull Off
            
            GLSLPROGRAM
            
            #pragma only_renderers gles3 gles
            
            #extension GL_OES_EGL_image_external : require
            #extension GL_OES_EGL_image_external_essl3 : enable
            
            uniform vec4 _UvInfo;
            
            vec4 GammaToLinear(vec4 color)
            {
                // UnityCG.cginc uses the same method.
                return color * (color * (color * 0.305306011 + 0.682171111) + 0.012522878);
            }
            
            #ifdef VERTEX
            
            varying vec2 uv;
            
            void main()
            {
                if (_UvInfo.z == 1.0)
                {
                    float offset = (1.0 - _UvInfo.x) / 2.0;
                    // Portrait
                    uv = vec2((1.0 - gl_MultiTexCoord0.y) * _UvInfo.y, ((1.0 - gl_MultiTexCoord0.x) * _UvInfo.x) + offset);
                }
                else if (_UvInfo.z == 2.0)
                {
                    float offset = (1.0 - _UvInfo.x) / 2.0;
                    // Portrait upside down
                    uv = vec2(gl_MultiTexCoord0.y * _UvInfo.y, ((gl_MultiTexCoord0.x * _UvInfo.x)) + offset);
                }
                else if (_UvInfo.z == 3.0)
                {
                    float offset = (1.0 - _UvInfo.y) / 2.0;
                    // Landscape left
                    uv = vec2(gl_MultiTexCoord0.x * _UvInfo.x, ((1.0 - gl_MultiTexCoord0.y) * _UvInfo.y) + offset);    
                }
                else
                {
                    float offset = (1.0 - _UvInfo.y) / 2.0;
                    // Landscape right, default
                    uv = vec2((1.0 - gl_MultiTexCoord0.x) * _UvInfo.x, (gl_MultiTexCoord0.y * _UvInfo.y) + offset);
                }
                gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
            }
            #endif
            
            #ifdef FRAGMENT
            
            varying vec2 uv;
            uniform samplerExternalOES _MainTex;
            
            void main()
            {
                vec4 color = texture2D(_MainTex, uv).rgba;
                
                #ifndef UNITY_COLORSPACE_GAMMA
                color = GammaToLinear(color);
                #endif
                
                gl_FragColor = color;
            }
            #endif
            ENDGLSL
        }
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            ZWrite Off
        
            CGPROGRAM
            #pragma exclude_renderers gles3 gles
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}