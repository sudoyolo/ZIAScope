using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SingleNavPath : MonoBehaviour
{
    public Transform start;
    public Transform end;
    private NavMeshPath path;
    public LineRenderer lineRenderer;
    private float lastUpdateTime;
    private float updateInterval;
    void Start()
    {
        lastUpdateTime = Time.time;
        updateInterval = 0.1f;
        path = new NavMeshPath();
    }

    void Update()
    {
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            bool firstHitBool = NavMesh.SamplePosition(start.position, out NavMeshHit hit1, 5f, NavMesh.AllAreas);
            bool secondHitBool = NavMesh.SamplePosition(end.position, out NavMeshHit hit2, 5f, NavMesh.AllAreas);
            if (!firstHitBool || !secondHitBool)
            {
                print("navmesh can't be found.");
            }
            NavMesh.CalculatePath(hit1.position, hit2.position, NavMesh.AllAreas, path);
            if (lineRenderer is not null && path is not null)
            {
                lineRenderer.positionCount = path.corners.Length;
                lineRenderer.SetPositions(path.corners);
            }

            lastUpdateTime = Time.time;
        }
       
    }
    /*
    NavMeshPath FindBestPathNear(Vector3 sourcePosition, Vector3 targetPosition, float radius, int sampleCount = 10)
    {
        NavMeshPath bestPath = null;
        float bestPathLength = float.MaxValue;

        for (int i = 0; i < sampleCount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * radius;
            Vector3 samplePos = targetPosition + new Vector3(randomOffset.x, 0, randomOffset.y);

            // Make sure it's on the NavMesh
            if (NavMesh.SamplePosition(samplePos, out NavMeshHit hit, 1.0f, NavMesh.AllAreas);)
            {
                NavMeshPath path = new NavMeshPath();
                if (NavMesh.CalculatePath(sourcePosition, hit.position, NavMesh.AllAreas, path)
                    && path.status == NavMeshPathStatus.PathComplete)
                {
                    float length = GetPathLength(path);
                    if (length < bestPathLength)
                    {
                        bestPath = path;
                        bestPathLength = length;
                    }
                }
            }
        }

        return bestPath;
    }

// Helper function to measure path length
    float GetPathLength(NavMeshPath path)
    {
        float length = 0f;
        for (int i = 1; i < path.corners.Length; i++)
        {
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }
        return length;
    }*/

}



