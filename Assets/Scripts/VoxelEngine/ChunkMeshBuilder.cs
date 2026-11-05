using System.Collections.Generic;
using UnityEngine;

public class ChunkMeshBuilder
{
    private static readonly Vector3[] cubeVertices = new Vector3[] {
        new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0),
        new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 1)
    };

    private static readonly int[][] faceTriangles = new int[][] {
        new int[] {0, 3, 1, 2}, // Back
        new int[] {5, 6, 4, 7}, // Front
        new int[] {3, 7, 2, 6}, // Top
        new int[] {1, 5, 0, 4}, // Bottom
        new int[] {4, 7, 0, 3}, // Left
        new int[] {1, 2, 5, 6}  // Right
    };

    private static readonly Vector3Int[] faceDirections = new Vector3Int[] {
        new Vector3Int(0, 0, -1), new Vector3Int(0, 0, 1), new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0), new Vector3Int(-1, 0, 0), new Vector3Int(1, 0, 0)
    };

    private const int variantsPerBlock = 2;

    public Mesh GenerateMesh(ChunkData chunk, ChunkController north, ChunkController south, ChunkController east, ChunkController west, Vector3 chunkWorldPos)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        // 7 total submeshes: 0-1 Stone, 2-3 Dirt, 4-5 StoneyDirt, 6 Bedrock
        List<int>[] submeshTriangles = new List<int>[7];
        for (int i = 0; i < 7; i++) submeshTriangles[i] = new List<int>();

        int globalVertexIndex = 0;

        for (int x = 0; x < ChunkData.Width; x++)
        {
            for (int y = 0; y < ChunkData.Height; y++)
            {
                for (int z = 0; z < ChunkData.Depth; z++)
                {
                    BlockType blockType = chunk.map[x, y, z];
                    if (blockType == BlockType.Air) continue;

                    Vector3 blockLocalPos = new Vector3(x, y, z);

                    int maxVariants = (blockType == BlockType.Bedrock) ? 1 : variantsPerBlock;

                    // Simple random selection (50% chance for each variant when maxVariants is 2)
                    int variantIndex = GetBlockVariant(maxVariants);
                    int baseSubmeshIndex = GetBaseSubmeshIndex(blockType);
                    int targetSubmesh = baseSubmeshIndex + variantIndex;

                    for (int i = 0; i < 6; i++)
                    {
                        if (CheckFaceVisible(chunk, x, y, z, faceDirections[i], north, south, east, west))
                        {
                            AddFace(vertices, uvs, submeshTriangles[targetSubmesh], blockLocalPos, i, ref globalVertexIndex);
                        }
                    }
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.subMeshCount = 7;
        for (int i = 0; i < 7; i++)
        {
            mesh.SetTriangles(submeshTriangles[i].ToArray(), i);
        }

        mesh.RecalculateNormals();
        return mesh;
    }

    private int GetBaseSubmeshIndex(BlockType blockType)
    {
        switch (blockType)
        {
            case BlockType.Stone: return 0;
            case BlockType.Dirt: return 2;
            case BlockType.StoneyDirt: return 4;
            case BlockType.Bedrock: return 6;
            default: return 0;
        }
    }

    private int GetBlockVariant(int maxVariants)
    {
        if (maxVariants <= 1) return 0;
        // Random.Range(min, max) is exclusive on the max integer bound, 
        // so Random.Range(0, 2) returns either 0 or 1 with equal 50% probability.
        return Random.Range(0, maxVariants);
    }

    private void AddFace(List<Vector3> vertices, List<Vector2> uvs, List<int> triangles, Vector3 blockPos, int faceIndex, ref int vertexIndex)
    {
        vertices.Add(blockPos + cubeVertices[faceTriangles[faceIndex][0]]);
        vertices.Add(blockPos + cubeVertices[faceTriangles[faceIndex][1]]);
        vertices.Add(blockPos + cubeVertices[faceTriangles[faceIndex][2]]);
        vertices.Add(blockPos + cubeVertices[faceTriangles[faceIndex][3]]);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));

        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 3);

        vertexIndex += 4;
    }

    private bool CheckFaceVisible(ChunkData chunk, int x, int y, int z, Vector3Int direction, ChunkController north, ChunkController south, ChunkController east, ChunkController west)
    {
        int nx = x + direction.x;
        int ny = y + direction.y;
        int nz = z + direction.z;

        if (nx >= 0 && nx < ChunkData.Width && ny >= 0 && ny < ChunkData.Height && nz >= 0 && nz < ChunkData.Depth)
        {
            return chunk.map[nx, ny, nz] == BlockType.Air;
        }

        if (nz >= ChunkData.Depth && north != null) return north.Data.map[nx, ny, 0] == BlockType.Air;
        if (nz < 0 && south != null) return south.Data.map[nx, ny, ChunkData.Depth - 1] == BlockType.Air;
        if (nx >= ChunkData.Width && east != null) return east.Data.map[0, ny, nz] == BlockType.Air;
        if (nx < 0 && west != null) return west.Data.map[ChunkData.Width - 1, ny, nz] == BlockType.Air;

        return true;
    }
}