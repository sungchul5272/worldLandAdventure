using Fusion;
using System;

[Serializable]
public struct PlayerInfo : INetworkStruct
{
    public NetworkString<_16> playerName;  // �÷��̾� �̸� (�ִ� 16��)
}