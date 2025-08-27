using UnityEngine;

public class VehicleController : MonoBehaviour
{
    public float throttle;

    public float pitch;
    public float yaw;
    public float roll;

    public Vehicle vehicle;
    private void Awake()
    {
        vehicle = GetComponent<Vehicle>();
    }
    private void FixedUpdate()
    {
        foreach (var thruster in
        vehicle.GetModules<Thruster>())
        {
            thruster.throttle = throttle;
        }
        foreach (var control in vehicle.GetModulesByInterface<IControlModule>())
        {
            control.pitch = pitch;
            control.yaw = yaw;
            control.roll = roll;
        }
    }
}
