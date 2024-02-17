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
            _cc.Move(50 * data.direction * Runner.DeltaTime);
        }
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
        _material.color = new Color(playerColor_r/255f,playerColor_g/255f,playerColor_b/255f);
    }
}