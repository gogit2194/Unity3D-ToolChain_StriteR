﻿
//Fog
#if defined(FOG_LINEAR)||defined(FOG_EXP)||defined(FOG_EXP2)
    #define IFOG
#endif

//Linear: (end-z)/(end-start) = z * (-1/(end-start)) + (end/(end-start))
//Exp  exp(-density*z)
//Exp2 exp(-(density*z)^2)
//z: 0-far

half FogFactor(float z)
{
    half fogFactor=0;

    #if defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2)
    float clipZ01=UNITY_Z_0_FAR_FROM_CLIPSPACE(z);
    #if defined(FOG_LINEAR)
    fogFactor = saturate(clipZ01 * unity_FogParams.z + unity_FogParams.w);
    #elif defined(FOG_EXP) || defined(FOG_EXP2)
    // factor = exp(-(density*z)^2)
    // -density * z computed at vertex
    return real(unity_FogParams.x * clipZ01);
    #endif
    #endif
    
    return fogFactor;
}

half FogDesnity(float fogFactor)
{
    half fogDensity=0;
    #if defined(FOG_EXP)
    fogDensity = saturate(exp2(-fogFactor));
    #elif defined(FOG_EXP2)
    fogDensity = saturate(exp2(-fogFactor * fogFactor));
    #elif defined(FOG_LINEAR)
    fogDensity = fogFactor;
    #endif
    return fogDensity;
}

half3 FogInterpolate(half3 srcColor,half fogFactor)
{
    half density=FogDesnity(fogFactor);
    return lerp(unity_FogColor.rgb,srcColor,density*unity_FogColor.a);
}

#if !defined(IFOG)||defined(NFOG)
    #define FOG_COORD(index)  
    #define FOG_TRANSFER(v)  
    #define FOG_MIX(i,col) 
#else
    #define FOG_COORD(index) half fogFactor:TEXCOORDindex;
    #define FOG_TRANSFER(v) v.fogFactor=FogFactor(v.positionCS.z);
    #define FOG_MIX(i,col) col=FogInterpolate(col,i.fogFactor);
#endif