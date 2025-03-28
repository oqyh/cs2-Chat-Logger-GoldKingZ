using System.Collections.Concurrent;
using CounterStrikeSharp.API.Core;

namespace Chat_Logger_GoldKingZ;


public static class Globals_Static
{
    public class PersonData
    {
        public ulong PlayerSteamID { get; set; }
        public string? Player_Name { get; set; }
        public string? Player_Message { get; set; }
        public int Toggle_Wall_Logo { get; set; }
        public int Toggle_Wall_Text { get; set; }
        public int Toggle_Player_Logo { get; set; }
        public DateTime DateAndTime { get; set; }
    }
}

public class Globals
{
    public string ServerPublicIpAdress = "";
    public string ServerPort = "";
    public class PlayerDataClass
    {
        public CCSPlayerController Player { get; set; }
        public string Locally_LastMessage { get; set; }
        public string Discord_LastMessage { get; set; }
        public string MySql_LastMessage { get; set; }
        public PlayerDataClass(CCSPlayerController player, string locally_LastMessage, string discord_LastMessage, string mySql_LastMessage)
        {
            Player = player;
            Locally_LastMessage = locally_LastMessage;
            Discord_LastMessage = discord_LastMessage;
            MySql_LastMessage = mySql_LastMessage;
        }
    }
    public Dictionary<CCSPlayerController, PlayerDataClass> Player_Data = new Dictionary<CCSPlayerController, PlayerDataClass>();


    public class ChatMessageStorage
    {
        public string? MapName { get; set; }
        public string? Date { get; set; }
        public string? Time { get; set; }
        public string? PlayerName { get; set; }
        public string? Message { get; set; }
        public string? ChatTeam { get; set; }
        public string? SteamID { get; set; }
        public string? SteamID3 { get; set; }
        public string? SteamID32 { get; set; }
        public string? SteamID64 { get; set; }
        public string? IpAdress { get; set; }
        public int Where { get; set; }
    }
    public ConcurrentBag<ChatMessageStorage> _chatMessages_Locally = new ConcurrentBag<ChatMessageStorage>();
    public ConcurrentBag<ChatMessageStorage> _chatMessages_Mysql = new ConcurrentBag<ChatMessageStorage>();
}