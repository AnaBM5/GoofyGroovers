using System.Collections.Generic;

public enum LobbyMode
{ Lobby = 1, RaceStarting, Race, RaceEnding }

public class Lobby
{
    public string lobbyName;
    public List<BlobEntity> playerList;
    public int raceStartTime = -1;
    public int controllingPlayerId = -1;
    public string controllingPlayerName = "";
    public LobbyMode lobbyMode;

    public Lobby()
    { }

    public Lobby(string name)
    {
        lobbyName = name;
        lobbyMode = LobbyMode.Lobby;
        playerList = new List<BlobEntity>();
    }

    public Lobby(string lobbyName, BlobEntity controllingPlayer) : this(lobbyName)
    {
        playerList.Add(controllingPlayer);
        controllingPlayerId = controllingPlayer.blobUserId;
    }
}