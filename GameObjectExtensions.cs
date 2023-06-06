using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions
{
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

    public static void SetLayerRecursive(this GameObject go, int new_layer)
    {
        if (go == null)
        {
            return;
        }
        SetLayerRecursive(go.transform, new_layer);
    }

    public static T GetOrAddComponent<T>(this GameObject go) where T : UnityEngine.Component
    {
        T c = go.GetComponent<T>();
        if(c == null)
        {
            c = go.AddComponent<T>();
        }
        return c;
    }
}
