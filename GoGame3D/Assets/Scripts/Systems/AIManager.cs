using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using Random = System.Random;

// If you lose to this, good luck in life

public class AIManager
{
    public virtual void InitializeAI()
    {
        Debug.Log("I WORK");
    }

    public virtual void MakeMove()
    {
        throw new System.NotImplementedException();
    }
}

public class StupidAI : AIManager
{
    public override void InitializeAI()
    {
        base.InitializeAI();
        //Debug.Log("StupidAI successfully connected!");
    }

    public override void MakeMove()
    {
        if (!Helper.GameCanContinue())
        {
            Debug.Log("No moves can be made");
            ResultPanel.Instance.InitializeResult();
            //GameMgr.Instance.GameHasEnded();
            return;
        }
        PlayerController.Instance.ChangePlayer();
        DOVirtual.DelayedCall(1f, () =>
        {
            Random rand = new Random();
        
            List<Slot> possibleTargets = Helper.returnSlots();
            Slot _slot = null;

            while (_slot == null)
            {
                Slot targetSlot = Helper.findTarget(possibleTargets);
                _slot = Helper.checkForValidMove(targetSlot);
            }
        
            _slot.InitializeStone(StoneColor.WHITE);
            GameMgr.Instance.NextTurn();
            GameMgr.Instance.currentGameboard.UpdateChecksum();
            GameMgr.Instance.currentGameboard.ClearGroup(StoneColor.WHITE);
            PlayerController.Instance.ChangePlayer();
            AudioMgr.Instance.PlayAudio(AudioMgr.Instance.putAudioClip);
        });
       
    }
}

public class SkyNetAI : AIManager
{
    public override void InitializeAI()
    {
        base.InitializeAI();
        Debug.Log("SKYNET ONLINE");
    }

    public override void MakeMove()
    {
        if (!Helper.GameCanContinue())
        {
            Debug.Log("No moves can be made");
            GameMgr.Instance.GameHasEnded();
            return;
        }

        PlayerController.Instance.ChangePlayer();
        DOVirtual.DelayedCall(1f, () =>
        {
            Slot bestMove = GetBestMove(StoneColor.WHITE);
            if (bestMove != null)
            {
                bestMove.InitializeStone(StoneColor.WHITE);
            }
            else
            {
                Debug.Log("SkyNetAI could not find a valid move using MinMax. Choosing a random move.");
                List<Slot> possibleTargets = Helper.returnSlots();
                Slot randomMove = GetRandomMove(possibleTargets);
                if (randomMove != null)
                {
                    randomMove.InitializeStone(StoneColor.WHITE);
                }
                else
                {
                    Debug.Log("Shit has hit the fan");
                }
            }

            GameMgr.Instance.NextTurn();
            GameMgr.Instance.currentGameboard.UpdateChecksum();
            GameMgr.Instance.currentGameboard.ClearGroup(StoneColor.WHITE);
            PlayerController.Instance.ChangePlayer();
            AudioMgr.Instance.PlayAudio(AudioMgr.Instance.putAudioClip);
        });
    }

    private int MinMax(Slot slot, StoneColor currColor, int depth)
    {
        if (depth == 0 || !Helper.GameCanContinue())
        {
            return EvaluateGameState(currColor);
        }

        int bestValue;
        if (currColor == StoneColor.WHITE)
        {
            bestValue = int.MinValue;
            foreach (var childSlot in GetChildSlots())
            {
                childSlot.InitializeStone(currColor);
                int value = MinMax(childSlot, StoneColor.BLACK, depth - 1);
                bestValue = Math.Max(bestValue, value);
                childSlot.placedStone = null; // Undo the move
            }
        }
        else
        {
            bestValue = int.MaxValue;
            foreach (var childSlot in GetChildSlots())
            {
                childSlot.InitializeStone(currColor);
                int value = MinMax(childSlot, StoneColor.WHITE, depth - 1);
                bestValue = Math.Min(bestValue, value);
                childSlot.placedStone = null; // Undo the move
            }
        }

        GameMgr.Instance.currentGameboard.CalculatePoints();
        bestValue = EvaluateGameState(currColor);
        return bestValue;
    }

    private int EvaluateGameState(StoneColor currColor)
    {
        if (currColor == StoneColor.BLACK)
            return GameMgr.Instance.blackPoints;
        return GameMgr.Instance.whitePoints;
    }

    private List<Slot> GetChildSlots()
    {
        return Helper.returnSlots().Where(slot => GameMgr.Instance.currentGameboard.CanPutStone(slot, StoneColor.WHITE)).ToList();
    }

    private Slot GetBestMove(StoneColor currColor)
    {
        Slot bestMove = null;
        int bestValue = int.MinValue;

        foreach (var slot in GetChildSlots())
        {
            slot.InitializeStone(currColor);
            int moveValue = MinMax(slot, StoneColor.BLACK, 100); // Adjust depth as needed
            slot.placedStone = null; // Undo the move
            if (moveValue > bestValue)
            {
                bestValue = moveValue;
                bestMove = slot;
            }
        }
        return bestMove;
    }

    private Slot GetRandomMove(List<Slot> possibleTargets)
    {
        Slot _slot = null;

        while (_slot == null)
        {
            Slot targetSlot = Helper.findTarget(possibleTargets);
            _slot = Helper.checkForValidMove(targetSlot);
        }
        return _slot;
    }
}



public static class Helper
{
    public static bool GameCanContinue()
    {
        foreach(var slot in GameMgr.Instance.currentGameboard.slots)
        {
            if (GameMgr.Instance.currentGameboard.CanPutStone(slot, StoneColor.WHITE))
            {
                return true;
            }
        }
        return false;
    }

    public static List<Slot> freeSlots(StoneColor _color)
    {
        List<Slot> freeSlots;
        if (GameMgr.Instance.currentGameboard is SphereGameboard)
        {
            var _gameBoard = GameMgr.Instance.currentGameboard as SphereGameboard;
            freeSlots = _gameBoard.Slots.FindAll(slot =>
                // slot.placedStone != null && slot.placedStone.stoneColor == null);
                slot.placedStone != null);

            return freeSlots;
        }
        
        List<Slot> gameboardSlots = GameMgr.Instance.currentGameboard.slots.Cast<Slot>().ToList();
        freeSlots = gameboardSlots.FindAll(slot =>
            slot.placedStone != null && slot.placedStone.stoneColor == StoneColor.BLACK);
        
        return freeSlots;
    }
    
    public static List<Slot> returnSlots()
    {
        List<Slot> possibleTargets;
        if (GameMgr.Instance.currentGameboard is SphereGameboard)
        {
            var _gameBoard = GameMgr.Instance.currentGameboard as SphereGameboard;
            possibleTargets = _gameBoard.Slots.FindAll(slot =>
                slot.placedStone != null && slot.placedStone.stoneColor == StoneColor.BLACK);
            return possibleTargets;
        }
        
        List<Slot> gameboardSlots = GameMgr.Instance.currentGameboard.slots.Cast<Slot>().ToList();
       possibleTargets = gameboardSlots.FindAll(slot =>
            slot.placedStone != null && slot.placedStone.stoneColor == StoneColor.BLACK);

       return possibleTargets;
    }
    
    public static Slot findTarget(List<Slot> _slots)
    {
        var rand = new Random();
        Slot _slot = _slots[rand.Next(0,_slots.Count - 1)];
        return _slot;
    }
    public static Slot checkForValidMove(Slot _slot)
    {
        var rand = new Random();

        for (int i = 0; i < 25; i++)
        {
            int randomNum = rand.Next(0, _slot.neighboringSlots.Count);
            int neighbourDepth = rand.Next(1,3);

            Slot placeSlot = _slot.neighboringSlots[randomNum];
            for(int j = 0;j < neighbourDepth; j++)
            {
                placeSlot = placeSlot.neighboringSlots[rand.Next(0, placeSlot.neighboringSlots.Count)];
            }
            
            if (GameMgr.Instance.currentGameboard.CanPutStone(placeSlot, StoneColor.WHITE))
            {
                return placeSlot;
            }
        }

        return null;
    }
}