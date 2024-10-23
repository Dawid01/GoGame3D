using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;


[Serializable]
struct CameraSettings
{
    public bool canZoom;
    public bool canMove;
    public bool canRotate;
}

public class CameraController : MonoBehaviour
{
    private GameMode _currentGameMode;
    private BoardSize _currentBoardSize;
    [SerializeField] private CameraSettings _cameraSettings;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private LayerMask _dragLayer;
    [SerializeField] private Transform _gameboard;
    public float zoomSensitivity = 0.25f;
    public float dragSensitivity = 5f;
    public float rotateSensitivity = 5f;

    private float _defaultCameraSize;
    private float _defaultCameraDistance = 20f;
    private float _zoomFactor = 1f;
    private float _minZoomFactor = 0.25f;
    private float _maxZoomFactor = 1f;
    private Vector3 _previousMousePosition = Vector3.zero;
    private bool _isDragging = false;
    private RaycastHit _hit;
    private int _boardSize;
    private float _lastClickTime = 0;
    private float _initialFieldOfView = 0;

    private void Awake()
    {
        //Cursor.lockState = CursorLockMode.Confined;
        _initialFieldOfView = _mainCamera.fieldOfView;
    }

    public void InitializeCamera(GameMode gameMode, BoardSize boardSize)
    {
        _currentGameMode = gameMode;
        _currentBoardSize = boardSize;
        transform.localPosition = transform.localPosition.With(x: 0f, z: 0f);
        _zoomFactor = 1f;
        _boardSize = GameMgr.Instance.currentGameboard.size;
        switch (gameMode)
        {
            case GameMode.NORMAL:
                _mainCamera.orthographic = true;
                _defaultCameraSize = _boardSize * 0.525f;
                _mainCamera.orthographicSize = _defaultCameraSize;
                _cameraSettings.canZoom = false;
                _cameraSettings.canMove = false;
                _cameraSettings.canRotate = false;
                _mainCamera.transform.parent.localPosition = _mainCamera.transform.localPosition.With(y: _defaultCameraDistance);
                break;
            case GameMode.LOOPING:
                _mainCamera.orthographic = true;
                _defaultCameraSize = _boardSize * 0.475f;
                _mainCamera.orthographicSize = _defaultCameraSize;
                _cameraSettings.canZoom = true;
                _cameraSettings.canMove = true;
                _cameraSettings.canRotate = false;
                _mainCamera.transform.parent.localPosition = _mainCamera.transform.localPosition.With(y: _defaultCameraDistance);
                break;
            case GameMode.CUBE:
                _mainCamera.orthographic = false;
                _cameraSettings.canZoom = false;
                _cameraSettings.canMove = false;
                _cameraSettings.canRotate = true;
                _mainCamera.transform.parent.localPosition = _mainCamera.transform.localPosition.With(y: (_defaultCameraDistance / 9f) * _boardSize);
                break;
            case GameMode.SPHERE:
                _mainCamera.orthographic = false;
                _cameraSettings.canZoom = false;
                _cameraSettings.canMove = false;
                _cameraSettings.canRotate = true;
                _mainCamera.transform.parent.localPosition = _mainCamera.transform.localPosition.With(y: (_defaultCameraDistance / 9f) * _boardSize);
                break;
        }
    }

    private void Update()
    {
        
        if (GameMgr.Instance.hasGameStarted)
        {
            HandleZoom();
            HandleMovement();
            HandleRotation();
        }
        else
        {
            _zoomFactor = 1f;
            _mainCamera.orthographicSize = _defaultCameraSize * _zoomFactor;
        }
    }

    void HandleZoom()
    {
        if(!_cameraSettings.canZoom) return;
        if (InputMgr.Instance.isMobile)
        {
           // _zoomFactor -= InputMgr.Instance.Zoom * zoomSensitivity;
        }
        else
        {
            _zoomFactor -= Input.mouseScrollDelta.y * zoomSensitivity;
        }

        _zoomFactor = Math.Clamp(_zoomFactor, _minZoomFactor, _maxZoomFactor);
        _mainCamera.orthographicSize = Mathf.Lerp(_mainCamera.orthographicSize, _defaultCameraSize * _zoomFactor, Time.deltaTime * 10f);
    }
    void HandleMovement()
    {
        if(!_cameraSettings.canMove) return;

        if (!InputMgr.Instance.isMobile)
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (Physics.Raycast(_mainCamera.ScreenPointToRay(InputMgr.Instance.InputPosition), out _hit,
                        Mathf.Infinity, _dragLayer))
                {
                    _isDragging = true;
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    _previousMousePosition = _hit.point;
                }
            }

            if (Input.GetMouseButtonUp(1))
            {
                if (_isDragging)
                {
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
                    _isDragging = false;
                }
            }

            if (_isDragging && Input.GetMouseButton(1))
            {
                if (Physics.Raycast(_mainCamera.ScreenPointToRay(InputMgr.Instance.InputPosition), out _hit,
                        Mathf.Infinity, _dragLayer))
                {
                    Vector3 pos = _previousMousePosition - _hit.point;
                    Vector3 move = new Vector3(pos.x * dragSensitivity, 0f, pos.z * dragSensitivity);
                    transform.Translate(move, Space.World);
                }
            }
        }
        else
        {
            if (InputMgr.Instance.IsDragging)
            {
                if (Physics.Raycast(_mainCamera.ScreenPointToRay(InputMgr.Instance.InputPosition), out _hit,
                        Mathf.Infinity, _dragLayer))
                {
                    _isDragging = true;
                    //Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    _previousMousePosition = _hit.point;
                }
            }

            if (InputMgr.Instance.IsDragging)
            {
                if (_isDragging)
                {
                    //Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
                    _isDragging = false;
                }
            }

            if (_isDragging && InputMgr.Instance.IsDragging)
            {
                if (Physics.Raycast(_mainCamera.ScreenPointToRay(InputMgr.Instance.InputPosition), out _hit,
                        Mathf.Infinity, _dragLayer))
                {
                    Vector3 pos = _previousMousePosition - _hit.point;
                    Vector3 move = new Vector3(pos.x * dragSensitivity, 0f, pos.z * dragSensitivity);
                    transform.Translate(move, Space.World);
                }
            }
        }

        ConstrainCameraMovement();
       
    }
    
    void HandleRotation()
    {
        if(!_cameraSettings.canRotate) return;

        if (Input.GetMouseButtonDown(1) || InputMgr.Instance.isMobile && InputMgr.Instance.IsDragging)
        {
            if (Physics.Raycast(_mainCamera.ScreenPointToRay(InputMgr.Instance.InputPosition), out _hit, Mathf.Infinity, _dragLayer))
            {
                _isDragging = true;
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                _previousMousePosition = _hit.point;
            }
        }

        if (Input.GetMouseButtonUp(1) || InputMgr.Instance.isMobile && !InputMgr.Instance.IsDragging)
        {
            if (_isDragging)
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
                _isDragging = false;
            }
        }

        if (_isDragging && Input.GetMouseButton(1) || InputMgr.Instance.isMobile && InputMgr.Instance.IsDragging && _isDragging)
        {
            if (Physics.Raycast(_mainCamera.ScreenPointToRay(InputMgr.Instance.InputPosition), out _hit, Mathf.Infinity, _dragLayer))
            {
                Vector3 pos = _previousMousePosition - _hit.point;
                Vector3 move = new Vector3(pos.x * rotateSensitivity, 0f, pos.z * rotateSensitivity);
                Vector3 right = Vector3.Cross(transform.up, _gameboard.position - transform.position);
                Vector3 up = Vector3.Cross(_gameboard.position - transform.position, right);
                _gameboard.rotation = Quaternion.AngleAxis(move.x, up) * _gameboard.rotation;
                _gameboard.rotation = Quaternion.AngleAxis(-move.z, right) * _gameboard.rotation;
                _previousMousePosition = _hit.point;
            }
        }

        if (IsDoubleClick())
        {
            if (Physics.Raycast(_mainCamera.ScreenPointToRay(InputMgr.Instance.InputPosition), out _hit, Mathf.Infinity,
                    LayerMask.GetMask("Slot")))
            {
                if (_hit.transform.TryGetComponent(out Slot slot))
                {
                    Transform board = slot.transform.parent.parent;
                    _gameboard.DOKill();
                    _gameboard.DORotateQuaternion(Quaternion.Inverse(board.localRotation), 0.25f);
                }

            }
            
        }
        
    }
    
    private void ConstrainCameraMovement()
    {
        float localX = transform.localPosition.x;
        float localY = transform.localPosition.z;
        float absX = Mathf.Abs(localX);
        float absY = Mathf.Abs(localY);
        float newX = 0f;
        float newY = 0f;

        if (absX >= _boardSize / 2f)
        {
            float scale = localX >= 0 ? 1f : -1f;
            newX = (_boardSize) * scale;
        }
        
        if (absY >= _boardSize / 2f)
        {
            float scale = localY >= 0 ? 1f : -1f;
            newY = (_boardSize) * scale;
        }

        _previousMousePosition = _previousMousePosition.With(x: _previousMousePosition.x - newX, y: _previousMousePosition.y - newY);
        transform.localPosition = transform.localPosition.With(x: localX - newX, z: localY - newY);
    }

    private bool IsDoubleClick()
    {
        bool doubleClick = false;
        if (Input.GetMouseButtonDown(1))
        {
            doubleClick = Time.time - _lastClickTime < 0.3f;
            _lastClickTime = Time.time;
        }

        return doubleClick;
    }
}
