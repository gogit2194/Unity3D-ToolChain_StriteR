﻿using System.Collections;
using System.Collections.Generic;
using Runtime.Geometry.Validation;
using Unity.Mathematics;
using UnityEngine;

namespace Runtime.Geometry
{

    public struct GFrustumRays:IEnumerable<GRay>
    {
        public GRay bottomLeft;                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               
        public GRay bottomRight;
        public GRay topRight;
        public GRay topLeft;
        public float farDistance;
        public GFrustumRays(float3 origin, quaternion rotation, float fov, float aspect, float zNear, float zFar)
        {
            var halfHeight = zNear * Mathf.Tan(fov * .5f * Mathf.Deg2Rad);
            var forward = math.mul(rotation , kfloat3.forward);
            var toRight = math.mul(rotation , kfloat3.right * halfHeight * aspect);
            var toTop = math.mul(rotation , kfloat3.up * halfHeight);

            var tl = forward * zNear + toTop - toRight;
            float scale = tl.magnitude() / zNear;
            tl = tl.normalize();
            tl *= scale;
            var tr = forward * zNear + toTop + toRight;
            tr = tr.normalize();
            tr *= scale;
            var bl = forward * zNear - toTop - toRight;
            bl = bl.normalize();
            bl *= scale;
            var br = forward * zNear - toTop + toRight;
            br = br.normalize();
            br *= scale;

            topLeft = new GRay(origin + tl * zNear, tl);
            topRight = new GRay(origin + tr * zNear, tr);
            bottomLeft = new GRay(origin + bl * zNear, bl);
            bottomRight = new GRay(origin + br * zNear, br);
            farDistance = zFar - zNear;
        }

        public GRay GetRay(float2 _viewportPoint)
        {
            return new GRay()
            {
                origin = umath.bilinearLerp(bottomLeft.origin, bottomRight.origin, topRight.origin, topLeft.origin,
                    _viewportPoint.x, _viewportPoint.y),
                direction = umath.bilinearLerp(bottomLeft.direction, bottomRight.direction, topRight.direction,
                    topLeft.direction, _viewportPoint)
            };

        }
        
        public IEnumerator<GRay> GetEnumerator()
        {
            yield return bottomLeft;
            yield return bottomRight;
            yield return topRight;
            yield return topLeft;
        }
        IEnumerator IEnumerable.GetEnumerator()=> GetEnumerator();

        public GFrustumPoints GetFrustumPoints()
        {
            var farBottomLeft = bottomLeft.GetPoint(farDistance);
            var farBottomRight = bottomRight.GetPoint(farDistance);
            var farTopRight = topRight.GetPoint(farDistance);
            var farTopLeft = topLeft.GetPoint(farDistance);
            return new GFrustumPoints()
            {
                nearBottomLeft = bottomLeft.origin,
                nearBottomRight = bottomRight.origin,
                nearTopRight = topRight.origin,
                nearTopLeft = topLeft.origin,
                farBottomLeft = farBottomLeft,
                farBottomRight = farBottomRight,
                farTopRight = farTopRight,
                farTopLeft = farTopLeft,
                bounding = UBounds.GetBoundingBox(new []{bottomLeft.origin,bottomRight.origin,topRight.origin,topLeft.origin,farBottomLeft,farBottomRight,farTopRight,farTopLeft}),
            };
        }
    }
}