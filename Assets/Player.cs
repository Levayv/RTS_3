using Fusion;
using UnityEngine;

public class Player : NetworkBehaviour
{
    private NetworkCharacterController _cc;
    [Networked] public byte playerColor_r { get; set; }
    [Networked] public byte playerColor_g { get; set; }
    [Networked] public byte playerColor_b { get; set; }
    private Material _material;

    public void Awake()
    {
        _cc = GetComponent<NetworkCharacterController>();
        _material = GetComponentInChildren<MeshRenderer>().material;
    }

    private ChangeDetector _changeDetector;

    public override void Spawned()
    {
        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        UpdateColor();
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

    public Vector3 targetPosition = Vector3.zero;
    public bool isAlive = true;
    public bool arrived = true;
    public bool isFollowing;
    public bool despawnInProgress = false;
    public float unitCollisionSize = 2f;
    public float rotateSpeed = 1f;
    public float moveSpeed = 1f;

    public void UpdateMovementTargetPosition(Vector3 target)
    {
        if (target != Vector3.zero)
        {
            Debug.Log($"updating move target {target}");
            targetPosition = target;
            arrived = false;
        }
    }

    void UpdatePlayerPosition()
    {
        // if (unitScript.statsScript.stats.isAlive)
        if (isAlive)
        {
            if (!arrived)
            {
                if (
                    Mathf.Abs(targetPosition.x - this.transform.position.x) >
                    0.5f * (isFollowing ? unitCollisionSize : 1f) ||
                    Mathf.Abs(targetPosition.z - this.transform.position.z) >
                    0.5f * (isFollowing ? unitCollisionSize : 1f)
                )
                {
                    Vector3 lookPos = targetPosition - transform.position;
                    rotateUnit(lookPos);
                    lookPos.y = 0;
                    transform.Translate(Vector3.forward * (Runner.DeltaTime * moveSpeed));
                    // unitScript.animScript.setMoving(true);
                    // this.OnUnitMove();
                }
                else
                {
                    // unitScript.animScript.setMoving(false);
                    arrived = true;
                    if (isFollowing)
                    {
                        // if (unitScript.faction == this.followingTo.faction)
                        // {
                        //     // arrived to friendly unit
                        // }
                        // else
                        // {
                        //     // arrived to enemy unit
                        //     unitScript.attackScript.offenceStart(this.followingTo.attackScript);
                        // }
                    }
                }
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
                transform.Translate(Vector3.down * (Time.deltaTime * 0.1f));
            }
        }
    }

    private void rotateUnit(Vector3 lookPos)
    {
        if (Mathf.Abs(lookPos.x) > 1f || Mathf.Abs(lookPos.z) > 1f)
        {
            if (transform.rotation != Quaternion.LookRotation(lookPos))
            {
                var rotation = Quaternion.LookRotation(lookPos);
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, Time.deltaTime * rotateSpeed);
            }
        }
    }
}