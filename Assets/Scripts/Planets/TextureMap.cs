using UnityEngine;

[CreateAssetMenu(menuName = "PlanetSurface/Texture Map")]
public class TextureMap : HeightMap
{
    public Texture2D texture;

    public Vector2 range;

    public override float GetHeight(Vector3 position)
    {
        float lat = Mathf.Asin(position.y / position.magnitude);
        float lon = Mathf.Atan2(position.x, position.z);

        // Convert to degrees
        lat = Mathf.Rad2Deg * lat;
        lon = Mathf.Rad2Deg * lon;

        var u = (lon + 180f) / 360f;
        var v = (lat + 90f) / 180f;

        return texture.GetPixelBilinear(u, v).r * (range.y - range.x) + range.x;
    }
}