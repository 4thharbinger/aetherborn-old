using Unity.Mathematics;

using UnityEngine;

public class ThrustVector : Module, ITickedModule, IControlModule
{
    public float maxAngle = 15f;

    public Vector3 pitchVector = Vector3.up;
    public Vector3 yawVector = Vector3.right;
    public Vector3 baseVector = -Vector3.forward;

    public Thruster thruster;

    public void Advance(double dt)
    {
        thruster.thrustVector = (maxAngle * pitch * pitchVector + maxAngle * yaw * yawVector + baseVector).normalized;
    }

    public float pitch { get; set; }
    public float yaw { get; set; }
    public float roll { get; set; }
}

/// <summary>
///     Module receives and can act on commands (pitch, yaw, roll) from a control source to change the attitude or position of the vehicle.
/// </summary>
public interface IControlModule : IModuleInterface
{
    /// <summary>
    ///     Pitch adjustment command, ranges f
    /// </summary>
    public float pitch { get; set; }
    public float yaw { get; set; }
    public float roll { get; set; }
}