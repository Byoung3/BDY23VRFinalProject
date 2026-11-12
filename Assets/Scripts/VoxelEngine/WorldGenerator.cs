using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    [Header("World Settings")]
    public GameObject chunkPrefab;
    public int worldSizeX = 3;
    public int worldSizeZ = 3;

    [Header("Center Chamber & Prefab")]
    public GameObject centerPrefab;
    public Transform vrPlayerRig;

    [Header("True 3D Cave Settings")]
    public float caveNoiseScale = 0.03f;
    [Range(0f, 1f)] public float caveThreshold = 0.52f;
    public float detailNoiseScale = 0.08f;

    private ChunkController[,,] chunks;

    void Start()
    {
        chunks = new ChunkController[worldSizeX, 1, worldSizeZ];
        GenerateWorld();
        CarveCenterChamberAndSpawnPrefab();
        PositionVRPlayer();
    }

    void GenerateWorld()
    {
        // Step 1: Instantiate all chunk GameObjects using local positioning
        for (int x = 0; x < worldSizeX; x++)
        {
            for (int z = 0; z < worldSizeZ; z++)
            {
                GameObject chunkObj = Instantiate(chunkPrefab, transform);
                // Force local position to prevent any world-space scaling drift
                chunkObj.transform.localPosition = new Vector3(x * ChunkData.Width, 0, z * ChunkData.Depth);
                chunkObj.transform.localRotation = Quaternion.identity;
                chunkObj.transform.localScale = Vector3.one;

                ChunkController controller = chunkObj.GetComponent<ChunkController>();

                ChunkData data = new ChunkData();
                PopulateTrue3DCaves(data, x, z);

                controller.Data = data;
                chunks[x, 0, z] = controller;
            }
        }

        // Step 2: Link neighbors and trigger mesh generation for all chunks
        for (int x = 0; x < worldSizeX; x++)
        {
            for (int z = 0; z < worldSizeZ; z++)
            {
                ChunkController current = chunks[x, 0, z];

                if (z < worldSizeZ - 1) current.northNeighbor = chunks[x, 0, z + 1];
                if (z > 0) current.southNeighbor = chunks[x, 0, z - 1];
                if (x < worldSizeX - 1) current.eastNeighbor = chunks[x + 1, 0, z];
                if (x > 0) current.westNeighbor = chunks[x - 1, 0, z];

                current.Initialize(current.Data);
            }
        }
    }

    void PopulateTrue3DCaves(ChunkData data, int chunkX, int chunkZ)
    {
        int globalOffsetX = chunkX * ChunkData.Width;
        int globalOffsetZ = chunkZ * ChunkData.Depth;

        for (int x = 0; x < ChunkData.Width; x++)
        {
            for (int z = 0; z < ChunkData.Depth; z++)
            {
                float worldX = x + globalOffsetX;
                float worldZ = z + globalOffsetZ;

                for (int y = 0; y < ChunkData.Height; y++)
                {
                    if (y == 0 || y == ChunkData.Height - 1)
                    {
                        data.map[x, y, z] = BlockType.Bedrock;
                        continue;
                    }

                    float xy = Mathf.PerlinNoise(worldX * caveNoiseScale, y * caveNoiseScale);
                    float yz = Mathf.PerlinNoise(y * caveNoiseScale, worldZ * caveNoiseScale);
                    float xz = Mathf.PerlinNoise(worldX * caveNoiseScale, worldZ * caveNoiseScale);

                    float caveNoise = (xy + yz + xz) / 3f;

                    if (caveNoise > caveThreshold)
                    {
                        data.map[x, y, z] = BlockType.Air;
                    }
                    else
                    {
                        float detailXY = Mathf.PerlinNoise(worldX * detailNoiseScale, y * detailNoiseScale);
                        float detailYZ = Mathf.PerlinNoise(y * detailNoiseScale, worldZ * detailNoiseScale);
                        float detailNoise = (detailXY + detailYZ);

                        if (detailNoise > 0.85f)
                        {
                            data.map[x, y, z] = BlockType.Dirt;
                        }
                        else if (detailNoise > 0.55f)
                        {
                            data.map[x, y, z] = BlockType.StoneyDirt;
                        }
                        else
                        {
                            data.map[x, y, z] = BlockType.Stone;
                        }
                    }
                }
            }
        }
    }

    void CarveCenterChamberAndSpawnPrefab()
    {
        float totalWorldWidth = worldSizeX * ChunkData.Width;
        float totalWorldDepth = worldSizeZ * ChunkData.Depth;
        int centerX = Mathf.RoundToInt(totalWorldWidth / 2f);
        int centerZ = Mathf.RoundToInt(totalWorldDepth / 2f);
        int centerY = ChunkData.Height / 2;

        int holeSize = 5;
        int halfHole = holeSize / 2;

        for (int xOffset = -halfHole; xOffset <= halfHole; xOffset++)
        {
            for (int yOffset = -halfHole; yOffset <= halfHole; yOffset++)
            {
                for (int zOffset = -halfHole; zOffset <= halfHole; zOffset++)
                {
                    int wx = centerX + xOffset;
                    int wy = centerY + yOffset;
                    int wz = centerZ + zOffset;

                    int cX = wx / ChunkData.Width;
                    int cZ = wz / ChunkData.Depth;
                    int lX = wx % ChunkData.Width;
                    int lZ = wz % ChunkData.Depth;

                    if (cX >= 0 && cX < worldSizeX && cZ >= 0 && cZ < worldSizeZ)
                    {
                        if (wy > 0 && wy < ChunkData.Height - 1)
                        {
                            chunks[cX, 0, cZ].Data.map[lX, wy, lZ] = BlockType.Air;
                        }
                    }
                }
            }
        }

        for (int x = 0; x < worldSizeX; x++)
        {
            for (int z = 0; z < worldSizeZ; z++)
            {
                chunks[x, 0, z].RebuildMesh();
            }
        }

        if (centerPrefab != null)
        {
            Vector3 centerLocalPos = new Vector3(centerX, centerY, centerZ);
            GameObject spawnedPrefab = Instantiate(centerPrefab, transform);
            spawnedPrefab.transform.localPosition = centerLocalPos;
            spawnedPrefab.transform.localRotation = Quaternion.identity;
        }
    }

    void PositionVRPlayer()
    {
        if (vrPlayerRig == null) return;

        // Calculate the exact center of the world (where the 5x5x5 chamber and prefab are)
        float totalWorldWidth = worldSizeX * ChunkData.Width;
        float totalWorldDepth = worldSizeZ * ChunkData.Depth;
        Vector3 centerLocal = new Vector3(totalWorldWidth / 2f, ChunkData.Height / 2f, totalWorldDepth / 2f);
        Vector3 centerGlobal = transform.TransformPoint(centerLocal);

        // Place the VR player right outside the 5x5x5 cave opening (local Z offset of -4 units from the center)
        // transform.TransformPoint automatically scales this down properly if your parent container is scaled!
        Vector3 localOffset = new Vector3(0f, 0f, -4f);
        Vector3 spawnGlobal = transform.TransformPoint(centerLocal + localOffset);

        vrPlayerRig.position = spawnGlobal;
        vrPlayerRig.LookAt(centerGlobal);
    }
}