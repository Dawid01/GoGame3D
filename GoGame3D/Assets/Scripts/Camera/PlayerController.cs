using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class PlayerController : Singleton<PlayerController>
{
    public StoneColor stoneColor;
    private RaycastHit _hit;
    [SerializeField] private Transform _previewStone;
    private Gameboard _gameboard;
    [SerializeField] private AudioClip _stonePutClip;

    private void Start()
    {
        _previewStone.GetChild(0).gameObject.SetActive(stoneColor == StoneColor.WHITE);
        _previewStone.GetChild(1).gameObject.SetActive(stoneColor != StoneColor.WHITE);
        GameMgr.OnGameInitialize += OnGameInitialize;
        
    }
    
    private void OnDestroy()
    {
        GameMgr.OnGameInitialize -= OnGameInitialize;
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
        if(!GameMgr.Instance.hasGameStarted) return;
        
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ChangePlayer();
        }
        
        if (EventSystem.current.IsPointerOverGameObject())
        {
            _previewStone.gameObject.SetActive(false);
            return;
        }
        
        if (!Input.GetMouseButton(1))
        {

            if (Input.GetMouseButtonDown(0))
            {
                if(!GameMgr.Instance.currentGameboard) return;
                if (GameMgr.Instance.currentGameboard.isSinglePlayer && stoneColor == StoneColor.WHITE)
                {
                    return;
                }

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _hit, Mathf.Infinity,
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
            
            // if (Input.GetKeyDown(KeyCode.T))
            // {
            //     if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _hit, Mathf.Infinity,
            //             LayerMask.GetMask("Slot", "Sphere")))
            //     {
            //         if (_hit.transform.TryGetComponent(out Slot slot))
            //         {
            //             slot.DisableNighbours();
            //         }
            //     }
            // }
            
            if (Input.GetMouseButtonDown(2))
            {
                if(!GameMgr.Instance.currentGameboard) return;

                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _hit, Mathf.Infinity,
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

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _hit, Mathf.Infinity,
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
        }
        else
        {
            _previewStone.gameObject.SetActive(false);
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
       
    }
}
