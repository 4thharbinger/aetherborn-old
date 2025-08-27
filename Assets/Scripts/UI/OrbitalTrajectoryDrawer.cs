using Unity.Mathematics;
using Unity.VisualScripting;

using UnityEngine;

public class OrbitalTrajectoryDrawer : MonoBehaviour
{
    private FloatingTransform floatingTransform;

    public int steps = 500;

    public double dt = 0.1;

    public float width;

    private Vector3[] positions;
    private Vector3[] velocities;

    private new LineRenderer renderer;

    private void Awake()
    {
        floatingTransform = GetComponentInParent<FloatingTransform>();
        renderer = GetComponent<LineRenderer>();
    }
    private void Update()
    {
        var position = floatingTransform.Position;
        var velocity = floatingTransform.Velocity;

        if (positions == null || positions.Length != steps)
        {
            positions = new Vector3[steps];
            velocities = new Vector3[steps];
        }

        for (var i = 0; i < steps; i++)
        {
            positions[i] = FloatingTransform.ToScenePosition(position) / SpaceViewManager.scaleFactor;
            velocities[i] = FloatingTransform.ToSceneVelocity(velocity);
            //position += velocity * dt;
            //velocity += CelestialBody.CalculateGravity(position) * dt;

            var k1v = GetAcceleration(position, velocity);
            var k1x = velocity;

            // k2
            var k2v = GetAcceleration(position + 0.5f * dt * k1x, velocity + 0.5f * dt * k1v);
            var k2x = velocity + 0.5f * dt * k1v;

            // k3
            var k3v = GetAcceleration(position + 0.5f * dt * k2x, velocity + 0.5f * dt * k2v);
            var k3x = velocity + 0.5f * dt * k2v;

            // k4
            var k4v = GetAcceleration(position + dt * k3x, velocity + dt * k3v);
            var k4x = velocity + dt * k3v;

            // Combine increments
            position += (dt / 6f) * (k1x + 2f * k2x + 2f * k3x + k4x);
            velocity += (dt / 6f) * (k1v + 2f * k2v + 2f * k3v + k4v);

            //if (i > 0)
            //{
            //    Gizmos.DrawLine(positions[i - 1], positions[i]);
            //    //Gizmos.DrawLine(positions[i], positions[i] + velocities[i]);
            //}
        }

        renderer.positionCount = steps;
        renderer.SetPositions(positions);
        renderer.startWidth = Vector3.Distance(Camera.main.transform.position, positions[0]) * width;
        renderer.endWidth = Vector3.Distance(Camera.main.transform.position, positions[0]) * width;
    }

    private double3 GetAcceleration(double3 position, double3 velocity) => CelestialBody.CalculateGravity(position);
}
