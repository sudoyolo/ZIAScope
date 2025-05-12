using UnityEngine;
using UnityEngine.UI;

public class TouchClickButton : MonoBehaviour
{
    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GameController") && button != null)
        {
            button.onClick.Invoke(); 
        }
    }
}
