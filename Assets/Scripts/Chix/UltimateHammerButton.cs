

using UnityEngine;
using System.Collections;

public class UltimateHammerButton : MonoBehaviour
{
    public void UseHammer()
    {
        if (ChixGameManager.Instance.isBossMode && ChixGameManager.Instance.currentBoss != null)
        {
            StartCoroutine(DelayedHammerHit());
        }
    }

    IEnumerator DelayedHammerHit()
    {
        // 1. Start the animation immediately
        ChixGameManager.Instance.PlayHammerHitAnimation();

        // 2. Wait for 0.5 seconds (the time it takes for the hammer to swing down)
        yield return new WaitForSeconds(0.5f);

        // 3. Now apply the damage and boss reaction
        if (ChixGameManager.Instance.currentBoss != null)
        {
            ChixGameManager.Instance.currentBoss.TakeDamage();
        }
    }
}