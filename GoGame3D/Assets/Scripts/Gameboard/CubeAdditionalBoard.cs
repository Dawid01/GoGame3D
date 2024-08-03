using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeAdditionalBoard : Gameboard
{

    public Gameboard mainBoard;
    public Gameboard topBoard;
    public Gameboard downBoard;
    public Gameboard rightBoard;
    public Gameboard leftBoard;

    private Gameboard[,] _gameboards;

    [SerializeField] private float topRotation;
    [SerializeField] private float botRotation;
    [SerializeField] private float leftRotation;
    [SerializeField] private float rightRotation;

    
    
    Matrix4x4[] _rotationMatrices = new Matrix4x4[]
    {
        Matrix4x4.identity,
        Matrix4x4.Rotate(Quaternion.Euler(0, 0, 90)),
        Matrix4x4.Rotate(Quaternion.Euler(0, 0, -90)),
        Matrix4x4.Rotate(Quaternion.Euler(0, 0, 180))
    };
    
    public override void InitializeGameboard(BoardSize _boardSize)
    {
        _gameboards = new Gameboard[,]
        {
            { null, leftBoard, null },
            { topBoard, this, downBoard },
            { null, rightBoard, null }
        };
        base.InitializeGameboard(_boardSize);
        float scale = size;
        transform.localPosition = Vector3.zero;
        background.localScale = new Vector3(scale, 0.1f, scale);
       
    }

    public override void ClearBoard()
    {
        base.ClearBoard();
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
                        
                        if (neighbourGameboard == rightBoard || neighbourGameboard == leftBoard)
                        {
                            float a = neighbourGameboard == rightBoard ? rightRotation : leftRotation;
                            boardPos = RotatePosition(boardPos, a);
                        }
                        else
                        {
                            float a = neighbourGameboard == topBoard ? topRotation : botRotation;
                            boardPos = RotatePosition(boardPos, a);
                        }

                        Slot neighbourSlot = neighbourGameboard.slots[boardPos.x, boardPos.y];

                        slot.neighboringSlots.Add(neighbourSlot);
                    }
                }
            }
        }
    }


    private Vector2Int RotatePosition(Vector2Int pos,  float angle)
    {
        float angleRad = angle * Mathf.Deg2Rad;

        int centerX = size / 2;
        int centerY = size / 2;

        int newX = Mathf.RoundToInt((pos.x - centerX) * Mathf.Cos(angleRad) - (pos.y - centerY) * Mathf.Sin(angleRad) + centerX);
        int newY = Mathf.RoundToInt((pos.x - centerX) * Mathf.Sin(angleRad) + (pos.y - centerY) * Mathf.Cos(angleRad) + centerY);

        newX = Mathf.Clamp(newX, 0, size - 1);
        newY = Mathf.Clamp(newY, 0, size - 1);

        return new Vector2Int(newX, newY);
    }
    
    
    public override void InitializeSlots()
    {
        float startPositionX = -((float)size / 2f) + 0.5f;
        float startYpositionY = ((float)size / 2f) - 0.5f;
            
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool disableCollider = x == 0 || x == size - 1 || y == 0 || y == size - 1;
                Vector2Int boardPostion = new Vector2Int(x, y);
                Slot slot = GameMgr.Instance.slotPoolingSystem.Spawn().GetComponent<Slot>();
                slot.Initialize(boardPostion, IsHoshiPoint(boardPostion), true, disableCollider);
                /*if (!disableCollider)
                {
                    slots[x, y] = slot;
                }*/
                    
                slots[x, y] = slot;


                slot.transform.parent = slotsAnchor;
                slot.transform.localEulerAngles = Vector3.zero;
                slot.transform.localPosition = new Vector3(startPositionX + x, 0f, startYpositionY - y);
            }
        }
    }
    
    public override bool IsKO(Slot clickedSlot = null, StoneColor stoneColor = StoneColor.WHITE)
    {
        return mainBoard.IsKO();
    }
    
    public override void UpdateChecksum()
    {
        mainBoard.UpdateChecksum();
    }
}
