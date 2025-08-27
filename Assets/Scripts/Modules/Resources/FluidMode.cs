using UnityEngine;

public enum FluidMode {
    [Tooltip("Gases can be pressurized to an arbitrary pressure provided the container can withstand it. Gases do not require ullage.")] Gas,
    [Tooltip("Liquids are pressurized to a fixed pressure. Liquids require ullage to flow out of the container.")] Liquid,
    [Tooltip("Electricity can be stored in the container and can be used to power the vehicle. It does not require ullage to flow out of the container.")]
    Electric
}