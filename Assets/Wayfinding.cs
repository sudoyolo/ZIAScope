using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Wayfinding : MonoBehaviour
{
    public Selection selection;
    public PathManager pathManager;
    public Transform user;
    public MoveAlongPath xrOrigin;
    public ScrollingStringList scrollingList;
    
    public void illuminatePathBetweenDestinations(String arg)
    {
        //can retrieve indexes of path. 
        if (selection.selectedObjects.Count == 1)
        {
            pathManager.DrawPath(user, selection.selectedObjects[0].transform, -1, selection.selectedObjects[0].GetInstanceID());
        }
        else if (selection.selectedObjects.Count == 2)
        { 
            pathManager.DrawPath(selection.selectedObjects[0].transform,selection.selectedObjects[1].transform,selection.selectedObjects[0].GetInstanceID(), selection.selectedObjects[1].GetInstanceID());
        }
    }

    public void clearSinglePath(String arg)
    {
        if (arg.Contains("ClearPathCalledFromNavMesh"))
        {
            String[] parts = arg.Split(' ');
            pathManager.ClearSinglePath(-1, int.Parse(parts[1]));
            return;
        }
        if (selection.selectedObjects.Count == 1)
        {
            pathManager.ClearSinglePath(-1, selection.selectedObjects[0].GetInstanceID());
        }
        else if (selection.selectedObjects.Count == 2)
        { 
            pathManager.ClearSinglePath(selection.selectedObjects[0].GetInstanceID(), selection.selectedObjects[1].GetInstanceID());
        }
        
    }

    public void clearPaths(String input)
    {
        pathManager.ClearPaths();
    }
    public void GoAlongPath(String input)
    {
        Debug.Log("does this ever get called");
        illuminatePathBetweenDestinations(input);
        if (selection.selectedObjects.Count == 1)
        {
            xrOrigin.startTravelling( selection.selectedObjects[0].GetInstanceID());
        }
        string stopcmd = "Press the right controller trigger button to stop moving along the path";
        scrollingList.AddString(stopcmd, "lightblue");
    }
    public void TeleportToObj(String input)
    {
        if (selection.selectedObjects.Count == 1)
        {
            NavMesh.SamplePosition(selection.selectedObjects[0].transform.position, out NavMeshHit hit1, 5f, NavMesh.AllAreas);
            xrOrigin.teleport(hit1.position);
        }
    }
}
