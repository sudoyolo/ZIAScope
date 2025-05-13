using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

// Computer Vision API call, sends an image to API alongside text prompt

public class ComputerVision : MonoBehaviour
{
    /*
    [Header("UI")]
    public GameObject xrDeviceSimulatorObject; 
    private MonoBehaviour xrSimulatorComponent;

    public TMP_InputField promptInput;
    public TextMeshProUGUI responseText;
    */
    public ScrollingStringList scrollingList;

    [Header("Camera")]
    public Camera captureCamera; 

    [Header("API Settings")]
    private string replicateUrl = "https://api.replicate.com/v1/predictions";
    private string replicateToken = ""; // GET RID BEFORE PUSH ELSE DIE
    private string llavaModelVersion = "80537f9eead1a5bfa72d5ac6ea6414379be41d4d4f6679fd776e9535d1eb58bb"; // LLaVA 13B

    void Start()
    {
        //xrSimulatorComponent = xrDeviceSimulatorObject.GetComponent<MonoBehaviour>();

        //promptInput.onSelect.AddListener(_ => ToggleXR(false));
        //promptInput.onDeselect.AddListener(_ => ToggleXR(true));
        //promptInput.onSubmit.AddListener(OnPromptSubmitted);
    }
    /*
    void ToggleXR(bool enabled)
    {
        if (xrSimulatorComponent != null)
            xrSimulatorComponent.enabled = enabled;
    }*/

    void OnPromptSubmitted(string text)
    {
        StartCoroutine(SendImageWithPromptToReplicate(text));
    }

    public void SubmitPrompt(string text)
    {
        string prompt = "";
        prompt += "The user is designing a scene in a virtual environment, and wants some feedback about this screen capture of their scene. In 25 words or less, provide them an answer to this question: ";
        prompt += text;
        prompt += "\nAlso offer critiques or improvements, and point out any flaws. Focus on color/lighting/placement changes, such as moving a group of objects backwards, or adding duplicates of existing objects.";
        prompt += "Try not to suggest adding new things that do not already exist. Do not describe what is obviously in the scene.";
        StartCoroutine(SendImageWithPromptToReplicate(prompt));
    }

    IEnumerator SendImageWithPromptToReplicate(string prompt)
    {
        // Capture full screen from XR camera
        int width = Screen.width;
        int height = Screen.height;
        RenderTexture rt = new RenderTexture(width, height, 24);
        Texture2D photo = new Texture2D(width, height, TextureFormat.RGB24, false);

        captureCamera.targetTexture = rt;
        captureCamera.Render();
        RenderTexture.active = rt;
        photo.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        photo.Apply();
        captureCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        byte[] imageBytes = photo.EncodeToJPG();
        string imageBase64 = Convert.ToBase64String(imageBytes);
        string imageDataUrl = $"data:image/jpeg;base64,{imageBase64}";

        // Create JSON payload
        var requestJson = new
        {
            version = llavaModelVersion,
            input = new Dictionary<string, string>
            {
                { "image", imageDataUrl },
                { "prompt", prompt }
            }
        };


        UnityWebRequest request = new UnityWebRequest(replicateUrl, "POST");
        // byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonString.Replace("\"value\":", "").TrimStart('{').TrimEnd('}'));

        string jsonString = JsonConvert.SerializeObject(new
        {
            version = llavaModelVersion,
            input = new Dictionary<string, string>
            {
                { "image", imageDataUrl },
                { "prompt", prompt }
            }
        });
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonString);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Authorization", "Token " + replicateToken);
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Request error: " + request.error);
            //responseText.text = "API request failed: " + request.error;
            yield break;
        }

        string pollUrl = GetPollUrl(request.downloadHandler.text);

        while (true)
        {
            UnityWebRequest poll = UnityWebRequest.Get(pollUrl);
            poll.SetRequestHeader("Authorization", "Token " + replicateToken);
            yield return poll.SendWebRequest();

            if (poll.result != UnityWebRequest.Result.Success)
            {
               //responseText.text = "Polling failed: " + poll.error;
                yield break;
            }

            var resultJson = poll.downloadHandler.text;
            if (resultJson.Contains("\"status\":\"succeeded\""))
            {
                string output = ExtractOutputFromJson(resultJson);
                scrollingList.AddString(output, "lightblue");
                //responseText.text = output;
                yield break;
            }
            else if (resultJson.Contains("\"status\":\"failed\""))
            {
                //responseText.text = "Prediction failed.";
                yield break;
            }

            yield return new WaitForSeconds(1f); // wait before polling again
        }
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T value;
    }

    // Extract poll URL
    private string GetPollUrl(string json)
    {
        int start = json.IndexOf("\"get\":\"") + 7;
        int end = json.IndexOf("\"", start);
        return json.Substring(start, end - start).Replace("\\/", "/");
    }

    // final output
    string ExtractOutputFromJson(string json)
    {
        try
        {
            JObject parsed = JObject.Parse(json);
            JArray outputArray = (JArray)parsed["output"];
            return string.Join(" ", outputArray.Select(token => token.ToString()));
        }
        catch
        {
            return "Failed to parse model output.";
        }
    }

}
