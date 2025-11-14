using UnityEngine;
using TMPro;
using System.Collections;

public class Dummy : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text damageText;

    [Header("Timing")]
    public float resetDelay = 1.5f;   // How long after no hits to clear the text

    private int stackedDamage = 0;
    private Coroutine resetRoutine;

    void Start()
    {
        if (damageText != null)
            damageText.text = "";  // Hide text on start
    }

    private void OnTriggerEnter(Collider other)
    {
        CollisionDetection swordHit = other.GetComponent<CollisionDetection>();
        if (swordHit == null) return;

        // Calculate final damage using the dummy's tag
        string dummyTag = gameObject.tag;
        int dmg = swordHit.GetFinalDamageForTag(dummyTag);

        // Stack damage
        stackedDamage += dmg;

        // Update UI
        if (damageText != null)
            damageText.text = stackedDamage.ToString();

        // Reset the timer when hit again
        if (resetRoutine != null)
            StopCoroutine(resetRoutine);

        resetRoutine = StartCoroutine(ResetDamageAfterDelay());
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
}
