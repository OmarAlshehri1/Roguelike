using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public BoardManager BoardManager;
    public PlayerController PlayerController;
    public UIDocument UIDoc;

    public TurnManager TurnManager { get; private set; }
    public int CurrentLevel => m_CurrentLevel;

    [SerializeField] private AudioClip gameOverSound;
    [SerializeField, Range(0f, 1f)]
    private float gameOverVolume = 1f;

    private int m_FoodAmount = 100;
    private int m_CurrentLevel = 0;
    private bool m_GameOver;

    private Label m_FoodLabel;
    private VisualElement m_GameOverPanel;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        TurnManager = new TurnManager();
        TurnManager.OnTick += OnTurnHappen;

        VisualElement root =
            UIDoc.rootVisualElement;

        m_FoodLabel =
            root.Q<Label>("FoodLabel");

        m_GameOverPanel =
            root.Q<VisualElement>("GameOverPanel");

        if (m_GameOverPanel != null)
        {
            m_GameOverPanel.style.display =
                DisplayStyle.None;
        }

        m_FoodLabel.text =
            "Food : " + m_FoodAmount;

        NewLevel();
    }

    private void Update()
    {
        if (m_GameOver &&
            Keyboard.current != null &&
            Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartGame();
        }
    }

    public void NewLevel()
    {
        m_CurrentLevel++;

        BoardManager.Clean();
        BoardManager.Init();

        PlayerController.enabled = true;

        PlayerController.Spawn(
            BoardManager,
            new Vector2Int(1, 1)
        );
    }

    private void OnTurnHappen()
    {
        ChangeFood(-1);
    }

    public void ChangeFood(int amount)
    {
        m_FoodAmount += amount;

        m_FoodLabel.text =
            "Food : " + m_FoodAmount;

        if (m_FoodAmount <= 0)
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        if (m_GameOver)
        {
            return;
        }

        m_GameOver = true;

        if (gameOverSound != null)
        {
            AudioSource.PlayClipAtPoint(
                gameOverSound,
                transform.position,
                gameOverVolume
            );
        }

        TurnManager.OnTick -= OnTurnHappen;
        PlayerController.enabled = false;

        if (m_GameOverPanel != null)
        {
            m_GameOverPanel.style.display =
                DisplayStyle.Flex;
        }
    }

    private void RestartGame()
    {
        m_GameOver = false;
        m_FoodAmount = 100;
        m_CurrentLevel = 0;

        m_FoodLabel.text =
            "Food : " + m_FoodAmount;

        if (m_GameOverPanel != null)
        {
            m_GameOverPanel.style.display =
                DisplayStyle.None;
        }

        TurnManager.OnTick -= OnTurnHappen;
        TurnManager.OnTick += OnTurnHappen;

        NewLevel();
    }
}
