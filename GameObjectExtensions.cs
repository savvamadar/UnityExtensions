using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions
{

    public static void PositionRotation(this GameObject go, Vector3 wPos, Quaternion wRot)
    {
        if (go == null)
        {
            return;
        }


        go.transform.PositionRotation(wPos, wRot);
    }

    public static void LocalPositionRotation(this GameObject go, Vector3 lPos, Quaternion lRot)
    {
        if (go == null)
        {
            return;
        }

        go.transform.LocalPositionRotation(lPos, lRot);
    }

    public static void SetLayerRecursive(this GameObject go, int new_layer)
    {
        if (go == null)
        {
            return;
        }

        go.transform.SetLayerRecursive(new_layer);
    }

    public static T GetOrAddComponent<T>(this GameObject go) where T : UnityEngine.Component
    {
        if(go == null)
        {
            return null;
        }

        T c = go.GetComponent<T>();
        if(c == null)
        {
            c = go.AddComponent<T>();
        }
        return c;
    }
}
