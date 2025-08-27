using UnityEngine;

public class DebugGyroscope : Module, IControlModule, ITickedModule
{
    public float pitch { get; set; }
    public float yaw { get; set; }
    public float roll { get; set; }

    public Vector3 torque;

    public void Advance(double dt)
    {
        var rb = vehicle.floatingTransform.rigidbody;
        rb.angularVelocity += transform.TransformDirection(new Vector3(pitch * torque.x, yaw * torque.y, roll * torque.z));
    }
}
