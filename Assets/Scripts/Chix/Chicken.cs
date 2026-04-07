using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Chicken : MonoBehaviour, IPointerClickHandler
{
    public enum ChickenType { Yellow, Blue, Green, Bomb }

    [Header("Chicken Settings")]
    public ChickenType type;
    public float speedMultiplier = 1f;

    [Header("Animation Prefabs")]
    public GameObject explosionPrefab;
    public GameObject hitEffectPrefab;

    private bool isClicked = false;
    private int lastClickFrame = -1;


    public void OnPointerClick(PointerEventData eventData)
    {
        if (isClicked || ChixGameManager.Instance == null || ChixGameManager.Instance.isBossMode)
            return;

        // ? START GAME if this is the first chicken
        if (!ChixGameManager.Instance.gameStarted)
        {
            ChixGameManager.Instance.StartGameFromFirstChicken();
        }

        // 2. Immediate exit if hammer is already swinging
        if (ChixGameManager.Instance.IsHammerBusy())
            return;

        isClicked = true;

        var img = GetComponent<UnityEngine.UI.Image>();
        if (img != null) img.raycastTarget = false;

        transform.DOKill();

        ChixGameManager.Instance.UseNormalHammer(transform.position, this);
    }
    public void ExecuteHit()
    {
        if (gameObject == null) return;

        transform.DOKill();

        transform.DOScale(Vector3.zero, 0.12f)
            .SetEase(Ease.InBack)
            .SetUpdate(true);

        transform.DOLocalMoveY(-25f, 0.12f)
            .SetRelative()
            .SetUpdate(true);

        if (type == ChickenType.Bomb)
            SpawnEffect(explosionPrefab, "Explode");
        else
            SpawnEffect(hitEffectPrefab, GetTriggerName());

        if (ChixGameManager.Instance != null)
            ChixGameManager.Instance.OnChickenHit(type);

        Destroy(gameObject, 0.15f);
    }

    private string GetTriggerName()
    {
        switch (type)
        {
            case ChickenType.Yellow: return "YExplode";
            case ChickenType.Blue: return "BExplode";
            case ChickenType.Green: return "GExplode";
            default: return "";
        }
    }

    private void SpawnEffect(GameObject prefab, string trigger)
    {
        if (prefab == null) return;

        GameObject effect = Instantiate(prefab, transform.parent, false);
        RectTransform effectRect = effect.GetComponent<RectTransform>();

        if (effectRect != null)
        {
            effectRect.anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
            effectRect.localScale = Vector3.one * 0.5f;
            effectRect.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
            effectRect.DOScale(Vector3.zero, 0.3f).SetDelay(0.6f).SetEase(Ease.InBack).SetUpdate(true).OnComplete(() => Destroy(effect));
        }

        Animator anim = effect.GetComponent<Animator>();
        if (anim != null)
        {
            anim.updateMode = AnimatorUpdateMode.UnscaledTime;
            anim.SetTrigger(trigger);
        }
    }

    public bool IsClicked()
    {
        return isClicked;
    }
}