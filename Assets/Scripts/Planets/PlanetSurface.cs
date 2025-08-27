using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Profiling;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]

public class PlanetSurface : MonoBehaviour
{
    private MeshFilter filter;
    private new MeshCollider collider;

    private new MeshRenderer renderer;

    public HeightMap[] heightStack;

    void Start()
    {
        mesh = new Mesh
        {
            name = name + " Generated Mesh",
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
        colliderMesh = new Mesh
        {
            name = name + " Generated Collision Mesh",
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
        localOrigin = transform.InverseTransformPoint(origin);
        GeneratePlanet();

        SpaceViewManager.planets.Add(this);
    }

    private void Awake()
    {
        filter = GetComponent<MeshFilter>();
        collider = GetComponent<MeshCollider>();
        floatingTransform = GetComponent<FloatingTransform>();
        renderer = GetComponent<MeshRenderer>();

        for (var i = 0; i < 6; i++)
        {
            chunks[i] = null;
        }
        spacePlanet = new GameObject(name + " SpaceView", typeof(MeshFilter), typeof(MeshRenderer)).AddComponent<SpacePlanet>();
        spacePlanet.GetComponent<MeshRenderer>().sharedMaterials = renderer.sharedMaterials;

        autoGenerate = false;
    }

    [Tooltip("Approximate radius of the planet.")]
    public float radius;

    [Tooltip("Number of quads in each axis of the grid.")]
    public int gridSize = 10;

    [Tooltip("Maximum number of subdivisions.")]
    public int maxDepth = 5;

    [Tooltip("Maximum distance in number of size steps a grid can to be subdivided.")]
    public float divisionStep = 3f;
    [Tooltip("Maximum distance the localOrigin moves before a remesh occurs.")]
    public float remeshStep = 1f;
    [Tooltip("Maximum chunks will have its geometry added to the collider mesh.")]
    public float collisionRange = 1f;

    [Tooltip("Multiplier for height map data.")]
    public float heightScale = 1f;

    private FloatingTransform floatingTransform;

    public Vector3 origin;
    public Vector3 offset;

    public SpacePlanet spacePlanet;

    private int highestDepth = 0;

    private Vector3 localOrigin;
    private Vector3 meshOrigin;

    private double3 meshWorldOrigin;

    private Mesh mesh;
    private Mesh colliderMesh;

    private Vector3[] vertices;
    private int[] triangles;
    private Vector3[] normals;
    private Vector3[] colliderVertices;
    private int[] colliderTriangles;

    private readonly Chunk[] chunks = new Chunk[6];
    private readonly CubeChunk cubeChunk = new(null, Vector3.zero, 0f, 0, Vector3.zero, Vector3.zero);

    private void FixedUpdate()
    {
        if (floatingTransform)
        {
            //floatingTransform.enabled = false;
            origin = FloatingOrigin.origin.transform.position;

            offset = FloatingOrigin.ToVector3(floatingTransform.Position - FloatingOrigin.origin.positionOrigin);
            localOrigin = transform.InverseTransformPoint(origin) - offset;
            if (Vector3.Distance(localOrigin, meshOrigin) > remeshStep || FloatingOrigin.origin.originReset)
            {
                meshWorldOrigin = FloatingOrigin.origin.positionOrigin;

                if (math.all(math.isfinite(localOrigin)))
                {
                    GeneratePlanet();
                } else
                {
                    Debug.LogWarning("Cannot generate planet at non-finite position " + localOrigin);
                }
            }
            floatingTransform.SetScenePosition((float3)(meshWorldOrigin - FloatingOrigin.origin.positionOrigin));
        }
        else
        {
            localOrigin = transform.InverseTransformPoint(origin);
            if (Vector3.Distance(localOrigin, meshOrigin) > remeshStep)
            {
                GeneratePlanet();
            }
        }
    }

#if UNITY_EDITOR
    [Tooltip("Automatically generate the mesh on parameter change. Radius is ignored.")]
    public bool autoGenerate;
    [Tooltip("Radius of the planet for editor display.")]
    public float displayRadius = 10f;
    [Tooltip("Grid size of the planet for editor display.")]
    public int displayQuality = 20;
    private void OnValidate()
    {
        if (!autoGenerate) return;
        
        renderer = GetComponent<MeshRenderer>();
        filter = GetComponent<MeshFilter>();
        collider = GetComponent<MeshCollider>();

        if (!mesh)
        mesh = new Mesh
        {
            name = name + " Generated Mesh",
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
        if (!colliderMesh)
        colliderMesh = new Mesh
        {
            name = name + " Generated Collider Mesh",
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };

        UnityEditor.EditorApplication.delayCall += () =>
        {
            var oldHeightScale = heightScale;
            var oldRadius = radius;
            var oldOrigin = origin;
            var oldGridSize = gridSize;

            try
            {
                gridSize = displayQuality;
                radius = displayRadius;
                heightScale = oldHeightScale * displayRadius / oldRadius;
                origin = transform.position;
                localOrigin = Vector3.zero;

                for (var i = 0; i < 6; i++)
                {
                    chunks[i] = null;
                }

                GeneratePlanet(false);
            }
            finally
            {
                heightScale = oldHeightScale;
                radius = oldRadius;
                origin = oldOrigin;
                gridSize = oldGridSize;
            }
        };
    }

#endif

    /// <summary>
    ///     Generates the planet.
    /// </summary>
    /// <param name="generateCollision"></param>
    public void GeneratePlanet(bool generateCollision = true)
    {
        Profiler.BeginSample("Generate Mesh::Reset");
        highestDepth = 0;
        meshOrigin = localOrigin;
        Profiler.EndSample();

        chunkIndex = 0;

        Profiler.BeginSample("Generate Mesh::Build Chunks");
        GenerateFace(new Vector3(-radius, radius, -radius), radius * 2f, Vector3.right, Vector3.forward);
        GenerateFace(new Vector3(radius, -radius, radius), radius * 2f, Vector3.back, Vector3.left);
        GenerateFace(new Vector3(radius, -radius, -radius), radius * 2f, Vector3.forward, Vector3.up);
        GenerateFace(new Vector3(-radius, radius, radius), radius * 2f, Vector3.down, Vector3.back);
        GenerateFace(new Vector3(-radius, -radius, radius), radius * 2f, Vector3.up, Vector3.right);
        GenerateFace(new Vector3(radius, radius, -radius), radius * 2f, Vector3.left, Vector3.down);
        Profiler.EndSample();
        //Profiler.BeginSample("Generate Mesh::PostProcess");
        //for (var i = 0; i < triangles.Count; i++)
        //{
        //    triangles[i] = vertexIndices[triangles[i]];
        //}
        //Profiler.EndSample();
        Profiler.BeginSample("Generate Mesh::Build Meshes");

        GenerateAllMeshes();

        Profiler.EndSample();

        Profiler.BeginSample("Generate Mesh::Combine Meshes");
        CombineMeshes(activeChunks, out triangles, out vertices, out normals);
        Profiler.EndSample();

        Profiler.BeginSample("Generate Mesh::Apply");

        activeChunks.Clear();

        remeshStep = radius / Mathf.Pow(2f, highestDepth);

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetNormals(normals);

        renderer.sharedMaterial.SetFloat("radius", radius);
        renderer.sharedMaterial.SetVector("_offset", offset);
        filter.sharedMesh = mesh;
        if (spacePlanet)
        spacePlanet.filter.sharedMesh = mesh;

        //if (generateCollision)
        //{
        //    colliderMesh.Clear();
        //    colliderMesh.SetVertices(colliderVertices);
        //    colliderMesh.SetTriangles(colliderTriangles, 0);
        //    colliderMesh.RecalculateNormals();

        //    collider.enabled = colliderVertices.Length > 0;
        //    if (collider.enabled)
        //        collider.sharedMesh = colliderMesh;
        //}
        Profiler.EndSample();
    }

    /// <summary>
    ///     Combine completed meshes into a single mesh.
    /// </summary>
    /// <param name="triangles"></param>
    /// <param name="vertices"></param>
    /// <param name="normals"></param>
    /// <param name="range"></param>
    public void CombineMeshes(IEnumerable<Chunk> chunks, out int[] triangles, out Vector3[] vertices, out Vector3[] normals)
    {
        //var triangle = new List<int>();
        //var vertex = new List<Vector3>();
        //var normal = new List<Vector3>();

        var triSize = gridSize * gridSize * 6;
        var vertexSize = (gridSize + 1) * (gridSize + 1);

        //foreach (var chunk in chunks)
        //{
        //    triangle.AddRange(chunk.triangles.Select(x => x + vertex.Count));
        //    vertex.AddRange(chunk.vertices);
        //    normal.AddRange(chunk.normals);
        //}

        //triangles = triangle.ToArray();
        //vertices = vertex.ToArray();
        //normals = normal.ToArray();

        //return;

        var triangleBuffer = new int[activeChunks.Count * triSize];
        var vertexBuffer = new Vector3[activeChunks.Count * vertexSize];
        var normalBuffer = new Vector3[activeChunks.Count * vertexSize];

        Parallel.ForEach(chunks, chunk =>
        //foreach (var chunk in chunks)
        {
            for (var i = 0; i < chunk.vertices.Length; i++)
            {
                vertexBuffer[chunk.id * vertexSize + i] = chunk.vertices[i] + offset;
            }
            Array.Copy(chunk.normals, 0, normalBuffer, chunk.id * vertexSize, vertexSize);

            for (var i = 0; i < chunk.triangles.Length; i++)
            {
                triangleBuffer[chunk.id * triSize + i] = chunk.triangles[i] + chunk.id * vertexSize;
            }
        });

        triangles = triangleBuffer;
        vertices = vertexBuffer;
        normals = normalBuffer;
    }

    private int chunkIndex = 0;

    /// <summary>
    ///     Generates a face of the planet.
    /// </summary>
    private void GenerateFace(Vector3 corner, float size, Vector3 x, Vector3 y)
    {
        chunks[chunkIndex] ??= new Chunk(cubeChunk, corner, size, 0, x, y);
        //this.z = Vector3.Cross(x, y);
        GenerateFace(chunks[chunkIndex], localOrigin);
        chunkIndex++;
    }

    /// <summary>
    ///     Chunks that don't have a mesh and need to be generated.
    /// </summary>
    private readonly List<Chunk> toGenerate = new();

    /// <summary>
    ///     Chunks that may <see langword="or"/> may not have a mesh but are going to be added to the final mesh.
    /// </summary>
    private readonly List<Chunk> activeChunks = new();

    /// <summary>
    ///     Recursively generates subchunks or queues them for generation if they are leaf chunks.
    /// </summary>
    /// <param name="chunk"></param>
    /// <param name="localOrigin"></param>
    public void GenerateFace(Chunk chunk, Vector3 localOrigin)
    {
        //Profiler.BeginSample("Generate Mesh::Build Face");
        var corner = chunk.corner;
        var size = chunk.size;
        var depth = chunk.depth;

        var distance = (ProjectOnSphere(corner + (chunk.x * size + chunk.y * size) * 0.5f, Vector3.zero, radius) - localOrigin).sqrMagnitude;

        if (distance >size * divisionStep * size * divisionStep || depth >= maxDepth)
        {
            if (depth > highestDepth)
            {
                highestDepth = depth;
            }

            activeChunks.Add(chunk);
            chunk.id = activeChunks.Count - 1;

            if (chunk.vertices == null)
            {
                toGenerate.Add(chunk);
            }

            //Profiler.EndSample();
        }
        else
        {
            chunk.GenerateChildren(chunk.x, chunk.y);

            GenerateFace(chunk.child1, localOrigin);
            GenerateFace(chunk.child2, localOrigin);
            GenerateFace(chunk.child3, localOrigin);
            GenerateFace(chunk.child4, localOrigin);
        }
        //Profiler.EndSample();
    }

    /// <summary>
    ///     Generate all meshes. TODO: Parallelize.
    /// </summary>
    public void GenerateAllMeshes()
    {
        Parallel.ForEach(toGenerate, GenerateMesh);

        toGenerate.Clear();
    }

    /// <summary>
    ///     Generates chunk meshes. TODO: Move to compute shader.
    /// </summary>
    /// <param name="chunk"></param>
    public void GenerateMesh(Chunk chunk)
    {
        var corner = chunk.corner;
        var size = chunk.size;
        var depth = chunk.depth;

        //var distance = (ProjectOnSphere(corner + (x * size + y * size) * 0.5f, Vector3.zero, radius) - localOrigin).sqrMagnitude;

        //Profiler.BeginSample("Generate Mesh::Mesh");
        var i = 0;
        var j = 0;
        var step = size / gridSize;

        var vertexBuffer = new Vector3[(gridSize + 1) * (gridSize + 1)];
        var normalBuffer = new Vector3[(gridSize + 1) * (gridSize + 1)];
        var triangleBuffer = new int[gridSize * gridSize * 6];

        for (var x = 0; x <= gridSize; x++)
        {
            for (var y = 0; y <= gridSize; y++)
            {
                var vertex = corner + step * x * chunk.x + step * y * chunk.y;
                var normalized = vertex.normalized;
                vertex = GetVertex(normalized, depth);
                vertexBuffer[i] = vertex;
                normalBuffer[i] = GetNormal(vertex, normalized, depth);
                i++;

                //if (vertexMap.TryGetValue(vertex, out var index))
                //{
                //    vertexIndices.Add(index);
                //}
                //else
                //{
                //    vertexMap[vertex] = vertices.Count - 1;
                //    vertexIndices.Add(vertices.Count - 1);
                //}

                if (x > 0 && y > 0)
                {
                    triangleBuffer[j] = i - 1;
                    triangleBuffer[j + 1] = i - 2;
                    triangleBuffer[j + 2] = i - gridSize - 2;

                    triangleBuffer[j + 3] = i - 2;
                    triangleBuffer[j + 4] = i - gridSize - 3;
                    triangleBuffer[j + 5] = i - gridSize - 2;

                    j += 6;
                }

            }
        }
        chunk.vertices = vertexBuffer;
        chunk.triangles = triangleBuffer;
        chunk.normals = normalBuffer;
    }

    public Vector3 GetVertex(Vector3 position, int depth)
    {
        var height = GetHeight(position, depth);
        return position * (height + radius);
    }

    public float GetHeight(Vector3 position, int depth)
    {
        var height = 0f;
        foreach (var map in heightStack)
        {
            if (depth < map.minDepth)
            {
                break;
            }
            height += map.GetHeight(position);
        }
        return height * heightScale;
    }

    public Vector3 GetNormal(Vector3 vertex, Vector3 position, int depth)
    {
        var h = 1e-4f;
        var z = (double3)(float3)position;
        var x = math.abs(math.dot(z, new double3(0, 1, 0))) > 0.9 ? math.cross(z, new double3(1, 0, 0)) : math.cross(z, new double3(0, 1, 0));
        var y = math.cross(z, x);

        //var x = Vector3.forward;
        //var y = Vector3.Cross(z,)

        //var px = GetVertex(position + x * h, depth);
        //var py = GetVertex(position + y * h, depth);
        //var nx = GetVertex(position - x * h, depth);
        //var ny = GetVertex(position - y * h, depth);
        //var nx = GetHeight(position - x * h, depth);
        //var ny = GetHeight(position - y * h, depth);

        var height = GetHeight(position, depth);
        var dx = x * h;
        var dy = y * h;
        var dhx = (double3)(float3)(height - GetHeight((float3)math.normalize(z + dx), depth));
        var dhy = (double3)(float3)(height - GetHeight((float3)math.normalize(z + dy), depth));

        return (float3)math.normalize(math.cross(dhx * z + 2f * radius * h * x, dhy * z + 2f * radius * h * y));
    }

    static Vector3 ProjectOnSphere(Vector3 vector, Vector3 origin, float radius)
    {
        return vector.normalized * radius + origin;
    }
}

public class CubeChunk : Chunk
{
    public Chunk child5;
    public Chunk child6;

    public CubeChunk(Chunk parent, Vector3 corner, float size, int depth, Vector3 x, Vector3 y) : base(parent, corner, size, depth, x, y)
    {
    }
}

public class Chunk
{
    public Chunk child1;
    public Chunk child2;
    public Chunk child3;
    public Chunk child4;

    public Chunk neighbor1;
    public Chunk neighbor2;
    public Chunk neighbor3;
    public Chunk neighbor4;

    public Chunk parent;

    public int id;

    public void GenerateChildren(Vector3 x, Vector3 y)
    {
        var halfSize = size * 0.5f;
        child1 ??= new Chunk(this, corner, halfSize, depth + 1, x, y);
        child2 ??= new Chunk(this, corner + halfSize * x, halfSize, depth + 1, x, y);
        child3 ??= new Chunk(this, corner + halfSize * y, halfSize, depth + 1, x, y);
        child4 ??= new Chunk(this, corner + halfSize * x + halfSize * y, halfSize, depth + 1, x, y);
        child1.neighbor1 = child2;
        child1.neighbor2 = child3;
        child2.neighbor1 = child1;
        child2.neighbor2 = child4;
        child3.neighbor1 = child1;
        child3.neighbor2 = child4;
        child4.neighbor1 = child2;
        child4.neighbor2 = child3;

    }

    public void GetNeighbors()
    {
        neighbor1 = parent.child1;
        neighbor2 = parent.child2;
        neighbor3 = parent.child3;
        neighbor4 = parent.child4;
    }

    public int depth;

    public Vector3[] vertices;
    public Vector3[] normals;
    public int[] triangles;

    public Vector3 corner;
    public float size;

    public Vector3 x;
    public Vector3 y;

    public Chunk(Chunk parent, Vector3 corner, float size, int depth, Vector3 x, Vector3 y)
    {
        this.parent = parent;
        this.corner = corner;
        this.size = size;
        this.depth = depth;
        this.x = x;
        this.y = y;
    }
}

public abstract class HeightMap : ScriptableObject
{
    public int minDepth;
    public abstract float GetHeight(Vector3 position);

    public virtual Vector3 GetGradient(Vector3 position)
    {
        var p = GetHeight(position);
        var h = 0.001f;
        var dx = GetHeight(new Vector3(position.x + h, position.y, position.z));
        var dy = GetHeight(new Vector3(position.x, position.y + h, position.z));
        var dz = GetHeight(new Vector3(position.x, position.y, position.z + h));
        return new Vector3((dx - p) / h, (dy - p) / h, (dz - p) / h);
    }
}
