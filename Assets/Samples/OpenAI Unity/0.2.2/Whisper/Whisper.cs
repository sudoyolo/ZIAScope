using OpenAI;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;

namespace Samples.Whisper
{
    public class Whisper : MonoBehaviour
    {
        [SerializeField] private Image progressBar;
        [SerializeField] private TextMeshProUGUI message;
        [SerializeField] private AIManager aiManager;
        private GlobalVariables globalVariables;
        public ScrollingStringList scrollingList;

        private readonly string fileName = "output.wav";
        private readonly int duration = 10;

        private AudioClip clip;
        private bool isRecording;
        private float time;
        private List<string> microphoneDevices = new List<string>();
        private OpenAIApi openai = new OpenAIApi("api key");
        private PlayerInputActions inputActions;
        private bool requestInProgress;
        private void Start()
        {
            isRecording = false;
            requestInProgress = false;
            time = 0f;
            inputActions = InputManager.inputActions;
            inputActions.Gameplay.ToggleRecording.performed += ToggleRecording; 
            globalVariables = FindObjectOfType<GlobalVariables>();
#if UNITY_WEBGL && !UNITY_EDITOR
            Debug.LogWarning("Microphone not supported on WebGL");
#else
            foreach (var device in Microphone.devices)
            {
                microphoneDevices.Add(device);
            }

            if (microphoneDevices.Count == 0)
            {
                Debug.LogError("No microphone devices found.");
            }
#endif
        }

        public void requestCompleted()
        {
            requestInProgress = false;
        }

        private void ToggleRecording(InputAction.CallbackContext context)
        {
            
            if (!requestInProgress)
            {
                if (isRecording)
                {
                    EndRecording();
                }
                else
                {
                    StartRecording();
                }
            }


        }

        private void StartRecording()
        {
            message.text = "...";
            isRecording = true;

#if !UNITY_WEBGL
            //int index = PlayerPrefs.GetInt("user-mic-device-index");
            int index = 0;
            if(globalVariables!=null) {
                index = globalVariables.microphoneIdx-1;
                Debug.Log("global variable idx is " + index);
            }
            /*foreach(string device in microphoneDevices) 
            {
                Debug.Log("device: " + device);
            }*/

            if (index >= 0 && index < microphoneDevices.Count)
            {
                string selectedDevice = microphoneDevices[index];
                clip = Microphone.Start(selectedDevice, false, duration, 44100);
            }
            else
            {
                Debug.LogError("Invalid microphone index: " + index);
                isRecording = false;
            }
#endif
        }

        private async void EndRecording()
        {
            isRecording = false;
            time = 0f;
#if !UNITY_WEBGL
            Microphone.End(null);
#endif
            requestInProgress = true;
            byte[] data = SaveWav.Save(fileName, clip);

            var req = new CreateAudioTranscriptionsRequest
            {
                FileData = new FileData() { Data = data, Name = "audio.wav" },
                Model = "whisper-1",
                Language = "en"
            };
            var res = await openai.CreateAudioTranscription(req);
            
            Color color = progressBar.color;
            color.a = 0f;
            progressBar.color = color;
            scrollingList.AddString(res.Text, "white");
            aiManager.GenerateAICommentary(res.Text);
            
        }

        private void Update()
        {
            
            if (isRecording)
            {
                time += Time.deltaTime;
                float pulse = (Mathf.Sin(Time.time * 6f) + 1f) / 2f;
                Color color = progressBar.color;
                color.a = Mathf.Lerp(0.3f, 1f, pulse); 
                progressBar.color = color;
            }
            else
            {
                Color color = progressBar.color;
                color.a = 0f; 
                progressBar.color = color;
                //progressBar.fillAmount = 0;
            }
            
            if (time >= duration)
            {
                EndRecording();
            }
        }
    }
}
