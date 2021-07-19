﻿Shader "Hidden/ShadowCaster"
{
    SubShader
    {
		Pass
		{
			NAME "MAIN"
			Tags{"LightMode" = "ShadowCaster"}
			HLSLPROGRAM
			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment
			#pragma multi_compile_instancing
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "../../CommonLightingInclude.hlsl"
				
			struct a2f
			{
				A2V_SHADOW_CASTER;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				V2F_SHADOW_CASTER;
			};

			v2f ShadowVertex(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				SHADOW_CASTER_VERTEX(v,o);
				return o;
			}

			float4 ShadowFragment(v2f i) :SV_TARGET
			{
				return 0;
			}
			ENDHLSL
		}	
		
    }
}
