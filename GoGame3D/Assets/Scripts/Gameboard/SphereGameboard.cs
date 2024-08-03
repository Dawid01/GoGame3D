using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SphereGameboard : Gameboard
{
    
    [SerializeField] private MeshRenderer linesRenderer;
    private List<Slot> _slots;

    public List<Slot> Slots => _slots;


    public override void InitializeGameboard(BoardSize _boardSize)
    {
        base.InitializeGameboard(_boardSize);
        linesRenderer.sharedMaterial.mainTextureScale = new Vector2(size, size);
        float scale = (size - 1);
        background.localScale = Vector3.one * scale;

    } 

    public override void ClearBoard()
    {
        base.ClearBoard();
        if (_slots == null)
        {
            _slots = new List<Slot>();
            return;
        }
        if(_slots.Count == 0) return;
        for (int i = 0; i < _slots.Count; i++)
        {
            Slot slot = _slots[i];
            GameMgr.Instance.slotPoolingSystem.Despawn(slot);
        }
        _slots.Clear();
    }

    public override void InitializeSlots()
    {
        for (int y = 0; y <= size; y++) {
            for (int x = 0; x <= size; x++) {
                InitialiseSlot(new Vector3(x, y, 0), new Vector2Int(x, y));
            }
            for (int z = 1; z <= size; z++) {
                InitialiseSlot(new Vector3(size, y, z),new Vector2Int(y, z));
            }
            for (int x = size - 1; x >= 0; x--) {
                InitialiseSlot(new Vector3(x, y, size), new Vector2Int(x, y));
            }
            for (int z = size - 1; z > 0; z--) {
                InitialiseSlot(new Vector3(0, y, z), new Vector2Int(y, z));
            }
        }

        for (int z = 1; z < size; z++) {
            for (int x = 1; x < size; x++) {
                InitialiseSlot(new Vector3(x, size, z),new Vector2Int(x, z));
            }
        }

        for (int z = 1; z < size; z++) {
            for (int x = 1; x < size; x++) {
                InitialiseSlot(new Vector3(x, 0, z),new Vector2Int(x, z));
            }
        }

        slotsAnchor.gameObject.SetActive(true);
        InitializeNeighbours();
    }
    
    private void InitialiseSlot(Vector3 position, Vector2Int boardPosition)
    {
        position = (position * 2f / size) - Vector3.one;
        float x2 = Mathf.Pow(position.x, 2);
        float y2 = Mathf.Pow(position.y, 2);
        float z2 = Mathf.Pow(position.z, 2);
        Vector3 s;
        s.x = position.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
        s.y = position.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
        s.z = position.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);
        Vector3 pos = position.normalized;
        position = ((pos + s)).normalized * (background.localScale.x - 0.5f);

        Slot slot = GameMgr.Instance.slotPoolingSystem.Spawn().GetComponent<Slot>();
        slot.Initialize(boardPosition, IsHoshiPoint(boardPosition));
        slot.transform.parent = slotsAnchor;
        slot.transform.localEulerAngles = Vector3.zero;
        slot.transform.localPosition = position;

        slot.transform.LookAt(slotsAnchor.position);
        slot.transform.localEulerAngles = slot.transform.localEulerAngles.With(x: slot.transform.localEulerAngles.x - 90f);
        _slots.Add(slot);

        //slot.transform.localPosition = slot.transform.localPosition.With(y: background.localScale.x - 0.5f);
    }

    public override void InitializeNeighbours()
    {
        
        for (int i = 0; i < _slots.Count; i++)
        {
            Slot currentSlot = _slots[i];
            Vector3 slotPosition = currentSlot.transform.position;
            //float sphereRadius = size;
            
            //RaycastHit[] hits = Physics.SphereCastAll(slotPosition, sphereRadius, Vector3.up, 0, LayerMask.GetMask("Slot"));
            
            List<Slot> neighbouringSlots = new List<Slot>();

            /*foreach (RaycastHit hit in hits)
            {
                Slot slot = hit.collider.GetComponent<Slot>();
                if (slot != null && slot != currentSlot)
                {
                    neighbouringSlots.Add(slot);
                }
            }*/

            neighbouringSlots = _slots.OrderBy(slot => Vector3.Distance(slotPosition, slot.transform.position)).ToList();
            neighbouringSlots.Remove(currentSlot);
            Vector2Int boardPos = currentSlot.boardPosition;
            bool isCorner = (boardPos == new Vector2Int(0, 0) ||
                             boardPos == new Vector2Int(0, size) ||
                             boardPos == new Vector2Int(size, 0) ||
                             boardPos == new Vector2Int(size, size));
            currentSlot.neighboringSlots = neighbouringSlots.Take(isCorner ? 3 : 4).ToList();
        }
    }

    public override string CalculateChecksum(Slot clickedSlot = null, StoneColor stoneColor = StoneColor.WHITE)
    {
        string checksum = "";
        if (clickedSlot)
        {
            clickedSlot = GetMainSlot(clickedSlot);
        }

        foreach (Slot slot in _slots)
        {
            if (slot == clickedSlot && slot.IsEmpty())
            {
                checksum += $"{(int)stoneColor}";
            }else if (!slotsToClear.Contains(slot))
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

        for (int i = 0; i < _slots.Count(); i++)
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
