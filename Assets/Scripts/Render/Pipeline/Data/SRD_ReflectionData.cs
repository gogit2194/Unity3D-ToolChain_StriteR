using System;
using System.Collections;
using System.Collections.Generic;
using Rendering.PostProcess;
using UnityEngine;
namespace Rendering.Pipeline
{
    [Serializable]
    public class SRD_ReflectionData
    {
        public EReflectionSpace m_Type;
        [MFoldout(nameof(m_Type), EReflectionSpace.PlanarScreenSpace)] [Range(1, 4)] public int m_Sample;

        [MFoldout(nameof(m_Type), EReflectionSpace.PlanarMirrorSpace)] public bool m_Recull;
        [MFoldout(nameof(m_Type), EReflectionSpace.PlanarMirrorSpace,nameof(m_Recull),true)] [Range(0,8)]public int m_AdditionalLightcount;
        [MFoldout(nameof(m_Type), EReflectionSpace.PlanarMirrorSpace)] public bool m_IncludeTransparent;
        
        [Header("Blur")]
        [Range(1,4)] public int m_DownSample;
        public bool m_EnableBlur;
        [MFoldout(nameof(m_EnableBlur), true)] public PPData_Blurs m_BlurParam;

        public static SRD_ReflectionData Default()
        {
            return new SRD_ReflectionData()
            {
                m_Type = EReflectionSpace.PlanarScreenSpace,
                m_IncludeTransparent = false,
                m_Recull = false,
                m_DownSample=2,
                m_AdditionalLightcount=8,
                m_Sample = 1,
                m_EnableBlur = true,
                m_BlurParam = UPipeline.GetDefaultPostProcessData<PPData_Blurs>(),
            };
        }
    }
}