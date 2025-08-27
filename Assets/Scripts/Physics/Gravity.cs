using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Gravity : MonoBehaviour
{
    private FloatingTransform floatingTransform;

    public double currentGravity;

    public void Awake()
    {
        floatingTransform = GetComponent<FloatingTransform>();
    }

    private void FixedUpdate()
    {
        var gravity = CelestialBody.CalculateGravity(floatingTransform.Position);
        floatingTransform.Velocity += gravity * Time.fixedDeltaTime;

        currentGravity = math.length(gravity);
    }
}
