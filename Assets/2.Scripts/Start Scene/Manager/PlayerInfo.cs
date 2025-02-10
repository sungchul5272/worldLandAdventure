using Fusion;
using System;

[Serializable]
public struct PlayerInfo : INetworkStruct
{
    public NetworkString<_16> playerName;  // 플레이어 이름 (최대 16자)
}