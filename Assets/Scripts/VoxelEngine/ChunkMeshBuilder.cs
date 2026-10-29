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

    public Mesh GenerateMesh(ChunkData chunk)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        int vertexIndex = 0;

        for (int x = 0; x < ChunkData.Width; x++)
        {
            for (int y = 0; y < ChunkData.Height; y++)
            {
                for (int z = 0; z < ChunkData.Depth; z++)
                {
                    if (chunk.map[x, y, z] == BlockType.Air) continue;

                    Vector3 blockPos = new Vector3(x, y, z);

                    for (int i = 0; i < 6; i++)
                    {
                        if (CheckFaceVisible(chunk, x, y, z, faceDirections[i]))
                        {
                            vertices.Add(blockPos + cubeVertices[faceTriangles[i][0]]);
                            vertices.Add(blockPos + cubeVertices[faceTriangles[i][1]]);
                            vertices.Add(blockPos + cubeVertices[faceTriangles[i][2]]);
                            vertices.Add(blockPos + cubeVertices[faceTriangles[i][3]]);

                            triangles.Add(vertexIndex);
                            triangles.Add(vertexIndex + 1);
                            triangles.Add(vertexIndex + 2);
                            triangles.Add(vertexIndex + 2);
                            triangles.Add(vertexIndex + 1);
                            triangles.Add(vertexIndex + 3);

                            vertexIndex += 4;
                        }
                    }
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }

    private bool CheckFaceVisible(ChunkData chunk, int x, int y, int z, Vector3Int direction)
    {
        int nx = x + direction.x;
        int ny = y + direction.y;
        int nz = z + direction.z;

        if (nx < 0 || nx >= ChunkData.Width || ny < 0 || ny >= ChunkData.Height || nz < 0 || nz >= ChunkData.Depth)
            return true;

        return chunk.map[nx, ny, nz] == BlockType.Air;
    }
}