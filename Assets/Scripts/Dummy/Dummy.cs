using UnityEngine;
using TMPro;
using System.Collections;

public class Dummy : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text damageText;

    [Header("Audio")] // <--- NEW SECTION
    public AudioSource audioSource;
    public AudioClip hitSound;

    [Header("Timing")]
    public float resetDelay = 1.5f;   // How long after no hits to clear the text

    private int stackedDamage = 0;
    private Coroutine resetRoutine;

    void Start()
    {
        // 1. Auto-find Audio Source if you forgot to drag it in
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (damageText != null)
            damageText.text = "";  // Hide text on start
    }

    private void OnTriggerEnter(Collider other)
    {
        int damageReceived = 0;
        string dummyTag = gameObject.tag;

        // Check for Sword Hit
        CollisionDetection swordHit = other.GetComponent<CollisionDetection>();
        if (swordHit != null)
        {
            damageReceived = swordHit.GetFinalDamageForTag(dummyTag);
        }

        // Check for Special Attack Hit (AOE or Projectile)
        ExplosionDamage specialHit = other.GetComponent<ExplosionDamage>();
        if (specialHit != null)
        {
            damageReceived = specialHit.GetFinalDamageForTag(dummyTag);
        }

        // --- IF WE GOT HIT ---
        if (damageReceived > 0)
        {
            // 2. Play the Sound immediately
            PlayHitSound();

            // Stack damage
            stackedDamage += damageReceived;

            // Update UI
            if (damageText != null)
                damageText.text = stackedDamage.ToString();

            // Reset the timer when hit again
            if (resetRoutine != null)
                StopCoroutine(resetRoutine);

            resetRoutine = StartCoroutine(ResetDamageAfterDelay());
        }
    }

    IEnumerator ResetDamageAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);

        // Clear and hide text
        stackedDamage = 0;
        if (damageText != null)
            damageText.text = "";

        resetRoutine = null;
    }

    // New helper function to play sound safely
    private void PlayHitSound()
    {
        if (audioSource != null && hitSound != null)
        {
            // PlayOneShot allows sounds to overlap (so rapid hits don't cut each other off)
            audioSource.PlayOneShot(hitSound);
        }
    }
}