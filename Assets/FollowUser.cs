using UnityEngine;

public class FollowUser : MonoBehaviour
{
    [Header("References")]
    public Transform xrCamera;

    [Header("Positioning")]
    public float preferredDistance = 2f;
    public float verticalOffset = 0f;
    public Vector3 canvasOffset = Vector3.zero;

    [Header("Collision Avoidance")]
    public LayerMask obstructionMask = ~0; // default to all layers
    public float wallBuffer = 0.1f;
    public float repositionSpeed = 5f;

    private void Update()
    {
        if (xrCamera == null)
        {
            Debug.LogWarning("XR Camera not assigned.");
            return;
        }

        // Desired position in front of camera
        Vector3 desiredPos = xrCamera.position 
            + xrCamera.forward * preferredDistance 
            + xrCamera.up * verticalOffset 
            + xrCamera.TransformDirection(canvasOffset);

        Vector3 directionToUser = xrCamera.position - desiredPos;
        float distanceToUser = directionToUser.magnitude;

        bool hitWall = false;

        // Raycast from desiredPos → user to check for wall between
        if (Physics.Raycast(desiredPos, directionToUser.normalized, out RaycastHit hit, distanceToUser, obstructionMask))
        {
            // Adjust position to be just in front of the hit point
            desiredPos = hit.point - directionToUser.normalized * wallBuffer;
            hitWall = true;
            Debug.Log($"[UI] Obstructed by: {hit.collider.gameObject.name}");
        }

        // Move smoothly
        transform.position = Vector3.Lerp(transform.position, desiredPos, Time.deltaTime * repositionSpeed);

        // Rotate to face user
        transform.rotation = Quaternion.LookRotation(transform.position - xrCamera.position);

        Debug.DrawLine(desiredPos, xrCamera.position, hitWall ? Color.red : Color.green);
    }

    // Optional: Gizmos for editor visualization
    private void OnDrawGizmosSelected()
    {
        if (xrCamera == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, xrCamera.position);
    }
}
