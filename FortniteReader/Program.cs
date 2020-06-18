using FortniteReader.Classes;
using FortniteReader.Enum;
using FortniteReplayReader;
using FortniteReplayReader.Models;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Serialization;

namespace FortniteReader
{
    class Program
    {

        static string sharkaum = "81b77011144b43a0bb821bf90533275e";
        static string confusedNova = "acdfcc46a44e411d8ba5f9dc76bc9625";

        static void Main(string[] args)
        {         
            GameData gameData;
            FortniteReplay replay;
            string gameTimeInput;
            FileInfo[] files = ReadAllReplays();
            List<GameData> games = new List<GameData>();

            do
            {
                Console.WriteLine($" Extract information from:");
                Console.WriteLine($"0 - All available games.");
                Console.WriteLine($"1 - Last 24 hours.");
                Console.WriteLine($"2 - Last game.");
                Console.WriteLine($"9 - Exit.");
                gameTimeInput = Console.ReadLine();
                Console.WriteLine(gameTimeInput);
                games.Clear();

                switch (gameTimeInput)
                {
                    case "0":
                        
                        for (int i = 0; i < files.Length; i++)
                        {
                            FileInfo replayFile = files[i];
                            gameData = new GameData();
                            replay = ReadReplay(replayFile);
                            if (IsValidReplay(replay))
                            {
                                ProcessReplay(gameData, replay);
                                games.Add(gameData);
                            }
                        }
                        ExportData(games);

                        break;
                    case "1":
                        var selectedFiles = files.Where(x => x.CreationTime > DateTime.Now.AddDays(-1)).ToList();
                        for (int i = 0; i < selectedFiles.Count; i++)
                        {
                            FileInfo replayFile = selectedFiles[i];
                            gameData = new GameData();
                            replay = ReadReplay(replayFile);
                            if (IsValidReplay(replay))
                            {
                                ProcessReplay(gameData, replay);
                                games.Add(gameData);

                            }
                        }
                        ExportData(games);
                        break;
                    case "2":
                        var lastReplay = files.OrderByDescending(x => x.CreationTime).FirstOrDefault();
                        replay = ReadReplay(lastReplay);
                        if (IsValidReplay(replay))
                        {
                            gameData = new GameData();
                            if (replay.Stats == null) break;
                            ProcessReplay(gameData, replay);
                            ExportData(gameData);
                        }
                        break;
                    default:
                        break;
                }

                Console.WriteLine($"======================================================================= ");
            } while (gameTimeInput != "9");
        }

        private static bool IsValidReplay(FortniteReplay replay)
        {
            return replay.Stats != null;
        }
        private static void ExportData(GameData gameData)
        {
            string json = JsonSerializer.Serialize(gameData);
            System.IO.File.WriteAllText($"C:\\lab\\ExtractFortniteData\\extract_data_{gameData.GameId}.json", json);
        }
        private static void ExportData(List<GameData> gamesData)
        {
            string json = JsonSerializer.Serialize(gamesData);
            System.IO.File.WriteAllText($"C:\\lab\\ExtractFortniteData\\extract_data_{DateTime.Now.ToString("yyyy-MM-dd")}.json", json);
        }
        private static void ProcessReplay(GameData gameData, FortniteReplay replay)
        {
            ExtractReplayData(gameData, replay);
            PrintInformation(gameData, replay);
        }
        private static void PrintInformation(GameData gameData, FortniteReplay replay)
        {
            Console.WriteLine($"{replay.Info.Timestamp}  Game Type: {gameData.GameType} - Final Position: {gameData.Position} - Kills: {gameData.PlayerInfo.Eliminations}");
            if (gameData.Position == 1)
                Console.WriteLine($"}}===========WINNER WINNER==========={{ ");

            Console.WriteLine($"PSN: {CountPlayerTypes(gameData.PlayersType, PlayerType.PSN)} / XBL: {CountPlayerTypes(gameData.PlayersType, PlayerType.XBL)} / " +
                $"Win: {CountPlayerTypes(gameData.PlayersType, PlayerType.PC)} / Mac: {CountPlayerTypes(gameData.PlayersType, PlayerType.MAC)} / " +
                $"Henchman: {CountPlayerTypes(gameData.PlayersType, PlayerType.Henchmam)} / Marauder: {CountPlayerTypes(gameData.PlayersType, PlayerType.Marauder)} / " +
                $"Bosses: {CountPlayerTypes(gameData.PlayersType, PlayerType.Boss)} / Bots Players: {CountPlayerTypes(gameData.PlayersType, PlayerType.BOT)} / " +
                $"Unknown: {CountPlayerTypes(gameData.PlayersType, PlayerType.Unknown)} ");

            Console.WriteLine($"-------------------------------------------------------------------- ");
        }
        private static FortniteReplay ReadReplay(FileInfo replayFile)
        {
            var reader = new ReplayReader();
            var replay = reader.ReadReplay(replayFile.FullName);
            return replay;
        }
        private static void ExtractReplayData(GameData gameData, FortniteReplay replay)
        {
            gameData.GameType = CalculateTypeOfGame(replay);
            gameData.GameId = replay.GameData.GameSessionId;
            gameData.Position = (int)replay.TeamStats.Position;
            gameData.PlayersType = GetAllPlayers(replay.PlayerData);
            gameData.PlayerInfo = SetPlayerInfo(replay, sharkaum);
            gameData.GameTime = replay.Info.Timestamp;
            gameData.Season = GetSeason(gameData.GameTime);
            gameData.TeamStats = (gameData.GameType == GameType.Duo || gameData.GameType == GameType.Squad)? GetTeamstats(replay): null;
        }
        private static TeamStats GetTeamstats(FortniteReplay replay)
        {
            TeamStats stats = new TeamStats();
            foreach (var team in replay.TeamData)
            {
                if (team.PlayerNames.Contains(sharkaum.ToUpper()))
                {
                    foreach (var player in team.PlayerNames.ToList())
                    {
                        stats.Players.Add(GetTeamPlayersInfo(replay, player));
                    }
                }
            }
            stats.TotalTeamKillsInAMatch = replay.PlayerData.FirstOrDefault(x => x.EpicId == sharkaum.ToUpper()).TeamKills;

            return stats;
        }

        private static PlayerInfo GetTeamPlayersInfo(FortniteReplay replay, string EpicId)
        {
            var playerInfo = replay.PlayerData.FirstOrDefault(x => x.EpicId == EpicId);

            return new PlayerInfo() {
                EpicId = playerInfo.EpicId,
                Kills = playerInfo.Kills
            };
        }

        private static int GetSeason(DateTime gameTime)
        {
            DateTime Season13Start = new DateTime(2020,06,17,6,0,0);
            if (DateTime.Compare(gameTime, Season13Start) < 0) return 12;
            return 13;
        }
        private static PlayerStats SetPlayerInfo(FortniteReplay replay, string playerEpicId)
        {
            PlayerStats player = new PlayerStats(playerEpicId);

            player.Accuracy = replay.Stats.Accuracy;
            player.Assists = replay.Stats.Assists;
            player.DamageTaken = replay.Stats.DamageTaken;
            player.DamageToPlayers = replay.Stats.DamageToPlayers;
            player.DamageToStructures = replay.Stats.DamageToStructures;
            player.Eliminations = replay.Stats.Eliminations;
            player.MaterialsGathered = replay.Stats.MaterialsGathered;
            player.MaterialsUsed = replay.Stats.MaterialsUsed;
            player.OtherDamage = replay.Stats.OtherDamage;
            player.Revives = replay.Stats.Revives;
            player.TotalTraveled = replay.Stats.TotalTraveled;
            player.WeaponDamage = replay.Stats.WeaponDamage;

            return player;
        }
        private static int CountPlayerTypes(List<PlayerType> playersType, PlayerType playerType)
        {
            return playersType.Count(x => x == playerType);
        }
        private static FileInfo[] ReadAllReplays()
        {
            DirectoryInfo d = new DirectoryInfo(@"C:\Users\thiag\AppData\Local\FortniteGame\Saved\Demos");
            FileInfo[] files = d.GetFiles("*.replay");
            return files;
        }
        private static GameType CalculateTypeOfGame(FortniteReplay replay)
        {
            // Need better investigation. No gameType property
            var numberOfTeams = replay.TeamData.ToList().Count;

            if (numberOfTeams == 2) return GameType.LTM;
            if (numberOfTeams < 40) return GameType.Squad;
            if (numberOfTeams < 80) return GameType.Duo;
            return GameType.Solo;
        }
        private static List<PlayerType> GetAllPlayers(IEnumerable<PlayerData> playerData)
        {
            //int psn = 0, xbl = 0, win = 0, henchman = 0, bot = 0, boss = 0;
            List<PlayerType> gameTypes = new List<PlayerType>();

            foreach (var player in playerData)
            {
                switch (player.Platform)
                {
                    case "PSN":
                        gameTypes.Add(PlayerType.PSN);
                        break;
                    case "XBL":
                        gameTypes.Add(PlayerType.XBL);
                        break;
                    case "WIN":
                        gameTypes.Add(PlayerType.PC);
                        break;
                    case "MAC":
                        gameTypes.Add(PlayerType.MAC);
                        break;
                    default:
                        gameTypes.Add(GetBotType(player));
                        break;
                }
            }

            return gameTypes;

        }
        private static PlayerType GetBotType(PlayerData player)
        {
            if (player.BotId != null) return PlayerType.BOT;
            else if (player.PlayerNameCustomOverride != null) {
                // Named Bots
                if (player.PlayerNameCustomOverride.Contains("Henchman")) return PlayerType.Henchmam;
                if (player.PlayerNameCustomOverride.Contains("Marauder")) return PlayerType.Marauder;
                return PlayerType.Boss;
            };
            return PlayerType.Unknown;            
        }
    }
}
