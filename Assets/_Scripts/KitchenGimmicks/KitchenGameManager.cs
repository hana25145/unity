using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchenGameManager : MonoBehaviour
{
    public static KitchenGameManager Instance { get; private set; }

    [SerializeField] private PlayerBall player;
    [SerializeField] private int startingLives = 3;
    [SerializeField] private float countdownSeconds = 3f;

    private enum RunState { Countdown, Playing, Paused, Cleared, GameOver }
    private RunState state = RunState.Countdown;
    private int lives;
    private int collected;
    private int totalCollectibles;
    private float countdown;
    private float runTime;
    private float bestTime;
    private string message;
    private float messageUntil;

    public void Configure(PlayerBall target)
    {
        player = target;
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (player == null)
            player = FindFirstObjectByType<PlayerBall>();

        totalCollectibles = FindObjectsByType<IngredientCollectible>(FindObjectsSortMode.None).Length;
        lives = startingLives;
        countdown = countdownSeconds;
        bestTime = PlayerPrefs.GetFloat("KitchenCourseBestTime", 0f);

        if (player != null)
        {
            player.Respawned += HandleRespawn;
            player.SetControlEnabled(false);
        }
    }

    private void OnDestroy()
    {
        if (player != null)
            player.Respawned -= HandleRespawn;
        if (Instance == this)
            Instance = null;
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) && (state == RunState.Playing || state == RunState.Paused))
        {
            bool pause = state == RunState.Playing;
            state = pause ? RunState.Paused : RunState.Playing;
            Time.timeScale = pause ? 0f : 1f;
            player?.SetControlEnabled(!pause);
        }

        if (state == RunState.Countdown)
        {
            countdown -= Time.unscaledDeltaTime;
            if (countdown <= 0f)
            {
                state = RunState.Playing;
                player?.SetControlEnabled(true);
                ShowMessage("GO!", 1f);
            }
        }
        else if (state == RunState.Playing)
        {
            runTime += Time.deltaTime;
        }
    }

    public void CollectIngredient()
    {
        if (state != RunState.Playing)
            return;

        collected++;
        ShowMessage($"INGREDIENT  {collected}/{totalCollectibles}", 1.2f);
    }

    public void TryCompleteCourse()
    {
        if (state != RunState.Playing)
            return;

        if (collected < totalCollectibles)
        {
            ShowMessage($"COLLECT {totalCollectibles - collected} MORE!", 2f);
            return;
        }

        state = RunState.Cleared;
        player?.SetControlEnabled(false);
        if (bestTime <= 0f || runTime < bestTime)
        {
            bestTime = runTime;
            PlayerPrefs.SetFloat("KitchenCourseBestTime", bestTime);
            PlayerPrefs.Save();
        }
    }

    public void ShowMessage(string text, float duration)
    {
        message = text;
        messageUntil = Time.unscaledTime + duration;
    }

    private void HandleRespawn()
    {
        if (state != RunState.Playing)
            return;

        lives--;
        if (lives <= 0)
        {
            lives = 0;
            state = RunState.GameOver;
            player?.SetControlEnabled(false);
        }
        else
        {
            ShowMessage($"OUCH!  {lives} LIVES LEFT", 1.8f);
        }
    }

    private void OnGUI()
    {
        GUIStyle hud = new(GUI.skin.box)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        GUI.Box(new Rect(20f, 18f, 190f, 44f), $"TIME  {runTime:0.0}", hud);
        GUI.Box(new Rect(220f, 18f, 170f, 44f), $"LIVES  {lives}", hud);
        GUI.Box(new Rect(Screen.width - 250f, 18f, 230f, 44f), $"INGREDIENTS  {collected}/{totalCollectibles}", hud);

        GUIStyle center = CenterStyle(42);
        if (state == RunState.Countdown)
            GUI.Label(new Rect(0f, Screen.height * .35f, Screen.width, 100f), Mathf.Max(1, Mathf.CeilToInt(countdown)).ToString(), center);
        else if (state == RunState.Paused)
            DrawOverlay("PAUSED", "ESC  Continue");
        else if (state == RunState.Cleared)
            DrawOverlay("COURSE CLEAR!", $"{runTime:0.0}s   BEST {bestTime:0.0}s\nR  Play Again");
        else if (state == RunState.GameOver)
            DrawOverlay("GAME OVER", "R  Try Again");

        if (!string.IsNullOrEmpty(message) && Time.unscaledTime < messageUntil)
            GUI.Label(new Rect(0f, Screen.height * .68f, Screen.width, 70f), message, center);

        GUIStyle help = CenterStyle(16);
        GUI.Label(new Rect(0f, Screen.height - 42f, Screen.width, 30f),
            "WASD / Arrow Keys  Move   |   ESC  Pause   |   R  Restart", help);
    }

    private void DrawOverlay(string title, string subtitle)
    {
        GUI.Box(new Rect(Screen.width * .5f - 250f, Screen.height * .5f - 100f, 500f, 200f), GUIContent.none);
        GUI.Label(new Rect(Screen.width * .5f - 240f, Screen.height * .5f - 75f, 480f, 70f), title, CenterStyle(38));
        GUI.Label(new Rect(Screen.width * .5f - 240f, Screen.height * .5f, 480f, 70f), subtitle, CenterStyle(20));
    }

    private static GUIStyle CenterStyle(int fontSize)
    {
        GUIStyle style = new(GUI.skin.label)
        {
            fontSize = fontSize,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        style.normal.textColor = Color.white;
        return style;
    }
}
