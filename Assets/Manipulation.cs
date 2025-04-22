using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


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
            AssignTag
        };
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void parseFunctions(string command)
    {
        if(command=="Nothing"){return;}
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

    public void ChangePosition(string arg)
    {
        foreach(GameObject obj in selection.selectedObjects) {
            if(arg.Contains("forward"))
            {
                obj.transform.position += new Vector3(0f, 0f, -1f);
            }
            else if(arg.Contains("backward"))
            {
                obj.transform.position += new Vector3(0f, 0f, 1f);
            }
            else if(arg.Contains("up"))
            {
                obj.transform.position += new Vector3(0f, 1f, 0f);
            }
            else if(arg.Contains("down"))
            {
                obj.transform.position += new Vector3(0f, -1f, 0f);
            }
            else if(arg.Contains("left"))
            {
                obj.transform.position += new Vector3(-1f, 0f, 0f);
            }
            else if(arg.Contains("right"))
            {
                obj.transform.position += new Vector3(1f, 0f, 0f);
            }
        }
    }

    public void ChangeColor(string arg) 
    {
        Debug.Log("change color!");
        foreach(GameObject obj in selection.selectedObjects) {
            Renderer rend = obj.GetComponent<Renderer>();
            if (rend == null) continue;
            if(arg.Contains("red"))
            {
                rend.material = materialList[0];
            }
            else if(arg.Contains("grey"))
            {
                rend.material = materialList[1];
            }
            else if(arg.Contains("blue"))
            {
                rend.material = materialList[2];
            }
            else if(arg.Contains("white"))
            {
                rend.material = materialList[3];
            } 
        } 
    }

    public void AssignTag(string arg)
    {
        Debug.Log("instantiate new child with tag");
    }
}
