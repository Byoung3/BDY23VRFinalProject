using UnityEngine;
using System.Collections;

public class LimenMiner : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float jumpHeight = 2f;

    [HideInInspector]
    public bool isBusy = false;

    // Dispatches this limen to a target voxel position and breaks it upon arrival
    public void DispatchTo(Vector3 targetWorldPos, ChunkController targetChunk, int x, int y, int z)
    {
        if (isBusy) return;
        StartCoroutine(MoveAndMineRoutine(targetWorldPos, targetChunk, x, y, z));
    }

    private IEnumerator MoveAndMineRoutine(Vector3 targetWorldPos, ChunkController targetChunk, int x, int y, int z)
    {
        isBusy = true;
        Vector3 startPos = transform.position;
        float journeyProgress = 0f;

        while (journeyProgress < 1f)
        {
            journeyProgress += Time.deltaTime * moveSpeed;
            float heightOffset = Mathf.Sin(journeyProgress * Mathf.PI) * jumpHeight;

            Vector3 currentPos = Vector3.Lerp(startPos, targetWorldPos, journeyProgress);
            currentPos.y += heightOffset;
            transform.position = currentPos;

            yield return null;
        }

        // Snap to exact target position
        transform.position = targetWorldPos;

        // Break the block if it's still valid/non-air
        if (targetChunk != null)
        {
            BlockType currentBlock = targetChunk.Data.map[x, y, z];
            if (currentBlock != BlockType.Air && currentBlock != BlockType.Bedrock)
            {
                targetChunk.ModifyBlock(x, y, z, BlockType.Air);
            }
        }

        isBusy = false; // Free up this limen for future tasks
    }
}