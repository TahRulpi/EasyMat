using Cinemachine;
using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class BlockManager : MonoBehaviour
{
    [Header("References")]
    public GameObject blockPrefab;
    public Transform spawnPoint;
    public float spawnHeightOffset = 3f;
    public CameraTarget cameraTarget;

    [Header("Score Settings")]
    public int score = 0;
    public TMP_Text scoreText;

    [Header("Holder Settings")]
    public float holderStepUp = 0.5f;

    [Header("Holder")]
    public Transform holder;

    private int blockCount = 0;
    private bool canSpawn = true;
    private Transform topBlock;

    [Header("UI Panels")]
    public GameObject newPanel; // <-- Your Game Over Panel
    public GameObject gamePanel;

    [Header("Timer Settings")]
    public float challengeTimeLimit = 30f; // The fixed 30-second challenge limit
    private float timer;
    private bool isTimerRunning = false;
    public TMP_Text timerText;

    // ----- NEW CHALLENGE VARIABLES -----
    [Header("Challenge Settings")]
    public int blocksToFirstChallenge = 15;
    public int blocksToNextChallenge = 10; // Subsequent challenges start after 10 blocks
    public int blocksGoalInChallenge = 7; // Must place 7 blocks within the time limit
    private int blocksLandedInCurrentChallenge = 0; // Counter for the current challenge
    private int totalBlocksLanded = 0; // Total blocks landed across the whole game
    private int blocksSinceLastChallengeTrigger = 0; // Counter for blocks until the next challenge starts
    // -----------------------------------

    [Header("Challenge UI")]
    public TMP_Text challengePopupText; // The TMP Text component for the popup message

    [Header("Wind Settings")]
    public bool windEnabled = false;
    public int windStartAfter = 10;
    public int windFrequency = 5; // Wind will appear every X blocks after the start
    private int blocksSinceLastWind = 0;
    private bool isCurrentBlockWindy = false;

    // --------------------------------------------------------------------
    // ? NEW WIND ANIMATION REFERENCES
    [Header("Wind Animation")]
    public GameObject leftWindAnimatorObject;
    public GameObject rightWindAnimatorObject;
    public string windAnimationName = "WindPlay"; // Set this to the name of your wind animation clip
    // --------------------------------------------------------------------

    [Header("Settings Panel")]
    public GameObject settingsPanel;

    [Header("Wind Particles")]
    public ParticleSystem windParticles;

    public CinemachineVirtualCamera virtualCamera;
    public float moveSpeed = 1f;
    public float moveRange = 2f;

    private Vector3 initialHolderPos;
    private float moveTimer = 0f;

    [Header("Final Score UI")]
    public TMP_Text finalScoreText;

    [Header("Sound Settings")]
    public AudioSource fallSound;
    public AudioSource gameOverSound;
    public AudioSource windSound;

    [Header("Wind Force Settings")]
    public float leftForce = -1.0f;
    public float rightForce = 1.0f;

    [Header("Camera Settings")]
    public float cameraStepOffset = 0.85f; // Used to manually adjust the camera tracking

    [Header("Perfect Placement Settings")]
    public float snapThreshold = 0.4f;
    public float failThreshold = 1.2f;

    // ------------------ FLOOD SETTINGS ------------------
    [Header("Flood Settings")]
    public GameObject floodObject;// Your flood sprite
    public Animator floodAnimator;// Flood animator
    public float floodAnimationDuration = 2.0f;
    public float floodVerticalOffset = 0f; // Adjust flood position relative to holder
                                           // ----------------------------------------------------

    // ?? NEW SLOW FALL SETTINGS ??
    [Header("Slow Fall Settings")]
    public int gravityReductionStartBlock = 18; // Block number (inclusive) to start reducing gravity
    public float reducedGravityScale = 0.5f; // The reduced gravity scale for slower falling
    // ?? END NEW SLOW FALL SETTINGS ??

    private void Start()
    {
        initialHolderPos = holder.position;

        timer = challengeTimeLimit; // Initialize the timer value
        isTimerRunning = false;
        timerText.text = ""; // Hide timer until challenge starts

        // Initialize challenge tracking
        totalBlocksLanded = 0;
        blocksSinceLastChallengeTrigger = 0;
        blocksLandedInCurrentChallenge = 0;

        // Initialize wind tracking
        blocksSinceLastWind = 0;

        if (floodObject != null)
            floodObject.SetActive(false);

        // Ensure wind visuals are off at start
        if (leftWindAnimatorObject != null) leftWindAnimatorObject.SetActive(false);
        if (rightWindAnimatorObject != null) rightWindAnimatorObject.SetActive(false);

        // ** NEW: Ensure the Challenge Popup is hidden at start **
        if (challengePopupText != null)
            challengePopupText.gameObject.SetActive(false);

        SpawnBlock(autoDrop: true);
    }

    public void AddScore(int amount)
    {
        score += amount;

        if (scoreText != null)
            scoreText.text = "" + score;
    }

    public IEnumerator ResetTimeScale(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // Use Realtime to wait regardless of timeScale
        Time.timeScale = 1f;
    }

    private void Update()
    {
        // ---------------------- CHALLENGE TIMER LOGIC ----------------------
        if (isTimerRunning)
        {
            timer -= Time.deltaTime;

            if (timerText != null)
                timerText.text = Mathf.Ceil(timer).ToString();

            if (timer <= 0)
            {
                isTimerRunning = false;
                timerText.text = "Time: 0";

                // FAILURE CONDITION: Timer ran out AND goal wasn't met
                if (blocksLandedInCurrentChallenge < blocksGoalInChallenge)
                {
                    Debug.Log("Challenge Failed: Goal not met in time.");

                    // 1. Perform non-UI cleanup
                    EndGame();

                    // 2. Start the coroutine to handle the delayed block cleanup and panel display (Flood)
                    StartCoroutine(HandleTimeOut());

                    canSpawn = false;
                }
                else
                {
                    // This case is technically a successful challenge, but was handled in OnBlockLanded.
                    EndChallengeSuccess();
                }
            }
        }
    }

    private IEnumerator HandleTimeOut()
    {
        // 1. Start the Flood Animation (Blocks are still visible!)
        if (floodObject != null)
            floodObject.SetActive(true);

        if (floodAnimator != null)
            floodAnimator.Play("flood");

        // 2. Wait for the animation
        yield return new WaitForSeconds(floodAnimationDuration);

        // 3. Cleanup: Hide blocks and holder now (just before the panel)
        CleanupGameObjects();

        // 4. Show the Game Over Panel
        ShowGameOverPanel();
    }

    // ** NEW METHOD: To display the challenge goal popup **
    private IEnumerator ShowChallengePopup(int goalBlocks, float duration)
    {
        if (challengePopupText != null)
        {
            // Set the message and make it visible
            challengePopupText.text = $"Drop {goalBlocks} Blocks!";
            challengePopupText.gameObject.SetActive(true);

            // Wait for the specified duration
            yield return new WaitForSeconds(duration);

            // Hide the text
            challengePopupText.gameObject.SetActive(false);
        }
    }


    // Handles successful completion of the time challenge
    private void EndChallengeSuccess()
    {
        Debug.Log("Challenge Succeeded!");
        isTimerRunning = false;
        timer = challengeTimeLimit; // Reset timer value
        timerText.text = ""; // Hide timer
        blocksSinceLastChallengeTrigger = 0; // Start counting 10 blocks again
    }


    private void LateUpdate()
    {
        UpdateHolderMovement();
    }

    private void UpdateHolderMovement()
    {
        if (holder == null || topBlock == null) return;

        moveTimer += Time.deltaTime * moveSpeed;
        float offsetX = Mathf.Sin(moveTimer) * moveRange;

        float desiredY = topBlock.position.y + (spawnHeightOffset - 0.5f);
        float targetY = holder.position.y;

        if (desiredY > holder.position.y)
            targetY = Mathf.Lerp(holder.position.y, desiredY, Time.deltaTime * 5f);

        holder.position = new Vector3(initialHolderPos.x + offsetX, targetY, holder.position.z);
    }

    // ? NEW METHOD: To set the Z rotation constraint on a Rigidbody2D
    public void SetBlockZRotationConstraint(Transform blockTransform, bool freeze)
    {
        // We use Rigidbody2D here, as blocks have already dropped.
        Rigidbody2D rb = blockTransform.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            if (freeze)
            {
                // Set the Freeze Rotation Z bit (lock it)
                rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
            }
            else
            {
                // Clear the Freeze Rotation Z bit (unlock it)
                rb.constraints &= ~RigidbodyConstraints2D.FreezeRotation;
            }
        }
    }

    public void OnBlockLanded(Transform landedBlock)
    {
        if (fallSound != null)
            fallSound.Play();

        // Update overall and challenge block counters
        totalBlocksLanded++;

        if (isTimerRunning)
        {
            blocksLandedInCurrentChallenge++;

            // SUCCESS CONDITION: Goal met before timer runs out
            if (blocksLandedInCurrentChallenge >= blocksGoalInChallenge)
            {
                EndChallengeSuccess();
                // The block count for the next challenge starts fresh from 0
            }
        }
        else // Timer is NOT running, check if it should start
        {
            blocksSinceLastChallengeTrigger++;

            int triggerThreshold = totalBlocksLanded <= blocksToFirstChallenge ? blocksToFirstChallenge : blocksToNextChallenge;

            // Check if the trigger threshold is reached
            if (blocksSinceLastChallengeTrigger >= triggerThreshold)
            {
                Debug.Log($"Starting new challenge: Block Goal = {blocksGoalInChallenge}, Time Limit = {challengeTimeLimit}s");
                isTimerRunning = true;
                timer = challengeTimeLimit; // Start timer at 30s
                blocksLandedInCurrentChallenge = 1; // This landed block counts as the first one!

                // ** NEW: Start the popup animation **
                StartCoroutine(ShowChallengePopup(blocksGoalInChallenge, 2.5f));
            }
        }


        // Game over if block placement is too far off
        if (topBlock != null)
        {
            float xDiff = landedBlock.position.x - topBlock.position.x;

            if (Mathf.Abs(xDiff) > failThreshold)
            {
                // Rigidbody rb = landedBlock.GetComponent<Rigidbody>(); // Original line was for 3D Rigidbody
                Rigidbody2D rb2d = landedBlock.GetComponent<Rigidbody2D>(); // Use 2D Rigidbody
                if (rb2d != null) rb2d.bodyType = RigidbodyType2D.Dynamic; // Ensure block falls if it missed
                EndGame();
                return;
            }

            if (Mathf.Abs(xDiff) <= snapThreshold)
            {
                landedBlock.position = new Vector3(
                    topBlock.position.x,
                    landedBlock.position.y,
                    landedBlock.position.z
                );
            }

            // ? CORE FIX: Freeze the PREVIOUS top block's Z rotation
            // The old top block is now part of the stable stack.
            SetBlockZRotationConstraint(topBlock, true);
        }

        AddScore(10);
        topBlock = landedBlock; // Set the NEW block as the top block

        // ? CORE FIX: UNFREEZE the NEW top block's Z rotation
        // This is the block that should be subject to rotation forces (e.g., wind).
        SetBlockZRotationConstraint(topBlock, false);

        StartCoroutine(MoveHolderUp(holderStepUp, 0.3f));

        UpdateVisualElementsPosition();

        // --- WIND FREQUENCY LOGIC ---
        // 1. Check for initial global wind activation
        if (!windEnabled && blockCount >= windStartAfter)
        {
            windEnabled = true;
            // Next block will be windy, so reset counter.
            blocksSinceLastWind = 0;
        }
        // 2. Increment the counter if wind is enabled globally
        else if (windEnabled)
        {
            blocksSinceLastWind++;

            // 3. Check for periodic wind trigger
            if (blocksSinceLastWind >= windFrequency)
            {
                // Next block will be windy, so reset counter.
                blocksSinceLastWind = 0;
            }
        }

        // 4. Ensure wind sound/particles stop if wind is NOT active for the next block
        if (blocksSinceLastWind > 0)
        {
            if (windSound != null) windSound.Stop();
            if (windParticles != null) windParticles.Stop();
            if (leftWindAnimatorObject != null) leftWindAnimatorObject.SetActive(false);
            if (rightWindAnimatorObject != null) rightWindAnimatorObject.SetActive(false);
        }
        // --- END WIND FREQUENCY LOGIC ---

        if (canSpawn)
            StartCoroutine(SpawnNextBlock());
    }

    private IEnumerator SpawnNextBlock()
    {
        canSpawn = false;
        yield return new WaitForSeconds(0.6f);
        SpawnBlock(autoDrop: false);
        canSpawn = true;
    }

    private void SpawnBlock(bool autoDrop)
    {
        blockCount++;
        Camofsetadd();

        // --- CHECK FOR WIND STATE FOR THIS BLOCK ---
        bool applyWind = false;

        // Wind applies if windEnabled is true AND the counter is at the trigger point (0)
        if (windEnabled && blocksSinceLastWind == 0)
        {
            applyWind = true;
            isCurrentBlockWindy = true;
        }
        // --- END WIND STATE CHECK ---

        float yOffset = -0.7f;
        spawnPoint.position = new Vector3(holder.position.x, holder.position.y + yOffset, holder.position.z);

        GameObject newBlock = Instantiate(blockPrefab, spawnPoint.position, Quaternion.identity);
        newBlock.transform.SetParent(holder);

        Block blockScript = newBlock.GetComponent<Block>();
        if (blockScript != null)
            // ?? MODIFIED LINE: Pass the blockCount and the determined 'applyWind' state to the block's Initialize method
            blockScript.Initialize(this, blockCount, autoDrop, applyWind);

        if (cameraTarget != null)
            cameraTarget.SetTopBlock(newBlock.transform);

        // Only show visuals/sound if wind is applied to THIS block
        if (applyWind && blockScript != null)
        {
            RotateWindParticles(blockScript.WindForce);

            float displayForce = 0f;
            if (blockScript.WindForce > 0) displayForce = rightForce;
            else if (blockScript.WindForce < 0) displayForce = leftForce;

            // Call the new method to show the directional animation
            ShowWindAnimation(displayForce);

            if (windSound != null && !windSound.isPlaying)
                windSound.Play();
        }
    }

    private IEnumerator MoveHolderUp(float step, float delay)
    {
        yield return new WaitForSeconds(delay);
        holder.position += new Vector3(0, step, 0);
    }

    private void RotateWindParticles(float force)
    {
        if (windParticles == null) return;

        // Turn off all animation objects before changing wind direction
        if (leftWindAnimatorObject != null) leftWindAnimatorObject.SetActive(false);
        if (rightWindAnimatorObject != null) rightWindAnimatorObject.SetActive(false);

        if (force > 0)
            windParticles.transform.rotation = Quaternion.Euler(0, 0, 0);
        else if (force < 0)
            windParticles.transform.rotation = Quaternion.Euler(0, 0, 180);
        else
            windParticles.Stop();

        if (!windParticles.isPlaying && force != 0)
            windParticles.Play();
    }

    // ? NEW POSITION UPDATE LOGIC
    private void UpdateVisualElementsPosition()
    {
        // 1. Update Wind Animators
        // Use a position relative to the holder's Y position
        Vector3 targetWindPos = new Vector3(
            leftWindAnimatorObject.transform.position.x,
            holder.position.y,
            leftWindAnimatorObject.transform.position.z
        );

        if (leftWindAnimatorObject != null)
            leftWindAnimatorObject.transform.position = targetWindPos;

        if (rightWindAnimatorObject != null)
            rightWindAnimatorObject.transform.position = targetWindPos;


        // 2. Update Flood Object?
        if (floodObject != null && topBlock != null)
        {
            Vector3 targetFloodPos = new Vector3(
                floodObject.transform.position.x,
                topBlock.position.y + floodVerticalOffset, // This line controls its height
                floodObject.transform.position.z
            );
            floodObject.transform.position = targetFloodPos;
        }
    }


    // ? NEW METHOD TO MANAGE WIND ANIMATION VISUALS
    public void ShowWindAnimation(float force)
    {
        if (!windEnabled || !isCurrentBlockWindy) return;

        // Ensure both are off initially
        if (leftWindAnimatorObject != null) leftWindAnimatorObject.SetActive(false);
        if (rightWindAnimatorObject != null) rightWindAnimatorObject.SetActive(false);

        if (force > 0) // Right Force
        {
            if (rightWindAnimatorObject != null)
            {
                rightWindAnimatorObject.SetActive(true);
                rightWindAnimatorObject.GetComponent<Animator>()?.Play(windAnimationName);
            }
        }
        else if (force < 0) // Left Force
        {
            if (leftWindAnimatorObject != null)
            {
                leftWindAnimatorObject.SetActive(true);
                leftWindAnimatorObject.GetComponent<Animator>()?.Play(windAnimationName);
            }
        }
    }

    void Camofsetadd()
    {
        if (virtualCamera != null)
            virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_TrackedObjectOffset.y += cameraStepOffset;
    }

    private void CleanupGameObjects()
    {
        if (holder != null)
            holder.gameObject.SetActive(false);

        Block[] allBlocks = FindObjectsOfType<Block>();
        foreach (var block in allBlocks)
            Destroy(block.gameObject);

        // Also ensure wind animations stop and hide during cleanup
        if (leftWindAnimatorObject != null) leftWindAnimatorObject.SetActive(false);
        if (rightWindAnimatorObject != null) rightWindAnimatorObject.SetActive(false);
        if (windParticles != null) windParticles.Stop();

        // ** Ensure popup is hidden during cleanup **
        if (challengePopupText != null)
            challengePopupText.gameObject.SetActive(false);
    }

    private void ShowGameOverPanel()
    {
        if (newPanel != null)
            newPanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = "" + score;
    }

    public void EndGame()
    {
        if (gameOverSound != null)
            gameOverSound.Play();

        if (gamePanel != null)
            gamePanel.SetActive(false);

        canSpawn = false;
        isTimerRunning = false;
        timerText.text = ""; // Ensure timer text is cleared/hidden

        // If EndGame is called by a block falling (timer > 0):
        if (timer > 0)
        {
            CleanupGameObjects();
            ShowGameOverPanel();
        }
        // If EndGame is called by time running out (timer <= 0),
        // HandleTimeOut() handles the final cleanup and panel display.
    }

    public void OpenSettings()
    {
        canSpawn = false;
        StopAllCoroutines();
        isTimerRunning = false;

        CleanupGameObjects(); // Clean up blocks and animations when opening settings

        if (gamePanel != null)
            gamePanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (gamePanel != null)
            gamePanel.SetActive(true);

        if (holder != null)
            holder.gameObject.SetActive(true);

        // Timer resumes if it was running before settings opened, or game continues count-up
        // We don't want to blindly set it to true, but the state management is complex here.
        // For simplicity in a single-file script, we'll allow it to be re-enabled if needed elsewhere.
        // For now, only re-enable spawning logic.
        canSpawn = true;
    }

    public void RestartGame()
    {
        // Get the name of the current active scene
        string currentSceneName = SceneManager.GetActiveScene().name;

        // Reload the scene
        SceneManager.LoadScene(currentSceneName);
    }
}