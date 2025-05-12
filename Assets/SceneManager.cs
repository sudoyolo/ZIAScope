using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using TMPro;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement; // Needed for SceneManager.LoadScene()

public class SceneManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI feedback;
    public static SceneManager Instance { get; private set; }
    [SerializeField] private Button button1;
    [SerializeField] private Button button2;
    [SerializeField] private Button button3;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Avoid duplicates
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); 
    }

    void Start()
    {
        // Set up button listeners
        if (button1 != null) button1.onClick.AddListener(() => LoadScene(1));
        if (button2 != null) button2.onClick.AddListener(() => LoadScene(2));
        if (button3 != null) button3.onClick.AddListener(() => LoadScene(3));
    }

    public void ProcessInputString(string input)
    {
        Match match = Regex.Match(input, @"^\s*(-?\d+)\s*(.*)");

        if (match.Success)
        {
            int sceneIndex = int.Parse(match.Groups[1].Value);
            string extra = match.Groups[2].Value;

            LoadScene(sceneIndex);
            GiveFeedback(extra);
        }
        else
        {
            GiveFeedback(input);
        }
    }

    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(DelayedSceneLoad(sceneIndex));
    }

    public void LoadHome(string buff)
    {
        StartCoroutine(DelayedSceneLoad(0));
        //UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    private IEnumerator DelayedSceneLoad(int sceneIndex)
    {
        yield return new WaitForSeconds(2f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }

    void GiveFeedback(string message)
    {
        Debug.Log($"[GiveFeedback] Message: {message}");
        if (feedback != null)
            feedback.text = message;
    }
}
