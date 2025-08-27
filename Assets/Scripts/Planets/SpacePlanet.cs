using UnityEngine;

public class SpacePlanet : MonoBehaviour
{
    public PlanetSurface planet;

    public MeshFilter filter;

    private void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Space");
        transform.localScale = Vector3.one / SpaceViewManager.scaleFactor;
        filter = GetComponent<MeshFilter>();
    }
}