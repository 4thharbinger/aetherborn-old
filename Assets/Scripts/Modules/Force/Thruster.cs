using Mono.Cecil;

using System;
using System.Linq;

using Unity.Mathematics;

using UnityEngine;

public class Thruster : Module, IActivityProvider, ITickedModule, IGraphicsModule
{
    public SerializedIModule<IActivityProvider> supply;

    public double thrust = 1.0;
    public double throttle = 1.0;

    public Vector3 thrustVector = Vector3.up;

    public ThrusterFX fx;

    public ParticleSystem particles;

    public float exhaustPressure = 101325f;

    private double previousActivity;
    private double previousDt;

    public double exhaustVelocity => supply.Get() is ResourceConverter converter ? thrust / converter.inputs.Sum(resource => resource.mass): 0f;
    public double currentThrust => thrust * throttle * previousActivity;

    public void Advance(double dt)
    {
        if (supply.Get() != null)
        {
            var work = supply.Get().ProvideActivity(dt);

            previousDt = dt;
            previousActivity = work;

            Thrust(work * dt);
        } else
        {
            Thrust(dt);
        }
    }

    public void Thrust(double dt)
    {
        vehicle.AddForce(transform.localPosition, transform.TransformDirection((float)dt * (float)thrust * thrustVector));
    }

    private void OnDrawGizmos()
    {
        var thrustDirection = transform.TransformDirection(thrustVector);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, thrustDirection);
    }

    public double ProvideActivity(double dt) => throttle;

    public void DrawGraphics()
    {
        if (particles)
        {
            var emission = particles.emission;
            emission.rateOverTimeMultiplier = (float)previousActivity;
            emission.rateOverDistanceMultiplier = (float)previousActivity;
        }
        if (fx)
        {
            fx.throttle = (float)throttle;
            fx.vacuum = 1 - (float)Math.Tanh(CelestialBody.CalculatePressure(vehicle.floatingTransform.Position) / exhaustPressure);
            fx.Draw();
        }
    }
}

/// <summary>
///     Activity is a measure of how much work a module did. Modules can request activity from sources, which performs the activity on the spot. Sources are "active", work will be done when the requesting module requests for activity. 
/// </summary>
public interface IActivitySource
{
    /// <summary>
    ///     Asks the source to perform work.
    /// </summary>
    /// <returns> Amount of work done. </returns>
    public double RequestActivity(double dt);

    public static double ConvertActivity(double oldDt, double newDt, double activity) => activity * oldDt / newDt;
}
