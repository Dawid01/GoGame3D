using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CubeGameboard : Gameboard
{
    [SerializeField] private Gameboard[] additionalBoards;
    public Gameboard topBoard;
    public Gameboard downBoard;
    public Gameboard rightBoard;
    public Gameboard leftBoard;
    private Gameboard[,] _gameboards;

    public List<Slot> _slots;

    public override void InitializeGameboard(BoardSize _boardSize)
    {
        _slots = new List<Slot>();
        _gameboards = new Gameboard[,]
        {
            { null, leftBoard, null },
            { topBoard, this, downBoard },
            { null, rightBoard, null }
        };
        base.InitializeGameboard(_boardSize);

        float scale = size + 0.15f;
        background.localScale = new Vector3(scale * 0.999f, scale * 0.999f, scale * 0.999f);
        background.localPosition = new Vector3(0f, -scale / 2f, 0f);
        transform.localPosition = Vector3.zero;
        transform.position += Vector3.up * scale / 2f;

        for (int i = 0; i < additionalBoards.Length; i++)
        {
            Gameboard additionalBoard = additionalBoards[i];
            additionalBoard.InitializeGameboard(_boardSize);
            additionalBoard.transform.localPosition += additionalBoard.transform.up * scale / 2f;
            additionalBoard.transform.localPosition -= Vector3.up * scale / 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    _slots.Add(additionalBoard.slots[x, y]);
                }
            }
        }

        InitializeNeighbours();
        for (int i = 0; i < additionalBoards.Length; i++)
        {
            additionalBoards[i].InitializeNeighbours();
        }
    }

    public override void InitializeSlots()
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
                _slots.Add(slot);
            }
        }

        //InitializeNeighbours();
    }


    public override void InitializeNeighbours()
    {
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Slot slot = slots[x, y];

                for (int i = 0; i < neighboursVectors.Length; i++)
                {
                    Vector2Int boardPosition = new Vector2Int(x, y) + neighboursVectors[i];
                    if (IsSlotExist(boardPosition))
                    {
                        slot.neighboringSlots.Add(slots[boardPosition.x, boardPosition.y]);
                    }
                    else
                    {
                        Vector2Int vec = new Vector2Int(1, 1) + neighboursVectors[i];
                        Gameboard neighbourGameboard = _gameboards[vec.x, vec.y];
                        Vector2Int boardPos = new Vector2Int((boardPosition.x + size) % size,
                            (boardPosition.y + size) % size);
                        slot.neighboringSlots.Add(neighbourGameboard.slots[boardPos.x, boardPos.y]);
                    }
                }
            }
        }
    }


    public override void ClearBoard()
    {
        base.ClearBoard();
        if (_slots == null)
        {
            _slots = new List<Slot>();
            return;
        }

        _slots.Clear();
    }

    public override string CalculateChecksum(Slot clickedSlot = null, StoneColor stoneColor = StoneColor.WHITE)
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

        for (int i = 0; i < additionalBoards.Length; i++)
        {
            foreach (Slot slot in additionalBoards[i].slots)
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
        }

        return checksum;
    }

    /*public virtual ulong CalculateChecksum(Slot clickedSlot = null, StoneColor stoneColor = StoneColor.WHITE)
    {
        ulong  checksum = 0;
        if (clickedSlot)
        {
            clickedSlot = GetMainSlot(clickedSlot);
        }

        foreach (Slot slot in slots)
        {
            ulong  num = (ulong)slot.boardPosition.x * (ulong)slot.boardPosition.y;
            if (slot == clickedSlot && slot.IsEmpty())
            {
                num += (ulong)stoneColor;
            }else if (!slotsToClear.Contains(slot))
            {
                if (!slot.IsEmpty())
                {
                    num += (ulong)slot.currentStone.stoneColor;
                }
            }

            checksum ^= num * 2654435761;
        }

        for (int i = 0; i < additionalBoards.Length; i++)
        {
            ulong x = (ulong)(i + 1);
            checksum += additionalBoards[i].CalculateChecksum(clickedSlot, stoneColor) / x;
        }

        return checksum;
    }*/

    public override bool IsKO(Slot clickedSlot = null, StoneColor stoneColor = StoneColor.WHITE)
    {
        string xor = CalculateChecksum(clickedSlot, stoneColor);
        return ChecksumQueue.Peek().Equals(xor);
    }

    public override void UpdateChecksum()
    {
        ChecksumQueue.Dequeue();
        ChecksumQueue.Enqueue(CalculateChecksum());
        for (int i = 0; i < additionalBoards.Length; i++)
        {
            Gameboard additionalBoard = additionalBoards[i];
            additionalBoard.ChecksumQueue.Dequeue();
            additionalBoard.ChecksumQueue.Enqueue(additionalBoard.CalculateChecksum());
        }
    }

    public override Vector2Int CalculatePoints()
    {
        int whitePoints = 0;
        int blackPoints = 0;

        HashSet<Slot> visited = new HashSet<Slot>();

        for (int i = 0; i < _slots.Count(); i++)
        {
            Slot slot = _slots[i];
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


        visited.Clear();

        for (int i = 0; i < _slots.Count; i++)
        {
            Slot slot = _slots[i];
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

        return new Vector2Int(whitePoints, blackPoints);
    }
}