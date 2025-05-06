using UnityEngine;
using System.Collections.Generic;
using System;

public class Selection : MonoBehaviour
{
    [Header("Scene Configuration")]
    public GameObject sceneObjects;
    public SceneHierarchyParser parser;

    [Header("Player and Visual Feedback")]
    [SerializeField] public Transform playerPos;
    [SerializeField] private Material selectedMat;

    [Header("Internal State")]
    public List<GameObject> selectedObjects = new List<GameObject>();
    private Dictionary<GameObject, int> originalLayers = new Dictionary<GameObject, int>();
    public string result;

    public void SelectObject(string objIdx)
    {
        // Revert layers of previously selected objects
        foreach (GameObject obj in selectedObjects)
        {
            if (originalLayers.ContainsKey(obj))
            {
                SetLayerRecursively(obj, originalLayers[obj]);
            }
        }

        selectedObjects.Clear();
        originalLayers.Clear();

        string[] parts = objIdx.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        int highlightLayer = LayerMask.NameToLayer("Highlight");

        if (highlightLayer == -1)
        {
            Debug.LogError("Layer 'Highlight' not defined in Tags and Layers.");
            return;
        }

        foreach (string part in parts)
        {
            if (int.TryParse(part, out int number))
            {
                if (number >= 0 && number < parser.rootChildren.Count)
                {
                    GameObject obj = parser.rootChildren[number];
                    selectedObjects.Add(obj);

                    // Store original layer
                    originalLayers[obj] = obj.layer;

                    // Set to Highlight layer
                    //SetLayerRecursively(obj, highlightLayer);

                    // Optional: change material
                    // ChangeMaterial(obj, selectedMat);
                }
                else
                {
                    Debug.LogWarning($"Index {number} is out of bounds in rootChildren.");
                }
            }
            else
            {
                Debug.LogWarning($"Could not parse '{part}' as an integer.");
            }
        }
    }

    public void ChangeMaterial(GameObject obj, Material newMat)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = newMat;
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}