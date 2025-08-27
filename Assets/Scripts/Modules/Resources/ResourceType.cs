using UnityEngine;

/// <summary>
///     Gases can be pressurized to an arbitrary pressure provided the container can withstand it. Gases do not require ullage.
/// </summary>
[CreateAssetMenu(fileName = "New Resource", menuName = "Resource", order = 0)]
public class ResourceType : ScriptableObject
{
    public double molarMass;
    public double criticalPressure;
    public double criticalTemperature;
    public double density;
    public double adiabaticIndex;

    public FluidMode mode;
}
