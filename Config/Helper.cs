using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.Modules.Cvars;
using Chat_Logger_GoldKingZ.Config;
using System.Text.RegularExpressions;
using System.Text;
using System.Drawing;
using System.Text.Json;

namespace Chat_Logger_GoldKingZ;

public class Helper
{
    public static bool IsPlayerInGroupPermission(CCSPlayerController player, string groups)
    {
        if (string.IsNullOrEmpty(groups))
        {
            return false;
        }
        var Groups = groups.Split(',');
        foreach (var group in Groups)
        {
            if (string.IsNullOrEmpty(group))
            {
                continue;
            }
            string groupId = group[0] == '!' ? group.Substring(1) : group;
            if (group[0] == '#' && AdminManager.PlayerInGroup(player, group))
            {
                return true;
            }
            else if (group[0] == '@' && AdminManager.PlayerHasPermissions(player, group))
            {
                return true;
            }
            else if (group[0] == '!' && player.AuthorizedSteamID != null && (groupId == player.AuthorizedSteamID.SteamId2.ToString() || groupId == player.AuthorizedSteamID.SteamId3.ToString().Trim('[', ']') ||
            groupId == player.AuthorizedSteamID.SteamId32.ToString() || groupId == player.AuthorizedSteamID.SteamId64.ToString()))
            {
                return true;
            }
            else if (AdminManager.PlayerInGroup(player, group))
            {
                return true;
            }
        }
        return false;
    }
    
    public static void ClearVariables()
    {
        var g_Main = ChatLoggerGoldKingZ.Instance.g_Main;
        g_Main.Player_Data.Clear();
    }

    public static void AddPlayerInGlobals(CCSPlayerController player)
    {
        if (!player.IsValid())return;
        var g_Main = ChatLoggerGoldKingZ.Instance.g_Main;

        if(!g_Main.Player_Data.ContainsKey(player))
        {
            g_Main.Player_Data.Add(player, new Globals.PlayerDataClass(player, "", "", ""));
        }
    }

    private static readonly HttpClient _httpClient = new HttpClient();
    private static string GetFormattedDateTime() => DateTime.Now.ToString($"{Configs.GetConfigData().Discord_DateFormat} {Configs.GetConfigData().Discord_TimeFormat}");
    public static async Task SendToDiscordAsync(int mode, string webhookUrl, string message, string steamUserId, string steamName, string serverIpPort = null!)
    {
        try
        {
            object payload = mode == 1 
                ? new { content = message }
                : await BuildEmbedPayload(mode, message, steamUserId, steamName, serverIpPort);

            var jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            await _httpClient.PostAsync(webhookUrl, content);
        }
        catch (Exception ex)
        {
            DebugMessage($"Discord Error: {ex.Message}");
        }
    }

    private static async Task<object> BuildEmbedPayload(int mode, string message, string steamUserId, string steamName, string serverIpPort)
    {
        var color = GetColorFromConfig();
        var profileLink = $"https://steamcommunity.com/profiles/{steamUserId}";
        var profilePicture = mode >= 3 ? 
            await GetProfilePictureAsync(steamUserId, Configs.GetConfigData().Discord_UsersWithNoAvatarImage) : null;

        var embed = new
        {
            type = "rich",
            color,
            title = mode == 2 ? steamName : null,
            url = mode == 2 ? profileLink : null,
            description = mode <= 3 ? message : null,
            author = mode >= 3 ? new { name = steamName, url = profileLink, icon_url = profilePicture } : null,
            fields = mode >= 4 ? new[] 
            {
                new { name = "Date/Time", value = GetFormattedDateTime(), inline = false },
                new { name = "Message", value = message, inline = false }
            } : null,
            footer = mode == 5 ? new 
            { 
                text = $"Server IP: {serverIpPort}",
                icon_url = Configs.GetConfigData().Discord_FooterImage 
            } : null
        };

        return new { embeds = new[] { embed } };
    }

    private static int GetColorFromConfig()
    {
        string colorString = Configs.GetConfigData().Discord_SideColor;
        if (colorString.StartsWith("#"))
        {
            colorString = colorString.Substring(1);
        }
        
        int colorss = int.Parse(colorString, System.Globalization.NumberStyles.HexNumber);
        Color color = Color.FromArgb(colorss >> 16, (colorss >> 8) & 0xFF, colorss & 0xFF);
        return color.ToArgb() & 0xFFFFFF;
    }

    public static async Task<string> GetProfilePictureAsync(string steamId64, string defaultImage)
    {
        try
        {
            string apiUrl = $"https://steamcommunity.com/profiles/{steamId64}/?xml=1";

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                string xmlResponse = await response.Content.ReadAsStringAsync();
                int startIndex = xmlResponse.IndexOf("<avatarFull><![CDATA[") + "<avatarFull><![CDATA[".Length;
                int endIndex = xmlResponse.IndexOf("]]></avatarFull>", startIndex);

                if (endIndex >= 0)
                {
                    string profilePictureUrl = xmlResponse.Substring(startIndex, endIndex - startIndex);
                    return profilePictureUrl;
                }
                else
                {
                    DebugMessage("Could not find avatarFull tag in XML response, returning default image");
                    return defaultImage;
                }
            }
            else
            {
                DebugMessage($"[DEBUG] HTTP request failed with status code: {response.StatusCode}");
                return null!;
            }
        }
        catch (Exception ex)
        {
            DebugMessage($"Error On GetProfilePictureAsync: {ex.Message}");
            return null!;
        }
    }

    public static async Task<string> GetPublicIp()
    {
        var services = new[]
        {
            ("https://icanhazip.com", "text"),
            ("https://checkip.amazonaws.com", "text"),
            ("https://api.ipify.org?format=text", "text"),
            ("https://1.1.1.1/cdn-cgi/trace", "cloudflare"),
            ("https://httpbin.org/ip", "httpbin")
        };

        foreach (var (url, type) in services)
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(3);
                
                var response = await client.GetStringAsync(url);
                string ip = "";

                switch (type)
                {
                    case "text":
                        ip = response.Trim();
                        break;
                        
                    case "cloudflare":
                        ip = response.Split('\n')
                                .FirstOrDefault(line => line.StartsWith("ip="))
                                ?.Split('=')[1]
                                .Trim()!;
                        break;
                        
                    case "httpbin":
                        using (var doc = JsonDocument.Parse(response))
                        {
                            ip = doc.RootElement.GetProperty("origin").GetString()!;
                        }
                        break;
                }

                if (!string.IsNullOrEmpty(ip))
                {
                    var parts = ip.Split('.');
                    if (parts.Length == 4 && parts.All(p => byte.TryParse(p, out _)))
                    {
                        return ip;
                    }
                }
            }
            catch (Exception ex)
            {
                DebugMessage($"Failed to get IP from {url}: {ex.Message}");
            }
        }
        DebugMessage("All IP services failed");
        return "";
    }
    
    public static void DeleteOldFiles(string folderPath, string searchPattern, TimeSpan maxAge)
    {
        try
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);

            if (directoryInfo.Exists)
            {
                FileInfo[] files = directoryInfo.GetFiles(searchPattern);
                DateTime currentTime = DateTime.Now;
                
                foreach (FileInfo file in files)
                {
                    TimeSpan age = currentTime - file.LastWriteTime;

                    if (age > maxAge)
                    {
                        file.Delete();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            DebugMessage($"Error in DeleteOldFiles: {ex.Message}");
        }
    }

    public static void DebugMessage(string message, bool prefix = true)
    {
        if (!Configs.GetConfigData().EnableDebug) return;

        Console.ForegroundColor = ConsoleColor.Magenta;
        string output = prefix ? $"[Chat Logger]: {message}" : message;
        Console.WriteLine(output);
        
        Console.ResetColor();
    }

    public static void LogLocally(CCSPlayerController player, string message, bool TeamChat)
    {
        var g_Main = ChatLoggerGoldKingZ.Instance.g_Main;
        if (!player.IsValid() || string.IsNullOrEmpty(message))return;

        string Fpath = Path.Combine(Configs.Shared.CookiesModule!,"../../plugins/Chat-Logger-GoldKingZ/logs/");
        string fileName = DateTime.Now.ToString(Configs.GetConfigData().Locally_DateFormat) + ".txt";
        string Tpath = Path.Combine(Configs.Shared.CookiesModule!,"../../plugins/Chat-Logger-GoldKingZ/logs/") + $"{fileName}";
        var Mapname = Server.MapName;
        var playername = player.PlayerName;
        var playersteamId2 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId2 : "InvalidSteamID";
        var playersteamId3 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId3 : "InvalidSteamID";
        var playersteamId32 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId32.ToString() : "InvalidSteamID";
        var playersteamId64 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId64.ToString() : "InvalidSteamID";
        var GetPlayerIpAddress = player.IpAddress;
        var playerip = GetPlayerIpAddress?.Split(':')[0] ?? "InValidIpAddress";

        if(Configs.GetConfigData().Locally_LogMessagesOnly == 2 && TeamChat || Configs.GetConfigData().Locally_LogMessagesOnly == 3 && !TeamChat)return;
        if(!string.IsNullOrEmpty(Configs.GetConfigData().Locally_IncludeTheseFlagsMessagesOnly) && !IsPlayerInGroupPermission(player, Configs.GetConfigData().Locally_IncludeTheseFlagsMessagesOnly))return;
        if(!string.IsNullOrEmpty(Configs.GetConfigData().Locally_ExcludeFlagsMessages) && IsPlayerInGroupPermission(player, Configs.GetConfigData().Locally_ExcludeFlagsMessages))return;
        if (!string.IsNullOrEmpty(Configs.GetConfigData().Locally_ExcludeMessagesStartWith))
        {
            char[] excludeChars = Configs.GetConfigData().Locally_ExcludeMessagesStartWith.ToCharArray();
            if (excludeChars.Any(c => message.StartsWith(c.ToString())))
            {
                return;
            }
        }
        if(Configs.GetConfigData().Locally_ExcludeMessagesContainsLessThanXLetters > 0 && message.Count() <= Configs.GetConfigData().Locally_ExcludeMessagesContainsLessThanXLetters)return;
        if(Configs.GetConfigData().Locally_ExcludeMessagesDuplicate && g_Main.Player_Data.ContainsKey(player) && g_Main.Player_Data[player].Locally_LastMessage.Equals(message, StringComparison.OrdinalIgnoreCase))return;
             
        string chatteam = TeamChat ? "[TEAM] " : "";
        int chatType = (player.TeamNum << 1) | (TeamChat ? 1 : 0);
        string Time = DateTime.Now.ToString(Configs.GetConfigData().Locally_TimeFormat);
        string Date = DateTime.Now.ToString(Configs.GetConfigData().Locally_DateFormat);

        string formattedMessage = !string.IsNullOrEmpty(Configs.GetConfigData().Locally_MessageFormat) 
            ? Configs.GetConfigData().Locally_MessageFormat.ReplaceMessages(
                Date,
                Time,
                playername,
                message,
                chatteam,
                playersteamId2,
                playersteamId3,
                playersteamId32,
                playersteamId64,
                playerip)
            : message;
        
        if(Configs.GetConfigData().Locally_Enable == 1)
        {
            if(!Directory.Exists(Fpath))
            {
                Directory.CreateDirectory(Fpath);
            }

            if(!File.Exists(Tpath))
            {
                using (File.Create(Tpath)) { }
            }
            
            try
            {
                File.AppendAllLines(Tpath, new[]{formattedMessage});
            }catch (Exception ex)
            {
                DebugMessage($"Error logging chat message: {ex.Message}");
            }
        }else 
        {
            g_Main._chatMessages_Locally.Add(new Globals.ChatMessageStorage
            {
                MapName = Mapname,
                Date = Date,
                Time = Time,
                PlayerName = playername,
                Message = message,
                ChatTeam = chatteam,
                SteamID = playersteamId2,
                SteamID3 = playersteamId3,
                SteamID32 = playersteamId32,
                SteamID64 = playersteamId64,
                IpAdress = playerip,
                Where = chatType,
            });
        }

        if(g_Main.Player_Data.ContainsKey(player) && !g_Main.Player_Data[player].Locally_LastMessage.Equals(message, StringComparison.OrdinalIgnoreCase))
        {
            g_Main.Player_Data[player].Locally_LastMessage = message;
        }
    }
    

    public static void DelayLogLocally()
    {
        var g_Main = ChatLoggerGoldKingZ.Instance.g_Main;
        if (g_Main._chatMessages_Locally.Count > 0)
        {
            string Fpath = Path.Combine(Configs.Shared.CookiesModule!, "../../plugins/Chat-Logger-GoldKingZ/logs/");
            string fileName = DateTime.Now.ToString(Configs.GetConfigData().Locally_DateFormat) + ".txt";
            string Tpath = Path.Combine(Fpath, fileName);

            try
            {
                if (!Directory.Exists(Fpath))
                {
                    Directory.CreateDirectory(Fpath);
                }

                if (!File.Exists(Tpath))
                {
                    using (File.Create(Tpath)) { }
                }

                var linesToWrite = new List<string>();
                
                foreach (var message in g_Main._chatMessages_Locally.Reverse())
                {
                    string formattedMessage = !string.IsNullOrEmpty(Configs.GetConfigData().Locally_MessageFormat) 
                        ? Configs.GetConfigData().Locally_MessageFormat.ReplaceMessages(
                            message.Date!,
                            message.Time!,
                            message.PlayerName!,
                            message.Message!,
                            message.ChatTeam!,
                            message.SteamID!,
                            message.SteamID3!,
                            message.SteamID32!,
                            message.SteamID64!,
                            message.IpAdress!)
                        : message.Message!;
                    linesToWrite.Add(formattedMessage);
                }

                File.AppendAllLines(Tpath, linesToWrite);
                g_Main._chatMessages_Locally.Clear();
            }
            catch (Exception ex)
            {
                DebugMessage($"Error logging chat messages on map end: {ex.Message}");
            }
        }
    }

    

    public static void LogMySql(CCSPlayerController player, string message, bool TeamChat)
    {
        var g_Main = ChatLoggerGoldKingZ.Instance.g_Main;
        if (!player.IsValid() || string.IsNullOrEmpty(message))return;

        var Mapname = Server.MapName;
        var playername = player.PlayerName;
        var playersteamId64 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId64.ToString() : "InvalidSteamID";

        if(Configs.GetConfigData().MySql_LogMessagesOnly == 2 && TeamChat || Configs.GetConfigData().MySql_LogMessagesOnly == 3 && !TeamChat)return;
        if(!string.IsNullOrEmpty(Configs.GetConfigData().MySql_IncludeTheseFlagsMessagesOnly) && !IsPlayerInGroupPermission(player, Configs.GetConfigData().MySql_IncludeTheseFlagsMessagesOnly))return;
        if(!string.IsNullOrEmpty(Configs.GetConfigData().MySql_ExcludeFlagsMessages) && IsPlayerInGroupPermission(player, Configs.GetConfigData().MySql_ExcludeFlagsMessages))return;
        if (!string.IsNullOrEmpty(Configs.GetConfigData().MySql_ExcludeMessagesStartWith))
        {
            char[] excludeChars = Configs.GetConfigData().MySql_ExcludeMessagesStartWith.ToCharArray();
            if (excludeChars.Any(c => message.StartsWith(c.ToString())))
            {
                return;
            }
        }
        if(Configs.GetConfigData().MySql_ExcludeMessagesContainsLessThanXLetters > 0 && message.Count() <= Configs.GetConfigData().MySql_ExcludeMessagesContainsLessThanXLetters)return;
        if(Configs.GetConfigData().MySql_ExcludeMessagesDuplicate && g_Main.Player_Data.ContainsKey(player) && g_Main.Player_Data[player].MySql_LastMessage.Equals(message, StringComparison.OrdinalIgnoreCase))return;
             
        string chatteam = TeamChat ? "[TEAM] " : "";
        int chatType = (player.TeamNum << 1) | (TeamChat ? 1 : 0);
        
        if(Configs.GetConfigData().MySql_Enable == 1)
        {
            _ = Task.Run(async () => 
            {
                try
                {
                    await MySqlDataManager.InsertChatLog(
                        DateTime.Now,
                        Mapname,
                        playersteamId64,
                        playername,
                        chatType,
                        message
                    );
                }
                catch (Exception ex)
                {
                    DebugMessage($"Failed to insert chat log: {ex.Message}");
                }
            });
        }else 
        {
            g_Main._chatMessages_Mysql.Add(new Globals.ChatMessageStorage
            {
                MapName = Mapname,
                PlayerName = playername,
                Message = message,
                SteamID64 = playersteamId64,
                Where = chatType,
            });
        }

        if(g_Main.Player_Data.ContainsKey(player) && !g_Main.Player_Data[player].MySql_LastMessage.Equals(message, StringComparison.OrdinalIgnoreCase))
        {
            g_Main.Player_Data[player].MySql_LastMessage = message;
        }
    }
    

    public static void DelayLogMySql()
    {
        var g_Main = ChatLoggerGoldKingZ.Instance.g_Main;
        if (g_Main._chatMessages_Mysql.Count > 0)
        {
            _ = Task.Run(async () => 
            {
                try 
                {
                    var messagesToInsert = new List<Globals.ChatMessageStorage>();
                    while (g_Main._chatMessages_Mysql.TryTake(out var message))
                    {
                        messagesToInsert.Add(message);
                    }

                    await MySqlDataManager.BatchInsertMessages(messagesToInsert);
                    
                }
                catch (Exception ex)
                {
                    DebugMessage($"Error batch logging messages: {ex.Message}");
                }
                finally
                {
                    g_Main._chatMessages_Mysql.Clear();
                    DebugMessage($"Processed and cleared chat messages.");
                }
            });
        }
    }


    public static void LogDiscord(CCSPlayerController player, string message, bool TeamChat)
    {
        if(string.IsNullOrEmpty(Configs.GetConfigData().Discord_WebHook))return;

        var g_Main = ChatLoggerGoldKingZ.Instance.g_Main;
        if (!player.IsValid() || string.IsNullOrEmpty(message))return;

        var Mapname = Server.MapName;
        var playername = player.PlayerName;
        var playersteamId2 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId2 : "InvalidSteamID";
        var playersteamId3 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId3 : "InvalidSteamID";
        var playersteamId32 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId32.ToString() : "InvalidSteamID";
        var playersteamId64 = (player.AuthorizedSteamID != null) ? player.AuthorizedSteamID.SteamId64.ToString() : "InvalidSteamID";
        var GetPlayerIpAddress = player.IpAddress;
        var playerip = GetPlayerIpAddress?.Split(':')[0] ?? "InValidIpAddress";

        if(Configs.GetConfigData().Discord_LogMessagesOnly == 2 && TeamChat || Configs.GetConfigData().Discord_LogMessagesOnly == 3 && !TeamChat)return;
        if(!string.IsNullOrEmpty(Configs.GetConfigData().Discord_IncludeTheseFlagsMessagesOnly) && !IsPlayerInGroupPermission(player, Configs.GetConfigData().Discord_IncludeTheseFlagsMessagesOnly))return;
        if(!string.IsNullOrEmpty(Configs.GetConfigData().Discord_ExcludeFlagsMessages) && IsPlayerInGroupPermission(player, Configs.GetConfigData().Discord_ExcludeFlagsMessages))return;
        if (!string.IsNullOrEmpty(Configs.GetConfigData().Discord_ExcludeMessagesStartWith))
        {
            char[] excludeChars = Configs.GetConfigData().Discord_ExcludeMessagesStartWith.ToCharArray();
            if (excludeChars.Any(c => message.StartsWith(c.ToString())))
            {
                return;
            }
        }
        if(Configs.GetConfigData().Discord_ExcludeMessagesContainsLessThanXLetters > 0 && message.Count() <= Configs.GetConfigData().Discord_ExcludeMessagesContainsLessThanXLetters)return;
        if(Configs.GetConfigData().Discord_ExcludeMessagesDuplicate && g_Main.Player_Data.ContainsKey(player) && g_Main.Player_Data[player].Discord_LastMessage.Equals(message, StringComparison.OrdinalIgnoreCase))return;
             
        string chatteam = TeamChat ? "[TEAM] " : "";
        int chatType = (player.TeamNum << 1) | (TeamChat ? 1 : 0);
        string Time = DateTime.Now.ToString(Configs.GetConfigData().Discord_TimeFormat);
        string Date = DateTime.Now.ToString(Configs.GetConfigData().Discord_DateFormat);

        string formattedMessage = !string.IsNullOrEmpty(Configs.GetConfigData().Discord_MessageFormat) 
            ? Configs.GetConfigData().Discord_MessageFormat.ReplaceMessages(
                Date,
                Time,
                playername,
                message,
                chatteam,
                playersteamId2,
                playersteamId3,
                playersteamId32,
                playersteamId64,
                playerip)
            : message;
        
        _ = Task.Run(async() =>
        {
            await SendToDiscordAsync(
                Configs.GetConfigData().Discord_Style,
                Configs.GetConfigData().Discord_WebHook,
                formattedMessage,
                playersteamId64,
                playername,
                g_Main.ServerPublicIpAdress + ":" + g_Main.ServerPort
            );
        });

        if(g_Main.Player_Data.ContainsKey(player) && !g_Main.Player_Data[player].Discord_LastMessage.Equals(message, StringComparison.OrdinalIgnoreCase))
        {
            g_Main.Player_Data[player].Discord_LastMessage = message;
        }
    }
    
}