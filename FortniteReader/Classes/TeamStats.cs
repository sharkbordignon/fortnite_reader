using System;
using System.Collections.Generic;

namespace FortniteReader.Classes
{
    public class TeamStats
    {
        public List<PlayerInfo> Players { get; set; }
        public uint? TotalTeamKillsInAMatch { get; set; }
    }
}
