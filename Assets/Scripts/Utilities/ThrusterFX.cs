using UnityEngine;

public class ThrusterFX : MonoBehaviour
{
    private SkinnedMeshRenderer renderer;

    [Range(0, 1)]
    public float throttle;

    [Range(0, 1)]
    public float vacuum;

    private Material materialShockwave;
    private Material materialCones;
    private Material materialCore;

    [GradientUsage(true)]
    public Gradient startColor;
    [GradientUsage(true)]
    public Gradient endColor;

    public AnimationCurve shockwaveLength;
    public AnimationCurve coneLength;
    public AnimationCurve coreLength;

    private void Awake()
    {
        renderer = GetComponent<SkinnedMeshRenderer>();

        materialShockwave = renderer.materials[0];
        materialCones = renderer.materials[1];
        materialCore = renderer.materials[2];
    }

    private void OnValidate()
    {
        Awake();
        Draw();
    }

    void Draw()
    {
        renderer.enabled = throttle > 0.01f;
        renderer.SetBlendShapeWeight(0, vacuum * 100f);
        renderer.SetBlendShapeWeight(1, (1-throttle) * 100f);

        materialShockwave.SetFloat("_Length", shockwaveLength.Evaluate(throttle));
        materialCones.SetFloat("_Length", coneLength.Evaluate(throttle));
        materialCore.SetFloat("_Length", coreLength.Evaluate(throttle));

        materialShockwave.SetVector("_Start_Color", startColor.Evaluate(throttle));
        materialShockwave.SetVector("_Tip_Color", endColor.Evaluate(throttle));
    }
}
