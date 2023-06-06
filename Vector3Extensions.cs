using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extensions
{
    public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
    {
        Vector3 AB = b - a;
        Vector3 AV = value - a;
        return Mathf.Clamp01(Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB));
    }

    public static Vector3 RelativeLocalPosition(Transform parent, Transform child)
    {
        if(parent == child)
        {
            return parent.localPosition;
        }
        return parent.InverseTransformPoint(child.position);
    }
}
