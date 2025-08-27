using Unity.Mathematics;

using UnityEngine;

public class Accelerator : MonoBehaviour
{
    public double3 velocity;
    public double3 acceleration;

    private FloatingTransform fTransform;

    private void Awake()
    {
fTransform = GetComponent<FloatingTransform>();
    }

    private void FixedUpdate()
    {
        fTransform.Velocity += acceleration * Time.fixedDeltaTime;
    }
}
