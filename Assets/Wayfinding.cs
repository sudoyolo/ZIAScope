using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Wayfinding : MonoBehaviour
{
    public Selection selection;
    public PathManager pathManager;
    public Transform user;

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
        if (selection.selectedObjects.Count == 1)
        {
            pathManager.ClearSinglePath(-1, selection.selectedObjects[0].GetInstanceID());
        }
        else if (selection.selectedObjects.Count == 2)
        { 
            pathManager.ClearSinglePath(selection.selectedObjects[0].GetInstanceID(), selection.selectedObjects[1].GetInstanceID());
        }
        
    }

    public void clearPaths(String arg)
    {
        pathManager.ClearPaths();
        
    }
}
