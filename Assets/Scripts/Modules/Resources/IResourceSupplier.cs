using System.Collections.Generic;
using System.Linq;

using Unity.Mathematics;
/// <summary>
///     Supplies resources to a module.
/// </summary>
public interface IResourceSupplier : IModuleInterface
{
    /// <summary>
    ///     Withdraws resources from the supplier.
    /// </summary>
    /// <param name="resource"> Resource to withdraw. </param>
    /// <returns> Amount of resources withdrawn. </returns>
    public double Withdraw(Resource resource);

    /// <summary>
    ///     Withdraws multiple resources simultaneously from the supplier, maintaining the ratios between resources.
    /// </summary>
    /// <param name="resources"> Resources to withdraw. </param>
    /// <returns> Ratio of resources withdrawn. (1 = all, 0 = none) </returns>
    public double Withdraw(IEnumerable<Resource> resources)
    {
        var ratio = GetAmount(resources);

        foreach (var resource in resources)
        {
            Withdraw(resource * ratio);
        }

        return ratio;
    }

    /// <summary>
    ///     Gets the amount of a resource in the supplier.
    /// </summary>
    /// <param name="type"></param>
    /// <returns> Amount of resources available. </returns>
    public double GetAmount(ResourceType type);

    /// <summary>
    ///     Simultaneously gets the amount of multiple resources in the supplier, maintaining the ratios between resources.
    /// </summary>
    /// <param name="types"></param>
    /// <returns> Ratio of resources available. (1 = all, 0 = none) </returns>
    public double GetAmount(IEnumerable<Resource> resources)
    {
        var ratio = 1.0;
        foreach (var resource in resources)
        {
            var available = GetAmount(resource.type);
            ratio = math.min(ratio, available / resource.mass);
        }
        return ratio;
    }

    /// <summary>
    ///     Simultaneously gets the amount of multiple resources in the supplier, maintaining the ratios between resources. Can be unbounded up to infinity rather than capped to 1.
    /// </summary>
    /// <param name="types"></param>
    /// <returns> Ratio of resources available. (1 = all, 0 = none) </returns>
    public double GetAmountAll(IEnumerable<Resource> resources)
    {
        var ratio = double.PositiveInfinity;
        foreach (var resource in resources)
        {
            var available = GetAmount(resource.type);
            ratio = math.min(ratio, available / resource.mass);
        }
        return ratio;
    }

    /// <summary>
    ///     Transfers resources from the supplier to an acceptor.
    /// </summary>
    /// <param name="resource"> Resource to transfer.  </param>
    /// <returns> Amount of resources transferred. </returns>
    public double Transfer(IResourceAcceptor target, Resource resource)
    {
        var amount = math.min(target.GetSpace(resource.type), math.min(GetAmount(resource.type), resource.mass));

        Withdraw(new Resource(resource.type, amount));
        target.Deposit(new Resource(resource.type, amount));

        return amount;
    }

    /// <summary>
    ///     Transfers multiple resources simultaneously from the supplier, maintaining the ratios between resources.
    /// </summary>
    /// <param name="resources"> Resource to transfer.  </param>
    /// <returns> Ratio of resources transferred. (1 = all, 0 = none) </returns>
    public double Transfer(IResourceAcceptor target, IEnumerable<Resource> resources)
    {
        var space = target.GetSpace(resources);
        var amount = Withdraw(resources.Select(resource => resource * space));
        target.Deposit(resources.Select(resource => resource * amount));

        return amount;
    }

    /// <summary>
    ///     Returns an enumerable of all resources in the supplier.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<Resource> GetResources();
}
