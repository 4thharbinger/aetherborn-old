
#if UNITY_EDITOR
using UnityEditor;
#endif

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

        //if (Application.isPlaying)
        //{
        //    materialShockwave = renderer.materials[0];
        //    materialCones = renderer.materials[1];
        //    materialCore = renderer.materials[2];
        //} else
        //{
            materialShockwave = renderer.sharedMaterials[0];
            materialCones = renderer.sharedMaterials[1];
            materialCore = renderer.sharedMaterials[2];
        //}
    }

    [ContextMenu("Save Materials")]
    private void SaveMaterials()
    {
#if UNITY_EDITOR

        var target = PrefabUtility.IsPartOfPrefabInstance(this) ? PrefabUtility.GetCorrespondingObjectFromSource(PrefabUtility.GetNearestPrefabInstanceRoot(gameObject)) : null;

        if (!target) return;

        Debug.Log(target, target);

        if (AssetDatabase.GetAssetPath(materialShockwave) != AssetDatabase.GetAssetPath(target))
        AssetDatabase.RemoveObjectFromAsset(materialShockwave);

        if (AssetDatabase.GetAssetPath(materialCones) != AssetDatabase.GetAssetPath(target))
            AssetDatabase.RemoveObjectFromAsset(materialCones);

        if (AssetDatabase.GetAssetPath(materialCore) != AssetDatabase.GetAssetPath(target))
            AssetDatabase.RemoveObjectFromAsset(materialCore);

        AssetDatabase.AddObjectToAsset(materialShockwave, target);
        AssetDatabase.AddObjectToAsset(materialCones, target);
        AssetDatabase.AddObjectToAsset(materialCore, target);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif
    }

    private void OnValidate()
    {
        Awake();

        if (!materialShockwave || !materialCones || !materialCore) return;
        Draw();
    }

    public void Draw()
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
