using System.Collections.Generic;
using UnityEngine;
using TMPro; 
using UnityEngine.UI;

public class ScrollingStringList : MonoBehaviour
{
    public Transform contentPanel; // Content panel of the scroll view
    public GameObject stringPrefab; // Prefab for each string entry (e.g., TextMeshProUGUI)
    public ScrollRect scrollRect;
    private Queue<(string, string)> stringQueue = new Queue<(string, string)>(); // Store both text and color
    private const int maxStrings = 10;

    void Start()
    {
        // Ensure there's no leftover objects in the content panel at the start
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }
    }

    public void AddString(string newString, string color = "default")
    {
        // Add the new string and its color to the queue
        if (stringQueue.Count >= maxStrings)
        {
            stringQueue.Dequeue(); // Remove the oldest entry if the limit is reached
        }
        stringQueue.Enqueue((newString, color));

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
        foreach (var (text, color) in stringQueue)
        {
            GameObject newText = Instantiate(stringPrefab, contentPanel);
            TextMeshProUGUI textComponent = newText.GetComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.color = GetColorFromName(color);
        }
    }

    private Color GetColorFromName(string colorName)
    {
        switch (colorName.ToLower())
        {
            case "lightblue": 
                return new Color(0.231f, 0.799f, 1.0f); // Light blue RGB
            case "white":
                return Color.white;
            default:
                return Color.white; // Default color
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
