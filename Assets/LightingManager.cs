using UnityEngine;
using System.Collections.Generic;

public class LightingManager : MonoBehaviour
{
    [System.Serializable]
    public class EnvironmentSet
    {
        public Material skyboxMaterial;
        public List<GameObject> lights;
    }

    [SerializeField]
    private List<EnvironmentSet> environments = new List<EnvironmentSet>();

    private int currentIndex = -1;

    public void SetEnvironment(string myString)
    {
        int index = int.Parse(myString);

        if (index < 0 || index >= environments.Count)
        {
            Debug.LogWarning("Environment index out of range.");
            return;
        }

        // Deactivate all light sets
        for (int i = 0; i < environments.Count; i++)
        {
            foreach (GameObject light in environments[i].lights)
            {
                if (light != null)
                    light.SetActive(false);
            }
        }

        // Activate selected environment
        EnvironmentSet selected = environments[index];

        RenderSettings.skybox = selected.skyboxMaterial;

        foreach (GameObject light in selected.lights)
        {
            if (light != null)
                light.SetActive(true);
        }

        currentIndex = index;
        Debug.Log("Environment set to index: " + index);
    }

    public void SetLightColorFromRGB(string rgbString)
    {
        Debug.Log("are we even getting called here");
        if (currentIndex < 0 || currentIndex >= environments.Count)
        {
            Debug.LogWarning("No active environment. Defaulting to index 0.");
            currentIndex = 0;
        }


        string[] parts = rgbString.Split(';');
        if (parts.Length != 3)
        {
            Debug.LogWarning("RGB format must be R;G;B");
            return;
        }

        // Try parsing as float
        if (float.TryParse(parts[0], out float r) &&
            float.TryParse(parts[1], out float g) &&
            float.TryParse(parts[2], out float b))
        {
            // If values are in 0–255 range, normalize them
            if (r > 1f || g > 1f || b > 1f)
            {
                r /= 255f;
                g /= 255f;
                b /= 255f;
            }

            Color color = new Color(r, g, b);

            foreach (var lightObj in environments[currentIndex].lights)
            {
                Light lightComponent = lightObj.GetComponent<Light>();
                if (lightComponent != null)
                {
                    lightComponent.color = color;
                    Debug.Log("changing the color rn");
                }
                else
                {
                    Debug.Log("not found");
                }
            }


            Debug.Log($"Set light color to ({r}, {g}, {b})");
        }
        else
        {
            Debug.LogWarning("Invalid RGB values.");
        }
    }

    public void ToggleAllLampChildren(string buff)
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("lamp") && obj.transform.childCount > 0)
            {
                Transform child = obj.transform.GetChild(0);
                bool currentState = child.gameObject.activeSelf;
                child.gameObject.SetActive(!currentState);
                Debug.Log($"Toggled {child.name} under {obj.name} to {!currentState}");
            }
        }
    }

}
