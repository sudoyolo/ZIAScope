using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using TMPro;

public class GlobalVariables : MonoBehaviour
{
    public static GlobalVariables Instance { get; private set; }
    public int microphoneIdx = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Avoid duplicates
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist between scenes
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
