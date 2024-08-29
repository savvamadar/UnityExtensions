using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extensions
{

    [System.Serializable]
    public class SerializableVector3
    {
        public float x;
        public float y;
        public float z;

        public SerializableVector3(float rX, float rY, float rZ)
        {
            x = rX;
            y = rY;
            z = rZ;
        }

        public SerializableVector3(Vector3 vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }

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

    public static void Ceil(ref this Vector3 v)
    {
        v.x = Mathf.Ceil(v.x);
        v.y = Mathf.Ceil(v.y);
        v.z = Mathf.Ceil(v.z);
    }

    public static void Floor(ref this Vector3 v)
    {
        v.x = Mathf.Floor(v.x);
        v.y = Mathf.Floor(v.y);
        v.z = Mathf.Floor(v.z);
    }

    public static void Round(ref this Vector3 v)
    {
        v.x = Mathf.Round(v.x);
        v.y = Mathf.Round(v.y);
        v.z = Mathf.Round(v.z);
    }
}
