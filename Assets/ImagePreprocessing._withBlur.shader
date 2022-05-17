Shader "ImagePreprocessingWithBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        CGINCLUDE
        #include "UnityCG.cginc"
        #pragma fragmentoption ARB_precision_hint_fastest
        #pragma enable_d3d11_debug_symbols
        
        sampler2D _MainTex;
        half4 _MainTex_TexelSize;
        float _BlurSize;
        
         struct appdata
        {
            float4 vertex : POSITION;
            half2 uv : TEXCOORD0;
        };
                
        struct v2f
        {
            half2 uv : TEXCOORD0;
            float4 vertex : SV_POSITION;
        };

        v2f vert (appdata v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.uv = v.uv;
            return o;
        }

        float lum(float3 color) {
            return color.r*.3 + color.g*.59 + color.b*.11;
        }
        ENDCG
        
        Pass
        { 
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest

            fixed4 frag (v2f i) : SV_Target
            {
                fixed3 color = tex2D(_MainTex, i.uv).rgb; 
                return fixed4(lum(color), lum(color), lum(color), 1); 
            } 
            ENDCG
        }
        
        ///////////////////////////////////////////
         // Third pass for horizontal blur //
         ///////////////////////////////////////////
        GrabPass {
            "_GrabTex2"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _GrabTex2;
            float4 _GrabTex2_TexelSize;
                  
            fixed4 frag (v2f i) : SV_Target
            {
                sampler2D tex = _GrabTex2; 
                float _texelw = _GrabTex2_TexelSize.x;
                float2 uv = i.uv; 
               

                float4 pix = mul(tex2D(tex, float2(uv.x + -3*_texelw, uv.y )),0.106595);   
                pix += mul(tex2D(tex, float2(uv.x + -2*_texelw, uv.y )),0.140367);   pix += mul(tex2D(tex, float2(uv.x + -1*_texelw, uv.y )),0.165569);   pix += mul(tex2D(tex, float2(uv.x + 0*_texelw, uv.y )),0.174938);   pix += mul(tex2D(tex, float2(uv.x + 1*_texelw, uv.y )),0.165569);   pix += mul(tex2D(tex, float2(uv.x + 2*_texelw, uv.y )),0.140367);   pix += mul(tex2D(tex, float2(uv.x + 3*_texelw, uv.y )),0.106595);   

                return pix;
            }
            ENDCG
        }
        
        ///////////////////////////////////////////
         // Third pass for vertical blur and electrode grab //
         ///////////////////////////////////////////
        GrabPass {
            "_GrabTex3"
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

            sampler2D _GrabTex3;
            float4 _GrabTex3_TexelSize;
           
            struct Electrode
            {
                float x;
                float y;
                float screen_x_pos;
                float screen_y_pos;
                float current; 
            };
            
            uniform RWStructuredBuffer<Electrode> electrodesBuffer : register(u2);
            uint numberElectrodes;
            uint xResolution; 
            uint yResolution;
            float amplitude; 
            
            uint pixelNumberX( float screenPos){
                 return ceil(xResolution * screenPos);
            }  
            uint pixelNumberY( float screenPos){
                 return ceil(yResolution * screenPos);
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                sampler2D tex = _GrabTex3; 
                float _texelw = _GrabTex3_TexelSize.x;
                float2 uv = i.uv; 
               
                float4 pix = mul(tex2D(tex, float2(uv.x, uv.y + -3*_texelw)),0.106595);   
                pix += mul(tex2D(tex, float2(uv.x, uv.y + -2*_texelw)),0.140367);   pix += mul(tex2D(tex, float2(uv.x, uv.y + -1*_texelw)),0.165569);   pix += mul(tex2D(tex, float2(uv.x, uv.y + 0*_texelw)),0.174938);   pix += mul(tex2D(tex, float2(uv.x, uv.y + 1*_texelw)),0.165569);   pix += mul(tex2D(tex, float2(uv.x, uv.y + 2*_texelw)),0.140367);   pix += mul(tex2D(tex, float2(uv.x, uv.y + 3*_texelw)),0.106595);   

                for(int currentElectrode = 0; currentElectrode < numberElectrodes; currentElectrode++){
                    if( pixelNumberX(i.uv.x) == pixelNumberX(electrodesBuffer[currentElectrode].x) 
                    && pixelNumberY(i.uv.y) == pixelNumberY(electrodesBuffer[currentElectrode].y)){
                        electrodesBuffer[currentElectrode].current = amplitude * pix[3]; 
                    }
                }

                
                return pix;
            }
            ENDCG
        }
    } 
}