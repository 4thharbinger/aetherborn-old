using UnityEngine;

[CreateAssetMenu(menuName = "PlanetSurface/Noise Map")]
public class NoiseMap : HeightMap
{
    private readonly FastNoiseLite noise = new();

    public FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.OpenSimplex2;

    public FastNoiseLite.FractalType fractalType = FastNoiseLite.FractalType.FBm;

    public float frequency = 0.01f;
    public float lacunarity = 2.0f;
    public float gain = 0.5f;
    public int octaves = 6;

    public Vector2 range;

    private bool initialized = false;

    private void OnValidate()
    {
        initialized = false;
    }
    public override float GetHeight(Vector3 position)
    {
        if (!initialized)
        {
            Initialize();
        }
        return noise.GetNoise(position.x, position.y, position.z) * (range.y - range.x) + range.x;
    }

    private void Initialize()
    {
        initialized = true;
        noise.SetFractalGain(gain);
        noise.SetFractalLacunarity(lacunarity);
        noise.SetFractalOctaves(octaves);
        noise.SetNoiseType(noiseType);
        noise.SetFractalType(fractalType);
        noise.SetFrequency(frequency);

    }
}
