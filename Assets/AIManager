using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TMPro;
using System.Text;
using Newtonsoft.Json.Linq;

public class AIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI aiCommentaryText;

    private const string API_URL = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";
    // url should look something like: https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent"
    private const string API_KEY = "AIzaSyCmmPpM_7x6em730kAqSWdIExZOggZLx2I";
    // should look something like: AIzaSyBF_Om4mTSLo5DLabTrQWGvy0iqVhgcB9q

    public void GenerateAICommentary(string arg1, string arg2)
    {
        string prompt = $"Write a poem about {arg1} and {arg2}.";
        // further tweaking of the prompt: 
        prompt += "Provide a response of 40 words or less and a title.";
        prompt += $"Use {arg1} and {arg2} in the title of the poem.";
        StartCoroutine(SendRequestToGemini(prompt));
    }

    private IEnumerator SendRequestToGemini(string prompt)
    {
        // construct the JSON request body
        string jsonData = $"{{\"contents\": [{{\"parts\": [{{\"text\": \"{prompt}\"}}]}}]}}";

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
                Debug.Log("Gemini API Response: " + response);

                string commentary = ExtractCommentaryFromResponse(response);
                aiCommentaryText.text = commentary;
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
            JObject json = JObject.Parse(response);
            return json["candidates"]?[0]?["content"]?["parts"]?[0]?["text"]?.ToString() ?? "I've got you this next time!";
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error parsing AI response: " + e.Message);
            return "Error retrieving AI commentary.";
        }
    }
}
