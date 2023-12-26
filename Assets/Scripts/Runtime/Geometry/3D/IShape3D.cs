﻿using Unity.Mathematics;

namespace Geometry
{
    public interface IShape3D : IShapeDimension<float3> 
    {
        // float3 GetSupportPoint(float3 _direction);
    }

    public interface IBounds3D
    {
        public GBox GetBoundingBox();
    }
}