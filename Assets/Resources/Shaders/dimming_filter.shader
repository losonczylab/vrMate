﻿Shader "dimming_filter"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _value ("_value", Range(1.0, 100.0)) = 2.0
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
            float _value;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
 
                fixed4 returnCol = col;
                returnCol.y = returnCol.y/_value;
                returnCol.x = returnCol.x/_value;
                returnCol.z = returnCol.z/_value;

                return returnCol;
            }
            ENDCG
        }
    }
}
