using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathManager : MonoBehaviour
{
    [SerializeField] 
    private Material lineMaterial;
    [SerializeField] 
    private float lineWidth = 0.05f;

    private int pathCount = 0;
    private List<GameObject> pathObjects;
    private Dictionary<Tuple<int, int>, GameObject> objectPaths;
    void Awake()
    {
        pathObjects = new List<GameObject>();
        objectPaths = new Dictionary<Tuple<int, int>, GameObject>();
    }
    
    /// <summary>
    /// Draws a new path using a LineRenderer.
    /// </summary>
    /// <param name="points">The positions to connect in the path.</param>
    public void DrawPath(Transform start, Transform end, int idx1, int idx2)
    {
        if (objectPaths.ContainsKey(Tuple.Create(idx1, idx2))) return;
        if (objectPaths.ContainsKey(Tuple.Create(idx2, idx1))) return;
        GameObject pathObj = new GameObject($"Path_{pathCount}");
        pathObj.transform.parent = this.transform;
        objectPaths[Tuple.Create(idx1, idx2)] = pathObj;
        pathCount++;

        LineRenderer lr = pathObj.AddComponent<LineRenderer>();

        SingleNavPath path_data = pathObj.AddComponent<SingleNavPath>();
        path_data.lineRenderer = lr;
        lr.widthMultiplier = lineWidth;
        lr.material = lineMaterial;
        lr.useWorldSpace = true;
        lr.numCapVertices = 4;
        lr.numCornerVertices = 4;
        path_data.start = start;
        path_data.end = end;
        pathObjects.Add(pathObj);
    }

    /// <summary>
    /// Clears all existing paths.
    /// </summary>
    public void ClearPaths()
    {
        foreach (var obj in pathObjects)
        {
            Destroy(obj);
        }
        pathObjects.Clear();
        pathCount = 0;
        objectPaths.Clear();
    }
    /// <summary>
    /// Clears a single path.
    /// </summary>
    public void ClearSinglePath(int idx1, int idx2)
    {
        if (!objectPaths.ContainsKey(Tuple.Create(idx1, idx2)) &&
            !objectPaths.ContainsKey(Tuple.Create(idx2, idx1))) return;
        if (objectPaths.ContainsKey(Tuple.Create(idx2, idx1)))
        {
            (idx1, idx2) = (idx2, idx1);
        }
        GameObject obj = objectPaths[Tuple.Create(idx1, idx2)];
        objectPaths.Remove(Tuple.Create(idx1, idx2));
        pathObjects.Remove(obj);
        Destroy(obj);
        pathCount -= 1;
    }
}
