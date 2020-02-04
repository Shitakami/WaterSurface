Shader "Unlit/Mirror"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ReflectionTex ("Reflection Texture", 2D) = "white" {}

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 ref : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _ReflectionTex;
            float4 _MainTex_ST;
            float4x4 _RefVP;
            float4x4 _RefW;

            v2f vert(appdata v) {

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.ref = mul(_RefVP, mul(_RefW, v.vertex));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {

                return tex2D(_ReflectionTex, i.ref.xy / i.ref.w * 0.5 + 0.5);

            }


            ENDCG
        }
    }
}
