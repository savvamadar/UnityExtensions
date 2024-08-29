using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MaterialExtensions
{
    private static string instanceString = " (Instance)";
    public static string nonInstancedName(this Material m)
    {
        if (m == null)
        {
            return null;
        }

        string s = m.name;
        while(s.EndsWith(instanceString))
        {
            s = s.Substring(0, s.Length - instanceString.Length);
        }
        return s;
    }

    public static bool isInstanced(this Material m)
    {
        if(m == null)
        {
            return false;
        }

        return m.name.EndsWith(instanceString);
    }
}
