
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using System;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEditor;


public class OffMeshLinkSimple
{
    public Vector3 start;
    public Vector3 end;
    public bool biDirectional = false;
}
#endif

public static class NavMeshExtensions
{
#if UNITY_EDITOR
    public static NavMeshData GetNavMeshData(Scene scene)
    {
        NavMeshData _nmd = null;

        if (scene != null && scene.IsValid() && !String.IsNullOrEmpty(scene.path))
        {
            StreamReader inp_stm = new StreamReader(scene.path);
            string navMeshData_Line = "";
            while (!inp_stm.EndOfStream)
            {
                string inp_ln = inp_stm.ReadLine().Trim();
                if (inp_ln.StartsWith("m_NavMeshData:"))
                {
                    navMeshData_Line = inp_ln.Trim();
                    break;
                }
            }
            inp_stm.Close();

            if (navMeshData_Line != "")
            {
                string guid_prefix = "guid:";
                if (navMeshData_Line.Contains(guid_prefix))
                {

                    string fileGUID = navMeshData_Line.Substring(navMeshData_Line.IndexOf(guid_prefix) + guid_prefix.Length);
                    fileGUID = fileGUID.Substring(0, fileGUID.IndexOf(","));
                    fileGUID = fileGUID.Trim();

                    var path = AssetDatabase.GUIDToAssetPath(fileGUID);

                    if (path != "")
                    {
                        _nmd = (NavMeshData)AssetDatabase.LoadAssetAtPath(path, typeof(NavMeshData));
                    }
                    else
                    {
                        Debug.LogError("Bad GUID: " + navMeshData_Line);
                    }
                }
                else
                {
                    Debug.LogError("No saved NavMeshData GUID: " + navMeshData_Line);
                }
            }
            else
            {
                Debug.LogError("No m_NavMeshData found in " + scene.path);
            }
        }
        else
        {
            Debug.LogError("Invalid scene");
        }

        return _nmd;

    }

    public static NavMeshData GetNavMeshData()
    {
        return GetNavMeshData(SceneManager.GetActiveScene());
    }

    public static List<OffMeshLinkSimple> GetOffMeshLinks()
    {
        List<OffMeshLinkSimple> l = new List<OffMeshLinkSimple>();

        NavMeshData nmd = GetNavMeshData(SceneManager.GetActiveScene());

        if(nmd != null)
        {
            SerializedObject serializedObject = new SerializedObject(nmd);
            SerializedProperty serializedPropertyMyArray = serializedObject.FindProperty("m_OffMeshLinks");
            if (serializedPropertyMyArray.isArray && serializedPropertyMyArray.arraySize > 0)
            {
                for (int i = 0; i < serializedPropertyMyArray.arraySize; i++)
                {
                    SerializedProperty offMeshLinkProperty = serializedPropertyMyArray.GetArrayElementAtIndex(i);

                    OffMeshLinkSimple ofms = new OffMeshLinkSimple();
                    ofms.start = offMeshLinkProperty.FindPropertyRelative("m_Start").vector3Value;
                    ofms.end = offMeshLinkProperty.FindPropertyRelative("m_End").vector3Value;
                    ofms.biDirectional = offMeshLinkProperty.FindPropertyRelative("m_LinkType").intValue == 2;
                }
            }
        }

        return l;
    }
#endif

    private static Dictionary<int, List<int>> GetSubMeshData(NavMeshTriangulation navmeshData)
    {
        Dictionary<int, List<int>> submeshIndices = new Dictionary<int, List<int>>();

        for (int i = 0; i < navmeshData.areas.Length; i++)
        {
            if (!submeshIndices.ContainsKey(navmeshData.areas[i]))
            {
                submeshIndices.Add(navmeshData.areas[i], new List<int>());
            }

            submeshIndices[navmeshData.areas[i]].Add(navmeshData.indices[3 * i]);
            submeshIndices[navmeshData.areas[i]].Add(navmeshData.indices[3 * i + 1]);
            submeshIndices[navmeshData.areas[i]].Add(navmeshData.indices[3 * i + 2]);
        }

        return submeshIndices;
    }

    public static Mesh GetSingleMesh()
    {
        NavMeshTriangulation navmeshData = NavMesh.CalculateTriangulation();
        Dictionary<int, List<int>> submeshes = GetSubMeshData(navmeshData);

        Mesh m = new Mesh();

        m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        m.name = "NavMesh";
        m.vertices = navmeshData.vertices;
        m.subMeshCount = submeshes.Count;

        int index = 0;
        foreach (KeyValuePair<int, List<int>> entry in submeshes)
        {
            m.SetTriangles(entry.Value.ToArray(), index);
            index++;
        }

        return m;
    }

    public static Mesh[] GetMultiMesh()
    {
        Mesh m = GetSingleMesh();
        Mesh[] ms = new Mesh[m.subMeshCount];
        Vector3[] source = m.vertices;
        for (int i=0; i < ms.Length; i++)
        {
            ms[i] = new Mesh();
            ms[i].indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            ms[i].name = "NavMesh_"+i;

            int[] indices = m.GetTriangles(i);


            List<Vector3> dest = new List<Vector3>();

            Dictionary<int, int> map = new Dictionary<int, int>();

            int[] newIndices = new int[indices.Length];

            for (int j = 0; j < indices.Length; j++)
            {
                int o = indices[j];
                int n;
                if (!map.TryGetValue(o, out n))
                {
                    dest.Add(source[o]);
                    n = dest.Count - 1;
                    map[o] = n;
                }
                newIndices[j] = n;
            }

            ms[i].SetVertices(dest);
            ms[i].triangles = newIndices;
        }

        return ms;
    }
}
