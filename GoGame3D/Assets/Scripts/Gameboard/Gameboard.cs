using System;
using System.Collections;
using System.Collections.Generic;
// using Unity.Android.Types;
using UnityEngine;
using UnityEngine.Serialization;

public enum BoardSize
{
    S9X9,
    S13X13,
    S19X19,
    S5X5,
    S7X7
}

public class Gameboard : MonoBehaviour
{
    public int size;
    private BoardSize _currentBoardSize;
    public Transform slotsAnchor;
    public Transform background;
    public Slot[,] slots;

    private List<Slot> _checkedSlots;
    [HideInInspector] public List<Slot> slotsToClear;
    private List<Slot> _banedSlots;

    public Queue<string> ChecksumQueue;

    public bool isSinglePlayer;
    public AIManager aiManager;

    [HideInInspector] public Vector2Int[] neighboursVectors =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
    };


    public virtual void InitializeGameboard(BoardSize _boardSize)
    {
        ClearBoard();
        transform.parent.localEulerAngles = Vector3.zero;
        _checkedSlots = new List<Slot>();
        slotsToClear = new List<Slot>();
        _banedSlots = new List<Slot>();
        isSinglePlayer = GameMgr.Instance.singerPlayer;


        if (isSinglePlayer == true)
        {
            Debug.Log("Singer Player");
            aiManager = new StupidAI();
            aiManager.InitializeAI();
        }

        this._currentBoardSize = _boardSize;
        switch (_boardSize)
        {
            case BoardSize.S5X5:
                size = 5;
                break;
            case BoardSize.S7X7:
                size = 7;
                break;
            case BoardSize.S9X9:
                size = 9;
                break;
            case BoardSize.S13X13:
                size = 13;
                break;
            case BoardSize.S19X19:
                size = 19;
                break;
        }

        slots = new Slot[size, size];
        background.localScale = new Vector3(size - 0.5f, 0.1f, size - 0.5f);

        InitializeSlots();
        ChecksumQueue = new Queue<string>();
        ChecksumQueue.Enqueue("");
        ChecksumQueue.Enqueue("");
    }

    public virtual void ClearBoard()
    {
        if (slots == null) return;
        if (slots.Length == 0) return;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Slot slot = slots[x, y];
                GameMgr.Instance.slotPoolingSystem.Despawn(slot);
            }
        }
    }

    public virtual void InitializeSlots()
    {
        float startPositionX = -((float)size / 2f) + 0.5f;
        float startYpositionY = ((float)size / 2f) - 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2Int boardPostion = new Vector2Int(x, y);
                Slot slot = GameMgr.Instance.slotPoolingSystem.Spawn().GetComponent<Slot>();
                slot.Initialize(boardPostion, IsHoshiPoint(boardPostion));
                slots[x, y] = slot;
                slot.transform.parent = slotsAnchor;
                slot.transform.localEulerAngles = Vector3.zero;
                slot.transform.localPosition = new Vector3(startPositionX + x, 0f, startYpositionY - y);
            }
        }

        InitializeNeighbours();
    }

    public virtual void InitializeNeighbours()
    {
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Slot slot = slots[x, y];

                for (int i = 0; i < neighboursVectors.Length; i++)
                {
                    Vector2Int boardPostion = new Vector2Int(x, y) + neighboursVectors[i];
                    if (IsSlotExist(boardPostion))
                    {
                        slot.neighboringSlots.Add(slots[boardPostion.x, boardPostion.y]);
                    }
                }
            }
        }
    }

    public bool IsSlotExist(Vector2Int boardPosition)
    {
        return boardPosition.x >= 0 && boardPosition.y >= 0 && boardPosition.x < size && boardPosition.y < size;
    }

    public bool IsHoshiPoint(Vector2Int boardPosition)
    {
        switch (_currentBoardSize)
        {
            case BoardSize.S9X9:
                return ((boardPosition.x == 2 || boardPosition.x == 6) &&
                        (boardPosition.y == 2 || boardPosition.y == 6)) ||
                       boardPosition.x == 4 && boardPosition.y == 4;
            case BoardSize.S13X13:
                return ((boardPosition.x == 3 || boardPosition.x == 9) &&
                        (boardPosition.y == 3 || boardPosition.y == 9)) ||
                       boardPosition.x == 6 && boardPosition.y == 6;
            case BoardSize.S19X19:
                return (boardPosition.x == 3 || boardPosition.x == 9 || boardPosition.x == 15) &&
                       (boardPosition.y == 3 || boardPosition.y == 9 || boardPosition.y == 15);
            default:
                return false;
        }
    }

    public int GetBoardSize()
    {
        return size;
    }

    public void HandleAI()
    {
        aiManager.MakeMove();
    }

    public bool CanPutStone(Slot slot, StoneColor stoneColor)
    {
        if (!slot.IsEmpty()) return false;

        //if(_banedSlots.Contains(slot)) return false;
        _checkedSlots.Clear();
        if (CanHaveLiberty(slot, stoneColor))
        {
            CheckGroupsForCapture(slot, stoneColor);
            return true;
        }

        if (CheckGroupsForCapture(slot, stoneColor))
        {
            if (!IsKO(slot, stoneColor))
            {
                return true;
            }

            slotsToClear.Clear();
            return false;
        }

        slotsToClear.Clear();
        return false;
    }

    public bool CheckGroupsForCapture(Slot slot, StoneColor stoneColor)
    {
        slot = GetMainSlot(slot);
        bool canClear = false;
        for (int i = 0; i < slot.neighboringSlots.Count; i++)
        {
            Slot n = slot.neighboringSlots[i];
            if (n.IsEmpty()) continue;
            if (n.placedStone.stoneColor != stoneColor)
            {
                _checkedSlots.Clear();
                _checkedSlots.Add(slot);
                if (!CanHaveLiberty(n, n.placedStone.stoneColor))
                {
                    slotsToClear.AddRange(_checkedSlots);
                    canClear = true;
                }
            }
        }

        return canClear;
    }

    private bool CanHaveLiberty(Slot slot, StoneColor stoneColor)
    {
        _checkedSlots.Add(slot);
        slot = GetMainSlot(slot);

        for (int i = 0; i < slot.neighboringSlots.Count; i++)
        {
            Slot n = slot.neighboringSlots[i];
            if (!_checkedSlots.Contains(n))
            {
                if (n.IsEmpty()) return true;
                if (n.placedStone.stoneColor == stoneColor)
                {
                    if (CanHaveLiberty(n, stoneColor))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public void ShakeNeighbours(Slot slot)
    {
        slot = GetMainSlot(slot);

        for (int i = 0; i < slot.neighboringSlots.Count; i++)
        {
            Slot n = slot.neighboringSlots[i];
            if (n.IsEmpty()) continue;
            n.placedStone.Shake();
            for (int j = 0; j < n.additionalSlots.Count; j++)
            {
                n.additionalSlots[j].placedStone.Shake();
            }
        }
    }

    public Slot GetMainSlot(Slot slot)
    {
        if (!slot.isMainSlot)
        {
            return slots[slot.boardPosition.x, slot.boardPosition.y];
        }

        return slot;
    }

    public void ClearGroup(StoneColor stoneColor)
    {
        if (slotsToClear.Count == 2)
        {
            _banedSlots.Add(slotsToClear[1]);
            _banedSlots.AddRange(slotsToClear[1].additionalSlots);
        }

        for (int i = 0; i < slotsToClear.Count; i++)
        {
            Slot slot = GetMainSlot(slotsToClear[i]);
            if (slot == null)
            {
                continue;
            }

            if (slot.placedStone == null)
            {
                continue;
            }

            if (slot.placedStone.stoneColor != stoneColor)
            {
                slot.ClearSlot(0f);
            }
        }

        slotsToClear.Clear();
        //AudioMgr.Instance.PlayAudio(AudioMgr.Instance.dropAudioClip);
    }

    public void ClearBanedSlots()
    {
        _banedSlots.Clear();
    }

    public virtual void UpdateChecksum()
    {
        ChecksumQueue.Dequeue();
        ChecksumQueue.Enqueue(CalculateChecksum());
    }

    public virtual string CalculateChecksum(Slot clickedSlot = null, StoneColor stoneColor = StoneColor.WHITE)
    {
        string checksum = "";
        if (clickedSlot)
        {
            clickedSlot = GetMainSlot(clickedSlot);
        }

        foreach (Slot slot in slots)
        {
            if (slot == clickedSlot && slot.IsEmpty())
            {
                checksum += $"{(int)stoneColor}";
            }
            else if (!slotsToClear.Contains(slot))
            {
                if (!slot.IsEmpty())
                {
                    checksum += $"{(int)slot.placedStone.stoneColor}";
                }
                else
                {
                    checksum += "0";
                }
            }
            else
            {
                checksum += "0";
            }
        }

        return checksum;
    }

    public virtual bool IsKO(Slot clickedSlot = null, StoneColor stoneColor = StoneColor.WHITE)
    {
        string xor = CalculateChecksum(clickedSlot, stoneColor);
        return ChecksumQueue.Peek().Equals(xor);
    }

    public virtual Vector2Int CalculatePoints()
    {
        int whitePoints = 0;
        int blackPoints = 0;

        HashSet<Slot> visited = new HashSet<Slot>();

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Slot slot = slots[x, y];
                if (!slot.IsEmpty() && !visited.Contains(slot))
                {
                    List<Slot> group = new List<Slot>();
                    bool isAlive = CanHaveLiberty(slot, slot.placedStone.stoneColor, group);

                    if (!isAlive)
                    {
                        if (slot.placedStone.stoneColor == StoneColor.BLACK)
                        {
                            whitePoints += group.Count;
                        }
                        else if (slot.placedStone.stoneColor == StoneColor.WHITE)
                        {
                            blackPoints += group.Count;
                        }

                        foreach (Slot s in group)
                        {
                            s.ClearSlot();
                        }
                    }

                    visited.UnionWith(group);
                }
            }
        }

        visited.Clear();

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Slot slot = slots[x, y];
                if (slot.IsEmpty() && !visited.Contains(slot))
                {
                    List<Slot> territoryGroup = new List<Slot>();
                    bool whiteBorder = false;
                    bool blackBorder = false;

                    CheckTerritory(slot, territoryGroup, ref whiteBorder, ref blackBorder);

                    if (whiteBorder && !blackBorder)
                    {
                        whitePoints += territoryGroup.Count;
                    }
                    else if (blackBorder && !whiteBorder)
                    {
                        blackPoints += territoryGroup.Count;
                    }

                    visited.UnionWith(territoryGroup);
                }
            }
        }

        return new Vector2Int(whitePoints, blackPoints);
    }

    public bool CanHaveLiberty(Slot slot, StoneColor stoneColor, List<Slot> group)
    {
        Stack<Slot> stack = new Stack<Slot>();
        stack.Push(slot);
        group.Add(slot);

        while (stack.Count > 0)
        {
            Slot current = stack.Pop();
            foreach (Slot neighbor in current.neighboringSlots)
            {
                if (neighbor.IsEmpty())
                {
                    return true;
                }

                if (neighbor.placedStone != null && neighbor.placedStone.stoneColor == stoneColor &&
                    !group.Contains(neighbor))
                {
                    stack.Push(neighbor);
                    group.Add(neighbor);
                }
            }
        }

        return false;
    }

    public void CheckTerritory(Slot slot, List<Slot> territoryGroup, ref bool whiteBorder, ref bool blackBorder)
    {
        Stack<Slot> stack = new Stack<Slot>();
        stack.Push(slot);
        territoryGroup.Add(slot);

        while (stack.Count > 0)
        {
            Slot current = stack.Pop();
            foreach (Slot neighbor in current.neighboringSlots)
            {
                if (!territoryGroup.Contains(neighbor))
                {
                    if (neighbor.IsEmpty())
                    {
                        stack.Push(neighbor);
                        territoryGroup.Add(neighbor);
                    }
                    else if (neighbor.placedStone != null)
                    {
                        if (neighbor.placedStone.stoneColor == StoneColor.WHITE)
                        {
                            whiteBorder = true;
                        }
                        else if (neighbor.placedStone.stoneColor == StoneColor.BLACK)
                        {
                            blackBorder = true;
                        }
                    }
                }
            }
        }
    }
}