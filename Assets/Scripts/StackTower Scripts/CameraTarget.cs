using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour
{
    public float smoothSpeed = 5f;    // Smooth follow speed
    public Vector3 offset;            // Optional offset from top block

    private Transform topBlock;       // Last spawned / top block

    void LateUpdate()
    {
        if (topBlock == null) return;

        // Only move up if top block is above current camera Y
        if (topBlock.position.y + offset.y > transform.position.y)
        {
            Vector3 targetPosition = new Vector3(transform.position.x, topBlock.position.y + offset.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        }
    }

    // Call this whenever a new block is spawned
    public void SetTopBlock(Transform newTopBlock)
    {
        topBlock = newTopBlock;
    }

}
