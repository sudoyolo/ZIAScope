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
    public void illuminatePathToDestination(String arg)
    {
        int.TryParse(arg, out int idxObject);
        if (selection.selectedObjects.Count != 1)
        {
            print("Invalid number of selected objects for illumination path");
            return;
        }
        pathManager.DrawPath(user,selection.selectedObjects[0].transform);
        
    }

    public void illuminatePathBetweenDestinations(String arg)
    {
        if (selection.selectedObjects.Count == 1)
        {
            print("Creating Path from user as only one destination is selected. ");
        }

        if (selection.selectedObjects.Count != 2)
        {
            print("Invalid number of selected objects for illumination path");
            return;
        }
        pathManager.DrawPath(selection.selectedObjects[0].transform,selection.selectedObjects[1].transform);
        
    }

    public void clearPaths(String arg)
    {
        //Debug.Log("Clear paths called");
        pathManager.ClearPaths();
        
    }
}
