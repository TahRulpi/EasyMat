using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BossChicken : MonoBehaviour, IPointerClickHandler
{
    public int maxHits = 20;
    private int currentHits = 0;

    // We use a container (Target) for damage juice so the Float stays on the Parent/Root
    private Transform visualTransform;

    [HideInInspector] public TextMeshProUGUI hitsText;
    [HideInInspector] public ChixGameManager gameManager;
    [HideInInspector] public bool ultimateHammerActive = false;

    private bool canBeHit = true;
    private float hitCooldown = 0.08f;
    private int lastHitFrame = -1;

    [Header("Defeat Effects")]
    public GameObject defAnimPrefab;

    void Awake()
    {
        // Reference the visual part of the chicken (usually the Image component transform)
        visualTransform = transform;
    }

    public void InitializeBoss()
    {
        currentHits = 0;
        canBeHit = true;
        lastHitFrame = -1;

        // Ensure the boss knows the hammer state immediately
        if (gameManager != null) ultimateHammerActive = gameManager.isUltiHammerActive;

        UpdateHitUI();

        // Delay floating until the "entry" animation is done
        DOVirtual.DelayedCall(1.2f, () => {
            StartFloating();
        }).SetUpdate(true);
    }

   

    public void OnPointerClick(PointerEventData eventData)
    {
        // REMOVED gameManager.isBossFrozen from this check
        if (!canBeHit || Time.frameCount == lastHitFrame || gameManager == null || !gameManager.isBossMode)
            return;

        canBeHit = false;
        lastHitFrame = Time.frameCount;

        gameManager.PlayHammerHitAnimation();
        TakeDamage();

        DOVirtual.DelayedCall(hitCooldown, () => canBeHit = true).SetUpdate(true);
    }

    private void StartFloating()
    {
        // Use SetRelative(true) so we don't need a basePosition variable
        transform.DOLocalMoveY(30f, 2f)
            .SetRelative(true)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetUpdate(true)
            .SetId("BossFloat");
    }

    /*public void TakeDamage()
    {
        if (gameManager == null || !gameManager.isBossMode) return;

        int hitAmount = ultimateHammerActive ? 2 : 1;
        currentHits += hitAmount;

        // Visual Feedback: Only kill the Punch/Scale, NOT the Float
        visualTransform.DOKill(true);

        float punchStrength = ultimateHammerActive ? 0.35f : 0.15f;
        visualTransform.DOPunchScale(new Vector3(punchStrength, -punchStrength, 0), 0.12f, 10, 1f).SetUpdate(true);

        // Flash Red logic
        Image img = GetComponent<Image>();
        if (img != null)
        {
            img.DOKill(); // Kill only the color tween
            img.color = Color.red;
            img.DOColor(Color.white, 0.1f).SetUpdate(true);
        }

        UpdateHitUI();

        if (currentHits >= maxHits)
        {
            HandleDeath();
        }
    }*/

    public void TakeDamage()
    {
        if (gameManager == null || !gameManager.isBossMode) return;

        // Determine damage: 2 if upgraded, 1 if not
        int hitAmount = gameManager.isUltiHammerActive ? 2 : 1;
        currentHits += hitAmount;

        // Visual punch (bigger punch for the big hammer)
        float punch = gameManager.isUltiHammerActive ? 0.4f : 0.2f;
        transform.DOPunchScale(new Vector3(punch, -punch, 0), 0.12f).SetUpdate(true);

        UpdateHitUI();

        if (currentHits >= maxHits)
        {
            HandleDeath();
        }
    }




    private void HandleDeath()
    {
        canBeHit = false;

        DOTween.Kill("BossFloat");
        transform.DOKill();

        RectTransform rect = GetComponent<RectTransform>();

        if (defAnimPrefab != null)
        {
            // Spawn defeat animation
            GameObject defeatFX = Instantiate(
                defAnimPrefab,
                gameManager.shakeRoot != null ? gameManager.shakeRoot : transform.parent
            );

            RectTransform fxRect = defeatFX.GetComponent<RectTransform>();

            if (fxRect != null && rect != null)
            {
                // Place FX exactly where boss is
                fxRect.position = rect.position;
                fxRect.localScale = Vector3.one;
                fxRect.SetAsLastSibling();
            }

            Animator anim = defeatFX.GetComponent<Animator>();
            if (anim != null)
            {
                anim.updateMode = AnimatorUpdateMode.UnscaledTime;
                anim.SetTrigger("defAnim");
            }

            // Hide the boss immediately
            gameObject.SetActive(false);

            // Move the DEFEAT animation into the hole
            if (fxRect != null)
            {
                DOVirtual.DelayedCall(0.4f, () =>
                {
                    fxRect.DOAnchorPos(new Vector2(0, -300f), 0.6f)
                        .SetEase(Ease.InQuart)
                        .SetUpdate(true);

                    fxRect.DOScale(Vector3.zero, 0.5f)
                        .SetEase(Ease.InBack)
                        .SetUpdate(true)
                        .OnComplete(() =>
                        {
                            Destroy(defeatFX);
                            gameManager.EndBossFight(true);
                            Destroy(gameObject);
                        });

                }).SetUpdate(true);
            }
            else
            {
                Destroy(defeatFX, 3f);
                gameManager.EndBossFight(true);
                Destroy(gameObject);
            }
        }
        else
        {
            gameManager.EndBossFight(true);
            Destroy(gameObject);
        }
    }


    /*private void HandleDeath()
    {
        canBeHit = false;
        DOTween.Kill("BossFloat");
        transform.DOKill();

        // 1. Hide the actual Boss prefab immediately so it "disappears"
        // We don't destroy it yet because we need this script to finish the logic
        GetComponent<Image>().enabled = false;

        // 2. Spawn the defeat animation at the EXACT position of the boss
        if (defAnimPrefab != null)
        {
            // Parent it to the boss hole so it moves relative to the arena
            GameObject defeatFX = Instantiate(defAnimPrefab, transform.parent);

            RectTransform fxRect = defeatFX.GetComponent<RectTransform>();
            if (fxRect != null)
            {
                // Position it exactly where the chicken was
                fxRect.anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
                fxRect.localScale = Vector3.one;
                fxRect.SetAsLastSibling();

                // 3. ANIMATION: Make the DEFEAT FX go back into the hole
                // Sequence: Play animation -> Wait a tiny bit -> Drop down
                Sequence defeatSeq = DOTween.Sequence().SetUpdate(true);

                defeatSeq.AppendInterval(0.1f); // Brief pause for the "Hit" impact

                defeatSeq.Append(fxRect.DOAnchorPos(new Vector2(0, -500f), 0.7f).SetEase(Ease.InQuart));
                defeatSeq.Join(fxRect.DOScale(new Vector3(0.5f, 1.5f, 1f), 0.7f).SetEase(Ease.InSine));

                defeatSeq.OnComplete(() =>
                {
                    // 4. Transition back to normal game
                    gameManager.EndBossFight(true);
                    Destroy(defeatFX);
                    Destroy(gameObject); // Now destroy the invisible boss object
                });
            }
        }
        else
        {
            // Fallback if prefab is missing
            gameManager.EndBossFight(true);
            Destroy(gameObject);
        }
    }*/



    void UpdateHitUI()
    {
        if (hitsText != null)
        {
            int remaining = Mathf.Max(0, maxHits - currentHits);
            hitsText.text = remaining.ToString();
        }
    }

    // Called when the user buys the UltiHam DURING the boss fight
    public void ActivateUltimateHammer()
    {
        ultimateHammerActive = true;
        // Visual indicator that the boss is taking more damage now (optional pulse)
        visualTransform.DOPunchPosition(Vector3.down * 10f, 0.5f).SetUpdate(true);
    }
}