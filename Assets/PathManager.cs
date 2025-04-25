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
    
    void Awake()
    {
        pathObjects = new List<GameObject>();
    }
    
    /// <summary>
    /// Draws a new path using a LineRenderer.
    /// </summary>
    /// <param name="points">The positions to connect in the path.</param>
    public void DrawPath(Transform start, Transform end)
    {
        GameObject pathObj = new GameObject($"Path_{pathCount}");
        pathObj.transform.parent = this.transform;
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
    }

}
