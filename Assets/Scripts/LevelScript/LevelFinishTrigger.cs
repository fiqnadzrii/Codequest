using UnityEngine;

public class LevelFinishTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering is the Player
        // Make sure your Player object has the tag "Player"
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered the finish zone!");
            
            // Trigger the score calculation
            if (GameScoreManager.Instance != null)
            {
                GameScoreManager.Instance.CalculateFinalScore();
            }
        }
    }
}