using System;
using System.Linq;

using UnityEngine;

public class ResourceConverter : Module, ITickedModule, IActivityProvider
{
    public Resource[] inputs;
    public Resource[] outputs;

    public SerializedIModule<IActivityProvider> activityProvider;

    public SerializedIModule<IResourceAcceptor> output;
    public SerializedIModule<IResourceSupplier> input;

    private double activity;
    private double dt;

    public void Advance(double dt)
    {
        if (activityProvider.Get() != null)
        {
            dt *= activityProvider.Get().ProvideActivity(dt);
        }

        var space = output.Get() == null ? (outputs.Length == 0 ? 1.0 : 0.0) : output.Get().GetSpace(outputs.Select(resource => resource * dt));
        var amount = input.Get() == null ? (inputs.Length == 0 ? 1.0 : 0.0) : input.Get().Withdraw(inputs.Select(resource => resource * dt * space));

        output.Get()?.Deposit(outputs.Select(resource => resource * dt * space * amount));

        activity = amount * space;
        this.dt = dt;
    }

    public double ProvideActivity(double dt) => IActivityProvider.ConvertActivity(this.dt, dt, activity);
}

[Serializable]
public class SerializedIModule
{
    [SerializeField]
    public Module module;
}

[Serializable]
public class SerializedIModule<TModuleInterface> : SerializedIModule where TModuleInterface : IModuleInterface
{
    public static implicit operator Module(SerializedIModule<TModuleInterface> module) => module.module;

    public TModuleInterface Get() => (TModuleInterface)(object)module;
    public void Set(TModuleInterface value) => module = (Module)(object)value;
}

/// <summary>
///     Interfaces deriving from this can be assigned in the editor.
/// </summary>
public interface IModuleInterface
{

}