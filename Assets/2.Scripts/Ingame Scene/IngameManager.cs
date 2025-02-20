using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameManager : NetworkBehaviour
{
    public static IngameManager _instance;
    int _currentPlayerIndex = 0;
    [SerializeField] private PlayerController[] _players;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

    public void MoveCurrentPlayer(int steps)
    {
        _players[_currentPlayerIndex].Move(steps);
        _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Length;
    }
}