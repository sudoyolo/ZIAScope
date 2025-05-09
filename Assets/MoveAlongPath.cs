using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.AI;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Comfort;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class MoveAlongPath : LocomotionProvider
{
    private bool travelling;
    public Selection selection;
    public Wayfinding wayfinding;
    public Camera mainCamera;
    private int gameObjectId;
    public PathManager pathManager;
    private SingleNavPath singleNavPath;
    private NavMeshPath path;
    public TunnelingVignetteController tunnelingVignette;
    public float timer;
    public float timeInterval = 0.5f;
    private int currentCornerIndex;
    private float moveDistance = 1f;
    public bool locomotionEnabled;
    void Start()
    {
        travelling = false;
    }

    public void startTravelling( int gameObjectId)
    {
        StartCoroutine(StartTravellingLinksAttached(gameObjectId));
    }

    IEnumerator StartTravellingLinksAttached(int gameObjectId)
    {
        yield return new WaitForSeconds(1);
        locomotionEnabled = TryPrepareLocomotion();
        GameObject pathObj = pathManager.GetNavPath(-1, gameObjectId);
        singleNavPath = pathObj.GetComponent<SingleNavPath>();
        path = singleNavPath.path;
        singleNavPath.shouldUpdate = false;
        travelling = true;
        currentCornerIndex = 0;
        timer = 0f;
        this.gameObjectId = gameObjectId;
    }

    public void teleport(Vector3 target)
    {
        //calculate local coordinates of mainCamera transform in respect to the parent's transform.
        //update parent transform, so that child ends up where it needs to be. 
        Vector3 mainCameraOffset = transform.InverseTransformPoint(mainCamera.transform.position);
        Vector3 desiredCameraPosition = new Vector3(target.x, mainCamera.transform.position.y, target.z); 
        Vector3 originNewPosition = desiredCameraPosition - mainCameraOffset;
        transform.position = originNewPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (travelling)
        {
            if (!locomotionEnabled)
            {
                locomotionEnabled = TryPrepareLocomotion();
                if (!locomotionEnabled) return;
            }

            timer += Time.deltaTime;
            if (timer >= timeInterval && path!=null && currentCornerIndex < path.corners.Length)
            {
                timer = 0f;
                Vector3 mainCameraOffset = transform.InverseTransformPoint(mainCamera.transform.position);
                Vector3 targetCorner = new Vector3(path.corners[currentCornerIndex].x, mainCamera.transform.position.y,path.corners[currentCornerIndex].z);
                Vector3 xrOriginDestination = (targetCorner - mainCameraOffset);
                Vector3 vecToTarget = xrOriginDestination - transform.position;
                if (vecToTarget.magnitude < moveDistance)
                {
                    transform.position = xrOriginDestination;
                    currentCornerIndex += 1;
                }
                else
                {
                    transform.position +=vecToTarget.normalized* moveDistance;
                }

                if (vecToTarget.magnitude > 0.01)
                {
                    //rotation logic
                    //Quaternion lookRotation = Quaternion.LookRotation(vecToCorner);
                    //transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 0.25f);
                }
                if (currentCornerIndex == path.corners.Length || (currentCornerIndex == path.corners.Length-1 && Vector3.Distance(transform.position,xrOriginDestination)<1) )
                {
                    //reached destination 
                    travelling = false;
                    path = null;
                    wayfinding.clearSinglePath("ClearPathCalledFromNavMesh "+ gameObjectId);
                    TryEndLocomotion();
                }
            }
        }
    }
    
}

