using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.Rendering;
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
    [SerializeField] private Camera _camera;
    [SerializeField] private Gameboard[] _gameboards;
    public PoolingSystem slotPoolingSystem;
    public PoolingSystem stonePoolingSystem;
    public Material whiteMaterial;
    public Material blackMaterial;
    public bool enableGUI = true;
    public GameObject pauseMenu;
    public  bool hasGameStarted = false;
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
    [HideInInspector] public bool isPause = false;

    public override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;
        gameboardObject.SetActive(false);
        FitBackground();

    }

    void Start()
    {
        bool isMobile = Application.isMobilePlatform;
        Application.targetFrameRate = isMobile ? 30 : 60;
        InitializeGame(true);
    }

    public void InitializeGame(bool onStart = false)
    {
        singerPlayer = aiModeToggle.isOn;
        splashScreenImage.DOKill();
        splashScreenImage.raycastTarget = true;
        isPause = false;
        UIMgr.Instance.SetPlayerBorderColor(StoneColor.BLACK, false);
        if(!onStart){
            splashScreenImage.DOFade(1f, 0.35f).OnComplete(() =>
            {
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
                //hasGameStarted = true;
                splashScreenImage.DOFade(0f, 0.35f);
                splashScreenImage.raycastTarget = false;
            });
        }
        else
        {
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
            //hasGameStarted = true;
            splashScreenImage.DOFade(0f, 0.35f);
            splashScreenImage.raycastTarget = false;
        }

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

    public void StartGame()
    {
        
        hasGameStarted = true;
        float targetX = (currentGameboard.size / 2f) * (_camera.orthographic ?  0.75f : 1.5f);
        float targetY = _camera.orthographic ?  -3f : -1f;
        gameboardObject.transform.DOKill();
        gameboardObject.transform.DOLocalMove(new Vector3(0f, targetY, 0f), 0.25f).SetEase(Ease.InOutSine);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            enableGUI = !enableGUI;
        }
        _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
        if (hasGameStarted) PauseMenuActive();
        float targetX = (currentGameboard.size / 2f) * (_camera.orthographic ?  0.75f : 1.5f);
        float targetY = _camera.orthographic ?  -3f : -1f;
        //float targetX = (currentGameboard.size / 2f) * 0.75f;
        

        if (hasGameStarted)
        {
            // if (Input.GetKeyDown(KeyCode.F))
            // {
            //     Vector2Int result = currentGameboard.CalculatePoints();
            //     Debug.Log($"White: {result.x} Black: {result.y}");
            // }
        }
        else
        {
            if (gameboardObject.transform.localPosition.x != targetX)
            {
                gameboardObject.transform.DOKill();
                gameboardObject.transform.localPosition = currentGameboard.transform.localPosition.With(x: targetX, y: targetY);
            }
        }
    }

    void PauseMenuActive()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPause)
            {
                UIMgr.Instance.DisactiveElementAnimation("Pause Menu");
            }
            else
            {
                UIMgr.Instance.ActiveElementAnimation("Pause Menu");
            }

            isPause = !isPause;
        }
    }

    public void GameHasEnded()
    {
        splashScreenImage.raycastTarget = true;
        UIMgr.Instance.SetPlayerBorderColor(StoneColor.BLACK, false);
        splashScreenImage.DOFade(1f, 0.35f).OnComplete(() =>
        {
            UIMgr.Instance.DisactiveElement("Result Panel");
            UIMgr.Instance.DisactiveElement("Confirm");
            UIMgr.Instance.ActiveElementAnimation("Menu Panel");

            hasGameStarted = false;
            gameboardObject.SetActive(false);
            splashScreenImage.DOFade(0f, 0.35f);
            splashScreenImage.raycastTarget = false;
            InitializeGame(true);
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
    }

    public void ResumeGame()
    {
        isPause = false;
    }

}
