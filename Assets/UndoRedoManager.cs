using UnityEngine;
using System.Collections.Generic;

public class UndoRedoManager : MonoBehaviour
{
    [SerializeField] private GameObject targetObject; 
    [SerializeField] public ScrollingStringList scrollingList;
    [SerializeField] private SceneHierarchyParser parser;
    [SerializeField] private Selection select;
    private const int maxSteps = 5;

    private List<GameObject> undoStack = new List<GameObject>();
    private List<GameObject> redoStack = new List<GameObject>();

    // Call this to log a new state
    public void LogState()
    {
        if (targetObject == null)
        {
            Debug.LogWarning("Target object is not assigned.");
            return;
        }

        // Clone and detach mutable components
        GameObject snapshot = Instantiate(targetObject);
        snapshot.SetActive(false);
        snapshot.name = targetObject.name + "_Snapshot";

        // Detach shared materials
        Renderer[] renderers = snapshot.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            // Create new material instance to capture its current visual state
            r.material = new Material(r.material);
        }

        undoStack.Add(snapshot);
        if (undoStack.Count > maxSteps)
        {
            Destroy(undoStack[0]);
            undoStack.RemoveAt(0);
        }

        ClearRedoStack();
        //Debug.Log("logged state");
    }


    public void Undo(string buff)
    {
        if (undoStack.Count == 0)
        {
            Debug.Log("No more undo steps.");
            scrollingList.AddString("No undo steps available", "lightblue");
            return;
        }

        // Save current state to redo
        StoreToRedoStack();

        // Swap to last undo state
        GameObject lastState = undoStack[undoStack.Count - 1];
        undoStack.RemoveAt(undoStack.Count - 1);

        ActivateSnapshot(lastState);

        // Destroy the snapshot after activation
        Destroy(lastState);
    }


    public void Redo(string buff)
    {
        if (redoStack.Count == 0)
        {
            Debug.Log("No more redo steps.");
            scrollingList.AddString("No redo steps available", "lightblue");
            return;
        }

        // Save current state to undo
        StoreToUndoStack();

        // Swap to last redo state
        GameObject nextState = redoStack[redoStack.Count - 1];
        redoStack.RemoveAt(redoStack.Count - 1);

        ActivateSnapshot(nextState);

        // Destroy the snapshot after activation
        Destroy(nextState);
    }


    private void StoreToRedoStack()
    {
        GameObject snapshot = Instantiate(targetObject);
        snapshot.SetActive(false);
        redoStack.Add(snapshot);
        if (redoStack.Count > maxSteps)
        {
            Destroy(redoStack[0]);
            redoStack.RemoveAt(0);
        }
    }

    private void StoreToUndoStack()
    {
        GameObject snapshot = Instantiate(targetObject);
        snapshot.SetActive(false);
        undoStack.Add(snapshot);
        if (undoStack.Count > maxSteps)
        {
            Destroy(undoStack[0]);
            undoStack.RemoveAt(0);
        }
    }

    private void ActivateSnapshot(GameObject snapshot)
    {
        if (targetObject != null)
        {
            Destroy(targetObject);
        }

        GameObject newActive = Instantiate(snapshot);
        newActive.name = snapshot.name.Replace("_Snapshot", "");
        newActive.SetActive(true);
        newActive.transform.position = snapshot.transform.position;
        newActive.transform.rotation = snapshot.transform.rotation;
        newActive.transform.localScale = snapshot.transform.localScale;

        targetObject = newActive;
        parser.setTargetObject(newActive);
        select.selectedObjects.Clear();
    }



    private void ClearRedoStack()
    {
        foreach (var redo in redoStack)
        {
            Destroy(redo);
        }
        redoStack.Clear();
    }
}
