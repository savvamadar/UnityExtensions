using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MaterialExtensions
{
    private static string _InstanceString = " (Instance)";
    public static string NonInstancedName(this Material m)
    {
        if (m == null)
        {
            return null;
        }

        if (!m.IsInstanced())
        {
            return m.name;
        }

        string s = m.name;
        while(s.EndsWith(_InstanceString))
        {
            s = s.Substring(0, s.Length - _InstanceString.Length);
        }
        return s;
    }

    public static bool IsInstanced(this Material m)
    {
        if(m == null)
        {
            return false;
        }

        return m.name.EndsWith(_InstanceString);
    }
}
