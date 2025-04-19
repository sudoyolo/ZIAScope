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

    private const string API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
    private const string API_KEY = "AIzaSyCmmPpM_7x6em730kAqSWdIExZOggZLx2I";
    private string scene_desc;
    private Queue<string> past_queries = new Queue<string>();
    private int maxSize = 10;

    void Start()
    {
        parser = FindObjectOfType<SceneHierarchyParser>();
        scene_desc = parser.result;
    }

    public void GenerateAICommentary(string arg1)
    {
        Push(arg1);
        string prompt = $"Given this prompt from a user: {arg1}, check if the user wants to do select an object in the scene. If not, reply \'Nothing\'.";
        prompt += "If yes, pick the object closest to what they ask for, and return its index and only its index, nothing else. ";
        prompt += "If multiple objects are selected, return a space separated list of only integers. ";
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
                aiCommentaryText.text = "Selected Object " + formatted;
                selection.SelectObject(formatted);
            }
            else
            {
                Debug.LogError("API Error: " + request.error);
                aiCommentaryText.text = "Error fetching AI commentary.";
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
