using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ChunkController : MonoBehaviour
{
    public ChunkData Data { get; set; }

    public ChunkController northNeighbor;
    public ChunkController southNeighbor;
    public ChunkController eastNeighbor;
    public ChunkController westNeighbor;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private ChunkMeshBuilder meshBuilder;

    public void Initialize(ChunkData data)
    {
        Data = data;
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshBuilder = new ChunkMeshBuilder();

        RebuildMesh();
    }

    public void RebuildMesh()
    {
        Mesh mesh = meshBuilder.GenerateMesh(Data, northNeighbor, southNeighbor, eastNeighbor, westNeighbor, transform.position);
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    public void ModifyBlock(int x, int y, int z, BlockType newType)
    {
        if (x < 0 || x >= ChunkData.Width || y < 0 || y >= ChunkData.Height || z < 0 || z >= ChunkData.Depth)
            return;

        Data.map[x, y, z] = newType;
        RebuildMesh();

        if (x == 0 && westNeighbor != null) westNeighbor.RebuildMesh();
        if (x == ChunkData.Width - 1 && eastNeighbor != null) eastNeighbor.RebuildMesh();
    }
}