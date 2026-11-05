using UnityEngine;

public class ChunkData
{
    public const int Width = 16;
    public const int Height = 64;
    public const int Depth = 16;

    public BlockType[,,] map;

    public ChunkData()
    {
        map = new BlockType[Width, Height, Depth];
    }
}