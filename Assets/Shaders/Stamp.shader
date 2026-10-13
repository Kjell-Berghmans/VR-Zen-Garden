Shader "Custom/Stamp"
{
    Properties
    {
        _MainTex ("Base", 2D) = "white" {}
        _BrushTex ("Brush", 2D) = "white" {}
        _Center ("Center (x,y,size,strength)", Vector) = (0,0,0.05,0.2)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };

            sampler2D _MainTex;
            sampler2D _BrushTex;
            float4 _Center; // x,y,size,strength

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                fixed4 baseCol = tex2D(_MainTex, uv);

                float2 rel = (uv - _Center.xy) / _Center.z;
                float dist = length(rel);

                // sample brush with uv transformed to brush space
                float2 brushUV = rel * 0.5 + 0.5;
                fixed4 brush = tex2D(_BrushTex, brushUV);

                // brush alpha modulated by distance falloff
                float stamp = saturate(brush.a * (1.0 - smoothstep(0.0, 1.0, dist)));

                // add strength to the red channel (height stored in R)
                baseCol.r = saturate(baseCol.r + stamp * _Center.w);

                return baseCol;
            }
            ENDCG
        }
    }
}
