using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System;

public class Selection: MonoBehaviour
{
    public GameObject sceneObjects;
    public List<GameObject> rootChildren = new List<GameObject>();
    [SerializeField] public Transform playerPos; 
    [SerializeField] private Material selectedMat;
    public List<GameObject> selectedObjects = new List<GameObject>();
    public string result; 
    
    void Start()
    {
        // set up indexed scene of all objects in scene
        foreach (Transform child in sceneObjects.transform){
            rootChildren.Add(child.gameObject);
        }
    }

    public void SelectObject(string objIdx)
    {
        selectedObjects.Clear();

        string[] parts = objIdx.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (string part in parts)
        {
            if (int.TryParse(part, out int number))
            {
                selectedObjects.Add(rootChildren[number]);
            }
            else
            {
                Console.WriteLine($"Warning: Could not parse '{part}' as an integer.");
            }
        }

        /*for (int i = 0; i<selectedObjects.Count; i++)
        {
            // change this to new appearance change function
            ChangeMaterial(selectedObjects[i], selectedMat);
        }*/

    }

    public void ChangeMaterial(GameObject obj, Material newMat)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = newMat;
        }
    }
}
