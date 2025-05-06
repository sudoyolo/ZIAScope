using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    [System.Serializable]
    public class UIGroup
    {
        public GameObject group;       // The UI group/panel
        public Button nextButton;      // Optional: next button (not required for last group)
    }

    [SerializeField]
    private List<UIGroup> uiGroups = new List<UIGroup>();

    private int currentIndex = 0;

    void Start()
    {
        for (int i = 0; i < uiGroups.Count; i++)
        {
            // Show only the first group
            uiGroups[i].group.SetActive(i == currentIndex);

            // Only assign nextButton listeners for groups *except* the last one
            if (i < uiGroups.Count - 1 && uiGroups[i].nextButton != null)
            {
                int index = i;
                uiGroups[i].nextButton.onClick.RemoveAllListeners();
                uiGroups[i].nextButton.onClick.AddListener(() => ShowNextGroup(index));
            }
        }
    }

    void ShowNextGroup(int index)
    {
        if (index < uiGroups.Count - 1)
        {
            uiGroups[index].group.SetActive(false);
            uiGroups[index + 1].group.SetActive(true);
            currentIndex = index + 1;
        }
    }
}
