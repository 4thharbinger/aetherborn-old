using System.Security.Cryptography;

using Unity.Mathematics;
using Unity.VisualScripting;

using UnityEngine;

public class Aerofoil : Module, ITickedModule, IGraphicsModule, IControlModule
{
    public float angle;
    public float pitch { get; set; }
    public float yaw { get; set; }
    public float roll { get; set; }

    public float lift = 10f;
    public float drag = 10f;

    public float controlAuthority = 15f;
    public float stallAngle = 30f;

    public float minArea = 1f;
    public float maxArea = 10f;

    public double airPressure;
    public double airDensity;

    public Vector3 controlResponse;

    public Vector3 offset;

    public Transform rotor;
    public Vector3 liftForce;
    public Vector3 dragForce;

    public double3 worldVelocity;

    public Vector3 streamVelocity;

    public double3 airVelocity;

    public void Advance(double dt)
    {
        worldVelocity = vehicle.floatingTransform.Velocity + (float3)vehicle.floatingTransform.rigidbody.GetRelativePointVelocity(transform.localPosition) - (float3)vehicle.floatingTransform.rigidbody.linearVelocity - airVelocity;
        streamVelocity = transform.InverseTransformVector((float3)worldVelocity);

        angle = math.atan2(streamVelocity.x, streamVelocity.z) * Mathf.Rad2Deg;

        airDensity = CelestialBody.CalculateDensity(vehicle.floatingTransform.GetWorldPosition() + (float3)transform.localPosition);
        airPressure = CelestialBody.CalculatePressure(vehicle.floatingTransform.GetWorldPosition() + (float3)transform.localPosition);

        var angleOffset = controlResponse.x * pitch + controlResponse.y * yaw + controlResponse.z * roll;

        angle += angleOffset * controlAuthority;

        var crossSectionArea = math.lerp(minArea, maxArea, math.sin(math.abs(angle * Mathf.Deg2Rad)));

        liftForce = (float)airDensity * (float)dt * CalculateLift(angle) * crossSectionArea * -lift * streamVelocity.magnitude * transform.right;

        dragForce = drag * (float)dt * crossSectionArea * -(float3) worldVelocity * (float)airDensity;

        vehicle.AddForce(transform.localPosition + offset, liftForce + dragForce);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, liftForce);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, dragForce);

        if (!part) return;
        var rb = GetComponentInParent<Rigidbody>();
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, rb.GetRelativePointVelocity(transform.localPosition) - rb.linearVelocity + (Vector3)(float3)vehicle.floatingTransform.Velocity);
        //Gizmos.color = Color.blue;
        //Gizmos.DrawRay(transform.position, rb.linearVelocity);
    }

    public float CalculateLift(float x)
    {
        if (math.abs(x) > stallAngle) return 0f;
        var root = math.sqrt(stallAngle);
        var scaledX = x / root;
        var num1 = root / root3;
        var num2 = stallAngle * num1 - num1 * num1 * num1;
        return (stallAngle * scaledX - scaledX * scaledX * scaledX)/ num2;
    }

    public const float root3 = 1.7320508075688772935274463415059f;

    public void DrawGraphics()
    {
        var angleOffset = controlResponse.x * vehicle.controller.pitch + controlResponse.y * vehicle.controller.yaw + controlResponse.z * vehicle.controller.roll;

        rotor.localEulerAngles = new Vector3(0, -angleOffset * controlAuthority, 0);
    }
    
}
