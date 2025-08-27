using System;

using Unity.Mathematics;

using UnityEditor.Rendering;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Internal;
using UnityEngine.Rendering;

public class OrbitalCameraController : MonoBehaviour
{
    [Header("Controls")]
    public double zoomSpeed = 1f;
    public double pitchSpeed;
    public double yawSpeed;

    public double zoomMin;
    public double dampTime = 0.2;

    [Header("Runtime")]
    public double3 origin;

    public FloatingTransform target;

    public double3 up;
    public double zoom = 1f;
    public double targetZoom = 1f;
    public double zoomVelocity;

    public double pitch;
    public double yaw;
    public double pitchVelocity;
    public double yawVelocity;

    public double targetPitch;
    public double targetYaw;

    private FloatingTransform floatingTransform;

    private void Awake()
    {
        floatingTransform = GetComponent<FloatingTransform>();

        RenderPipelineManager.beginCameraRendering += BeginRender;
    }

    private void OnDestroy()
    {
        RenderPipelineManager.beginCameraRendering -= BeginRender;
    }

    void BeginRender(ScriptableRenderContext context, Camera camera)
    {
        if (target)
        {
            origin = target.GetWorldPosition();
        }
        var direction = new double3(math.cos(yaw) * math.cos(pitch), math.sin(pitch), math.sin(yaw) * math.cos(pitch));

        var distance = zoom + zoomMin;

        floatingTransform.SetWorldPosition(origin + (double3)(float3)direction * distance);
        floatingTransform.LookAt(origin);
    }
    private void Update()
    {

        targetZoom *= math.exp(zoomSpeed * InputSystem.actions.FindAction("Zoom").ReadValue<float>());

        if (Input.GetMouseButton(0))
        {
            var input = InputSystem.actions.FindAction("Orbit").ReadValue<Vector2>();

            targetPitch += pitchSpeed * input.y;
            targetYaw += yawSpeed * input.x;
        }

        yaw = SmoothDampAngle(yaw, targetYaw, ref yawVelocity, dampTime, double.PositiveInfinity, Time.unscaledDeltaTime);
        pitch = SmoothDampAngle(pitch, targetPitch, ref pitchVelocity, dampTime, double.PositiveInfinity, Time.unscaledDeltaTime);
        zoom = SmoothDamp(zoom, targetZoom, ref zoomVelocity, dampTime, double.PositiveInfinity, Time.unscaledDeltaTime);
    }

    [ExcludeFromDocs]
    public static double SmoothDamp(double current, double target, ref double currentVelocity, double smoothTime, double maxSpeed)
    {
        double deltaTime = Time.deltaTime;
        return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
    }

    [ExcludeFromDocs]
    public static double SmoothDamp(double current, double target, ref double currentVelocity, double smoothTime)
    {
        double deltaTime = Time.deltaTime;
        double maxSpeed = double.PositiveInfinity;
        return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
    }

    public static double SmoothDamp(double current, double target, ref double currentVelocity, double smoothTime, [DefaultValue("Mathf.Infinity")] double maxSpeed, [DefaultValue("Time.deltaTime")] double deltaTime)
    {
        smoothTime = math.max(0.0001f, smoothTime);
        double num = 2f / smoothTime;
        double num2 = num * deltaTime;
        double num3 = 1f / (1f + num2 + 0.48f * num2 * num2 + 0.235f * num2 * num2 * num2);
        double value = current - target;
        double num4 = target;
        double num5 = maxSpeed * smoothTime;
        value = math.clamp(value, 0f - num5, num5);
        target = current - value;
        double num6 = (currentVelocity + num * value) * deltaTime;
        currentVelocity = (currentVelocity - num * num6) * num3;
        double num7 = target + (value + num6) * num3;
        if (num4 - current > 0f == num7 > num4)
        {
            num7 = num4;
            currentVelocity = (num7 - num4) / deltaTime;
        }

        return num7;
    }

    [ExcludeFromDocs]
    public static double SmoothDampAngle(double current, double target, ref double currentVelocity, double smoothTime, double maxSpeed)
    {
        double deltaTime = Time.deltaTime;
        return SmoothDampAngle(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
    }

    [ExcludeFromDocs]
    public static double SmoothDampAngle(double current, double target, ref double currentVelocity, double smoothTime)
    {
        double deltaTime = Time.deltaTime;
        double maxSpeed = double.PositiveInfinity;
        return SmoothDampAngle(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
    }

    public static double SmoothDampAngle(double current, double target, ref double currentVelocity, double smoothTime, [DefaultValue("Mathf.Infinity")] double maxSpeed, [DefaultValue("Time.deltaTime")] double deltaTime)
    {
        target = current + DeltaAngle(current, target);
        return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
    }
    public static double DeltaAngle(double current, double target)
    {
        double num = Repeat(target - current, 360f);
        if (num > 180f)
        {
            num -= 360f;
        }

        return num;
    }
    //
    // Summary:
    //     Loops the value t, so that it is never larger than length and never smaller than
    //     0.
    //
    // Parameters:
    //   t:
    //
    //   length:
    public static double Repeat(double t, double length)
    {
        return math.clamp(t - math.floor(t / length) * length, 0f, length);
    }

}
