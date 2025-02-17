Shader "Arcade/UI/ProgressBar"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Alpha ("_Alpha", Float) = 1.0
        _Radius ("Radius", Float) = 0
        _Ratio ("Height/Width", Float) = 1

        _Stencil ("Stencil", Int) = 0
        _StencilOp ("StencilOp", Int) = 0
        _StencilComp ("StencilComp", Int) = 8
        _StencilReadMask ("StencilReadMask", Int) = 255
        _StencilWriteMask ("StencilWriteMask", Int) = 255
        _ColorMask ("ColorMask", Int) = 15
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue" = "Transparent"
            "PreviewType"="Plane"
        }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
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

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Radius;
            float _Ratio;
            float _Alpha;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 p = abs(step(0.5, i.uv) - i.uv);
                float4 col = tex2D(_MainTex, i.uv) * (step(_Radius * _Ratio, p.x) || step(_Radius, p.y) || step(
                    length(float2(p.x / _Ratio - _Radius, p.y - _Radius)), _Radius));
                col.a *= 1 - 4 * (p.y - 0.5) * (p.y - 0.5);
                col.a *= 1 + 8 * (p.x - 0.5) * (p.x - 0.5) * (p.x - 0.5);
                col.a *= _Alpha;
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}