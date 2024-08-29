using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions
{
    public static void PositionRotation(this Transform t, Vector3 wPos, Quaternion wRot)
    {
        if (t == null)
        {
            return;
        }


        t.position = wPos;
        t.rotation = wRot;
    }

    public static void LocalPositionRotation(this Transform t, Vector3 lPos, Quaternion lRot)
    {
        if (t == null)
        {
            return;
        }

        t.localPosition = lPos;
        t.localRotation = lRot;
    }

    public static void SetLayerRecursive(this Transform t, int new_layer)
    {
        if (t == null)
        {
            return;
        }

        t.gameObject.layer = new_layer;

        for (int i = 0; i < t.childCount; i++)
        {
            SetLayerRecursive(t.GetChild(i), new_layer);
        }
    }

    public static T GetOrAddComponent<T>(this Transform t) where T : UnityEngine.Component
    {
        if (t == null)
        {
            return null;
        }

        return t.gameObject.GetOrAddComponent<T>();
    }
}
