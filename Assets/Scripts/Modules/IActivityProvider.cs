
//public interface IActivitySupplier : IActivityProvider
//{
//    /// <summary>
//    ///     Supplies work to another module.
//    /// </summary>
//    /// <param name="dt"> Time step. </param>
//    /// <returns> Ratio of work done. (1 = all, 0 = none) </returns>
//    public double RequestActivity(double dt);

//    double IActivityProvider.ProvideActivity(double dt) => RequestActivity(dt);
//}


/// <summary>
///     Activity is a measure of how much work a module did. Activity providers perform some activity every tick, which can be used by other modules. Providers are "passive", only reading from the work done. Compare with sources, which only work when something is pinging it for activity.
/// </summary>
public interface IActivityProvider : IModuleInterface
{
    /// <summary>
    ///     Amount of work done last tick.
    /// </summary>
    /// <returns> Amount of work done last tick. </returns>
    public double ProvideActivity(double dt);

    public static double ConvertActivity(double oldDt, double newDt, double activity) => activity * oldDt / newDt;
}
