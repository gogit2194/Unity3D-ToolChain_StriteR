﻿Shader "Game/Lit/UberPBR"
{
	Properties
	{
		[Header(Base Tex)]
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
		[ToggleTex(_NORMALMAP)][NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		[Header(Detail Tex)]
		[ToggleTex(_DETAILNORMALMAP)]_DetailNormalTex("Normal Tex",2D)="white"{}
		[Enum(Linear,0,Overlay,1,PartialDerivative,2,UDN,3,Reoriented,4)]_DetailBlendMode("Normal Blend Mode",int)=0
		[ToggleTex(_MATCAP)] [NoScaleOffset]_Matcap("Mat Cap",2D)="white"{}		[ToggleTex(_PBRMAP)] [NoScaleOffset]_PBRTex("PBR Tex",2D)="white"{}
		
		[Header(PBR)]
		[Fold(_PBRMAP)]_Glossiness("Glossiness",Range(0,1))=1
        [Fold(_PBRMAP)]_Metallic("Metalness",Range(0,1))=0
		[Header(Roughness.Metallic.AO)]
        [KeywordEnum(BlinnPhong,CookTorrance,Beckmann,Gaussian,GGX,TrowbridgeReitz,Anisotropic_TrowbridgeReitz,Anisotropic_Ward)]_NDF("Normal Distribution:",float) = 2
		[Foldout(_NDF_ANISOTROPIC_TROWBRIDGEREITZ,_NDF_ANISOTROPIC_WARD)]_AnisoTropicValue("Anisotropic Value:",Range(0,1))=1
		[KeywordEnum(BlinnPhong,GGX)]_VF("Vsibility * Fresnel:",float)=1
	
		[Header(Depth)]
		[ToggleTex(_DEPTHMAP)][NoScaleOffset]_DepthTex("Texure",2D)="white"{}
		[Foldout(_DEPTHMAP)]_DepthScale("Scale",Range(0.001,.5))=1
		[Foldout(_DEPTHMAP)]_DepthOffset("Offset",Range(-.5,.5))=0
		[Toggle(_DEPTHBUFFER)]_DepthBuffer("Affect Buffer",float)=1
		[Foldout(_DEPTHBUFFER)]_DepthBufferScale("Affect Scale",float)=1
		[Toggle(_PARALLAX)]_Parallax("Parallax",float)=0
		[Enum(_16,16,_32,32,_64,64,_128,128)]_ParallaxCount("Parallax Count",int)=16
		
		[Header(Misc)]
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",int)=1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend",int)=0
        [Enum(Off,0,On,1)]_ZWrite("Z Write",int)=1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
        [Enum(Off,0,Front,1,Back,2)]_Cull("Cull",int)=2
	}
	SubShader
	{
		Tags { "Queue" = "Geometry" }
		Blend [_SrcBlend] [_DstBlend]
		ZWrite [_ZWrite]
		ZTest [_ZTest]
		Cull [_Cull]
		Pass
		{
			NAME "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma target 3.5
			
			#include "Assets/Shaders/Library/CommonInclude.hlsl"
			#include "Assets/Shaders/Library/CommonLightingInclude.hlsl"
			#include "Assets/Shaders/Library/BRDFInclude.hlsl"
			#include "Assets/Shaders/Library/GlobalIlluminationInclude.hlsl"
			
			#pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_CALCULATE_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
			
			#pragma shader_feature_local _PBRMAP
			#pragma shader_feature_local _SPECULAR
			#pragma shader_feature_local _NORMALMAP
			#pragma shader_feature_local _DETAILNORMALMAP
			#pragma shader_feature_local _MATCAP
            
			#pragma multi_compile_local _NDF_BLINNPHONG _NDF_COOKTORRANCE _NDF_BECKMANN _NDF_GAUSSIAN _NDF_GGX _NDF_TROWBRIDGEREITZ _NDF_ANISOTROPIC_TROWBRIDGEREITZ _NDF_ANISOTROPIC_WARD
			#pragma multi_compile_local _VF_BLINNPHONG _VF_GGX
		
			TEXTURE2D( _MainTex); SAMPLER(sampler_MainTex);
			TEXTURE2D(_PBRTex);SAMPLER(sampler_PBRTex);
			TEXTURE2D(_Matcap);SAMPLER(sampler_Matcap);
			TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
			TEXTURE2D(_DetailNormalTex);SAMPLER(sampler_DetailNormalTex);
			TEXTURE2D(_DepthTex);SAMPLER(sampler_DepthTex);
			UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
				INSTANCING_PROP(float,_Glossiness)
				INSTANCING_PROP(float,_Metallic)
				INSTANCING_PROP(float,_DetailBlendMode)
				INSTANCING_PROP(float,_AnisoTropicValue)
				INSTANCING_PROP(float4,_MainTex_ST)
				INSTANCING_PROP(float4,_DetailNormalTex_ST)
				INSTANCING_PROP(float4, _Color)
				INSTANCING_PROP(float,_DepthScale)
				INSTANCING_PROP(float,_DepthOffset)
				INSTANCING_PROP(float,_DepthBufferScale)
				INSTANCING_PROP(int ,_ParallaxCount)
			UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)
			
			#include "Assets/Shaders/Library/Additional/Parallax.hlsl"
			#pragma shader_feature_local _PARALLAX
			#pragma shader_feature_local _DEPTHBUFFER
			#pragma shader_feature_local _DEPTHMAP

			struct a2f
			{
				half3 positionOS : POSITION;
				half3 normalOS:NORMAL;
				half4 tangentOS:TANGENT;
				half2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				half4 positionCS : SV_POSITION;
				half4 uv:TEXCOORD0;
				half3 normalWS:TEXCOORD1;
				half3 tangentWS:TEXCOORD2;
				half3 biTangentWS:TEXCOORD3;
				float3 positionWS:TEXCOORD4;
				half4 positionHCS:TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f vert(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.uv = half4( TRANSFORM_TEX_INSTANCE(v.uv,_MainTex),TRANSFORM_TEX_INSTANCE(v.uv,_DetailNormalTex));
				o.positionCS = TransformObjectToHClip(v.positionOS);
				o.positionWS=  TransformObjectToWorld(v.positionOS);
				o.normalWS=normalize(mul((float3x3)unity_ObjectToWorld,v.normalOS));
				o.tangentWS=normalize(mul((float3x3)unity_ObjectToWorld,v.tangentOS.xyz));
				o.biTangentWS=cross(o.normalWS,o.tangentWS)*v.tangentOS.w;
				o.positionHCS=o.positionCS;
				return o;
			}
			
			half4 frag(v2f i,out half depth:SV_DEPTH) :SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				float3 positionWS=i.positionWS;
				half3 normalWS=normalize(i.normalWS);
				half3 biTangentWS=normalize(i.biTangentWS);
				half3 tangentWS=normalize(i.tangentWS);
				half3x3 TBNWS=half3x3(tangentWS,biTangentWS,normalWS);
				half3 viewDirWS=normalize(TransformWorldToViewDir(positionWS,UNITY_MATRIX_V));
				half3 lightDirWS=normalize(_MainLightPosition.xyz);
				half3 normalTS=half3(0,0,1);
				half2 baseUV=i.uv.xy;
				depth=i.positionCS.z;
				ParallaxUVMapping(baseUV,depth,positionWS,TBNWS,viewDirWS);
				
				#if _NORMALMAP
					normalTS=DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,baseUV));
					#if _DETAILNORMALMAP
						half3 detailNormalTS= DecodeNormalMap(SAMPLE_TEXTURE2D(_DetailNormalTex,sampler_DetailNormalTex,i.uv.zw));
						normalTS=BlendNormal(normalTS,detailNormalTS,INSTANCE(_DetailBlendMode));
					#endif
					normalWS=normalize(mul(transpose(TBNWS), normalTS));
				#endif
				
				half4 color=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,baseUV)*INSTANCE(_Color);
				half3 albedo=color.rgb;

				half glossiness=INSTANCE(_Glossiness);
				half metallic=INSTANCE(_Metallic);
				half ao=1.h;
				float anisotropic=INSTANCE(_AnisoTropicValue);
				#if _PBRMAP
					half3 mix=SAMPLE_TEXTURE2D(_PBRTex,sampler_PBRTex,baseUV).rgb;
					glossiness=1.h-mix.r;
					metallic=mix.g;
					ao=mix.b;
				#endif

				BRDFSurface surface=BRDFSurface_Ctor(albedo,glossiness,metallic,ao,normalWS,tangentWS,viewDirWS);
				
				
				#if _MATCAP
					half2 matcapUV=half2(dot(UNITY_MATRIX_V[0].xyz,normalWS),dot(UNITY_MATRIX_V[1].xyz,normalWS));
					matcapUV=matcapUV*.5h+.5h;
					lightCol=SAMPLE_TEXTURE2D(_Matcap,sampler_Matcap,matcapUV).rgb;
				#endif
				half3 brdfColor=0;
				half3 indirectDiffuse=IndirectBRDFDiffuse(surface.normal);
				half3 indirectSpecular=IndirectBRDFSpecular(surface.reflectDir, surface.perceptualRoughness,i.positionHCS,normalTS);
				brdfColor+=BRDFGlobalIllumination(surface,indirectDiffuse,indirectSpecular);

				Light mainLight=GetMainLight(TransformWorldToShadowCoord(positionWS));
				BRDFLight brdfMainLight=BRDFLight_Ctor(surface,mainLight.direction,mainLight.color,mainLight.shadowAttenuation,anisotropic);
				brdfColor+=BRDFLighting(surface,brdfMainLight);
				
				#if _ADDITIONAL_LIGHTS
            	uint pixelLightCount = GetAdditionalLightsCount();
			    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
			    {
			    	BRDFLight light=BRDFLight_Ctor(surface, GetAdditionalLight(lightIndex,i.positionWS),anisotropic);
					brdfColor+=BRDFLighting(surface,light);
			    }
            	#endif
				return half4(brdfColor,1.h);
			}
			ENDHLSL
		}

		USEPASS "Hidden/ShadowCaster/MAIN"
		USEPASS "Hidden/DepthOnly/MAIN"
	}
}