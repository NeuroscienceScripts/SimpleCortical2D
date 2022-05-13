Shader "Unlit/CorticalModel"
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
        
        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };
                    
        struct v2f
        {
            float4 grabPos : TEXCOORD0;
            float4 pos : SV_POSITION;
        };
        
        v2f vert (appdata v)
        {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.grabPos = ComputeGrabScreenPos(o.pos);
            return o;
        }
        
        ENDCG
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            
            struct Electrode
            {
                float xPosition;
                float yPosition;
                float screenPosX;
                float screenPosY;
                float current;
            };
            
            uniform RWStructuredBuffer<Electrode> electrodesBuffer : register(u2);
            StructuredBuffer<float> pixelsToElectrodesGaussBuffer;
            
            uint numberElectrodes;
            uint xResolution;
            uint yResolution; 

            uint pixelNumberX( float screenPos){
                return ceil(xResolution * screenPos);
            }  
            uint pixelNumberY( float screenPos){
                return ceil(yResolution * screenPos);
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                uint xPixel = pixelNumberX(i.grabPos.x)-1;
                if (xPixel <= xResolution/2.0f)
                    return fixed4(0.03,.03,.03,1); 
                
                uint yPixel = pixelNumberY(i.grabPos.y)-1;
                uint loc1D = (yPixel*xResolution) + xPixel; 
                float brightness = 0; 
                
                for(uint e=0; e < numberElectrodes; e++)
                {
                    brightness += electrodesBuffer[e].current * pixelsToElectrodesGaussBuffer[(loc1D*numberElectrodes) + e]; 
                }
                return fixed4(brightness, brightness, brightness, 1); 
            }
            ENDCG
        }
    }
}
