using UnityEngine;
using System.Collections;

public class SceneTeleporter : MonoBehaviour
{
    [Header("Settings")]
    public string targetSpawnPointName;
    
    [Header("Optimization")]
    [Tooltip("Check this if this teleporter leads INTO the New Castle. Uncheck if it leads to Outskirts.")]
    public bool leadsToCastle = true;

    [Header("Transition")]
    public CanvasGroup fadeScreen;
    public float transitionTime = 1f;

    public static string PendingSpawnPointName;
    private bool isTeleporting = false;

    private void Start()
    {
        if (fadeScreen != null)
        {
            fadeScreen.alpha = 0f;
            fadeScreen.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isTeleporting)
        {
            PendingSpawnPointName = targetSpawnPointName;
            StartCoroutine(FadeAndTeleport(other.gameObject));
        }
    }

    private IEnumerator FadeAndTeleport(GameObject player)
    {
        isTeleporting = true;

        // 1. Fade out
        if (fadeScreen != null)
        {
            fadeScreen.gameObject.SetActive(true);
            float elapsed = 0f;
            while (elapsed < transitionTime)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeScreen.alpha = Mathf.Clamp01(elapsed / transitionTime);
                yield return null;
            }
        }

        // 2. TOGGLE AREAS (FPS FIX)
        // We tell the ScoreManager to turn the correct map folder on/off
        if (GameScoreManager.Instance != null)
        {
            GameScoreManager.Instance.SetAreaActive(leadsToCastle);
        }

        // 3. Teleport
        GameObject spawnPoint = GameObject.Find(targetSpawnPointName);
        if (spawnPoint != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false; 

            player.transform.position = spawnPoint.transform.position;
            player.transform.rotation = spawnPoint.transform.rotation;

            if (cc != null) cc.enabled = true;
        }

        if (GameScoreManager.Instance != null)
        {
            GameScoreManager.Instance.ResetForNewArea();
        }

        yield return new WaitForSecondsRealtime(0.5f); 

        // 4. Fade back in
        if (fadeScreen != null)
        {
            float elapsed = 0f;
            while (elapsed < transitionTime)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeScreen.alpha = Mathf.Clamp01(1 - (elapsed / transitionTime));
                yield return null;
            }
            fadeScreen.gameObject.SetActive(false);
        }

        isTeleporting = false;
    }
}