using UnityEngine;

public enum SubscriptionTier
{
    None,     // Normal
    Pro,
    Premium
}

public class GameManagerMap : MonoBehaviour
{
    public static GameManagerMap Instance;

    /*[Header("User Info")]
    public SubscriptionTier subscriptionTier = SubscriptionTier.None;*/

    [Header("Subscription")]
    public SubscriptionTier currentTier = SubscriptionTier.None;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
}
