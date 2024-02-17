using Fusion;
using UnityEngine;
using UnityEngine.AI;

public class Player : NetworkBehaviour
{
    [SerializeField] private NetworkTransform _nTransform;
    [Networked] public byte playerColor_r { get; set; }
    [Networked] public byte playerColor_g { get; set; }
    [Networked] public byte playerColor_b { get; set; }
    private Material _material;

    public void Awake()
    {
        _nTransform = GetComponent<NetworkTransform>();
        _material = GetComponentInChildren<MeshRenderer>().material;
        _agent = GetComponentInChildren<NavMeshAgent>();
    }
    
    private ChangeDetector _changeDetector;

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        UpdateColor();
    }

    public void SetInitialPosition(Vector3 position)
    {
        _nTransform.Teleport(position, Quaternion.identity);
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            data.direction.Normalize();
            UpdateMovementTargetPosition(data.position);
        }

        UpdatePlayerPosition();
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            switch (change)
            {
                case nameof(playerColor_r):
                case nameof(playerColor_g):
                case nameof(playerColor_b):
                    UpdateColor();
                    break;
            }
        }
    }

    private void UpdateColor()
    {
        _material.color = new Color(playerColor_r / 255f, playerColor_g / 255f, playerColor_b / 255f);
    }

    public bool isAlive = true;
    public bool arrived = true;
    public bool isFollowing;
    public bool despawnInProgress = false;
    public float unitCollisionSize = 2f;
    public float rotateSpeed = 1f;
    public float moveSpeed = 1f;
    private NavMeshAgent _agent;
    private Vector3 _targetPosition;

    private Vector3 targetPosition
    {
        get => _targetPosition;
        set
        {
            _targetPosition = value;
            _agent.destination = value;
        }
    }
    public void UpdateMovementTargetPosition(Vector3 target)
    {
        if (target != Vector3.zero)
        {
            if (targetPosition != target)
            {
                Debug.Log($"updating move target {target}");
                targetPosition = target;
                arrived = false;    
            }
            
        }
    }

    void UpdatePlayerPosition()
    {
        // if (unitScript.statsScript.stats.isAlive)
        if (isAlive)
        {
            if (!arrived)
            {
                if (_agent.hasPath)
                {
                    Debug.Log("x33 path pending");
                    // unitScript.animScript.setMoving(true);
                    this.OnUnitMove();
                }
                else
                {
                    // if (agent.pathStatus != NavMeshPathStatus.PathComplete)
                    // throw new Exception("wtf PathInvalid or PathPartial");

                    Debug.Log("x33 path done");
                    // unitScript.animScript.setMoving(false);
                    arrived = true;
                }
                // if (
                //     Mathf.Abs(targetPosition.x - this.transform.position.x) >
                //     0.5f * (isFollowing ? unitCollisionSize : 1f) ||
                //     Mathf.Abs(targetPosition.z - this.transform.position.z) >
                //     0.5f * (isFollowing ? unitCollisionSize : 1f)
                // )
                // {
                //     Vector3 lookPos = targetPosition - transform.position;
                //     rotateUnit(lookPos);
                //     lookPos.y = 0;
                //     transform.Translate(Vector3.forward * (Runner.DeltaTime * moveSpeed));
                //     // unitScript.animScript.setMoving(true);
                //     // this.OnUnitMove();
                // }
                // else
                // {
                //     // unitScript.animScript.setMoving(false);
                //     arrived = true;
                //     if (isFollowing)
                //     {
                //         // if (unitScript.faction == this.followingTo.faction)
                //         // {
                //         //     // arrived to friendly unit
                //         // }
                //         // else
                //         // {
                //         //     // arrived to enemy unit
                //         //     unitScript.attackScript.offenceStart(this.followingTo.attackScript);
                //         // }
                //     }
                // }
            }
            else
            {
                // if (this.unitScript.attackScript.isAttacking)
                // {
                //     rotateUnit(targetPosition - transform.position);
                // }
            }
        }
        else
        {
            if (despawnInProgress)
            {
                // transform.Translate(Vector3.down * (Time.deltaTime * 0.1f));
            }
        }
    }
    
    void OnUnitMove()
    {
        // foreach (MoveScript eachFollowerMoveScript in followersTransforms)
        // {
        //     // eachFollowerMoveScript.targetPostion = unitScript.transform.position;
        //     eachFollowerMoveScript.targetPostion = new Vector3(
        //         unitScript.transform.position.x,
        //         unitScript.transform.position.y,
        //         unitScript.transform.position.z - 2f
        //     );
        //     eachFollowerMoveScript.arrived = false;
        // }
    }
}