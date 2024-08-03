using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum GameMode
{
    NORMAL, LOOPING, CUBE, SPHERE
}



public class GameMgr : Singleton<GameMgr>
{

    public GameMode currentGameMode;
    public BoardSize currentBoardSize;
    public Gameboard currentGameboard;
    public int whitePoints, blackPoints;
    public CameraController cameraController;
    [SerializeField] private Gameboard[] _gameboards;
    public PoolingSystem slotPoolingSystem;
    public PoolingSystem stonePoolingSystem;
    public Material whiteMaterial;
    public Material blackMaterial;
    public bool enableGUI = true;
    public GameObject pauseMenu;
    [HideInInspector] public  bool hasGameStarted = false;
    [SerializeField] private SpriteRenderer backgroundRenderer;

    [SerializeField] private TextMeshProUGUI whitePointsTextUI;
    [SerializeField] private TextMeshProUGUI blackPointsTextUI;
    
    public static Action OnGameInitialize;
    public static Action OnNextTurn;
    private float _deltaTime = 0.0f;
    private Camera mainCamera;
    [SerializeField] private GameObject gameboardObject;
    [SerializeField] private Image splashScreenImage;
    [HideInInspector] public bool singerPlayer;
    [SerializeField] private Toggle aiModeToggle;

    public override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;
        gameboardObject.SetActive(false);
        FitBackground();

    }

    void Start()
    {
        Application.targetFrameRate = 60;
        //InitializeGame();
    }

    public void InitializeGame()
    {
        singerPlayer = aiModeToggle.isOn;
        splashScreenImage.DOKill();
        splashScreenImage.raycastTarget = true;
        splashScreenImage.DOFade(1f, 0.35f).OnComplete(() =>
        {
            UIMgr.Instance.DisactiveElement("Game Creator");
            currentGameboard?.ClearBoard();
            gameboardObject.SetActive(true);
            currentGameboard = _gameboards[(int)currentGameMode % _gameboards.Length];
            for (int i = 0; i < _gameboards.Length; i++)
            {
                _gameboards[i].gameObject.SetActive(false);
            }
            currentGameboard.gameObject.SetActive(true);
            currentGameboard.InitializeGameboard(currentBoardSize);
            cameraController.InitializeCamera(currentGameMode, currentBoardSize);
            OnGameInitialize?.Invoke();
            UpdatePoints(0, 0);
            FitBackground();
            hasGameStarted = true;
            splashScreenImage.DOFade(0f, 0.35f);
            splashScreenImage.raycastTarget = false;
        });
        
    }

    public void NextTurn()
    {
        if (currentGameboard)
        {
            currentGameboard.ClearBanedSlots();
            //gameboard.UpdateXORChecksum();
        }
        //OnNextTurn?.Invoke();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            enableGUI = !enableGUI;
        }
        _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
        if (hasGameStarted) PauseMenuActive();
        
        if (hasGameStarted)
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                Vector2Int result = currentGameboard.CalculatePoints();
                Debug.Log($"White: {result.x} Black: {result.y}");
            }
        }
    }

    void PauseMenuActive()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.SetActive(true);
        }
    }

    public void GameHasEnded()
    {
        splashScreenImage.raycastTarget = true;
        splashScreenImage.DOFade(1f, 0.35f).OnComplete(() =>
        {
            UIMgr.Instance.ActiveElement("Menu Panel");
            hasGameStarted = false;
            gameboardObject.SetActive(false);
            splashScreenImage.DOFade(0f, 0.35f);
            splashScreenImage.raycastTarget = false;
        });
        
    }

    public void UpdatePoints(int whitePoints, int blackPoints)
    {
        this.whitePoints = whitePoints;
        this.blackPoints = blackPoints;
        whitePointsTextUI.text = "" + whitePoints;
        blackPointsTextUI.text = "" + blackPoints;

    }

    private void FitBackground()
    {
        float cameraHeight;
        float cameraWidth;
        float spriteHeight = backgroundRenderer.sprite.bounds.size.y;
        float spriteWidth = backgroundRenderer.sprite.bounds.size.x;
        if (mainCamera.orthographic)
        {
            cameraHeight = mainCamera.orthographicSize * 2;
            cameraWidth = cameraHeight * mainCamera.aspect;
        }
        else
        {
            float scale = spriteHeight / spriteWidth;
            float distance = Vector3.Distance(mainCamera.transform.position, backgroundRenderer.transform.position);
            cameraWidth = 4 * distance * Mathf.Tan(Mathf.Deg2Rad * (mainCamera.fieldOfView / 2f));
            cameraHeight = scale * cameraWidth;
        }

        Vector3 spriteScale = backgroundRenderer.transform.localScale;
        spriteScale.x = cameraWidth / spriteWidth;
        spriteScale.y = cameraHeight / spriteHeight;
        backgroundRenderer.transform.localScale = spriteScale;
    } 

    void OnGUI()
    {
        
        float fps = 1.0f / _deltaTime;
        GUILayout.BeginArea(new Rect(10, Screen.height - 30, 100, 20));
        GUILayout.Label("FPS: " + Mathf.Ceil(fps));
        GUILayout.EndArea();
        
        return;
        if (!enableGUI) return;

        GUIStyle guiStyle = new GUIStyle(GUI.skin.button);
        guiStyle.fontSize = 14;

        GUILayout.BeginArea(new Rect(10, 10, 500, 50)); 
        GUILayout.BeginHorizontal();

        string[] boardSizeOptions = { "9x9", "13x13", "19x19", "5x5", "7x7" };
        currentBoardSize = (BoardSize)GUILayout.SelectionGrid((int)currentBoardSize, boardSizeOptions, 5, guiStyle);

        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        GUILayout.BeginArea(new Rect(10, 40, 330, 90)); 
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();

        string[] gameModeOptions = { "NORMAL", "LOOPING", "CUBE", "SPHERE" };
        currentGameMode = (GameMode)GUILayout.SelectionGrid((int)currentGameMode, gameModeOptions, 4, guiStyle);

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.EndArea();

        if (GUI.Button(new Rect(10, 80, 120, 40), "Generate")) 
        {
            InitializeGame();
        }
        
    }

}
