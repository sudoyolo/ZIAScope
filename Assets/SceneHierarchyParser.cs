using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System;

public class SceneHierarchyParser : MonoBehaviour
{
    [Header("SceneObjects folder to parse to LLM")]
    public GameObject sceneObjects;
    [Header("Indexed List of Root's Children")]
    public List<GameObject> rootChildren = new List<GameObject>();
    [SerializeField] public Transform playerPos; 
    [SerializeField] private Material selectedMat;


    public List<GameObject> selectedObjects = new List<GameObject>();
    public string result; 
    void Start()
    {
        if (sceneObjects != null)
        {
            rootChildren.Clear();
            result = ParseHierarchy();
            Debug.Log(result);
        }
        else
        {
            Debug.LogWarning("Root object is not assigned.");
        }
    }

    public string ParseHierarchy()
    {
        GameObject root = sceneObjects;
        StringBuilder output = new StringBuilder();
        output.AppendLine($"Items in {root.name}:");

        int index = 0;
        foreach (Transform child in root.transform){
            rootChildren.Add(child.gameObject);
            output.AppendLine($"[{index}] {DescribeObject(child.gameObject)}");
            index++;
        }

        return output.ToString();
    }

    string DescribeObject(GameObject obj)
    {
        Transform t = obj.transform;
        Vector3 size = GetObjectSize(obj);
        Renderer renderer = obj.GetComponent<Renderer>();
        string materialName = renderer != null && renderer.material != null ? renderer.material.name.Replace(" (Instance)", "") : "None";

        // Default to "N/A" if no renderer or color available
        string colorDescription = "N/A";
        if (renderer != null && renderer.material != null && renderer.material.HasProperty("_BaseColor"))
        {
            Color color = renderer.material.GetColor("_BaseColor");
            colorDescription = $"RGBA({color.r:F2}, {color.g:F2}, {color.b:F2}, {color.a:F2})";
        }
        else if (renderer != null && renderer.material != null && renderer.material.HasProperty("_Color"))
        {
            // Fallback for legacy/standard shader "_Color" property
            Color color = renderer.material.GetColor("_Color");
            colorDescription = $"RGBA({color.r:F2}, {color.g:F2}, {color.b:F2}, {color.a:F2})";
        }

        StringBuilder sb = new StringBuilder();
        sb.Append($"{obj.name}: properties: ");
        sb.Append($"[size: {size}], ");
        sb.Append($"[position: {t.localPosition}], ");
        sb.Append($"[distance from user: {GetIntegerDistance(t, playerPos)}], ");
        sb.Append($"[rotation: {t.localEulerAngles}], ");
        sb.Append($"[material: {materialName}], ");
        sb.Append($"[color: {colorDescription}]");

        // List immediate children
        if (t.childCount > 0)
        {
            sb.Append(". Tags: ");
            for (int i = 0; i < t.childCount; i++)
            {
                sb.Append(t.GetChild(i).name);
                if (i < t.childCount - 1)
                    sb.Append(", ");
            }
        }

        return sb.ToString();
    }


    public static int GetIntegerDistance(Transform a, Transform b)
    {
        if (a == null || b == null)
        {
            Debug.LogWarning("One or both transforms are null");
            return -1; 
        }

        float distance = Vector3.Distance(a.position, b.position);
        return Mathf.RoundToInt(distance);
    }

    Vector3 GetObjectSize(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.size;
        }
        else
        {
            return Vector3.one;
        }
    }
}
