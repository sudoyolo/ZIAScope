using System.Collections.Generic;
using UnityEngine;
using TMPro; 
using UnityEngine.UI;


public class ScrollingStringList : MonoBehaviour
{
    public Transform contentPanel; // Content panel of the scroll view
    public GameObject stringPrefab; // Prefab for each string entry (e.g., TextMeshProUGUI)
    public ScrollRect scrollRect;
    private Queue<string> stringQueue = new Queue<string>();
    private const int maxStrings = 10;

    void Start()
    {
        // Ensure there's no leftover objects in the content panel at the start
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }
    }

    public void AddString(string newString)
    {
        // Add the new string to the queue
        if (stringQueue.Count >= maxStrings)
        {
            stringQueue.Dequeue(); // Remove the oldest string if the limit is reached
        }
        stringQueue.Enqueue(newString);

        // Update the display
        UpdateDisplay();
        ScrollToBottom();
    }

    private void UpdateDisplay()
    {
        // Clear current UI items
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // Create a new UI element for each string in the queue
        foreach (string str in stringQueue)
        {
            GameObject newText = Instantiate(stringPrefab, contentPanel);
            TextMeshProUGUI textComponent = newText.GetComponent<TextMeshProUGUI>(); // Change to TextMeshProUGUI
            textComponent.text = str;
        }
    }

    public void ScrollToBottom()
    {
        // Force the content layout to update before scrolling
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f; // 0 = bottom, 1 = top
        Canvas.ForceUpdateCanvases();
    }
}
