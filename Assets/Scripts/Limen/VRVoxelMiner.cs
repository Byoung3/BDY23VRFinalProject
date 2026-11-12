using UnityEngine;
using UnityEngine.InputSystem;

public class VRVoxelMiner : MonoBehaviour
{
    [Header("VR Input Action")]
    public InputActionProperty triggerAction;

    [Header("Mining Settings")]
    public float maxReachDistance = 5f;
    public string limenTag = "Limen"; // Make sure your Limen prefab has this tag

    void Update()
    {
        float triggerValue = triggerAction.action != null ? triggerAction.action.ReadValue<float>() : (Input.GetAxis("Trigger") > 0.1f ? 1f : 0f);

        // Check for trigger press (using Down or continuous depending on design; using a simple frame check or threshold)
        if (triggerValue > 0.5f)
        {
            TrySelectAndMineBlock();
        }
    }

    void TrySelectAndMineBlock()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxReachDistance))
        {
            ChunkController chunk = hit.collider.GetComponent<ChunkController>();
            if (chunk != null)
            {
                Vector3 hitPoint = hit.point - (hit.normal * 0.01f);
                Vector3 localPos = chunk.transform.InverseTransformPoint(hitPoint);

                int x = Mathf.FloorToInt(localPos.x);
                int y = Mathf.FloorToInt(localPos.y);
                int z = Mathf.FloorToInt(localPos.z);

                if (x >= 0 && x < ChunkData.Width && y >= 0 && y < ChunkData.Height && z >= 0 && z < ChunkData.Depth)
                {
                    BlockType selectedBlock = chunk.Data.map[x, y, z];

                    // Only mine non-air and non-bedrock blocks
                    if (selectedBlock != BlockType.Air && selectedBlock != BlockType.Bedrock)
                    {
                        Vector3 targetWorldPos = chunk.transform.TransformPoint(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));

                        // Find the closest AVAILABLE (idle) Limen
                        LimenMiner availableLimen = FindClosestAvailableLimen(targetWorldPos);

                        if (availableLimen != null)
                        {
                            // Dispatch it independently!
                            availableLimen.DispatchTo(targetWorldPos, chunk, x, y, z);
                        }
                    }
                }
            }
        }
    }

    LimenMiner FindClosestAvailableLimen(Vector3 targetPos)
    {
        GameObject[] limenObjects = GameObject.FindGameObjectsWithTag(limenTag);
        LimenMiner closestLimen = null;
        float minDistanceSqr = Mathf.Infinity;

        foreach (GameObject obj in limenObjects)
        {
            LimenMiner miner = obj.GetComponent<LimenMiner>();
            // Check if the limen exists and is NOT currently busy mining another block
            if (miner != null && !miner.isBusy)
            {
                float distSqr = (obj.transform.position - targetPos).sqrMagnitude;
                if (distSqr < minDistanceSqr)
                {
                    minDistanceSqr = distSqr;
                    closestLimen = miner;
                }
            }
        }

        return closestLimen;
    }
}