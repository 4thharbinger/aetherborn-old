using System;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.InputSystem;

public class ControlSource : MonoBehaviour
{
    public VehicleController controller;
    public float targetPitch;
    public float targetYaw;
    public float targetRoll;

    public float pitchVelocity;
    public float yawVelocity;
    public float rollVelocity;

    public SASMode sasMode;

    public float sasResponse = -0.5f;

    public PIDControllerAngle pitch;
    public PIDControllerAngle yaw;
    public PIDControllerAngle roll;

    public Vector3 target;

    public float smoothTime = 0.1f;

    private void Awake()
    {
        controller = GetComponent<VehicleController>();
    }

    private void OnEnable()
    {
        InputSystem.actions.FindAction("Throttle Up").performed += OnThrottleUp;
        InputSystem.actions.FindAction("Throttle Down").performed += OnThrottleDown;
    }

    private void OnDisable()
    {
InputSystem.actions.FindAction("Throttle Up").performed -= OnThrottleUp;
        InputSystem.actions.FindAction("Throttle Down").performed -= OnThrottleDown;
    }

    private void OnThrottleDown(InputAction.CallbackContext context)
    {
        controller.throttle = 0f;
    }

    private void OnThrottleUp(InputAction.CallbackContext obj)
    {
        controller.throttle = 1f;
    }

    private void Update()
    {
        if(Time.timeScale <= 0.00001f)
        {
            return;
        }
        controller.pitch = Mathf.SmoothDampAngle(controller.pitch, targetPitch, ref pitchVelocity, smoothTime);
        controller.yaw = Mathf.SmoothDampAngle(controller.yaw, targetYaw, ref yawVelocity, smoothTime);
        controller.roll = Mathf.SmoothDampAngle(controller.roll, targetRoll, ref rollVelocity, smoothTime);

        targetPitch = InputSystem.actions.FindAction("Pitch").ReadValue<float>();
        targetYaw = InputSystem.actions.FindAction("Yaw").ReadValue<float>();
        targetRoll = InputSystem.actions.FindAction("Roll").ReadValue<float>();

        switch (sasMode)
        {
            case SASMode.Off: break;
            case SASMode.Stabilize:
                var rb = controller.vehicle.floatingTransform.rigidbody;
                var angularVelocities = rb.transform.InverseTransformDirection(rb.angularVelocity);

                targetPitch = BlendSAS(targetPitch, Mathf.Clamp(angularVelocities.x, -1f, 1f) * -sasResponse);
                targetYaw = BlendSAS(targetYaw, Mathf.Clamp(angularVelocities.y, -1f, 1f) * sasResponse);
                targetRoll = BlendSAS(targetRoll, Mathf.Clamp(angularVelocities.z, -1f, 1f) * -sasResponse);
                break;
            case SASMode.Prograde:
                var velocity = (float3)controller.vehicle.floatingTransform.Velocity;
                SetTarget(velocity);
                break;
            case SASMode.Retrograde:
                velocity = (float3)controller.vehicle.floatingTransform.Velocity;
                SetTarget(-velocity);
                break;
        }
    }

    private void SetTarget(Vector3 vector)
    {
        var pyr = GetPitchYawRoll(transform.rotation);
        target = GetPitchYawRoll(Quaternion.LookRotation(vector));
        var rb = controller.vehicle.floatingTransform.rigidbody;
        var angularVelocities = rb.transform.InverseTransformDirection(rb.angularVelocity);

        BlendSAS(ref targetPitch, ref targetYaw, ref targetRoll,
            Mathf.Clamp(pitch.Evaluate(pyr.x, target.x, angularVelocities.x, Time.deltaTime), -1f, 1f),
            Mathf.Clamp(yaw.Evaluate(pyr.y, target.y, angularVelocities.y, Time.deltaTime), -1f, 1f),
            Mathf.Clamp(roll.Evaluate(pyr.z, target.z, angularVelocities.z, Time.deltaTime), -1f, 1f));
    }

    public void SetSASMode (SASMode mode) => sasMode = mode;

    private float BlendSAS(float player, float sas)
    {
        return Mathf.Lerp(sas, player, Mathf.Abs(player));
    }
    private void BlendSAS(ref float pX, ref float pY, ref float pZ, float x, float y, float z)
    {
        pX = BlendSAS(pX, x);
        pY = BlendSAS(pY, y);
        pZ = BlendSAS(pZ, z);
    }
    public static Vector3 GetPitchYawRoll(Quaternion q)
    {
        // Convert quaternion to yaw-pitch-roll (Y-X-Z order)
        // Reference: aerospace convention
        float sinr_cosp = 2f * (q.w * q.x + q.y * q.z);
        float cosr_cosp = 1f - 2f * (q.x * q.x + q.y * q.y);
        float pitch = Mathf.Atan2(sinr_cosp, cosr_cosp); // X axis

        float sinp = 2f * (q.w * q.y - q.z * q.x);
        float yaw;
        if (Mathf.Abs(sinp) >= 1f)
            yaw = Mathf.PI / 2f * Mathf.Sign(sinp); // use 90° if out of range
        else
            yaw = Mathf.Asin(sinp); // Y axis

        float siny_cosp = 2f * (q.w * q.z + q.x * q.y);
        float cosy_cosp = 1f - 2f * (q.y * q.y + q.z * q.z);
        float roll = Mathf.Atan2(siny_cosp, cosy_cosp); // Z axis

        // Convert to degrees
        return new Vector3(
            pitch * Mathf.Rad2Deg,
            yaw * Mathf.Rad2Deg,
            roll * Mathf.Rad2Deg
        );
    }
}

[Serializable]
public class PIDControllerAngle
{
    public float kP;
    public float kI;
    public float kD;

    public float integralError;

    public float Evaluate(float current, float target, float velocity, float dt)
    {
        var error = Mathf.DeltaAngle(current, target);
        integralError += error * dt;

        return kP * error + kI * integralError + kD * velocity;
    }
}

[Serializable]
public class PIDController
{
    public float kP;
    public float kI;
    public float kD;

    public float integralError;
    public float lastError;

    public float Evaluate(float current, float target, float dt)
    {
        var error = target - current;
        integralError += error * dt;
        var derivative = (error - lastError) / dt;
        lastError = error;

        return kP * error + kI * integralError + kD * derivative;
    }
}

public enum SASMode
{
    Stabilize,
    Off,
    Prograde,
    Retrograde
}