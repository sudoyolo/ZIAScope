using OpenAI;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using TMPro;
using UnityEngine.InputSystem;

namespace Samples.Whisper
{
    public class WhisperHomeVersion : MonoBehaviour
    {
        [SerializeField] private Image progressBar;
        [SerializeField] private Dropdown dropdown;
        [SerializeField] private AIManagerHome aiManagerHome;
        public GlobalVariables globalvariables;
        private readonly string fileName = "output.wav";
        private readonly int duration = 10;
        
        private AudioClip clip;
        private bool isRecording;
        private float time;
        private OpenAIApi openai = new OpenAIApi("api key");
        private bool requestInProgress;
        private PlayerInputActions inputActions;
        private void Start()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            dropdown.options.Add(new Dropdown.OptionData("Microphone not supported on WebGL"));
            #else
            foreach (var device in Microphone.devices)
            {
                dropdown.options.Add(new Dropdown.OptionData(device));
            }
            dropdown.onValueChanged.AddListener(ChangeMicrophone);
            
            var index = PlayerPrefs.GetInt("user-mic-device-index");
            globalvariables.microphoneIdx = index;
            dropdown.SetValueWithoutNotify(index);
            #endif
            requestInProgress = false;
            inputActions = InputManager.inputActions;
            inputActions.Gameplay.ToggleRecording.performed += ToggleRecording;
            time = 0f;
        }
        void OnDisable()
        {
            inputActions.Gameplay.ToggleRecording.performed -= ToggleRecording;
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
        
        private void ChangeMicrophone(int index)
        {
            PlayerPrefs.SetInt("user-mic-device-index", index);
            globalvariables.microphoneIdx = index;
        }
        
        private void StartRecording()
        {
            isRecording = true;

            int index = PlayerPrefs.GetInt("user-mic-device-index");
            //int index = 2;
            #if !UNITY_WEBGL
            clip = Microphone.Start(dropdown.options[index].text, false, duration, 44100);
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
                FileData = new FileData() {Data = data, Name = "audio.wav"},
                // File = Application.persistentDataPath + "/" + fileName,
                Model = "whisper-1",
                Language = "en"
            };
            requestInProgress = true;
            var res = await openai.CreateAudioTranscription(req);
            Debug.Log(res.Text);
            Color color = progressBar.color;
            color.a = 0f;
            progressBar.color = color;
            aiManagerHome.GenerateAICommentary(res.Text);
            isRecording = false;
            time = 0f;
        }
        public void requestCompleted()
        {
            requestInProgress = false;
        }
        private void Update()
        {
            
            if (isRecording)
            {
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
