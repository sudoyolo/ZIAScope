using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SingleNavPath : MonoBehaviour
{
    public Transform start;
    public Transform end;
    public NavMeshPath path;
    public LineRenderer lineRenderer;
    private float lastUpdateTime;
    private float updateInterval;
    public bool shouldUpdate;
    void Start()
    {
        lastUpdateTime = Time.time;
        updateInterval = 0.1f;
        path = new NavMeshPath();
        shouldUpdate = true;
    }

    void Update()
    {
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            bool firstHitBool = NavMesh.SamplePosition(start.position, out NavMeshHit hit1, 100f, NavMesh.AllAreas);
            bool secondHitBool = NavMesh.SamplePosition(end.position, out NavMeshHit hit2, 100f, NavMesh.AllAreas);
            if (!firstHitBool || !secondHitBool)
            {
                print("navmesh can't be found.");
            }

            if (shouldUpdate)
            {
                NavMesh.CalculatePath(hit1.position, hit2.position, NavMesh.AllAreas, path);
            }

            if (lineRenderer is not null && path is not null)
            {
                lineRenderer.positionCount = path.corners.Length;
                Vector3[] raised_path = new Vector3[path.corners.Length];
                for (int i = 0; i < path.corners.Length; i++)
                {
                    raised_path[i] = path.corners[i] + Vector3.up * 0.03f; // Lift by 1 cm
                }
                lineRenderer.SetPositions(raised_path);
            }

            lastUpdateTime = Time.time;
        }
       
    }
    public NavMeshPath GetPath()
    {
        return path;
    }
}