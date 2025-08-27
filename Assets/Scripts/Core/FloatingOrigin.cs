using Unity.Mathematics;

using UnityEngine;

[Tooltip("The center point of the double-based world space. All FloatingTransforms are relative to this point.")]
public class FloatingOrigin : FloatingTransform
{
    public static FloatingOrigin origin;

    public float resetRange = 65536f;

    public bool originReset;

    private void Awake()
    {
        if (origin != null && origin != this)
        {
            Debug.LogWarning("There can only be one floating origin in the scene.");
        }
        origin = this;
        rb = GetComponent<Rigidbody>();
    }

    private new void FixedUpdate()
    {
        base.FixedUpdate();

        originReset = false;

        if (transform.position.sqrMagnitude > resetRange * resetRange)
        {
            positionOrigin = Position;
            transform.position = Vector3.zero;
            oldScenePosition = Vector3.zero;

            velocityOrigin = Velocity;
            originReset = true;
        }
    }

    public double3 positionOrigin;
    public double3 velocityOrigin;

    public static Vector3 ToVector3(double3 vector) => new((float)vector.x, (float)vector.y, (float)vector.z);
}
