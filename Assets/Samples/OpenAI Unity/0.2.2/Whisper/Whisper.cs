using OpenAI;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace Samples.Whisper
{
    public class Whisper : MonoBehaviour
    {
        [SerializeField] private Button recordButton;
        [SerializeField] private Image progressBar;
        [SerializeField] private TextMeshProUGUI message;
        [SerializeField] private AIManager aiManager;
        private GlobalVariables globalVariables;
        public ScrollingStringList scrollingList;

        private readonly string fileName = "output.wav";
        private readonly int duration = 4;

        private AudioClip clip;
        private bool isRecording;
        private float time;
        private List<string> microphoneDevices = new List<string>();
        private OpenAIApi openai = new OpenAIApi("api-key");

        private void Start()
        {
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

            recordButton.onClick.AddListener(StartRecording);
#endif
        }

        private void StartRecording()
        {
            message.text = "...";
            isRecording = true;
            recordButton.enabled = false;

#if !UNITY_WEBGL
            //int index = globalVariables.microphoneIdx;
            int index = 0;



            if (index >= 0 && index < microphoneDevices.Count)
            {
                string selectedDevice = microphoneDevices[index];
                clip = Microphone.Start(selectedDevice, false, duration, 44100);
            }
            else
            {
                Debug.LogError("Invalid microphone index: " + index);
                isRecording = false;
                recordButton.enabled = true;
            }
#endif
        }

        private async void EndRecording()
        {
#if !UNITY_WEBGL
            Microphone.End(null);
#endif

            byte[] data = SaveWav.Save(fileName, clip);

            var req = new CreateAudioTranscriptionsRequest
            {
                FileData = new FileData() { Data = data, Name = "audio.wav" },
                Model = "whisper-1",
                Language = "en"
            };
            var res = await openai.CreateAudioTranscription(req);

            progressBar.fillAmount = 0;
            scrollingList.AddString(res.Text, "white");
            aiManager.GenerateAICommentary(res.Text);
            recordButton.enabled = true;
        }

        private void Update()
        {
            if (isRecording)
            {
                time += Time.deltaTime;
                progressBar.fillAmount = time / duration;

                if (time >= duration)
                {
                    time = 0;
                    isRecording = false;
                    EndRecording();
                }
            }
        }
    }
}
