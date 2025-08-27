using System.Linq;

using TMPro;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class DebugVehicleUI : MonoBehaviour
{
    public VehicleController vehicleController;

    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        var str = "";
        var mass = vehicleController.vehicle.floatingTransform.rigidbody.mass;

        str += "Throttle: " + vehicleController.throttle * 100 + "%\n";
        str += "Mass: " + mass + " kg\n";

        var thrusters = vehicleController.vehicle.GetModulesByInterface<Thruster>();
        var thrust = 0.0;
        var fuelMass = 0.0;
        var burnTime = 0.0;
        var speed = math.length(vehicleController.vehicle.floatingTransform.Velocity);

        foreach (var thruster in thrusters)
        {
            thrust += thruster.currentThrust;

            if (thruster.supply.Get() is ResourceConverter converter) {
                var time = converter.input.Get().GetAmountAll(converter.inputs);
                fuelMass += time * converter.inputs.Sum(resource => resource.mass);
                burnTime += time;
            }
        }

        var exhaustV = thrust / fuelMass * burnTime;
        var deltaV = math.log(mass / (mass - fuelMass)) * exhaustV;

        str += "Thrust: " + thrust + " N\n";
        str += "Delta V: " + math.round(deltaV) + " m/s\n";
        str += "Exhaust V: " + math.round(exhaustV) + " m/s\n";
        str += "Resources: ";

        var suppliers = vehicleController.vehicle.GetModulesByInterface<IResourceSupplier>();

        if (suppliers.Count() == 0) str += "None\n";
        else
        {
            var contents = new ResourceBuffer();
            var capacities = new ResourceBuffer();
            foreach (var resource in suppliers)
            {
                contents.Deposit(resource);

                if (resource is IResourceContainer container)
                    capacities.DepositCapacities(container);
            }

            foreach (var resource in contents.GetResources())
            {
                str += "\n\t" + resource.type + ": " + math.round(resource.mass * 100d) / 100d + (capacities.GetAmount(resource.type) == 0 ? "" : " / " + (math.round(capacities.GetAmount(resource.type)* 100d) / 100d)) + " kg";
            }
        }

        str += "Orbit:\n";

        var closest = CelestialBody.GetNearest(vehicleController.vehicle.floatingTransform.GetWorldPosition());

        var orbit = Orbit.FromPositionVelocity(closest.floatingTransform.GetWorldPosition() - vehicleController.vehicle.floatingTransform.GetWorldPosition(), vehicleController.vehicle.floatingTransform.Velocity, closest.mass);

        str += "Perigee: " + math.round((orbit.perigee - closest.radius) * 100d) / 100000d + " km\n";
        str += "Apogee: " + math.round((orbit.apogee - closest.radius) * 100d) / 100000d + " km\n";

        text.text = str;
    }
}

public class Orbit
{
    public double apogee;
    public double perigee;

    public double ecentricity => (apogee - perigee) / (apogee + perigee);

    public Orbit(double apogee, double perigee)
    {
        this.apogee = apogee;
        this.perigee = perigee;
    }

    public static Orbit FromPositionVelocity(double3 position, double3 velocity, double mass)
    {
        double mu = CelestialBody.G * mass;

        double r = math.length(position);
        double v = math.length(velocity);

        // Specific energy
        double energy = 0.5 * v * v - mu / r;

        // Semi-major axis
        double a = -mu / (2.0 * energy);

        // Specific angular momentum
        double3 h = math.cross(position, velocity);

        // Eccentricity vector
        double3 eVec = (math.cross(velocity, h) / mu) - (position / r);
        double e = math.length(eVec);

        // Periapsis and apoapsis
        double rp = a * (1 - e);
        double ra = a * (1 + e);

        return new Orbit(ra, rp);
    }
}