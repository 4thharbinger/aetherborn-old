using System.Collections.Generic;

using Unity.Mathematics;

using UnityEngine;

[Tooltip("Represents a double-based world space for transforms.")]
public class FloatingTransform : MonoBehaviour
{
    [SerializeField]
    protected double3 position;
    [SerializeField]
    protected double3 velocity;

    protected Vector3 oldScenePosition;
    protected Vector3 oldSceneVelocity;
    
    protected Rigidbody rb;

    public new Rigidbody rigidbody => rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    protected void FixedUpdate()
    {
        if (transform.position.sqrMagnitude < 1e10)
        {
            position += (float3)(transform.position - oldScenePosition);
        }

        if (rb && Physics.simulationMode == SimulationMode.FixedUpdate) 
        {
            velocity += (float3)(rb.linearVelocity - oldSceneVelocity);
            rb.linearVelocity= GetSceneVelocity();

            position += FloatingOrigin.origin.velocityOrigin * Time.fixedDeltaTime;

            oldSceneVelocity = rb.linearVelocity;
        } else
        {
            position += velocity * Time.fixedDeltaTime;
        }
        transform.position = GetScenePosition();

        oldScenePosition = transform.position;
    }

    public static List<FloatingTransform> transforms = new();

    public Vector3 GetScenePosition() { return FloatingOrigin.ToVector3(position - FloatingOrigin.origin.positionOrigin); }
    public double3 GetWorldPosition() { return position + (float3)(transform.position - oldScenePosition); }

    public static Vector3 ToScenePosition(double3 position) { return FloatingOrigin.ToVector3(position - FloatingOrigin.origin.positionOrigin); }
    public static double3 ToWorldPosition(Vector3 position) { return (float3)position + FloatingOrigin.origin.positionOrigin; }
    public static Vector3 ToSceneVelocity(double3 velocity) { return FloatingOrigin.ToVector3(velocity - FloatingOrigin.origin.velocityOrigin); }
    public static double3 ToWorldVelocity(Vector3 velocity) { return (float3)velocity + FloatingOrigin.origin.velocityOrigin; }

    public void SetScenePosition(Vector3 position) { transform.position = position; oldScenePosition = position; }
    public void SetWorldPosition(double3 position) { this.position = position; transform.position = GetScenePosition(); }

    public void LookAt(double3 position) { transform.rotation = Quaternion.LookRotation(FloatingOrigin.ToVector3(position - this.position)); }

    public virtual double3 Position
    {
        get => position; set
        {
            if (math.all(math.isnan(value)))
            {
                Debug.LogError("Invalid position assign attempt " + value);
            }
            else
            {
                position = value;
            }
        }
    }
    public virtual double3 Velocity
    {
        get => velocity; set
        {
            if (math.all(math.isnan(value)))
            {
                Debug.LogError("Invalid velocity assign attempt " + value);
            }
            else
            {
                velocity = value;
            }
        }
    }

    public virtual double mass { get => rb ? rb.mass : 1; set => rb.mass = (float) value; }

    public Vector3 GetSceneVelocity() => FloatingOrigin.ToVector3(velocity - FloatingOrigin.origin.velocityOrigin);
    public double3 GetWorldVelocity() => velocity;

    public void SetSceneVelocity(Vector3 velocity) { rb.linearVelocity = velocity; this.velocity = (float3)velocity + FloatingOrigin.origin.velocityOrigin; }

    public void SetWorldVelocity(double3 velocity) {this.velocity = velocity; rb.linearVelocity = GetSceneVelocity();}


}
