﻿Shader "Game/Effects/HDREmitter"
{
	Properties
	{
	    [HDR]_Color("HDR Color",Color)=(1,1,1,1)
		_ShapeTexture("Shape",2D)="white"{}
	}

	SubShader
	{ 
		Tags {"RenderType" = "HDREmitter" "IgnoreProjector" = "True" "Queue" = "Transparent" }
		
		Lighting Off Fog { Color(0,0,0,0) }
		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha

		CGINCLUDE
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 color : TEXCOORD0;
				float2 uv:TEXCOORD1;
			};

			sampler2D _ShapeTexture;
			UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
			UNITY_INSTANCING_BUFFER_END(Props)

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				o.uv = v.uv;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color*UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return tex2D(_ShapeTexture,i.uv).r*i.color;
			}
		ENDCG

		Pass
		{
			Cull Back 
			name "MAIN"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}

	}
}
