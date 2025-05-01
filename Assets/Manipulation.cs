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
    public Wayfinding wayfinding;
    public SceneHierarchyParser parser;
    public ScrollingStringList scrollingList;
    public UndoRedoManager undoredo;

    // Start is called before the first frame update
    void Start()
    {
        //selection = GetComponent<Selection>();
        functionList = new List<Action<string>>
        {
            selection.SelectObject,                      // 0
            ChangePosition,                              // 1
            SetPosition,                                 // 2
            RotateObj,                                   // 3
            ChangeColor,                                 // 4
            ChangeMaterial,                              // 5
            AssignTag,                                   // 6
            DuplicateObject,                             // 7
            DeleteObject,                                // 8
            wayfinding.illuminatePathBetweenDestinations,// 9
            wayfinding.clearSinglePath,                  // 10
            wayfinding.clearPaths,                       // 11
            wayfinding.GoAlongPath,                      // 12
            wayfinding.TeleportToObj,                    // 13
            undoredo.Undo,                               // 14
            undoredo.Redo                                // 15
        };
    }

    void Update()
    {
        
    }

    public void parseFunctions(string command)
    {
        if (string.IsNullOrWhiteSpace(command) || command.Contains("Nothing"))
        {
            return;
        }

        int i = 0;
        while (i < command.Length)
        {
            // Skip whitespace
            while (i < command.Length && command[i] == ' ') { i++; }
            if (i >= command.Length)
                break;

            // Handle commands starting with '?'
            if (command[i] == '?')
            {
                i++; // move past '?'
                while (i < command.Length && command[i] == ' ') { i++; }
                if (i >= command.Length || command[i] != '"')
                {
                    Debug.LogError("Expected opening quotation mark after '?' at position " + i);
                    break;
                }
                i++; // move past opening quote

                int quoteStart = i;
                while (i < command.Length && command[i] != '"')
                {
                    i++;
                }
                if (i >= command.Length)
                {
                    Debug.LogError("Missing closing quotation mark for special command starting at position " + quoteStart);
                    break;
                }

                string specialCommand = command.Substring(quoteStart, i - quoteStart);
                HandleSpecialCommand(specialCommand);

                i++; // move past closing quote
                while (i < command.Length && (command[i] == ' ' || command[i] == ',' || command[i]=='\n')) { i++; } // move past whitespace/comma
                continue;
            }

            if(i == command.Length || i == command.Length-1){break;}
            // Parse full function index (supports multi-digit numbers)
            if (!char.IsDigit(command[i]))
            {
                Debug.LogError($"Unexpected character '{command[i]}' at position {i}. Expected a digit or '?'.");
                break;
            }

            int funcStart = i;
            while (i < command.Length && char.IsDigit(command[i])) {i++;}

            string funcStr = command.Substring(funcStart, i - funcStart);
            if (!int.TryParse(funcStr, out int funcIndex))
            {
                Debug.LogError($"Failed to parse function index at position {funcStart}: '{funcStr}'");
                break;
            }

            int argStart = i;
            while (i < command.Length && command[i] != ',')
            {
                i++;
            }
            string arg = command.Substring(argStart, i - argStart);

            // Call the function
            if (funcIndex >= 0 && funcIndex < functionList.Count)
            {
                if (funcIndex != 0 && (selection == null || selection.selectedObjects.Count == 0))
                {
                    Debug.LogWarning("No previous selection exists, only manipulation applied.");
                    // still continue parsing next functions!
                }
                functionList[funcIndex].Invoke(arg);
            }
            else
            {
                Debug.LogError("Invalid function index: " + funcIndex);
            }

            if (i < command.Length && command[i] == ',')
                i++; // move past comma
        }
    }

    private void HandleSpecialCommand(string specialCommand)
    {
        Debug.Log("Handling special command: \"" + specialCommand + "\"");
        scrollingList.AddString(specialCommand, "lightblue");
    }

    public void SetPosition(string input)
    {
        string[] values = input.Split(';');
        if (values.Length != 3)
        {
            Debug.LogError("Invalid Vector3 format. Expected format: x,y,z");
        }

        float x = float.Parse(values[0]);
        float y = float.Parse(values[1]);
        float z = float.Parse(values[2]);
        
        foreach (GameObject obj in selection.selectedObjects)
        {
            obj.transform.localPosition = new Vector3(x, y+1.0f, z);
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

    public void RotateObj(string degree)
    {
        Debug.Log("object rotation by " + degree);

        if (float.TryParse(degree, out float yRotation))
        {
            foreach (GameObject obj in selection.selectedObjects)
            {
                Vector3 currentEuler = obj.transform.eulerAngles;
                obj.transform.rotation = Quaternion.Euler(currentEuler.x, currentEuler.y + yRotation, currentEuler.z);
            }
        }
        else
        {
            Debug.LogError("Invalid input. Please provide a single float value.");
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
                rend.material = materialList[4];
            } 
            else if(arg.Contains("ceramic"))
            {
                rend.material = materialList[5];
            } 
            else if(arg.Contains("untextured"))
            {
                rend.material = materialList[6];
            } 
            else if(arg.Contains("paintedwall"))
            {
                rend.material = materialList[7];
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
        for (int i = 0; i < selection.selectedObjects.Count; i++)
        {
            GameObject original = selection.selectedObjects[i];
            GameObject duplicate = Instantiate(original);
            duplicate.transform.SetParent(original.transform.parent, false);
            duplicate.transform.position = original.transform.position + new Vector3(0.5f, 0.0f, 0.5f);
            duplicate.name = original.name + "_Copy";

            selection.selectedObjects[i] = duplicate;

            //string buff = parser.ParseHierarchy(); // Still unsure if this needs to be called each loop
        }
        string buff = parser.ParseHierarchy();
    }


    public void DeleteObject(string objname)
    {
        Debug.Log("Selected objects count: " + selection.selectedObjects.Count);
        foreach(GameObject obj in selection.selectedObjects) {
            Destroy(obj);
            string buff = parser.ParseHierarchy();

        }
    }

}