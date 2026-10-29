using UnityEngine;

public class ChunkData
{
    // Configuration variable
    public float ChunkSize = 1.0f;

    // Converted to regular fields
    public int Width;
    public int Height;
    public int Depth;

    // Declare the array at the class level without initializing its size yet
    public BlockType[,,] map;

    // Allocate the array size inside the constructor
    public ChunkData()
    {
        // 1. Calculate and assign the field values first
        Width = (int)(ChunkSize * 16);
        Height = (int)(ChunkSize * 32);
        Depth = (int)(ChunkSize * 16);

        // 2. Use those fields to initialize the array size
        map = new BlockType[Width, Height, Depth];
    }
}