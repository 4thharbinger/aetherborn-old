using NUnit.Framework;

using System.Collections.Generic;
using System.Linq;

using Unity.Mathematics;

using UnityEngine;

public class CelestialBody : MonoBehaviour
{
    [Tooltip("Mass of the body in kilograms.")]
    public double mass;

    [Tooltip("Radius of the body in meters. Defines sea level.")]
    public double radius;

    [Tooltip("Density of the atmosphere in kg/m^3 at sea level.")]
    public double atmospherePressure;

    [Tooltip("Altitude of half atmospheric pressure.")]
    public double atmosphereFalloff;

    [Tooltip("Composition of the atmosphere in mass fractions.")]
    public Resource[] atmosphereComposition;

    [Tooltip("Temperature of the surface in Kelvin.")]
    public double surfaceTemperature = 293.15;

    [Tooltip("Molar mass of the atmosphere in kilograms per mole. Calculated automatically from the atmosphere composition.")]
    public double molarMass = 28.9;

    public FloatingTransform floatingTransform;

    public static List<CelestialBody> bodies = new();

    public const double G = 6.67408e-11;

    private void OnValidate()
    {
        var totalMass = atmosphereComposition.Sum(x => x.mass);

        if (totalMass > 0)
        {
            for (var i = 0; i < atmosphereComposition.Length; i++)
            {
                atmosphereComposition[i].mass /= totalMass;
            }
            molarMass = atmosphereComposition.Sum(x => x.type ? x.mass * x.type.molarMass : 0);
        }
    }

    private void Awake()
    {
        bodies.Add(this);

        floatingTransform = GetComponent<FloatingTransform>();
    }

    private void OnDestroy()
    {
        bodies.Remove(this);
    }

    /// <summary>
    ///     Calculates the gravity of all bodies at a given position.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public static double3 CalculateGravity(double3 position)
    {
        var gravity = new double3(0, 0, 0);

        foreach (var body in bodies)
        {
            var dir = body.floatingTransform.GetWorldPosition() - position;
            var distance = math.length(dir);

            dir /= distance;

            gravity += G * body.mass * dir / (distance * distance);
        }

        return gravity;
    }

    /// <summary>
    ///     Calculates the gravitational potential energy of all bodies at a given position.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public static CelestialBody GetNearest(double3 double3)
    {
        var closest = bodies[0];
        var closestDistance = math.length(bodies[0].floatingTransform.GetWorldPosition() - double3);

        foreach (var body in bodies)
        {
            var distance = math.length(body.floatingTransform.GetWorldPosition() - double3);

            if (distance < closestDistance)
            {
                closest = body;
                closestDistance = distance;
            }
        }

        return closest;
    }

    /// <summary>
    ///     Calculates the pressure of all atmospheres at a given position.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public static double CalculatePressure(double3 position)
    {
        var atmosphere = 0d;

        foreach (var body in bodies)
        {
            var dir = body.floatingTransform.GetWorldPosition() - position;
            var distance = math.length(dir);
            var altitude = distance - body.radius;

            atmosphere += body.atmospherePressure * math.pow(0.5, altitude / body.atmosphereFalloff);
        }

        return atmosphere;
    }
    /// <summary>
    ///     Calculates the density of all atmospheres at a given position.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public static double CalculateDensity(double3 position)
    {
        var atmosphere = 0d;

        foreach (var body in bodies)
        {
            var dir = body.floatingTransform.GetWorldPosition() - position;
            var distance = math.length(dir);
            var altitude = distance - body.radius;

            atmosphere += PressureToDensity(body.atmospherePressure, body.surfaceTemperature, body.molarMass) * math.pow(0.5, altitude / body.atmosphereFalloff);
        }

        return atmosphere;
    }

    public const double gasConstant = 8.31446261815324;
    
    
    /// <summary>
    ///     Converts pressure, temperature and molar mass to density. p / R * M / T= rho
    /// </summary>
    /// <param name="pressure"></param>
    /// <param name="temperature"></param>
    /// <param name="molarMass"></param>
    /// <returns></returns>
    public static double PressureToDensity(double pressure, double temperature, double molarMass) => pressure * molarMass / (gasConstant * temperature);
}
