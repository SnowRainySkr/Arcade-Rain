Shader "Arcade/UI/HpTopInsightTop"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" { }
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Hp ("Hp", Range(0, 1)) = 1
        _SubTex ("SubTexture", 2D) = "white" { }
        _TopAlpha ("Top Alpha", Range(0, 1)) = 1
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
            #include "Assets/CustomResources/ReadOnly/Shaders/ColorSpace.hlsl"

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
            float4 _Color;
            float _Hp;
            sampler2D _SubTex;
            float _TopAlpha;

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
                float y = 1 - 220 / 730.0 * (1 - i.uv.y);
                float4 col = float4(1, 1, 1, tex2D(_MainTex, i.uv).a) * _Color;

                if (y < _Hp)
                {
                    float3 hpColor = (float3)tex2D(_SubTex, float2(i.uv.x, y));
                    hpColor = rgb2hsv(hpColor);
                    hpColor = float3(hpColor.x, 1, 1);
                    hpColor = hsv2rgb(hpColor);
                    float aa = 0.8 / (i.uv.y + 1);
                    hpColor = aa + (1 - aa) * hpColor;
                    float sa = 1 - (1 - _TopAlpha) * (1 - col.a);
                    float3 rgb = (hpColor * _TopAlpha + col.rgb * col.a * (1 - _TopAlpha)) / sa;
                    col = float4(rgb, step(0.1, col.a) * (_TopAlpha + 1) * 0.5);
                }

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}