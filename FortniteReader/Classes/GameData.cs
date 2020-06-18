using FortniteReader.Classes;
using FortniteReader.Enum;
using System;
using System.Collections.Generic;

namespace FortniteReader
{
    public class GameData
    {
        public string GameId { get; set; }
        public DateTime GameTime { get; set; }
        public GameType GameType { get; set; }
        public int Position { get; set; }
        public PlayerInfo PlayerInfo { get; set; }
        public List<PlayerType> PlayersType { get; set; }
        public int Season { get; set; }
        public TeamStats TeamStats{ get; set; }

    }
}