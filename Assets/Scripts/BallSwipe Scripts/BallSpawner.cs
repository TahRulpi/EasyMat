using UnityEngine;

public class BallSpawner : MonoBehaviour




{


    [Header("Spawn Settings")]
    public bool middleColorsUnlocked = false;  // GameManager will control this



    public GameObject ballPrefab;
    public Transform spawnPoint;
    public GameManager gameManager;

    private float spawnDelay = 1f; // base delay
    private float lastSpawnTime;

    [Header("Ball Settings")]
    public float ballScale = 0.5f;

    private void Update()
    {
        if (gameManager.currentBall == null && Time.time - lastSpawnTime >= spawnDelay)
        {
            SpawnBall();
        }
    }

    public BallController SpawnBall()
    {
        if (ballPrefab == null || spawnPoint == null)
            return null;

        GameObject go = Instantiate(ballPrefab, spawnPoint.position, Quaternion.identity);
        go.transform.localScale *= ballScale;

        BallController bc = go.GetComponent<BallController>();
        if (bc == null) bc = go.AddComponent<BallController>();

        int colorCount = middleColorsUnlocked ? 6 : 4;
        bc.InitRandom(colorCount);


        if (gameManager != null)
            gameManager.currentBall = bc;

        lastSpawnTime = Time.time;
        spawnDelay = 1f; // always slow initially

        return bc;
    }
}
