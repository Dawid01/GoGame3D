using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class LoopingGameboard : Gameboard
{
    [SerializeField] private Gameboard[] additionalBoards;
    [SerializeField] private LineRenderer _border;

    private Vector3[] _additionalPositionVectors = new[]
    {
        new Vector3(1f, 0f, 0f),
        new Vector3(-1f, 0f, 0f),
        new Vector3(0f, 0f, 1f),
        new Vector3(0f, 0f, -1f),
        new Vector3(1f, 0f, 1f),
        new Vector3(-1f, 0f, 1f),
        new Vector3(1f, 0f, -1f),
        new Vector3(-1f, 0f, -1f),
    };

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
        background.localScale = new Vector3(scale, 0.1f, scale);
        //border.SetPositions(_borderPositions);
        for (int i = 0; i < _borderPositions.Length; i++)
        {
            _border.SetPosition(i, _borderPositions[i] * (size / 2f));
        }

        for (int i = 0; i < additionalBoards.Length; i++)
        {
            Gameboard additionalBoard = additionalBoards[i];
            additionalBoard.InitializeGameboard(_boardSize);
            additionalBoard.transform.localPosition = _additionalPositionVectors[i] * scale;
        }
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
                    Vector2Int boardPostion = new Vector2Int((x + neighboursVectors[i].x + size) % size, 
                        (y + neighboursVectors[i].y + size) % size);

                    slot.neighboringSlots.Add(slots[boardPostion.x, boardPostion.y]);
                }
            }
        }
    }

    public override void ClearBoard()
    {
        base.ClearBoard();
    }

    public override void InitializeSlots()
    {
        base.InitializeSlots();
    }
}
