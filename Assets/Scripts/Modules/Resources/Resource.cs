using System;

[Serializable]
public struct Resource
{
    /// <summary>
    ///     The type of gas in the container.
    /// </summary>
    public ResourceType type;

    /// <summary>
    ///     The amount of gas in the container in kg.
    /// </summary>
    public double mass;

    public Resource(ResourceType type)
    {
        this.type = type;

        mass = 1f;
    }

    public Resource(ResourceType type, double mass)
    {
        this.type = type;
        this.mass = mass;
    }

    public static Resource operator *(Resource a, double b) => new(a.type, a.mass * b);

    public static Resource operator /(Resource a, double b) => new(a.type, a.mass / b);

    public static Resource operator +(Resource a, Resource b) => new(a.type, a.mass + b.mass);

    public static Resource operator -(Resource a, Resource b) => new(a.type, a.mass - b.mass);

    public static Resource operator +(Resource a, double b) => new(a.type, a.mass + b);

    public static Resource operator -(Resource a, double b) => new(a.type, a.mass - b);

    public static Resource operator +(double a, Resource b) => new(b.type, b.mass + a);

    public static Resource operator -(double a, Resource b) => new(b.type, b.mass - a);

}
