using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Slot : PoolableObject
{
    [SerializeField] private GameObject hoshiMarker;
    [SerializeField] private GameObject[] lines;
    public Transform slotAnchor;
    public Vector2Int boardPosition;
    public bool isMainSlot;
    public bool isHoshiPoint;
    public List<Slot> neighboringSlots;
    public List<Slot> additionalSlots;
    public Stone placedStone;
    [SerializeField] private BoxCollider _boxCollider;

    public override void OnSpawn()
    {
        base.OnSpawn();
        _boxCollider.enabled = true;
        neighboringSlots = new List<Slot>();
        additionalSlots = new List<Slot>();
        
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i].SetActive(true);
        }
    }

    public override void OnDespawn()
    { 
        base.OnDespawn();

        if (!IsEmpty())
        {
            GameMgr.Instance.stonePoolingSystem.Despawn(placedStone);
            placedStone = null;
        }
        neighboringSlots.Clear();
        additionalSlots.Clear();
    }

    public void Initialize(Vector2Int boardPosition, bool isHoshi, bool mainSlot = true, bool disableCollider = false)
    {
        gameObject.SetActive(true);
        this.isMainSlot = mainSlot;
        this.boardPosition = boardPosition;
        this.isHoshiPoint = isHoshi;
        gameObject.name = $"({boardPosition.x}, {boardPosition.y})";
        //_boxCollider.enabled = !disableCollider;

        if (!this.isMainSlot)
        {
            GameMgr.Instance.currentGameboard.slots[boardPosition.x, boardPosition.y].additionalSlots.Add(this);
        }

        InitializeLines();
    }

    public void InitializeStone(StoneColor stoneColor)
    {
        if(!IsEmpty()) return;

        if (!isMainSlot)
        {
            Slot mainSlot = GameMgr.Instance.currentGameboard.slots[boardPosition.x, boardPosition.y];
            mainSlot.InitializeStone(stoneColor);
            return;
        }

        Stone stone = GameMgr.Instance.stonePoolingSystem.Spawn().GetComponent<Stone>();
        stone.Initialize(stoneColor).Show();
        placedStone = stone;
        Transform tStone = stone.transform;
        tStone.parent = slotAnchor;
        tStone.localPosition = Vector3.zero;
        tStone.localEulerAngles = Vector3.zero;

        if (additionalSlots.Count > 0)
        {
            for (int i = 0; i < additionalSlots.Count; i++)
            {
                Slot additionalSlot = additionalSlots[i];
                stone = GameMgr.Instance.stonePoolingSystem.Spawn().GetComponent<Stone>();
                stone.Initialize(stoneColor).Show();
                additionalSlot.placedStone = stone;
                tStone = stone.transform;
                tStone.parent = additionalSlot.slotAnchor;
                tStone.localPosition = Vector3.zero;
                tStone.localEulerAngles = Vector3.zero;
            }
        }
    }
    
    private void InitializeLines()
    {
        int size = GameMgr.Instance.currentGameboard.GetBoardSize();
        hoshiMarker.SetActive(isHoshiPoint);

        if (GameMgr.Instance.currentGameMode == GameMode.SPHERE)
        {
            hoshiMarker.SetActive(false);
            SpriteRenderer renderer;

            for (int i = 0; i < lines.Length; i++)
            {
                lines[i].SetActive(false);
                // renderer = lines[i].GetComponent<SpriteRenderer>();
                // renderer.color = Color.black;
                // renderer.sortingOrder = 0;
            }
        }

        // if (GameMgr.Instance.gameMode == GameMode.LOOPING)
        // {
        //     if (boardPosition.x == 0 || boardPosition.x == size - 1)
        //     {
        //         SpriteRenderer renderer = lines[2].GetComponent<SpriteRenderer>();
        //         renderer.color = Color.white;
        //         renderer.sortingOrder = 1;
        //         
        //         renderer = lines[3].GetComponent<SpriteRenderer>();
        //         renderer.color = Color.white;
        //         renderer.sortingOrder = 1;
        //     }
        //     
        //     if (boardPosition.y == 0 || boardPosition.y == size - 1)
        //     {
        //         SpriteRenderer renderer = lines[0].GetComponent<SpriteRenderer>();
        //         renderer.color = Color.white;
        //         renderer.sortingOrder = 1;
        //         
        //         renderer = lines[1].GetComponent<SpriteRenderer>();
        //         renderer.color = Color.white;
        //         renderer.sortingOrder = 1;
        //     }
        // }

        if(GameMgr.Instance.currentGameMode == GameMode.LOOPING || GameMgr.Instance.currentGameMode == GameMode.CUBE) return;
        if (boardPosition.x == 0)
        {
            lines[1].SetActive(false);
        }else if (boardPosition.x == size - 1)
        {
            lines[0].SetActive(false);
        }
        
        if (boardPosition.y == 0)
        {
            lines[2].SetActive(false);
        }else if (boardPosition.y == size - 1)
        {
            lines[3].SetActive(false);
        }
    }

    public void ClearSlot(float delay = 0f)
    {
        if(IsEmpty()) return;
        placedStone.Hide(() =>
        {
            placedStone = null;
        }, delay);

        if (isMainSlot)
        {
            for (int i = 0; i < additionalSlots.Count; i++)
            {
                additionalSlots[i].ClearSlot(delay);
            }
        }
    }

    public bool IsEmpty()
    {
        return !placedStone;
    }

    [ContextMenu("Nighbours")]
    public void DisableNighbours()
    {
        foreach (var slot in neighboringSlots)
        {
            slot.gameObject.SetActive(!slot.gameObject.activeSelf);
        }
    }
}
