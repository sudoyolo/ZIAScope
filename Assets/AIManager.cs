using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using System.Text;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;



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
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor leftRayInteractor;
    [SerializeField] private UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rightRayInteractor;
    public SceneHierarchyParser parser;
    public Selection selection;
    public Manipulation manipulation;
    public Transform playerpos;
    //public ScrollingStringList scrollingList;

    public Samples.Whisper.Whisper whisper;
    private const string API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
    private const string API_KEY = "AIzaSyAfwHYTIMHPY4SWAqwRu25x0YIRgt_kiTU";
    private string scene_desc;
    private string prefabs_list;
    public Queue<string> past_queries = new Queue<string>();
    private int maxSize = 5;

    void Start()
    {
        parser = FindObjectOfType<SceneHierarchyParser>();
        scene_desc = parser.result;
    }
    
    void Update()
    {
        CheckRayHit(leftRayInteractor, "Left");
        CheckRayHit(rightRayInteractor, "Right");
    }
    
    void CheckRayHit(UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rayInteractor, string handLabel)
    {
        if (rayInteractor != null && rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
        {
            //Debug.Log($"{handLabel} controller hit: {hit.collider.gameObject.name}");
        }
        else
        {
            //Debug.Log($"Hitting nothing right now");
        }
        
    }

    public void GenerateAICommentary(string arg1)
    {
        Push(arg1);

        scene_desc = parser.ParseHierarchy();
        prefabs_list = manipulation.GetPrefabList();
        string prompt;
        int currentIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
        if (currentIndex == 2)
        {
            prompt = getNavigationString(arg1);
        } else {
            prompt = getManipulationString(arg1);
        }
        
        StartCoroutine(SendRequestToGemini(prompt));
    }

    private string getManipulationString(string arg1) 
    {
        string prompt = $"A user gives you this prompt: {arg1}, check if the user wants to do any of the following functions: "; 
        prompt += "Reply with the relevant function index and its necessary arguments as a comma separated list. Do not include new lines anywhere. The functions are as follows:\n";
        
        // SELECTION
        prompt += "[0] Selection: User wants to select something. Pick the object that matches what they ask for, return its index and only its index. If no clear match exists, prompt for more clarity, prompts should start with a ? and be quotation encapsulated. When users say \'Everything\' they mean every object. ";
        prompt += "If multiple objects are selected, return a space separated list of only integers. When users say \'this object\' they typically mean the object closest to them, while \'that over there\' means further away; take this into account.\n";
        // ALL MANIPULATION FUNCTIONS
        prompt += "[1] Change position: User wants to move an object. Pick a direction closest to: forward/backward/up/down/left/right. Return the direction they ask for as a string. If they ask for a numerical distance return that too, otherwise return 1. Example: \'1 forward 3\'\n";
        prompt += "[2] Set position: User wants to set the position of an object. If they wish to move an object to another existing object, return that target object's coordinate position with three numbers ; separated, e.g. \'Move the cushion to the couch\' should select the cushions first, then return in format \'2 5;4;2\'\n";        
        prompt += "[3] Rotate object: User wants to rotate an object about the y axis. Return the number they specify, clockwise being positive and anti-clockwise being negative.\n";
        prompt += "[4] Change color: User wants to change color of object (eg red, green, cyan, note that this is different to texture). Return 4 followed by a set of rgb values matching the color they say, eg. \'cyan\' returns \'75;201;197\'. Do not return name of color. Return 4 followed by the semicolon separated rgb values.\n";
        prompt += "[5] Change material: User wants to change the physical material of the object. Return one of the following options closest to what the user says: wood, steel, bronze, floorboards, fabric, ceramic, untextured, paintedwall. If none are close then return \'3 null\'\n";
        prompt += "[6] Assign tag: User wants to apply a tag to an object. Return the name of the tag as a string.\n";
        prompt += "[7] Create duplicate: user wants to create a new duplicate object. Return \'empty\' in place of the args, eg. \'7 empty\'. This has to be an object that exists in the scene. If not, look through prefabs and call 14 creation instead.\n";
        prompt += "[8] Delete object: user wants to delete a selected object. Return \'empty\' in place of the args, eg. \'8 empty\'.\n";
        // WAYFINDING
        prompt += "[9] Show path between two locations: If user themselves is one of the location objects, only include one argument. If only one object is specified in the query, it is implied that the user is the other object. e.g. the query \'route to fridge\' returns \'9 12\'. If user is not one of the location objects, return object indices of the relevant objects. \'9 23 62\'\n";
        prompt += "[10] Clear single path: user wants to remove a single path. Return only one argument if the path is between the user and an object. e.g. \'10 63\'. Return two arguments if the path is between two objects that don't include the user \'10 23 64\'\n";
        prompt += "[11] Clear all existing paths: user wants to remove and stop showing all previous paths. Simply return \'11 clear paths\'\n";
        prompt += "[12] Move/take the user along the path to an object. Return the destination object index as an argument. Return \'12 35\'";
        prompt += "[13] Teleport to Object: Teleport the user to the location of an object. Pass the index of the object as the argument. E.g. \'13 25\' ";
        // OBJECT CREATION
        prompt += "[14] Create/add/generate an object: Look through the list of available objects in this list, if the user wants to generate a new object in this list, return just the function index and object's index, eg \'14 4\'. If no object match exists, respond with only \"?\"Sorry, the object you requested is not available.\"\"\n";
        prompt += "If the user wants to generate multiple objects, eg \"create four new walls\" then respond with \"14 idx, 14 idx, 14 idx, 14 idx, ?\"Made four new walls\"\" replacing idx with actual index number. List of available objects:\n";
        prompt += prefabs_list;
        // UNDO REDO
        prompt += "[15] Undo: revert previous action, simply return 15 undo, ?\"Undoing previous action.\"\n";
        prompt += "[16] Redo: redo previous action, simply return 16 redo, ?\"Redoing previous action.\"\n";
        // LIGHTING
        prompt += "[17] Change time of day: user wants to change scene to a certain time of day. Choose closest between these options and return the index: [0] daytime, [1] nighttime, [2] sunset/dusk. For example you could return 17 1, ?\"Changing scene to nighttime.\"\n";
        prompt += "[18] Change scene lighting color: user wants to change the color of the overall scene lighting. Return 18 followed by an rgb value based on color they say, semicolon separated. Eg if the user asks \'Change the lighting to be marigold\', return 18 242;179;41, ?\"Changing the scene lighting to marigold.\"\n";
        prompt += "[19] Toggle on/off the lights: user wants to turn off or on the lamp lights. Note that this is different to the overall scene lighting. Return simply 19 lights, ?\"Toggling the lights.\"\n";
        // FEEDBACK
        prompt += "[20] User wants some qualitative feedback on how their scene looks, such as asking \'Does the color scheme look okay?\' or \'Does this fit a children's theme\'. You will be invoking a computer vision model for more feedback. Return a string in the format of 20, followed by the user's prompt, followed by simply ?\"Generating feedback: \" . For example: 20 does color scheme look good, ?\"Generating feedback: \"\n";
        // RETURN TO HOME
        prompt += "[21] User wants to return to this app's home page (also known as landing page or starting page), simply return 21 empty, ?\"Returning home...\"\n";
        // TWEAKS
        prompt += "For all functions except for Selection, if there is an implicit choice of object, eg. \'make chairs red\' then selection should be called as well as further object manipulation. Often you will have to check past prompt to ensure you are selecting the correct object.\n";
        prompt += "Example, where chair is scene idx 1: \'Select the chair, change its color to red, move it back\' should return string \'0 1, 4 red, 1 backward 1, ?\"Made the chairs red and moved them backwards by one.\"\' \n";
        prompt += "Another example, where entire scene is couches with idx 0 1: \'Move everything forward and tag them as new\' should return string \'0 0 1, 1 forward 1, 6 new,?\"Every object moved foward and tagged as new\"\' \n";
        prompt += "Another example, if the previous prompt was \'Make the couch red\' and the current prompt is \'Can you make it a bit darker\', you should alter the already selected couch with 4 156;38;30, ?\"Setting a slightly darker red\"\n";
        prompt += "Given more complex prompts, you can be creative, for example a prompt \'Arrange the walls into a room\' if four walls (indexes 0 1 2 3) exist all at 0,0,0 position and rotation, you could call 0 0, 1 5;0;5, 0 1, 1 -5;0;5, 0 2, 1 5;0;-5, 0 3, 1 -5;0;-5, 0 0, 3 90, 0 2, 3 90 ?\"Creating a room structure\" which makes four walls arrayed around each other.\n";
        // NAVIGATION TWEAKS
        prompt += "For functions involving paths and teleportation, call selection on relevant objects prior to calling the relevant Path command. Paths involving the user should have one selected object. Paths not including the user should have two selected objects. \n";
        prompt += "An example, the fridge idx is 63: \'Take me to the fridge\' should return \'0 63, 9 63\'\n";
        prompt += "Another example, the fridge idx is 63 and the couch idx is 25: \'Show me the shortest route between the couch and the fridge\' should return \'0 63 25, 9 63 25\'\n";
        prompt += "A query such as \'Show path to fridge\' or \'Delete path to fridge\' assumes the user as one of the location objects for the path and thus only returns one argument.\n";
        prompt += "If the user query relates to showing or illustrating a path, choose function 9 as the target function to return. If a user query relates to moving or travelling along a path, choose function 12 as the target function to return. Call selection prior to both of these functions, e.g. \'0 25, 9 25\' or \'0 36, 12 36\' where 25 and 36 are the destination object indices. \n";
        prompt += "Similarly, for deleting a single path, if the query includes the user as an object for the path, only include one argument in the selection. e.g. \'Delete the route to the couch\' when the couch idx is 25 should be \'0 25, 10 25\'\n";
        prompt += "If the route doesn't include the user as an object for the path, there should be two arguments for each of the corresponding objects in the selection. e.g. \'Delete the route between the couch and the fridge\', then if the couch index is 63 and the fridge index is 25, return \'0 63 25, 10 63 25\'\n";
        prompt += "For teleportation, selection should also be called on the destination object prior to the teleportation function call, e.g. \'0 25, 13 25\' where 25 is the destination object. ";
        prompt += "If describing an object for user routing using other objects. Only select the destination object that's relevant. For example \'Move me along the path to the plant near the lamp\', should return \'0 35, 12 35\' where 35 is the index of the plant";
        // FEEDBACK
        prompt += "For every command, make sure to also provide short feedback note explaining what you understood, in the format of a ? followed by text enclosed in quotation marks.\n";
        prompt += "For example, an entire command string could be 0 5, 4 0;0;0, ?\"Changing chair to black\"\n";
        // FURTHER PROMPTING
        prompt += "If the user's input is unclear, \'?\' followed by a message can also be used to ask for more confirmation. This message should be encapsulated in quotation marks, e.g. ? \"Please repeat what you said.\"\n";
        prompt += "For example, if they ask to select something that is not in the scene, return something like \'? \"Sorry, I couldn't understand which object you meant, try again?\"\'";
        prompt += "Another example, if it is unclear which specific object is being referred to, such as \'Select that chair\', you can ask \'Which chair? The one closer to the door or to yourself?\'\n";
        prompt += "Do not be afraid to prompt for clarity, it is better to ask and be sure than to do something the user does not want. Especially for spatial queries. Avoid using numbers, especially object indexes. Do not ask for rgb values.\n";
        // ADDITIONAL INFO
        prompt += $"The user's position is {playerpos.position}. ";
        prompt += $"The entire game scene is described here {scene_desc}. ";
        prompt += $"These are the past 5 prompts the user has used, take of previous ones building on top of current prompt if relevant, prioritize recent queries {ConcatenateQueue(past_queries)}. ";
        prompt += "For example, if the previous query was \'Change the couches to red\' and the current query is \'Also do that to the table\' then understand these commands together, and change the table to red.";
        return prompt;
    }

    private string getNavigationString(string arg1)
    {
        string prompt = $"A user gives you this prompt: {arg1}, check if the user wants to do any of the following functions: "; 
        prompt += "Reply with the relevant function index and its necessary arguments as a comma separated list. Do not include new lines anywhere.\n";
        prompt += "Some of the functions are unavailable to correctly index the available functions. Do not choose these unavailable numbers. Choose the numbers that do correspond to available functions";
        prompt += "The functions are as follows:\n";
        
        // SELECTION
        prompt += "[0] Selection: User wants to select something. Pick the object that matches what they ask for, return its index and only its index. If no clear match exists, prompt for more clarity, prompts should start with a ? and be quotation encapsulated. When users say \'Everything\' they mean every object. ";

        // ALL MANIPULATION FUNCTIONS DISABLED
        prompt += "[1] Change position: NOT AVAILABLE\n";
        prompt += "[2] Set position: NOT AVAILABLE\n";        
        prompt += "[3] Rotate object: NOT AVAILABLE\n";
        prompt += "[4] Change color: NOT AVAILABLE\n";
        prompt += "[5] Change material: NOT AVAILABLE\n";
        prompt += "[6] Assign tag: NOT AVAILABLE\n";
        prompt += "[7] Create duplicate: NOT AVAILABLE\n";
        prompt += "[8] Delete object: NOT AVAILABLE\n";
        // WAYFINDING
        prompt += "[9] Show path between two locations: If user is one of the location objects, only include one argument. If only one object is specified in the query, then it is implied that the user is the other objects. e.g. the query \'route to fridge\' returns \'9 12\'. If user is not one of the location objects, return object indices of the relevant objects. \'9 23 62\'\n";
        prompt += "[10] Clear single path: user wants to remove a single path. Return only one argument if the path is between the user and an object. e.g. \'10 63\'. Return two arguments if the path is between two objects that don't include the user \'10 23 64\'\n";
        prompt += "[11] Clear all existing paths: user wants to remove and stop showing all previous paths. Simply return \'11 clear paths\'\n";
        prompt += "[12] Move/take the user along the path to an object. Return the destination object index as an argument. Return \'12 35\'";
        prompt += "[13] Teleport to Object: Teleport the user to the location of an object. Pass the index of the object as the argument. E.g. \'13 25\' ";
        // OBJECT CREATION
        prompt += "[14] Create object: NOT AVAILABLE\n";
        // UNDO REDO
        prompt += "[15] Undo: NOT AVAILABLE\n";
        prompt += "[16] Redo: NOT AVAILABLE\n";
        // LIGHTING
        prompt += "[17] Change time of day: NOT AVAILABLE\n";
        prompt += "[18] Change scene lighting color: NOT AVAILABLE\n";
        prompt += "[19] Toggle on/off the lights: NOT AVAILABLE\n";
        // FEEDBACK
        prompt += "[20] Computer Vision Feedback: NOT AVAILABLE\n";
        // RETURN TO HOME
        prompt += "[21] User wants to return to this app's home page (also known as landing page or starting page), simply return 21 empty, ?\"Returning home...\"\n";
        // FEEDBACK TWEAKS TO BLOCK MANIPULATION
        prompt += "If the function does not match an available one, respond with ?\"The function you requested is not available.\" Do not respond affirming functions are called when they are not.\n";
        // WAYFINDING TWEAKS
        prompt += "For functions involving paths and teleportation, call selection on relevant objects prior to calling the relevant Path command. Paths involving the user should have one selected object. Paths not including the user should have two selected objects. \n";
        prompt += "An example, the fridge idx is 63: \'Take me to the fridge\' should return \'0 63, 9 63\'\n";
        prompt += "Another example, the fridge idx is 63 and the couch idx is 25: \'Show me the shortest route between the couch and the fridge\' should return \'0 63 25, 9 63 25\'\n";
        prompt += "A query such as \'Show path to fridge\' or \'Delete path to fridge\' assumes the user as one of the location objects for the path and thus only returns one argument.\n";
        prompt += "If the user query relates to showing or illustrating a path, choose function 9 as the target function to return. If a user query relates to moving or travelling along a path, choose function 12 as the target function to return. Call selection prior to both of these functions, e.g. \'0 25, 9 25\' or \'0 36, 12 36\' where 25 and 36 are the destination object indices. \n";
        prompt += "Similarly, for deleting a single path, if the query includes the user as an object for the path, only include one argument in the selection. e.g. \'Delete the route to the couch\' when the couch idx is 25 should be \'0 25, 10 25\'\n";
        prompt += "If the route doesn't include the user as an object for the path, there should be two arguments for each of the corresponding objects in the selection. e.g. \'Delete the route between the couch and the fridge\', then if the couch index is 63 and the fridge index is 25, return \'0 63 25, 10 63 25\'\n";
        prompt += "For teleportation, selection should also be called on the destination object prior to the teleportation function call, e.g. \'0 25, 13 25\' where 25 is the destination object. ";
        prompt += "If describing an object for user routing using other objects. Only select the destination object that's relevant. For example \'Move me along the path to the plant near the lamp\', should return \'0 35, 12 35\' where 35 is the index of the plant";
        // FEEDBACK
        prompt += "For every command, make sure to also provide short feedback note explaining what you understood, in the format of a ? followed by text enclosed in quotation marks.\n";
        prompt += "For example, an entire command string could be 0 23 64, 10 23 64, ?\"Removing the path between the exit and the bathroom.\"\n";
        prompt += "Sometimes a user is just asking for more context, without necessarily wanting to call a function. If this occurs reply naturally with a ? followed by enclosed quotations. For example, the user could ask \"is there a cafe here\" and you can reply something like \"Yes, would you like me to show you where it is?\" if it indeed does exist in the scene.\n";
        // FURTHER PROMPTING
        prompt += "If the user's input is unclear, \'?\' followed by a message can also be used to ask for more confirmation. This message should be encapsulated in quotation marks, e.g. ? \"Please repeat what you said.\"\n";
        prompt += "For example, if they ask to select something that is not in the scene, return something like \'? \"Sorry, I couldn't understand which object you meant, try again?\"\'";
        prompt += "Another example, if it is unclear which specific object is being referred to, such as \'Select that chair\', you can ask ?\"Which chair? The one closer to the door or to yourself?\"\n";
        prompt += "Do not be afraid to prompt for clarity, it is better to ask and be sure than to do something the user does not want. Especially for spatial queries. Avoid using numbers, especially object indexes. Do not ask for rgb values.\n";
        // ADDITIONAL INFO
        prompt += $"The user's position is {playerpos.position}. ";
        prompt += $"The entire game scene is described here {scene_desc}. ";
        prompt += $"These are the past 5 prompts the user has used, take of previous ones building on top of current prompt if relevant, prioritize recent queries {ConcatenateQueue(past_queries)}. ";
        return prompt;
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
                /*if(formatted[0]=='?')
                {
                    string parsed = formatted.Substring(2);
                    aiCommentaryText.text = parsed;
                    scrollingList.AddString(parsed, "lightblue");
                }
                else
                {*/
                    manipulation.parseFunctions(formatted);
                //}
                                
            }
            else
            {
                Debug.LogError("API Error: " + request.error);
                //aiCommentaryText.text = "Error fetching AI commentary.";
            }
            whisper.requestCompleted();
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
