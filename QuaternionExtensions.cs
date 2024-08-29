using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class QuaternionExtensions
{
    [System.Serializable]
    public class SerializableQuaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public SerializableQuaternion(float rX, float rY, float rZ, float rW)
        {
            x = rX;
            y = rY;
            z = rZ;
            w = rW;
        }

        public SerializableQuaternion(Quaternion quaternion)
        {
            x = quaternion.x;
            y = quaternion.y;
            z = quaternion.z;
            w = quaternion.w;
        }

        public Quaternion ToQuaternion()
        {
            return new Quaternion(x, y, z, w);
        }
    }

    public static float InverseLerp(Quaternion a, Quaternion b, Quaternion value)
    {
        float maxDiff = Quaternion.Angle(a, b);
        float avDiff = Quaternion.Angle(a, value);
        float bvDiff = Quaternion.Angle(b, value);

        //very overshot
        if (avDiff >= maxDiff && bvDiff >= maxDiff)
        {
            if (avDiff <= bvDiff)
            {
                //angle is overshooting a and b but closer to a
                return 0f;
            }
            else
            {
                //angle is overshooting a and b but closer to b
                return 1f;
            }
        }

        if (avDiff <= maxDiff && bvDiff >= maxDiff)
        {
            //also over shot but wraps around "validly" closer to a
            return 0f;
        }

        if (bvDiff <= maxDiff && avDiff >= maxDiff)
        {
            //also over shot but wraps around "validly" closer to b
            return 1f;
        }

        return Mathf.Clamp01(avDiff / maxDiff);
    }

    //Get the difference between two quaternions 
    public static Quaternion Difference(Quaternion a, Quaternion b)
    {
        return a * Quaternion.Inverse(b);
    }

    //Adds Quaternion b to a
    public static Quaternion Add(Quaternion a, Quaternion b)
    {
        return b * a;
    }

    public static Quaternion RelativeLocalRotation(Transform parent, Transform child)
    {
        if(parent == child)
        {
            return parent.localRotation;
        }
        return Quaternion.Inverse(parent.rotation) * child.rotation;
    }
}
