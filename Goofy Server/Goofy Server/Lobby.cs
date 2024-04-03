using System.Collections.Generic;

namespace Goofy_Server
{
    public enum LobbyMode
    { Lobby = 1, RaceStarting, Race, RaceEnding }

    public class Lobby
    {
        public string lobbyName;
        public List<BlobEntity> playerList;
        public int controllingPlayerId;
        public LobbyMode lobbyMode;

        public Lobby() { }
        public Lobby(string name)
        {
            lobbyName = name;
            lobbyMode = LobbyMode.Lobby;
            playerList = new List<BlobEntity>();
            controllingPlayerId = -1;
        }

        public Lobby(string lobbyName, BlobEntity controllingPlayer) : this(lobbyName)
        {
            playerList.Add(controllingPlayer);
            controllingPlayerId = controllingPlayer.blobUserId;
        }
    }
}