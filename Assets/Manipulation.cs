using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;



public class Manipulation : MonoBehaviour
{
    List<Action<string>> functionList;
    [SerializeField] private List<GameObject> prefabList;
    public Selection selection;
    public List<Material> materialList = new List<Material>();
    public Wayfinding wayfinding;
    public SceneHierarchyParser parser;
    public ScrollingStringList scrollingList;
    public UndoRedoManager undoredo;
    public AIManager aiManager;
    public LightingManager lightingManager;
    //public SceneLoader sceneManager;
    public ComputerVision computerVision;
    private int lastCreatedIdx = -1;
    private int numCreated = 0;
    

    // Start is called before the first frame update
    void Start()
    {
        //sceneManager = FindObjectOfType<SceneLoader>();

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
            CreateObject,                                // 14
            undoredo.Undo,                               // 15
            undoredo.Redo,                               // 16
            lightingManager.SetEnvironment,              // 17
            lightingManager.SetLightColorFromRGB,        // 18
            lightingManager.ToggleAllLampChildren,       // 19
            computerVision.SubmitPrompt,                 // 20
            LoadHome                                     // 21  //sceneManager.LoadHome                        
        };

    }

    void Update()
    {
        
    }

    public string GetPrefabList()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int i = 0; i < prefabList.Count; i++)
        {
            string name = prefabList[i] != null ? prefabList[i].name : "[Missing]";
            sb.AppendLine($"{i}: {name}");
        }

        return sb.ToString();
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
            while (i < command.Length && command[i] == ' ') { i++; }
            if (i >= command.Length)
                break;

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

            if(funcIndex<=8 && funcIndex > 0)
            {
                undoredo.LogState();
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
                }
                if(funcIndex!=14){numCreated = 0;}
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
            obj.transform.position = new Vector3(x, y+1.0f, z);
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
        
        foreach (GameObject obj in selection.selectedObjects) {
            string[] parts = rgbString.Split(';');
            if (parts.Length != 3) {
                Debug.LogError("RGB string must have exactly 3 components separated by semicolons.");
                continue;
            }

            if (float.TryParse(parts[0], out float r) &&
                float.TryParse(parts[1], out float g) &&
                float.TryParse(parts[2], out float b))
            {
                // If values appear to be in 0–255 range, normalize
                if (r > 1f || g > 1f || b > 1f) {
                    r /= 255f;
                    g /= 255f;
                    b /= 255f;
                }

                Color color = new Color(r, g, b);

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
                Debug.LogError("Failed to parse RGB values as floats.");
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
            duplicate.transform.position = new Vector3(aiManager.playerpos.position.x + 0.25f, selection.selectedObjects[i].transform.position.y, aiManager.playerpos.position.z + 0.25f);
            //duplicate.transform.position = original.transform.position + new Vector3(0.5f, 0.0f, 0.5f);
            duplicate.name = original.name + "_Copy";

            selection.selectedObjects[i] = duplicate;

            //string buff = parser.ParseHierarchy(); // Still unsure if this needs to be called each loop
        }
        string buff = parser.ParseHierarchy();
    }


    public void DeleteObject(string objname)
    {
        foreach(GameObject obj in selection.selectedObjects) {
            Destroy(obj);
        }
        string buff = parser.ParseHierarchy();
    }

    public void CreateObject(string indexString)
    {
        if (!int.TryParse(indexString, out int index))
        {
            Debug.LogError($"Invalid index string: '{indexString}'");
            return;
        }

        if (index < 0 || index >= prefabList.Count)
        {
            Debug.LogError($"Index {index} out of range (0 to {prefabList.Count - 1})");
            return;
        }

        GameObject prefab = prefabList[index];
        if (prefab == null)
        {
            Debug.LogError($"Prefab at index {index} is null.");
            return;
        }
        float originalY = prefab.transform.position.y;
        Vector3 newPosition = new Vector3(aiManager.playerpos.position.x + 0.25f, originalY, aiManager.playerpos.position.z + 0.25f);
        Quaternion newRotation = Quaternion.Euler(-90f, 0f, 0f);
        GameObject instance = Instantiate(prefab, newPosition, newRotation);
        //GameObject instance = Instantiate(prefab, newPosition, prefab.transform.rotation);
        /*Quaternion currentRotation = instance.transform.rotation;
        Quaternion additionalRotation = Quaternion.Euler(-90f, 0f, 0f);
        instance.transform.rotation = currentRotation * additionalRotation;*/
        instance.transform.localScale = new Vector3(
            instance.transform.localScale.x * 0.7f,
            instance.transform.localScale.y * 0.7f,
            instance.transform.localScale.z * 0.7f
        );

        if(lastCreatedIdx==index){
            numCreated ++;
            //Vector3 euler = instance.transform.eulerAngles;
            //euler.y -= 90f*numCreated;  // or euler.y += 270f for same effect
            //instance.transform.eulerAngles = euler;
            instance.transform.position += new Vector3(0.25f * numCreated, 0, 0.25f*numCreated);
        }else{numCreated = 0;}
        lastCreatedIdx = index;

        instance.name = prefab.name + "_Clone";
        instance.transform.parent = parser.sceneObjects.transform;
    }
    public void LoadHome(string buff)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }


}