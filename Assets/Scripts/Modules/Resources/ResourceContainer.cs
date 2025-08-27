using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Mathematics;

using UnityEngine;

[Serializable]
public class ResourceContainer : Module, IMassModifier, ISerializationCallbackReceiver, IResourceContainer
{
    public struct ResourceSlot
    {
        public double mass;
        public double capacity;
    }

    private readonly Dictionary<ResourceType, ResourceSlot> resources = new();

    [Serializable]
    public struct SerializedResource
    {
        public ResourceType type;
        public double mass;
        public double capacity;
    }

    [SerializeField]
    private SerializedResource[] serializedResources;

    public FluidMode mode;

    public virtual double mass => resources.Sum(resource => resource.Value.mass);

    /// <summary>
    ///     Expands the container by the specified amount.
    /// </summary>
    /// <param name="resource"> Resource to expand. </param>
    public virtual void Expand(Resource resource)
    {
        if (resource.type == null || resource.type.mode != mode || resource.mass == 0)
        {
            return;
        }

        if (resources.TryGetValue(resource.type, out ResourceSlot slot))
        {
            slot.capacity += resource.mass;
            resources[resource.type] = slot;
        }
        else
        {
            resources.Add(resource.type, new ResourceSlot { capacity = resource.mass, mass = 0 });
        }
    }
    /// <summary>
    ///     Shrinks the container by the specified amount. Will also remove any extra resources.
    /// </summary>
    /// <param name="resource"> Resource to expand. </param>
    public virtual void Shrink(Resource resource)
    {
        if (resource.type == null || resource.type.mode != mode || resource.mass == 0)
        {
            return;
        }

        if (resources.TryGetValue(resource.type, out ResourceSlot slot))
        {
            slot.capacity -= resource.mass;

            if (slot.mass > slot.capacity)
                slot.mass = slot.capacity;

            if (slot.capacity <= 0f)
            {
                resources.Remove(resource.type);
            }
            else
            {
                resources[resource.type] = slot;
            }
        }
    }

    /// <summary>
    ///     Add a resource to the container.
    /// </summary>
    /// <param name="resource"> Resource to add. </param>
    /// <returns> Mass of resources not added. </returns>
    public virtual double Deposit(Resource resource)
    {
        if (resource.type == null || resource.type.mode != mode|| resource.mass == 0)
        {
            return resource.mass;
        }

        if (!resources.TryGetValue(resource.type, out ResourceSlot slot))
        {
            return resource.mass;
        }

        var added = math.min(resource.mass, slot.capacity - slot.mass);

        slot.mass += added;
        resources[resource.type] = slot;

        return added;
    }

    /// <summary>
    ///     Removes resources from the container.
    /// </summary>
    /// <param name="resource"> Resource to remove. </param>
    /// <returns> Amount of resources removed. </returns>
    public virtual double Withdraw(Resource resource)
    {
        if (resource.type == null || resource.type.mode != mode || resource.mass == 0)
        {
            return 0;
        }

        if (!resources.TryGetValue(resource.type, out ResourceSlot slot))
        {
            return 0;
        }

        var removed = math.min(resource.mass, slot.mass);

        slot.mass -= removed;
        resources[resource.type] = slot;

        return removed;
    }

    public virtual double GetAmount(ResourceType type) => resources.TryGetValue(type, out ResourceSlot slot) ? slot.mass : 0;
    public virtual double GetSpace(ResourceType type) => resources.TryGetValue(type, out ResourceSlot slot) ? slot.capacity - slot.mass : 0;
    public virtual double GetCapacity(ResourceType type) => resources.TryGetValue(type, out ResourceSlot slot) ? slot.capacity : 0;

    /// <summary>
    ///     Transfers resources between containers.
    /// </summary>
    /// <param name="other"></param>
    /// <param name="resource"></param>
    /// <returns> Amount of resources transferred. </returns>
    public virtual double Transfer(IResourceAcceptor other, Resource resource)
    {
        var amount = math.min(math.min(resources[resource.type].mass, other.GetSpace(resource.type)), resource.mass);

        resources[resource.type] = new ResourceSlot { capacity = resources[resource.type].capacity, mass = resources[resource.type].mass - amount };

        if (other is ResourceContainer container)
        {
            container.resources[resource.type] = new ResourceSlot { capacity = other.GetCapacity(resource.type), mass = container.resources[resource.type].mass + amount };
        } else
        {
            other.Deposit(new Resource(resource.type, amount));
        }

        return amount;
    }

    public void OnAfterDeserialize()
    {
        foreach (var sResource in serializedResources)
        {
            resources[sResource.type] = new ResourceSlot { capacity = sResource.capacity, mass = sResource.mass };
        }
    }

    public void OnBeforeSerialize()
    {
        serializedResources = resources.Select(resource => new SerializedResource { type = resource.Key, mass = resource.Value.mass, capacity = resource.Value.capacity }).ToArray();
    }

    public IEnumerable<Resource> GetResources() => resources.Select(resource => new Resource(resource.Key, resource.Value.mass));
}


/// <summary>
///     Supplies or accepts resources.
/// </summary>
public interface IResourceContainer : IResourceSupplier, IResourceAcceptor
{

}

/// <summary>
///     Resource buffer, can store an unlimited number of resources. Useful in intermediate calculations.
/// </summary>
public class ResourceBuffer : IResourceContainer
{
    private readonly Dictionary<ResourceType, double> resources = new();
    public double Deposit(Resource resource)
    {
        if (resource.type == null || resource.mass == 0)
            return 0;

        resources.Add(resource.type, resource.mass);
        return resource.mass;
    }

    /// <summary>
    ///     Deposits all resources from a supplier.
    /// </summary>
    /// <param name="supplier"></param>
    /// <returns></returns>
    public double Deposit(IResourceSupplier supplier)
    {
        foreach (var resource in supplier.GetResources())
        {
            Deposit(resource);
        }
        return 0;
    }

    /// <summary>
    ///     Deposits all resources from a supplier.
    /// </summary>
    /// <param name="supplier"></param>
    /// <returns></returns>
    public double DepositCapacities(IResourceContainer supplier)
    {
        foreach (var resource in supplier.GetResources())
        {
            Deposit(new Resource(resource.type, resource.mass + supplier.GetSpace(resource.type)));
        }
        return 0;
    }

    public double GetAmount(ResourceType type) => type == null ? 0 : resources.TryGetValue(type, out var mass) ? mass : 0;

    public double GetCapacity(ResourceType type) => type == null ? 0 : float.MaxValue;

    public IEnumerable<Resource> GetResources() => resources.Select(resource => new Resource(resource.Key, resource.Value));

    public double GetSpace(ResourceType type) => type == null ? 0 : float.MaxValue;

    public double Withdraw(Resource resource)
    {
        if (resource.type == null)
        {
            return 0;
        }

        if (resources.TryGetValue(resource.type, out var mass))
        {
            var amount = math.min(resource.mass, mass);
            resources[resource.type] = mass - amount;
            return amount;
        }
        return 0;
    }
}