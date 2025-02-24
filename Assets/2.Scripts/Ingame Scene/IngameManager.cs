using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameManager : NetworkBehaviour
{
    public static IngameManager _instance;
    int _currentPlayerIndex = 0;


    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
    }

}