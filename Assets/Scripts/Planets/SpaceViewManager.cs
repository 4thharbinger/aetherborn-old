using NUnit.Framework;

using System.Collections.Generic;

using Unity.VisualScripting;

using UnityEngine;
using UnityEngine.Rendering;

public class SpaceViewManager : MonoBehaviour
{
    public static List<PlanetSurface> planets = new();

    public Camera mainCamera;
    public Camera spaceCamera;

    public static float scaleFactor = 1e4f;

    private void Awake()
    {
        planets.Clear();

        mainCamera = Camera.main;

        spaceCamera = GetComponent<Camera>();

        RenderPipelineManager.beginCameraRendering += BeginCameraRendering;
    }

    private void OnDestroy()
    {
RenderPipelineManager.beginCameraRendering -= BeginCameraRendering;
    }

    private void BeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        transform.SetPositionAndRotation(mainCamera.transform.position / scaleFactor, mainCamera.transform.rotation);

        foreach (var planet in planets)
        {
            planet.spacePlanet.transform.SetPositionAndRotation(planet.transform.position / scaleFactor, planet.transform.rotation);
        }
    }
}
