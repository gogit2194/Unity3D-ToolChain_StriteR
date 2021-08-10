﻿Shader "Hidden/PostProcess/VHS"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            
            #include "Assets/Shaders/Library/PostProcess.hlsl"
            #pragma multi_compile_local _ _SCREENCUT_HARD _SCREENCUT_SCALED
            #pragma shader_feature_local _LINEDISTORT
            #pragma shader_feature_local _PIXELDISTORT
            float2 _ScreenCutTarget;
            #if _LINEDISTORT
            float _LineDistortSpeed;
            float _LineDistortStrength;
            float _LineDistortClip;
            float _LineDistortFrequency;
            #endif
            #if _PIXELDISTORT
            float2 _PixelDistortScale;
            float _PixelDistortFrequency;
            float _PixelDistortClip;
            float _PixelDistortStrength;
            #endif
            float2 RemapUV(float2 uv)
            {
                uv -= 0.5;
                #if _SCREENCUT_HARD
                uv.x=sign(uv.x)*clamp(abs(uv.x),0,_ScreenCutTarget.x);
                uv.y=sign(uv.y)*clamp(abs(uv.y),0,_ScreenCutTarget.y);
                #elif _SCREENCUT_SCALED
                uv*=2;
                uv.x=sign(uv.x)*lerp(0,_ScreenCutTarget.x,abs(uv.x));
                uv.y=sign(uv.y)*lerp(0,_ScreenCutTarget.y,abs(uv.y));
                #endif
                
                #if _LINEDISTORT
                half lineDistort= lerp(-1,1,frac(uv.y*_LineDistortFrequency+_Time.y*_LineDistortSpeed));
                lineDistort=abs(lineDistort);
                lineDistort=smoothstep(_LineDistortClip,1,lineDistort);
                uv.x+=lineDistort*_LineDistortStrength;
                #endif
                uv += .5;

                #if _PIXELDISTORT
                half2 pixelDistort=floor(uv*_PixelDistortScale*_MainTex_TexelSize.zw)*(_PixelDistortScale*_MainTex_TexelSize.xy)+random01(floor(_Time.y*_PixelDistortFrequency)/_PixelDistortFrequency);
                half pixelDistortRandom=random01(pixelDistort);
                uv += step(_PixelDistortClip,pixelDistortRandom)*lerp(-1,1,pixelDistort)*_PixelDistortStrength;
                #endif
            	return uv;
            }

            #pragma shader_feature_local _COLORBLEED
            #pragma shader_feature_local _GRAIN
            #pragma shader_feature_local _GRAIN_CIRCLE
            #pragma shader_feature_local _VIGNETTE
            #if _COLORBLEED
            float _ColorBleedStrength;
            float _ColorBleedIteration;
            float _ColorBleedSize;
            float2 _ColorBleedR;
            float2 _ColorBleedG;
            float2 _ColorBleedB;
            #endif
            #if _GRAIN
            float2 _GrainScale;
            float4 _GrainColor;
            float _GrainClip;
            float _GrainFrequency;
            #if _GRAIN_CIRCLE
            float _GrainCircleWidth;
            #endif
            #endif
            #if _VIGNETTE
            float3 _VignetteColor;
            float _VignetteValue;
            #endif
            half3 VHS(half3 col,float2 uv)
            {
                #if _COLORBLEED
                half colorBleedOffset=0;
                half3 bleedCol=0;
                for(int k=0;k<_ColorBleedIteration;k++)
                {
                    colorBleedOffset+=_ColorBleedSize;
                    bleedCol.r+=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv+colorBleedOffset*_MainTex_TexelSize.xy*_ColorBleedR).r;
                    bleedCol.g+=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv+colorBleedOffset*_MainTex_TexelSize.xy*_ColorBleedG).g;
                    bleedCol.b+=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv+colorBleedOffset*_MainTex_TexelSize.xy*_ColorBleedB).b;
                }
                bleedCol/=_ColorBleedIteration;
                col=lerp(col,bleedCol,_ColorBleedStrength);
                #endif
                
                #if _GRAIN
                half2 grainUV=uv*_MainTex_TexelSize.zw*_GrainScale;
                half rand= random01(floor(grainUV)+random01(floor(_Time.y*_GrainFrequency)/_GrainFrequency));
                half grain=step(_GrainClip,rand)*rand*_GrainColor.a;
                #if _GRAIN_CIRCLE
                float2 circleUV=grainUV%1-.5;
                float circleDistance=dot(circleUV,circleUV);
                grain*=step(circleDistance,.5-_GrainCircleWidth);
                #endif
                col=lerp(col,_GrainColor.rgb,grain);
                #endif

                #if _VIGNETTE
                uv-=.5;
                float vignette = ( 1-uv.y*uv.y*_VignetteValue)*saturate(1-uv.x*uv.x*_VignetteValue);
                col=lerp(_VignetteColor,col,saturate(vignette));
                #endif
				return col;
            }
            half4 frag (v2f_img i) : SV_Target
            {
                float2 uv= RemapUV(i.uv);
                half3 col= SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv).rgb;
				col=VHS(col,uv);
                return half4(col,1);
            }
            ENDHLSL
        }
    }
}
