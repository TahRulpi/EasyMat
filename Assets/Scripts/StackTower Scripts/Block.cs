using UnityEngine;

public class Block : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool hasDropped = false;
    private bool hasLanded = false;
    private BlockManager manager;
    private static bool firstBlockGrounded = false;
    private float windForce = 0f;
    private int blockNumber;

    public GameObject windArrow;

    public float WindForce => windForce;

    // **CRITICAL FIX:** Assign rb in Awake() to ensure it's available before Update() runs.
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            // This error is crucial if it appears. It means the prefab is missing Rigidbody2D.
            Debug.LogError("FATAL SETUP ERROR: Rigidbody2D is missing from the Block GameObject. Please add it to the prefab.", this);
        }
    }

    public void Initialize(BlockManager blockManager, int blockCount, bool autoDrop = false, bool applyWind = false)
    {
        manager = blockManager;
        blockNumber = blockCount;

        // Ensure rb was successfully found in Awake()
        if (rb == null) return;

        // Only apply wind if the manager explicitly says so for this block
        if (manager != null && manager.windEnabled && applyWind)
        {
            // Randomly choose direction (left or right)
            if (Random.value > 0.5f)
                windForce = manager.rightForce;
            else
                windForce = manager.leftForce;

            // Ensure the wind arrow is visible if wind is applied
            if (windArrow != null)
                windArrow.SetActive(true);
        }
        else
        {
            windForce = 0f;
            // Ensure the wind arrow is invisible if no wind is applied
            if (windArrow != null)
                windArrow.SetActive(false);
        }

        // Set initial physics state (Kinematic)
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.drag = 0f;
        rb.angularDrag = 0.05f;

        // Blocks start with Z-rotation locked.
        if (manager != null)
        {
            manager.SetBlockZRotationConstraint(this.transform, true);
        }
    }

    private void Update()
    {
        // Only allow dropping if we haven't dropped and we have a Rigidbody
        if (!hasDropped && rb != null)
        {
            // Handle horizontal movement here if needed (e.g., using A/D keys or touch drag)

            if (Input.GetMouseButtonDown(0))
                DropBlock();
        }
    }

    private void DropBlock()
    {
        if (hasDropped || rb == null) return;

        hasDropped = true;

        // 1. Unparent from holder
        transform.SetParent(null);

        // 2. Switch to Dynamic physics
        rb.bodyType = RigidbodyType2D.Dynamic;

        // Clear any residual velocity from the moving holder
        rb.velocity = Vector2.zero;

        // 3. Apply soft-fall mechanics (Gravity and Drag)
        if (manager != null && blockNumber >= manager.gravityReductionStartBlock)
        {
            rb.gravityScale = manager.reducedGravityScale;
            // Greatly increase drag to slow down the fall and damp rotation before impact
            rb.drag = 5f; // Increase linear drag dramatically
            rb.angularDrag = 10f; // Increase angular drag dramatically
            Debug.Log($"Block {blockNumber} dropping softly with Gravity: {manager.reducedGravityScale}, Drag: {rb.drag}");
        }
        else
        {
            rb.gravityScale = 1f; // Default gravity for early blocks
            rb.drag = 0f;
            rb.angularDrag = 0.05f;
        }

        // 4. Apply the fixed wind force as an impulse
        if (manager != null && manager.windEnabled && Mathf.Abs(windForce) > 0f)
            rb.AddForce(new Vector2(windForce, 0f), ForceMode2D.Impulse);

        if (windArrow != null)
            windArrow.SetActive(false);

        // 5. Briefly slow down time when the block drops
        Time.timeScale = 0.8f;

        // Also call a routine on the manager to reset Time.timeScale shortly after drop
        if (manager != null)
        {
            manager.StartCoroutine(manager.ResetTimeScale(0.3f));
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (rb == null) return; // Safety check

        if (collision.gameObject.CompareTag("Ground"))
        {
            if (!firstBlockGrounded)
            {
                firstBlockGrounded = true;
                rb.bodyType = RigidbodyType2D.Static;

                // Freeze Z-Rotation for the first block permanently on the ground
                if (manager != null)
                {
                    manager.SetBlockZRotationConstraint(this.transform, true);
                }
            }
            else
            {
                // This block missed the stack and hit the ground after the first block
                if (manager != null)
                {
                    manager.EndGame();
                }
            }
        }

        if (!hasLanded && collision.gameObject.CompareTag("Block"))
        {
            hasLanded = true;

            if (manager != null)
            {
                manager.OnBlockLanded(this.transform);
            }

            // OPTIONAL: Make blocks static once landed to prevent further movement
            // rb.bodyType = RigidbodyType2D.Static; 
        }
    }
}