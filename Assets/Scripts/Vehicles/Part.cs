using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
/// <summary>
///     A part of a vehicle.
/// </summary>
[Serializable]
public class Part : MonoBehaviour
{
    public List<Module> modules = new();

    public Vehicle vehicle;

    public double baseMass;

    private readonly List<IMassModifier> massModifiers = new();
    private readonly List<IGraphicsModule> graphicsModules = new();

    [NonSerialized]
    private bool initialized = false;

    public double mass => baseMass + massModifiers.Sum(modifier => modifier.mass);

    public void Initialize()
    {
        if (initialized) return;

        foreach (var module in modules)
        {
            module.part = this;

            if (module is IGraphicsModule graphics)
                graphicsModules.Add(graphics);

            if (module is IMassModifier massModifier)
                massModifiers.Add(massModifier);
        }

        initialized = true;
    }

    public void Advance(double dt)
    {
        Initialize();
        foreach (var module in modules)
        {
            if (module is ITickedModule ticked)
                ticked.Advance(dt);
            if (module is IGraphicsModule graphics)
                graphics.DrawGraphics();
        }
    }

    public void DrawGraphics()
    {
        Initialize();
        foreach (var module in modules)
        {
            if (module is IGraphicsModule graphics)
                graphics.DrawGraphics();
        }
    }
}

/// <summary>
///     Module will be updated periodically.
/// </summary>
public interface ITickedModule : IModuleInterface
{
    /// <summary>
    ///     Advances the module forward in time by the specified amount.
    /// </summary>
    /// <param name="dt"></param>
    public void Advance(double dt);
}

/// <summary>
///     Module will be updated once per frame.
/// </summary>
public interface IGraphicsModule : IModuleInterface
{
    /// <summary>
    ///     Updates graphics for the <see langword="module"/>.
    /// </summary>
    public void DrawGraphics();
}