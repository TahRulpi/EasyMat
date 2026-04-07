using UnityEngine;

public class HolderController : MonoBehaviour
{
    public float moveSpeed = 3f;   // speed of movement
    public float moveRange = 2.5f; // max distance from start
    private float direction = 1f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.position += Vector3.right * direction * moveSpeed * Time.deltaTime;

        if (Mathf.Abs(transform.position.x - startPos.x) >= moveRange)
        {
            direction *= -1; // reverse direction
        }
    }
}
