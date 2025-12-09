using System.Collections.Generic;

public static class MigrationBackup
{
    public static List<PlayerDataSerializable> Players = new List<PlayerDataSerializable>();
    public static string LobbyName;
    public static int MaxPlayers;
    public static bool IsPrivate;

    public static void Clear()
    {
        Players.Clear();
        LobbyName = "";
        MaxPlayers = 4;
        IsPrivate = false;
    }
}