using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerController : NetworkBehaviour
{

    [Networked] int _currentTileIndex { get; set; }

    public void Move(int steps)
    {
        _currentTileIndex += steps;
        Transform targetTile = BoardManager._instance.GetTile(_currentTileIndex % 40);
        if (targetTile != null)
        {
            transform.position = targetTile.position;
        }
    }
}