Shader "red_filter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FilterRed ("Filter Red", Range(0.0, 1.0)) = 1.0
        _FilterBlue ("Filter Blue", Range(0.0, 1.0)) = 0.0
        _FilterGreen ("Filter Green", Range(0.0, 1.0)) = 0.0
        _ColorFilter ("Color Filter", Color) = (1.0, 0.0, 0.0, 0.0)

    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float _FilterRed;
            float _FilterBlue;
            float _FilterGreen;
            float4 _ColorFilter;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                //fixed4 inv_filt = 8 * (1 - _ColorFilter);
                //fixed4 g_corr = (col.g * inv_filt) / 8;
                //g_corr.g = 0;
                //col = col + g_corr;
                //fixed4 returnCol = col - _ColorFilter;

                fixed4 returnCol = col;
                returnCol.x = 0;
                returnCol.y = returnCol.y + col.x/2;
                returnCol.z = returnCol.z + col.x/2;

                return returnCol;
            }
            ENDCG
        }
    }
}
