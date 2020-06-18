namespace FortniteReader
{
    public class PlayerInfo
    {
        public PlayerInfo(string playerEpicId)
        {
            EpicId = playerEpicId;
        }

        public string EpicId { get; set; }
        public uint Eliminations { get; set; }
        public float Accuracy { get; set; }
        public uint Assists { get; set; }
        public uint WeaponDamage { get; set; }
        public uint OtherDamage { get; set; }
        public uint DamageToPlayers { get; set; }
        public uint Revives { get; set; }
        public uint DamageTaken { get; set; }
        public uint DamageToStructures { get; set; }
        public uint MaterialsGathered { get; set; }
        public uint MaterialsUsed { get; set; }
        public uint TotalTraveled { get; set; }

    }
}