using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class MoveAlongPath : MonoBehaviour
{
    private NavMeshAgent agent;
    private bool travelling;
    private Vector3 target;
    public LocomotionMediator locomotionMediator;
    public TeleportationProvider teleportationProvider;
    public DynamicMoveProvider moveProvider;
    public SnapTurnProvider snapTurnProvider;
    public Selection selection;
    public Wayfinding wayfinding;
    public float rotationSpeed = 1.0f;
    private int gameObjectId;
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        travelling = false;
    }

    public void startTravelling(Vector3 target, int gameObjectId)
    {
        this.target = target;
        agent.SetDestination(target);
        travelling = true;
        locomotionMediator.enabled = false;
        teleportationProvider.enabled = false;
        moveProvider.enabled = false;
        snapTurnProvider.enabled = false;
        this.gameObjectId = gameObjectId;
        agent.updateRotation = false;

    }

    public void teleport(Vector3 target)
    {
        transform.position = target;
    }

    // Update is called once per frame
    void Update()
    {
        if (travelling)
        {
            if (Vector3.Distance(transform.position, target) < 2f)
            {
                //reached destination 
                travelling = false;
                agent.ResetPath();
                locomotionMediator.enabled = true;
                teleportationProvider.enabled = true;
                moveProvider.enabled = true;
                snapTurnProvider.enabled = true;
                wayfinding.clearSinglePath("ClearPathCalledFromNavMesh "+ gameObjectId);
                
            }
        }
    }
    
}
