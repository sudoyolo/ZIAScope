using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using System.Text;
using System;
using System.Collections.Generic;

[System.Serializable]
public class GeminiPart
{
    public string text;
}

[System.Serializable]
public class GeminiContent
{
    public GeminiPart[] parts;
}

[System.Serializable]
public class GeminiCandidate
{
    public GeminiContent content;
}

[System.Serializable]
public class GeminiResponse
{
    public GeminiCandidate[] candidates;
}

public class AIManager : MonoBehaviour
{
    //[SerializeField] private TextMeshProUGUI aiCommentaryText;
    public SceneHierarchyParser parser;
    public Selection selection;
    public Manipulation manipulation;

    private const string API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
    private const string API_KEY = "AIzaSyCmmPpM_7x6em730kAqSWdIExZOggZLx2I";
    private string scene_desc;
    public Queue<string> past_queries = new Queue<string>();
    private int maxSize = 10;

    void Start()
    {
        parser = FindObjectOfType<SceneHierarchyParser>();
        scene_desc = parser.result;
    }

    public void GenerateAICommentary(string arg1)
    {
        Push(arg1);
        /*string prompt = $"Given this prompt from a user: {arg1}, check if the user wants to do select an object in the scene. If not, reply \'Nothing\'.";
        prompt += "If yes, pick the object closest to what they ask for, and return its index and only its index, nothing else. ";
        prompt += "If multiple objects are selected, return a space separated list of only integers. ";
        prompt += $"The entire game scene is described here {scene_desc}. ";
        prompt += $"These are the past 10 prompts the user has used, take account of them only if relevant, prioritize recent queries {ConcatenateQueue(past_queries)}. ";
        */

        scene_desc = parser.ParseHierarchy();
        string prompt = $"A user gives you this prompt: {arg1}, check if the user wants to do any of the following functions: "; 
        prompt += "Reply with the relevant function index and its necessary arguments as a comma separated list. The functions are as follows:\n";
        
        // SELECTION
        prompt += "[0] Selection: User wants to select something. Pick the object closest matching what they ask for, return its index and only its index. ";
        prompt += "If multiple objects are selected, return a space separated list of only integers. When users say \'this object\' they typically mean an object close to them, while \'that over there\' means a further distance; take this into account.\n";
        // ALL MANIPULATION FUNCTIONS
        prompt += "[1] Change position: User wants to move a previously selected object. Pick a direction closest to: forward/backward/up/down/left/right. Return the direction they ask for as a string. If they ask for a numerical distance return that too, otherwise return 1. Example: \'1 forward 3\'\n";
        prompt += "[2] Set position: User wants to move one object towards another. Return the target object's coordinate position with three numbers ; separated, e.g. \'Move the cushion to the couch\' should select the cushions first, then return in format \'2 5;4;2\' \n";        
        prompt += "[3] Rotate object: User wants to rotate an object about the y axis. Return the number they specify, clockwise being positive and anti-clockwise being negative.\n";
        prompt += "[4] Change color: User wants to change color of object (eg red, green, cyan, note that this is different to texture). Return a set of rgb values that matches the color they say, eg. \'cyan\' returns \'75;201;197\'. Do not return name of color only the rgb values.\n";
        prompt += "[5] Change material: User wants to change the physical material of the object. Return one of the following options closest to what the user says: wood, steel, bronze, floorboards, fabric, ceramic, untextured, paintedwall. If none are close then return \'3 null\'\n";
        prompt += "[6] Assign tag: User wants to apply a tag to a previously selected object. Return the name of the tag as a string.\n";
        prompt += "[7] Create duplicate: user wants to create a new duplicate object of previously selected objects. Return \'empty\' in place of the args, eg. \'5 empty\'.\n";
        prompt += "[8] Add Path from user: user wants to illuminate a path to a location object. e.g. \'8 5\'\n";
        prompt += "[9] Add Path between two objects: user wants to illuminate a path to two location objects e.g. \'9 7 12\'";
        // TWEAKS
        prompt += "For all functions except for Selection, if there is an implicit choice of object, eg. \'make chairs red\' then selection should be called before color change.\n";
        prompt += "Example, where chair is scene idx 1: \'Select the chair, change its color to red, move it back\' should return string \'0 1, 4 red, 1 backward 1\' \n";
        prompt += "Another example, where two spheres are idx 0 1: \'Move the spheres forward and tag them as new\' should return string \'0 0 1, 1 forward, 6 new\' \n";
        prompt +=
            "For the function involving paths, call selection on relevant objects prior to calling the Add Path command. \n";
        prompt +=
            "An example of this is where the fridge idx is 63: \'Take me to the fridge\' should return \'0 63, 8 63\'\n";
        prompt +=
            "Another example of this is where the fridge idx is 63 and the couch idx is 25: \'Show me the shortest route between the couch and the fridge\' should return \'0 63 25, 8 63 25\'\n";
        prompt += "Reply \'Nothing\' if no functions apply.";
        prompt += $"The entire game scene is described here {scene_desc}. ";
        prompt += $"These are the past 10 prompts the user has used, take account of them only if relevant, prioritize recent queries {ConcatenateQueue(past_queries)}. ";
        
        
        StartCoroutine(SendRequestToGemini(prompt));
    }

    private IEnumerator SendRequestToGemini(string prompt)
    {
        // Manually construct the JSON body
        string jsonData = "{\"contents\":[{\"parts\":[{\"text\":\"" + EscapeJsonString(prompt) + "\"}]}]}";

        using (UnityWebRequest request = new UnityWebRequest($"{API_URL}?key={API_KEY}", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                string formatted = ExtractCommentaryFromResponse(response);
                Debug.Log("Gemini API Response: " + formatted);
                //aiCommentaryText.text = "Gemini Response: " + formatted;
                //selection.SelectObject(formatted);
                manipulation.parseFunctions(formatted);
            }
            else
            {
                Debug.LogError("API Error: " + request.error);
                //aiCommentaryText.text = "Error fetching AI commentary.";
            }
        }
    }

    private string ExtractCommentaryFromResponse(string response)
    {
        try
        {
            GeminiResponse parsed = JsonUtility.FromJson<GeminiResponse>(response);
            if (parsed.candidates != null && parsed.candidates.Length > 0 &&
                parsed.candidates[0].content != null &&
                parsed.candidates[0].content.parts != null &&
                parsed.candidates[0].content.parts.Length > 0)
            {
                return parsed.candidates[0].content.parts[0].text;
            }
            return "No valid content received.";
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing AI response: " + e.Message);
            return "Error retrieving AI commentary.";
        }
    }

    private string EscapeJsonString(string str)
    {
        return str.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    public void Push(string item)
    {
        if (past_queries.Count >= maxSize)
        {
            past_queries.Dequeue(); // remove oldest item
        }
        past_queries.Enqueue(item);
    }

    public string Pop()
    {
        if (past_queries.Count == 0)
        {
            throw new InvalidOperationException("Queue is empty");
        }
        return past_queries.Dequeue();
    }

    public static string ConcatenateQueue(Queue<string> queue)
    {
        if (queue == null || queue.Count == 0)
            return string.Empty;

        StringBuilder sb = new StringBuilder();

        foreach (string item in queue)
        {
            sb.Append(item);
        }

        return sb.ToString();
    }
}
