using UnityEngine;

public class TunnelingManager : MonoBehaviour
{
    public Material vignetteMaterial; // Assign in Inspector
    public float movementThreshold = 0.1f;
    public float fadeSpeed = 2f;

    private Transform playerTransform;
    private Vector3 lastPosition;

    private float currentStrength = 0f;

    void Start()
    {
        playerTransform = Camera.main.transform;
        lastPosition = playerTransform.position;
    }

    void Update()
    {
        float speed = (playerTransform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = playerTransform.position;

        float targetStrength = speed > movementThreshold ? 1f : 0f;
        currentStrength = Mathf.Lerp(currentStrength, targetStrength, Time.deltaTime * fadeSpeed);

        vignetteMaterial.SetFloat("_Intensity", currentStrength); // Adjust key name as needed
    }
}
