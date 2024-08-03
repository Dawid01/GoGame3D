using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AdditionalBoard : Gameboard
{
    
    [SerializeField] private LineRenderer _border;

    private Vector3[] _borderPositions = new[]
    {
        new Vector3(1, 0, 1),
        new Vector3(-1, 0, 1),
        new Vector3(-1, 0, -1),
        new Vector3(1, 0, -1),
        new Vector3(1, 0, 1),
    };
    
    public override void InitializeGameboard(BoardSize _boardSize)
    {
        base.InitializeGameboard(_boardSize);
        float scale = size;
        
        for (int i = 0; i < _borderPositions.Length; i++)
        {
            _border.SetPosition(i, _borderPositions[i] * (size / 2f));
        }
        
        transform.localPosition = Vector3.zero;
        background.localScale = new Vector3(scale, 0.1f, scale);
    }

    public override void ClearBoard()
    {
        base.ClearBoard();
    }


    public override void InitializeNeighbours()
    {
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
                slot.Initialize(boardPostion, IsHoshiPoint(boardPostion), false, disableCollider);
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
}
