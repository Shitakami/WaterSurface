Shader "Unlit/Water"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TessFactor("Tess Factor", Vector) = (2, 2, 2, 2)
        _LODFactor("LODFactor", Range(0, 10)) = 1
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
            #pragma hull HS
            #pragma domain DS
            #define INPUT_PATCH_SIZE 3
            #define OUTPUT_PATCH_SIZE 3
            #pragma multi_compile_fog

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2h
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : POS;
                float3 normal : NORMAL;
                float4 worldPos : TEXCOORD1;
            };

            struct h2d_main {
                float3 vertex : POS;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float4 worldPos : TEXCOORD1;
            };

            struct h2d_const {
                float tess_factor[3] : SV_TessFactor;
                float InsideTessFactor : SV_InsideTessFactor;
            }

            struct d2f { 
                float4 vertex : SV_Position;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            uniform float _LODFactor;
            uniform float4 _TessFactor;
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = float4(v.vertex.xyz, 1.0f);
                o.uv = v.uv;
                o.normal = v.normal;

                o.worldPos = mul(unity_ObjectToWorld, v.vert);
                return o;
            }

            h2d_const HSConst(InputPatch<v2h, INPUT_PATCH_SIZE> i) {

                h2d_const o = (h2d_const)0;
                o.tess_factor[0] = _TessFactor.x * _LODFactor;
                o.tess_factor[1] = _TessFactor.y * _LODFactor;
                o.tess_factor[2] = _TessFactor.z * _LODFactor;
                o.InsideTessFactor = _TessFactor.w * _LODFactor;
            }

            [domain("tri")]
            [partitioning("integer")]
            [outputtopology("triangle_cw")]
            [outputcontrolpoints(OUTPUT_PATCH_SIZE)]
            [patchconstantfunc("HSConst")]
            h2d_main HS(InputPatch<v2h, INPUT_PATCH_SIZE> i, uint id : SV_OUTPUTCONTROLPOINTID) {

                h2d_main o = (h2d_main)0;
                o.vertex = i[id].vertex;
                o.uv = i[id].uv;
                o.normal = i[id].normal;
                o.worldPos = i[id].worldPos;
                
                return o;

            }
            
            [domain("tri")]
            d2f DS(h2d_const h2)

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
