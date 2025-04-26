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
    [SerializeField] private TextMeshProUGUI aiCommentaryText;
    public SceneHierarchyParser parser;
    public Selection selection;
    public Manipulation manipulation;
    public Transform playerpos;

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

        scene_desc = parser.ParseHierarchy();
        string prompt = $"A user gives you this prompt: {arg1}, check if the user wants to do any of the following functions: "; 
        prompt += "Reply with the relevant function index and its necessary arguments as a comma separated list. Do not include quotation marks or new lines anywhere. The functions are as follows:\n";
        
        // SELECTION
        prompt += "[0] Selection: User wants to select something. Pick the object that matches what they ask for, return its index and only its index. If no clear match exists, prompt for more clarity, prompts should start with a ?. ";
        prompt += "If multiple objects are selected, return a space separated list of only integers. When users say \'this object\' they typically mean an object close to them, while \'that over there\' means a further distance; take this into account.\n";
        // ALL MANIPULATION FUNCTIONS
        prompt += "[1] Change position: User wants to move an object. Pick a direction closest to: forward/backward/up/down/left/right. Return the direction they ask for as a string. If they ask for a numerical distance return that too, otherwise return 1. Example: \'1 forward 3\'\n";
        prompt += "[2] Set position: User wants to move one object towards another. Return the target object's coordinate position with three numbers ; separated, e.g. \'Move the cushion to the couch\' should select the cushions first, then return in format \'2 5;4;2\' \n";        
        prompt += "[3] Rotate object: User wants to rotate an object about the y axis. Return the number they specify, clockwise being positive and anti-clockwise being negative.\n";
        prompt += "[4] Change color: User wants to change color of object (eg red, green, cyan, note that this is different to texture). Return a set of rgb values that matches the color they say, eg. \'cyan\' returns \'75;201;197\'. Do not return name of color. Return the rgb values.\n";
        prompt += "[5] Change material: User wants to change the physical material of the object. Return one of the following options closest to what the user says: wood, steel, bronze, floorboards, fabric, ceramic, untextured, paintedwall. If none are close then return \'3 null\'\n";
        prompt += "[6] Assign tag: User wants to apply a tag to an object. Return the name of the tag as a string.\n";
        prompt += "[7] Create duplicate: user wants to create a new duplicate object. Return \'empty\' in place of the args, eg. \'7 empty\'.\n";
        prompt += "[8] Delete object: user wants to delete a selected object. Return \'empty\' in place of the args, eg. \'8 empty\'.\n";
        // WAYFINDING
        prompt += "[9] Show path from user: user wants to find a path between themselves and a location object. Return object index e.g. \'8 5\'\n";
        prompt += "[10] Show path between two objects: user wants to illuminate a path to two location objects e.g. \'9 7 12\'";
        prompt += "[11] Clear all existing paths: user wants to remove and stop showing all previous paths. Simply return \'10 clear paths\'";
        // TWEAKS
        prompt += "For all functions except for Selection, if there is an implicit choice of object, eg. \'make chairs red\' then selection should be called before color change.\n";
        prompt += "Example, where chair is scene idx 1: \'Select the chair, change its color to red, move it back\' should return string \'0 1, 4 red, 1 backward 1\' \n";
        prompt += "Another example, where two spheres are idx 0 1: \'Move the spheres forward and tag them as new\' should return string \'0 0 1, 1 forward, 6 new\' \n";
        prompt += "For the function involving paths, call selection on relevant objects prior to calling the Add Path command. \n";
        prompt += "An example, the fridge idx is 63: \'Take me to the fridge\' should return \'0 63, 8 63\'\n";
        prompt += "Another example, the fridge idx is 63 and the couch idx is 25: \'Show me the shortest route between the couch and the fridge\' should return \'0 63 25, 9 63 25\'\n";
        // BACK-AND-FORTH
        prompt += "If the user's input is unclear, return ? followed by a message asking for more confirmation.\n";
        prompt += "For example, if they ask to select something that is not in the scene, return something like \'? Sorry, I couldn't understand which object you meant, try again?\'";
        prompt += "Another example, if it is unclear which specific object is being referred to, such as \'Select that chair\', you can ask \'Which chair? The one closer to the door or to yourself?\'\n";
        prompt += "Do not be afraid to prompt for clarity, it is better to be sure than to do something the user does not want. Especially for spatial queries. Avoid using numbers, especially object indexes.\n";
        // ADDITIONAL INFO
        prompt += $"The user's position is {playerpos.position}. ";
        prompt += $"The entire game scene is described here {scene_desc}. ";
        prompt += $"These are the past 10 prompts the user has used, take of previous ones building on top of current prompt if relevant, prioritize recent queries {ConcatenateQueue(past_queries)}. ";
        prompt += "For example, if the previous query was \'Change the couches to red\' and the current query is \'Also do that to the table\' then understand these commands together.";
        
        
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
                if(formatted[0]=='?')
                {
                    string parsed = formatted.Substring(2);
                    aiCommentaryText.text = parsed;
                }
                else
                {
                    manipulation.parseFunctions(formatted);
                }
                                
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
