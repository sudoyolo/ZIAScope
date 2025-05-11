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
            int index = 1;



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
            requestInProgress = true;
            var res = await openai.CreateAudioTranscription(req);

            progressBar.fillAmount = 0;
            scrollingList.AddString(res.Text, "white");
            aiManager.GenerateAICommentary(res.Text);
            isRecording = false;
        }

        private void Update()
        {
            if (isRecording)
            {
                progressBar.fillAmount = 1;
            }
            else
            {
                progressBar.fillAmount = 0;
            }
        }
    }
}
