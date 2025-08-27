using System.Collections.Generic;

using Unity.Mathematics;
/// <summary>
///     Accepts resources from a module.
/// </summary>
public interface IResourceAcceptor : IModuleInterface
{
    /// <summary>
    ///     Deposits resources into the acceptor.
    /// </summary>
    /// <param name="resource"> Resource to deposit. </param>
    /// <returns> Amount of resources accepted. </returns>
    public double Deposit(Resource resource);

    /// <summary>
    ///     Deposits multiple resources simultaneously into the acceptor, maintaining the ratios between resources.
    /// </summary>
    /// <param name="resources"> Resources to deposit. </param>
    /// <returns> Ratio of resources deposited. (1 = all, 0 = none) </returns>
    public double Deposit(IEnumerable<Resource> resources)
    {
        var space = GetSpace(resources);

        foreach (var resource in resources)
        {
            Deposit(resource * space);
        }

        return space;
    }

    /// <summary>
    ///     Returns the amount of space available for a resource type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public double GetSpace(ResourceType type);

    /// <summary>
    ///     Returns the amount of space available simultaneously for multiple resource types, maintaining the ratios between resources.
    /// </summary>
    /// <param name="type"></param>
    /// <returns> Ratio of space available to requested. (1 = all, 0 = none) </returns>
    public double GetSpace(IEnumerable<Resource> resources)
    {
        var ratio = 1.0;
        foreach (var resource in resources)
        {
            var available = GetSpace(resource.type);
            ratio = math.min(ratio, available / resource.mass);
        }
        return ratio;
    }

    /// <summary>
    ///     Returns the capacity of a resource type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public double GetCapacity(ResourceType type);
}