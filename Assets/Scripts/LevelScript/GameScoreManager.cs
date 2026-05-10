using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameScoreManager : MonoBehaviour
{
    public static GameScoreManager Instance;

    [Header("Area Optimization (FPS FIX)")]
    public GameObject areaOutskirts; 
    public GameObject areaNewCastle; 

    [Header("UI References")]
    public GameObject scoreboard; 
    public GameObject gameplayHUD; 
    public TextMeshProUGUI timeScoreText;
    public TextMeshProUGUI combatScoreText;
    public TextMeshProUGUI totalScoreText; 
    public TextMeshProUGUI rank_Text;
    public Image rankDisplayImage;

    [Header("Button References")]
    public Button tryAgainButton; 
    public Button homeButton;     

    [Header("Animation Settings")]
    public float timePerSection = 0.8f; 

    [Header("Rank Sprites")]
    public Sprite spriteBigO;
    public Sprite spriteDiamond;
    public Sprite spriteGold;
    public Sprite spriteSilver;
    public Sprite spriteBronze;

    [Header("Scoring Settings")]
    public int scorePerEnemy = 200; 
    public float maxTimeBonus = 350f; 
    public float timeDecayRate = 5f;

    [Header("Rank Thresholds")]
    public int scoreForBigO = 1200;    
    public int scoreForDiamond = 1000; 
    public int scoreForGold = 800;
    public int scoreForSilver = 400;

    [Header("Respawn Settings")]
    public GameObject playerObject; 
    public Transform respawnPoint; 
    public Transform castleOutskirtsPoint; 

    private int enemiesDefeated = 0;
    private float startTime;
    private bool isLevelFinished = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        if(scoreboard != null) scoreboard.SetActive(false);
    }

    void Start()
    {
        startTime = Time.time;

        // Ensure the game starts with Outskirts visible and Castle hidden
        if (areaOutskirts != null) areaOutskirts.SetActive(true);
        if (areaNewCastle != null) areaNewCastle.SetActive(false);
    }

    // Called when clicking "Try Again" on the scoreboard
    public void TryAgain()
    {
        // When trying again, we ensure the Castle is ON and Outskirts are OFF
        SetAreaActive(true); 
        TeleportPlayer(respawnPoint);
        ResetLevelData();
    }

    // Called when clicking "Home" on the scoreboard
    public void GoHome()
    {
        // When going home, we ensure Outskirts are ON and Castle is OFF
        SetAreaActive(false);
        TeleportPlayer(castleOutskirtsPoint);
        ResetLevelData();
    }

    // Logic to switch the entire map folders on/off
    public void SetAreaActive(bool isInsideCastle)
    {
        if (areaNewCastle != null) areaNewCastle.SetActive(isInsideCastle);
        if (areaOutskirts != null) areaOutskirts.SetActive(!isInsideCastle);
    }

    private void TeleportPlayer(Transform targetPoint)
    {
        Time.timeScale = 1f;
        if (playerObject != null && targetPoint != null)
        {
            CharacterController cc = playerObject.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            playerObject.transform.position = targetPoint.position;
            playerObject.transform.rotation = targetPoint.rotation;
            if (cc != null) cc.enabled = true;
        }
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ResetLevelData()
    {
        enemiesDefeated = 0;
        startTime = Time.time;
        isLevelFinished = false;
        scoreboard.SetActive(false);
        if (gameplayHUD != null) gameplayHUD.SetActive(true);
    }

    public void AddKill() { if (!isLevelFinished) enemiesDefeated++; }

    public void CalculateFinalScore()
    {
        if (isLevelFinished) return;
        isLevelFinished = true;

        if (gameplayHUD != null) gameplayHUD.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        float timeTaken = Time.time - startTime;
        int killScore = enemiesDefeated * scorePerEnemy;
        int timeBonus = Mathf.Max(0, Mathf.RoundToInt(maxTimeBonus - (timeTaken * timeDecayRate)));
        int totalScore = killScore + timeBonus;

        if(tryAgainButton != null) tryAgainButton.interactable = false;
        if(homeButton != null) homeButton.interactable = false;

        if (scoreboard != null) scoreboard.SetActive(true);
        StartCoroutine(AnimateScoreboardSequentially(timeTaken, killScore, totalScore));
        PauseGame();
    }

    IEnumerator AnimateScoreboardSequentially(float finalTime, int finalCombat, int finalTotal)
    {
        timeScoreText.text = "";
        combatScoreText.text = "";
        totalScoreText.text = "";
        rank_Text.text = "";
        rankDisplayImage.gameObject.SetActive(false);

        yield return StartCoroutine(CountNumber(timeScoreText, 0, finalTime, true));
        yield return new WaitForSecondsRealtime(0.3f);
        yield return StartCoroutine(CountNumber(combatScoreText, 0, finalCombat, false));
        yield return new WaitForSecondsRealtime(0.3f);
        yield return StartCoroutine(CountNumber(totalScoreText, 0, finalTotal, false));
        yield return new WaitForSecondsRealtime(0.5f);

        rankDisplayImage.gameObject.SetActive(true);
        SetRankUI(finalTotal);

        if(tryAgainButton != null) tryAgainButton.interactable = true;
        if(homeButton != null) homeButton.interactable = true;
    }

    IEnumerator CountNumber(TextMeshProUGUI targetText, float start, float end, bool isTime)
    {
        float elapsed = 0f;
        while (elapsed < timePerSection)
        {
            elapsed += Time.unscaledDeltaTime;
            float current = Mathf.Lerp(start, end, elapsed / timePerSection);
            if (isTime) targetText.text = $"{(int)current / 60:00}:{current % 60:00}";
            else targetText.text = Mathf.RoundToInt(current).ToString();
            yield return null;
        }
        if (isTime) targetText.text = $"{(int)end / 60:00}:{end % 60:00}";
        else targetText.text = Mathf.RoundToInt(end).ToString();
    }

    void SetRankUI(int score)
    {
        if (score >= scoreForBigO) { rank_Text.text = "BIG O OF ONE (O(1))"; rankDisplayImage.sprite = spriteBigO; }
        else if (score >= scoreForDiamond) { rank_Text.text = "DIAMOND"; rankDisplayImage.sprite = spriteDiamond; }
        else if (score >= scoreForGold) { rank_Text.text = "GOLD"; rankDisplayImage.sprite = spriteGold; }
        else if (score >= scoreForSilver) { rank_Text.text = "SILVER"; rankDisplayImage.sprite = spriteSilver; }
        else { rank_Text.text = "BRONZE"; rankDisplayImage.sprite = spriteBronze; }
    }

    void PauseGame() { Time.timeScale = 0f; }
    public void ResetForNewArea() { ResetLevelData(); }
}