using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : Singleton<PlayerController>
{
    public StoneColor stoneColor;
    private RaycastHit _hit;
    [SerializeField] private Transform _previewStone;
    private Gameboard _gameboard;
    [SerializeField] private AudioClip _stonePutClip;
    private Camera _mainCamera;

    private void Start()
    {
        _mainCamera = Camera.main;
        _previewStone.GetChild(0).gameObject.SetActive(stoneColor == StoneColor.WHITE);
        _previewStone.GetChild(1).gameObject.SetActive(stoneColor != StoneColor.WHITE);
        
        // if(InputMgr.Instance.isMobile)
        //     _previewStone.gameObject.SetActive(false);
        
        GameMgr.OnGameInitialize += OnGameInitialize;
        InputMgr.OnLongTouch += RemoveStone;
    }

    private void OnDestroy()
    {
        GameMgr.OnGameInitialize -= OnGameInitialize;
        InputMgr.OnLongTouch -= RemoveStone;
    }

    private void OnGameInitialize()
    {
        _gameboard = GameMgr.Instance.currentGameboard;
        if (stoneColor == StoneColor.WHITE)
        {
            ChangePlayer();
        }
    }

    void Update()
    {
        if (!GameMgr.Instance.hasGameStarted) return;
        if(GameMgr.Instance.isPause) return;

        if (InputMgr.Instance.InputDown && !InputMgr.Instance.isMobile || InputMgr.Instance.InputUp && InputMgr.Instance.isMobile && !InputMgr.Instance.AfterDragging)
        {
            if (!GameMgr.Instance.currentGameboard) return;
            if (GameMgr.Instance.currentGameboard.isSinglePlayer && stoneColor == StoneColor.WHITE)
            {
                return;
            }

            if (EventSystem.current.IsPointerOverGameObject())
            {
                _previewStone.gameObject.SetActive(false);
                return;
            }

            if (Physics.Raycast(_mainCamera.ScreenPointToRay(InputMgr.Instance.InputPosition), out _hit, Mathf.Infinity,
                    LayerMask.GetMask("Slot", "Sphere")))
            {
                if (_hit.transform.TryGetComponent(out Slot slot))
                {
                    if (slot.IsEmpty())
                    {
                        if (_gameboard.CanPutStone(slot, stoneColor))
                        {
                            slot.InitializeStone(stoneColor);
                            GameMgr.Instance.NextTurn();
                            _gameboard.UpdateChecksum();
                            _gameboard.ClearGroup(stoneColor);
                            AudioMgr.Instance.PlayAudio(AudioMgr.Instance.putAudioClip);

                            if (!_gameboard.isSinglePlayer)
                            {
                                ChangePlayer();
                            }
                            else
                            {
                                _gameboard.HandleAI();
                            }
                        }
                        else
                        {
                            _gameboard.ShakeNeighbours(slot);
                            _previewStone.DOKill(true);
                            _previewStone.DOShakePosition(0.25f, new Vector3(1f, 0f, 1f) * 0.1f, 15, 90f);
                        }
                    }
                    else
                    {
                        slot.placedStone.Shake();
                        _previewStone.DOKill(true);
                        _previewStone.DOShakePosition(0.25f, new Vector3(1f, 0f, 1f) * 0.1f, 15, 90f);
                    }
                }
            }
        }

        if (InputMgr.Instance.IsDragging || InputMgr.Instance.AfterDragging)
        {
            _previewStone.gameObject.SetActive(false);
            return;
        }

        if (Physics.Raycast(_mainCamera.ScreenPointToRay(InputMgr.Instance.InputPosition), out _hit, Mathf.Infinity,
                LayerMask.GetMask("Slot", "Sphere")))
        {
            if (_hit.transform.TryGetComponent(out Slot slot))
            {
                _previewStone.gameObject.SetActive(slot.IsEmpty());
                _previewStone.position = slot.slotAnchor.position;
                _previewStone.rotation = slot.slotAnchor.rotation;
            }
            else
            {
                _previewStone.gameObject.SetActive(false);
            }
        }
        else
        {
            _previewStone.gameObject.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ChangePlayer();
        }

        if (Input.GetMouseButtonDown(2))
        {
            RemoveStone();
        }
    }

    private void RemoveStone()
    {
        if (!GameMgr.Instance.currentGameboard) return;

        if (Physics.Raycast(_mainCamera.ScreenPointToRay(InputMgr.Instance.InputPosition), out _hit, Mathf.Infinity,
                LayerMask.GetMask("Slot", "Sphere")))
        {
            if (_hit.transform.TryGetComponent(out Slot slot))
            {
                if (!slot.IsEmpty())
                {
                    _gameboard.UpdateChecksum();
                    slot.ClearSlot();
                }
            }
        }
    }

    public void ChangePlayer()
    {
        stoneColor = stoneColor == StoneColor.BLACK ? StoneColor.WHITE : StoneColor.BLACK;
        _previewStone.GetChild(0).gameObject.SetActive(stoneColor == StoneColor.WHITE);
        _previewStone.GetChild(1).gameObject.SetActive(stoneColor != StoneColor.WHITE);
        if (GameMgr.Instance.currentGameboard.isSinglePlayer && stoneColor == StoneColor.WHITE)
        {
            _previewStone.GetChild(0).gameObject.SetActive(false);
        }
        UIMgr.Instance.SetPlayerBorderColor(stoneColor, true);
    
        // if(InputMgr.Instance.isMobile)
        //     _previewStone.gameObject.SetActive(false);
    }
}
