
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using System;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEditor;

public class NavMeshEdge
{
    public NavMeshEdge(Vector3 s, Vector3 e, Vector3 n)
    {
        normal = n;
        start = s;
        end = e;
    }

    public NavMeshEdge()
    {
        normal = Vector3.zero;
        start = Vector3.zero;
        end = Vector3.zero;
    }

    public Vector3 normal;
    public Vector3 start;
    public Vector3 end;

}

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

    public static List<OffMeshLinkSimple> GetOffMeshLinks(NavMeshData _nmd, bool rayCastCorrect = false)
    {
        List<OffMeshLinkSimple> l = new List<OffMeshLinkSimple>();

        NavMeshData nmd = _nmd;

        if (nmd != null)
        {
            SerializedObject serializedObject = new SerializedObject(nmd);

            SerializedProperty serializedPropertyMyArray = serializedObject.FindProperty("m_OffMeshLinks");

            float maxDrop = GetGeneratedOffMeshDropHeight(nmd);
            float maxJump = GetGeneratedOffMeshJumpHeight(nmd);
            float radius = GetNavMeshAgentRadius(nmd);

            if (serializedPropertyMyArray.isArray && serializedPropertyMyArray.arraySize > 0)
            {
                for (int i = 0; i < serializedPropertyMyArray.arraySize; i++)
                {
                    SerializedProperty offMeshLinkProperty = serializedPropertyMyArray.GetArrayElementAtIndex(i);

                    OffMeshLinkSimple ofms = new OffMeshLinkSimple();
                    ofms.start = offMeshLinkProperty.FindPropertyRelative("m_Start").vector3Value;
                    ofms.end = offMeshLinkProperty.FindPropertyRelative("m_End").vector3Value;
                    ofms.biDirectional = offMeshLinkProperty.FindPropertyRelative("m_LinkType").intValue == 2;
                    bool add = !rayCastCorrect;
                    if (rayCastCorrect)
                    {
                        Vector3 down_start = ofms.end + Vector3.up * 1000f;
                        RaycastHit hit;
                        if(Physics.Raycast(ofms.end, Vector3.up, out hit, Mathf.Infinity, -1, QueryTriggerInteraction.Ignore))
                        {
                            down_start = hit.point;
                        }

                        if (Physics.Raycast(down_start, Vector3.down, out hit, Mathf.Infinity, -1, QueryTriggerInteraction.Ignore))
                        {
                            Vector3 dir = (hit.point - ofms.start);
                            if(dir.y >= 0f)
                            {
                                if (dir.magnitude <= maxDrop)
                                {
                                    NavMeshHit nmh;
                                    if (NavMesh.SamplePosition(hit.point, out nmh, radius, -1))
                                    {
                                        ofms.end = nmh.position;
                                        add = true;
                                    }
                                }
                            }
                            else
                            {
                                if(dir.magnitude <= maxJump)
                                {
                                    NavMeshHit nmh;
                                    if (NavMesh.SamplePosition(hit.point, out nmh, radius, -1))
                                    {
                                        ofms.end = nmh.position;
                                        add = true;
                                    }
                                }
                            }
                        }
                    }

                    if (add) {
                        l.Add(ofms);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("NavMeshData = Null");
        }

        return l;
    }

    public static List<OffMeshLinkSimple> GetOffMeshLinks(Scene scene, bool useRayCastCorrect = false)
    {
        return GetOffMeshLinks(GetNavMeshData(scene), useRayCastCorrect);
    }

    public static Vector3[] GetBounds(NavMeshData _nmd)
    {
        Vector3[] bounds = new Vector3[2] { Vector3.zero, Vector3.zero };
        Bounds b = _nmd.sourceBounds;
        bounds[0] = b.min;
        bounds[1] = b.max;

        return bounds;
    }

    public static Vector3 GetDimensions(NavMeshData _nmd)
    {
        Vector3[] bounds = GetBounds(_nmd);
        return bounds[1] - bounds[0];
    }

    public static float GetNavMeshAgentRadius(NavMeshData _nmd)
    {
        SerializedObject serializedObject = new SerializedObject(_nmd);
        SerializedProperty serializedNavSettings = serializedObject.FindProperty("m_NavMeshBuildSettings");

        return serializedNavSettings.FindPropertyRelative("agentRadius").floatValue;
    }

    public static float GetGeneratedOffMeshDropHeight(NavMeshData _nmd)
    {
        SerializedObject serializedObject = new SerializedObject(_nmd);
        SerializedProperty serializedNavSettings = serializedObject.FindProperty("m_NavMeshBuildSettings");

        return serializedNavSettings.FindPropertyRelative("ledgeDropHeight").floatValue;
    }

    public static float GetGeneratedOffMeshJumpHeight(NavMeshData _nmd)
    {
        SerializedObject serializedObject = new SerializedObject(_nmd);
        SerializedProperty serializedNavSettings = serializedObject.FindProperty("m_NavMeshBuildSettings");

        return serializedNavSettings.FindPropertyRelative("maxJumpAcrossDistance").floatValue;
    }

    public static List<Vector3> GetInnerEdgePoints(NavMeshData _nmd)
    {
        Vector3[] outterBounds = GetBounds(_nmd);

        float navAgentRadius = GetNavMeshAgentRadius(_nmd);

        NavMeshTriangulation navmeshData = NavMesh.CalculateTriangulation();

        List<Vector3> innerEdgePoints = new List<Vector3>();

        Vector3[] vertices = navmeshData.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            NavMeshHit nmh;

            bool skip = ((vertices[i].x - navAgentRadius) * 1.001f <= outterBounds[0].x);
            skip = skip || ((vertices[i].x + navAgentRadius) * 1.001f >= outterBounds[1].x);
            skip = skip || ((vertices[i].z - navAgentRadius) * 1.001f <= outterBounds[0].z);
            skip = skip || ((vertices[i].z + navAgentRadius) * 1.001f >= outterBounds[1].z);

            if (!skip && NavMesh.FindClosestEdge(vertices[i], out nmh, -1) && nmh.distance <= 0.0001f)
            {
                innerEdgePoints.Add(vertices[i]);
            }

        }

        return innerEdgePoints;
    }

    public static List<NavMeshEdge> CalculateEdges(List<Vector3> edgePoints, float agentRadius)
    {
        List<NavMeshEdge> navMeshEdges = new List<NavMeshEdge>();
        for (int i = 0; i < edgePoints.Count; i++)
        {
            for (int j = 0; j < edgePoints.Count; j++)
            {
                if (j != i)
                {
                    NavMeshPath nmp = new NavMeshPath();
                    if (NavMesh.CalculatePath(edgePoints[i], edgePoints[j], -1, nmp) && nmp.status == NavMeshPathStatus.PathComplete)
                    {
                        bool validPath = true;
                        int pathPieces = 20;
                        Vector3 headingPiece = (edgePoints[j] - edgePoints[i]) / ((float)pathPieces);
                        Vector3 currentMoveAmount = Vector3.zero;
                        Dictionary<string, Vector3> modeNV = new Dictionary<string, Vector3>();
                        Dictionary<string, int> modeN = new Dictionary<string, int>();

                        for (int parts = 0; parts < pathPieces; parts++)
                        {
                            currentMoveAmount += headingPiece;

                            if (!NavMesh.SamplePosition(edgePoints[i] + currentMoveAmount, out _, 0.1f, -1))
                            {
                                validPath = false;
                                break;
                            }
                            else
                            {
                                NavMeshHit nmhEdge;
                                bool hasEdge = NavMesh.FindClosestEdge(edgePoints[i] + currentMoveAmount, out nmhEdge, -1);
                                if (!hasEdge)
                                {
                                    validPath = false;
                                    break;
                                }
                                else
                                {
                                    if (nmhEdge.distance > 0.1f)
                                    {
                                        validPath = false;
                                        break;
                                    }
                                    else
                                    {
                                        string f2 = nmhEdge.normal.ToString("F2");
                                        if (!modeN.ContainsKey(f2))
                                        {
                                            modeNV[f2] = nmhEdge.normal;
                                            modeN[f2] = 0;
                                        }
                                        modeN[f2]++;
                                    }
                                }

                            }
                        }

                        if (validPath)
                        {
                            int greatestCount = 0;
                            Vector3 modeNormal = Vector3.zero;

                            foreach (var kp in modeN)
                            {
                                if (greatestCount < kp.Value)
                                {
                                    greatestCount = kp.Value;
                                    modeNormal = modeNV[kp.Key];
                                }
                            }

                            modeNormal.y = 0f;

                            navMeshEdges.Add(new NavMeshEdge(edgePoints[i], edgePoints[j], modeNormal));
                        }
                    }
                }
            }
        }

        return navMeshEdges;

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
