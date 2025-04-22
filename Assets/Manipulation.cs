using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;


public class Manipulation : MonoBehaviour
{
    List<Action<string>> functionList;
    public Selection selection;
    public List<Material> materialList = new List<Material>();

    // Start is called before the first frame update
    void Start()
    {
        //selection = GetComponent<Selection>();
        functionList = new List<Action<string>>
        {
            selection.SelectObject,
            ChangePosition,
            ChangeColor,
            ChangeMaterial,
            AssignTag,
            DuplicateObject,
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void parseFunctions(string command)
    {
        if(command.Contains("Nothing")){return;}
        int i = 0;
        while (i < command.Length)
        {
            // read first function idx
            if(command[i]==' '){i++;}
            int funcIndex = int.Parse(command[i].ToString());
            i++; // move past the digit

            // extract argument
            int argStart = i;
            while (i < command.Length && command[i] != ',')
            {
                i++;
            }
            string arg = command.Substring(argStart, i - argStart);
            // Call the function
            if (funcIndex >= 0 && funcIndex < functionList.Count)
            {
                functionList[funcIndex].Invoke(arg);
            }
            else
            {
                Debug.LogError("Invalid function index: " + funcIndex);
            }
            i++; 
        }

    }

// temporary position change
    public void ChangePosition(string arg)
    {
        // Extract the number from the string (default to 1 if not found)
        int amount = 1;
        Match match = Regex.Match(arg, @"\d+");
        if (match.Success)
        {
            int.TryParse(match.Value, out amount);
        }

        // Determine the direction vector
        Vector3 direction = Vector3.zero;

        if (arg.Contains("forward"))
        {
            direction = new Vector3(0f, 0f, -1f);
        }
        else if (arg.Contains("backward"))
        {
            direction = new Vector3(0f, 0f, 1f);
        }
        else if (arg.Contains("up"))
        {
            direction = new Vector3(0f, 1f, 0f);
        }
        else if (arg.Contains("down"))
        {
            direction = new Vector3(0f, -1f, 0f);
        }
        else if (arg.Contains("left"))
        {
            direction = new Vector3(-1f, 0f, 0f);
        }
        else if (arg.Contains("right"))
        {
            direction = new Vector3(1f, 0f, 0f);
        }

        // Apply movement to all selected objects
        foreach (GameObject obj in selection.selectedObjects)
        {
            obj.transform.position += direction * amount;
        }
    }

// temporary patch for color change, may add texture change
    public void ChangeColor(string rgbString) 
    {
        Debug.Log("change color to " + rgbString);
        
        foreach(GameObject obj in selection.selectedObjects) {
            string[] parts = rgbString.Split(';');
            if (int.TryParse(parts[0], out int r) &&
                int.TryParse(parts[1], out int g) &&
                int.TryParse(parts[2], out int b))
            {
                Color color = new Color(r / 255f, g / 255f, b / 255f);

                Renderer rend = obj.GetComponent<Renderer>();
                if (rend != null && rend.material != null)
                {
                    rend.material.SetColor("_BaseColor", color); // Universal RP Base Map
                }
                else
                {
                    Debug.LogError("Renderer or material missing on target object.");
                }
            }
            else
            {
                Debug.LogError("Failed to parse RGB values.");
            }
        }
       
    }

    public void ChangeMaterial(string arg)
    {
        foreach(GameObject obj in selection.selectedObjects) {
            Renderer rend = obj.GetComponent<Renderer>();
            if (rend == null) continue;
            if(arg.Contains("wood"))
            {
                rend.material = materialList[0];
            }
            else if(arg.Contains("steel"))
            {
                rend.material = materialList[1];
            }
            else if(arg.Contains("bronze"))
            {
                rend.material = materialList[2];
            }
            else if(arg.Contains("floorboards"))
            {
                rend.material = materialList[3];
            } 
            else if(arg.Contains("fabric"))
            {
                rend.material = materialList[3];
            } 
        } 
    }

// Assigns a new empty child object to an object labelled tag
    public void AssignTag(string tagname)
    {
        foreach(GameObject obj in selection.selectedObjects) {
            if (obj != null)
            {
                //tagname = "Tag: " + tagname;
                GameObject newChild = new GameObject(tagname);       // empty GameObject
                newChild.transform.SetParent(obj.transform);  // Set as child of global object
                newChild.transform.localPosition = Vector3.zero;       // Optional: reset position
            }
            else
            {
                Debug.LogWarning("Tag adding failed");
            }
        }
    }

    public void DuplicateObject(string objname)
    {
        foreach(GameObject obj in selection.selectedObjects) {
            GameObject duplicate = Instantiate(obj);
            // Set the duplicate's parent to the original object's parent
            duplicate.transform.SetParent(obj.transform.parent, false);
            // Apply a small offset to avoid overlap
            duplicate.transform.position = obj.transform.position + new Vector3(0.5f, 0.0f, 0.5f);
            // Optional: rename or mark it as a duplicate
            duplicate.name = obj.name + "_Copy";
        }
    }
}
