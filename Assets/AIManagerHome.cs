using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using System.Text;
using System;
using System.Collections.Generic;


public class AIManagerHome : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI aiCommentaryText;
    [SerializeField] private SceneManager sceneManager;
    private const string API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
    private const string API_KEY = "AIzaSyAfwHYTIMHPY4SWAqwRu25x0YIRgt_kiTU";
    private string scene_desc;
    private int maxSize = 5;

    void Start()
    {

    }

    public void GenerateAICommentary(string arg1)
    {
        string prompt = "The user wants to selct one scene out of three available scenes. These are: \n";
        prompt += "[1] Default Interior Design: Living Room.\n[2] Default Navigation: Shopping Mall.\n[3]Interior Design: Empty Room.";
        prompt += "This is the user's prompt: ";
        prompt += arg1;
        prompt += "Choose the one closest to what the user asks for, and respond with its index, followed by a message confirming the choice. ";
        prompt += "For example, if the user says \"choose the shopping mall\", you can return \"1 Default Navigation: Shopping Mall scene chosen.\"";
        prompt += "If it is impossible to tell what the user wants, or if they made an irrelevant prompt, return \"Choice was unclear, please try again.\"";
        prompt += "Return in the format of my examples, and only that. Messages should be very short.";

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
                sceneManager.ProcessInputString(formatted);
                                
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

}
