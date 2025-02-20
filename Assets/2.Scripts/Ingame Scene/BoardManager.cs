using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class BoardManager : NetworkBehaviour
{
    public static BoardManager _instance;

    [SerializeField] Transform[] _boardTiles;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    public Transform GetTile(int index)
    {
        if (index >= 0 && index < _boardTiles.Length)
        {
            return _boardTiles[index];
        }
        return null;
    }
}
