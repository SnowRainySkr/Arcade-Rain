Shader "Arcade/Notes/Shadow"
{
	Properties
	{
		[PerRendererData] _Color ("Color", Color) = (1,1,1,1)
		[PerRendererData] _From ("From", Float) = 0
		[PerRendererData] _To ("To", Float) = 1
	}
	SubShader
	{
		Tags { "Queue" = "Transparent"  "RenderType"="Transparent" "CanUseSpriteAtlas"="true" "PreviewType" = "Plane" }

        Cull Off
        Lighting Off
		ZWrite Off
        Blend One OneMinusSrcAlpha

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			float _From,_To;
			float4 _Color;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = TransformObjectToHClip((float3)v.vertex);
				o.uv = v.uv;
				return o;
			}

			half4 frag (v2f i) : SV_Target
			{
			    if(i.uv.y < _From || i.uv.y > _To) return 0;
				float4 c = _Color;
				c.rgb *= c.a;
				return c;
			}
			ENDHLSL
		}
	}
}

