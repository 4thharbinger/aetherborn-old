
using Mono.Cecil.Cil;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Mathematics;
using Unity.VisualScripting;

using UnityEngine;

/// <summary>
///     A collection of parts and linkages that behave together under physics.
/// </summary>
[Serializable]
public class Vehicle : MonoBehaviour
{
    public VehicleController controller;
    public List<Part> parts;

    private readonly MultiDictionary<Type, Module> modulesByType = new();

    public FloatingTransform floatingTransform;

    private bool initialized;

    public IEnumerable<T> GetModules<T>() where T : Module => modulesByType.TryGetValue(typeof(T), out var modules) ? modules.Cast<T>() : Enumerable.Empty<T>();

    public IEnumerable<T> GetModulesByInterface<T>() where T : IModuleInterface => modulesByType.TryGetValue(typeof(T), out var modules) ? modules.Cast<T>() : Enumerable.Empty<T>();

    public void Advance(double dt)
    {
        Initialize();
        var mass = 0.0;
        foreach (var part in parts)
        {
            part.Advance(dt);
            mass += part.mass;
        }
        floatingTransform.mass = mass;
    }

    public void Initialize()
    {
        if (initialized)
            return;

        modulesByType.Clear();

        var mass = 0.0;
        foreach (var part in parts)
        {
            part.vehicle = this;
            part.Initialize();
            mass += part.mass;

            foreach (var module in part.modules)
            {
                modulesByType.Add(module.GetType(), module);

                foreach (var @interface in module.GetType().GetInterfaces())
                {
                    modulesByType.Add(@interface, module);
                }
            }
        }

        floatingTransform.mass = mass;
        initialized = true;
    }

    private void Awake()
    {
        floatingTransform =     GetComponent<FloatingTransform>();
        controller = GetComponent<VehicleController>();
    }
    public void AddForce(Vector3 localPosition, Vector3 worldForce)
    {
        if (Physics.simulationMode == SimulationMode.FixedUpdate)
        {
            var forcePoint = floatingTransform.rigidbody.position + transform.TransformDirection(localPosition);

            floatingTransform.rigidbody.AddForceAtPosition(worldForce, forcePoint, ForceMode.Impulse);
        } else
        {
            floatingTransform.Velocity += (float3)worldForce / floatingTransform.rigidbody.mass;
        }
    }

    private void FixedUpdate()
    {
Advance(Time.fixedDeltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(GetComponent<Rigidbody>().centerOfMass + transform.position, 0.1f);
    }
}

public class MultiDictionary<TKey, TValue> : Dictionary<TKey, List<TValue>>
{
    public void Add(TKey key, TValue value)
    {
        if (TryGetValue(key, out var values))
        {
            values.Add(value);
        }
        else
        {
            Add(key, new List<TValue> { value });
        }
    }

    public void Remove(TKey key, TValue value)
    {
        if (TryGetValue(key, out var values))
        {
            values.Remove(value);

            if (values.Count == 0)
            {
                Remove(key);
            }
        }
    }
}