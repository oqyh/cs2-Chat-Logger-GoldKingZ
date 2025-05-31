using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Events;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Chat_Logger_GoldKingZ.Config;

namespace Chat_Logger_GoldKingZ;

public class MainPlugin : BasePlugin
{
    public override string ModuleName => "Chat Logger (Log Chat To Locally/Discord WebHook/MySql/Web Server)";
    public override string ModuleVersion => "1.1.2";
    public override string ModuleAuthor => "Gold KingZ";
    public override string ModuleDescription => "https://github.com/oqyh";
    public static MainPlugin Instance { get; set; } = new();
    private readonly SayText2 OnSayText2 = new();
    public Globals g_Main = new();
    public override void Load(bool hotReload)
    {
        Instance = this;
        Configs.Load(ModuleDirectory);

        RegisterListener<Listeners.OnMapStart>(OnMapStart);
        RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
        
        RegisterEventHandler<EventPlayerChat>(OnEventPlayerChat);
        RegisterEventHandler<EventPlayerConnectFull>(OnEventPlayerConnectFull);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        RegisterEventHandler<EventRoundEnd>(OnEventRoundEnd);

        HookUserMessage(118, OnUserMessage_OnSayText2, HookMode.Pre);

        AddCommandListener("say", OnPlayerSay, HookMode.Post);
        AddCommandListener("say_team", OnPlayerSay_Team, HookMode.Post);

        if (hotReload)
        {
            g_Main.ServerPublicIpAdress = ConVar.Find("ip")?.StringValue!;
            g_Main.ServerPort = ConVar.Find("hostport")?.GetPrimitiveValue<int>().ToString()!;

            if (Configs.GetConfigData().Locally_AutoDeleteLogsMoreThanXdaysOld > 0)
            {
                string Fpath = Path.Combine(ModuleDirectory, "../../plugins/Chat-Logger-GoldKingZ/logs/");
                Helper.DeleteOldFiles(Fpath, "*" + ".txt", TimeSpan.FromDays(Configs.GetConfigData().Locally_AutoDeleteLogsMoreThanXdaysOld));
            }

            _ = Task.Run(async () =>
            {
                string ip = await Helper.GetPublicIp();
                if (!string.IsNullOrEmpty(ip))
                {
                    g_Main.ServerPublicIpAdress = ip;
                }
            });

            if (Configs.GetConfigData().MySql_Enable > 0)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await MySqlDataManager.CreateTableIfNotExistsAsync();
                        await MySqlDataManager.DeleteOldMessagesAsync();
                    }
                    catch (Exception ex)
                    {
                        Helper.DebugMessage($"hotReload Error: {ex.Message}");
                    }
                });
            }
        }
    }

    public void OnMapStart(string Map)
    {
        g_Main.ServerPublicIpAdress = ConVar.Find("ip")?.StringValue!;
        g_Main.ServerPort = ConVar.Find("hostport")?.GetPrimitiveValue<int>().ToString()!;

        if(Configs.GetConfigData().Locally_AutoDeleteLogsMoreThanXdaysOld > 0)
        {
            string Fpath = Path.Combine(ModuleDirectory,"../../plugins/Chat-Logger-GoldKingZ/logs/");
            Helper.DeleteOldFiles(Fpath, "*" + ".txt", TimeSpan.FromDays(Configs.GetConfigData().Locally_AutoDeleteLogsMoreThanXdaysOld));
        }

        _ = Task.Run(async () => 
        {
            string ip = await Helper.GetPublicIp();
            if(!string.IsNullOrEmpty(ip))
            {
                g_Main.ServerPublicIpAdress = ip;
            }                
        });
        
        if (Configs.GetConfigData().MySql_Enable > 0)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await MySqlDataManager.CreateTableIfNotExistsAsync();
                    await MySqlDataManager.DeleteOldMessagesAsync();
                }
                catch (Exception ex)
                {
                    Helper.DebugMessage($"OnMapStart Error: {ex.Message}");
                }
            });
        }
    }

    public HookResult OnEventPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (@event == null)return HookResult.Continue;

        var player = @event.Userid;
        if (!player.IsValid())return HookResult.Continue;

        Helper.AddPlayerInGlobals(player);

        return HookResult.Continue;
    }
    
    private HookResult OnPlayerSay(CCSPlayerController? player, CommandInfo info)
    {
        if (!player.IsValid()) return HookResult.Continue;
        Helper.AddPlayerInGlobals(player);

        var eventmessage = info.ArgString;
        eventmessage = eventmessage.Trim().Trim('"').Trim();

        if (string.IsNullOrWhiteSpace(eventmessage)) return HookResult.Continue;

        OnSayText2.OnSayText2(player, eventmessage, false);

        return HookResult.Continue;
    }
    private HookResult OnPlayerSay_Team(CCSPlayerController? player, CommandInfo info)
    {
        if (!player.IsValid()) return HookResult.Continue;
        Helper.AddPlayerInGlobals(player);

        var eventmessage = info.ArgString;
        eventmessage = eventmessage.Trim().Trim('"').Trim();
    
        if (string.IsNullOrWhiteSpace(eventmessage)) return HookResult.Continue;

        OnSayText2.OnSayText2(player, eventmessage, true);

        return HookResult.Continue;
    }
    public HookResult OnEventPlayerChat(EventPlayerChat @event, GameEventInfo info)
    {
        if (@event == null)return HookResult.Continue;
        var EventPlayerChatUserid = @event.Userid;
        var message = @event.Text;
        var TeamChat = @event.Teamonly;

        var player = Utilities.GetPlayerFromUserid(EventPlayerChatUserid);
        if (!player.IsValid()) return HookResult.Continue;
        Helper.AddPlayerInGlobals(player);

        message = message.Trim();
        if (string.IsNullOrWhiteSpace(message)) return HookResult.Continue;

        OnSayText2.OnSayText2(player, message, TeamChat);

        return HookResult.Continue;
    }
    private HookResult OnUserMessage_OnSayText2(CounterStrikeSharp.API.Modules.UserMessages.UserMessage um)
    {
        var entityindex = um.ReadInt("entityindex");
        var player = Utilities.GetPlayerFromIndex(entityindex);
        if (!player.IsValid()) return HookResult.Continue;
        Helper.AddPlayerInGlobals(player);

        var messagename = um.ReadString("messagename");
        var message = um.ReadString("param2");
        message = message.Trim();
        if (string.IsNullOrWhiteSpace(message)) return HookResult.Continue;

        bool TeamChat = false;
        if (messagename.Equals("Cstrike_Chat_CT") || messagename.Equals("Cstrike_Chat_CT_Loc") || messagename.Equals("Cstrike_Chat_T") || messagename.Equals("Cstrike_Chat_T_Loc")
        || messagename.Equals("Cstrike_Chat_Spec") || messagename.Equals("Cstrike_Chat_CT_Dead") || messagename.Equals("Cstrike_Chat_T_Dead"))
        {
            TeamChat = true;
        }

        OnSayText2.OnSayText2(player, message, TeamChat);
        return HookResult.Continue;
    }
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (@event == null) return HookResult.Continue;

        var player = @event.Userid;
        if (!player.IsValid())return HookResult.Continue;

        if (g_Main.Player_Data.ContainsKey(player))g_Main.Player_Data.Remove(player);

        return HookResult.Continue;
    }

    public HookResult OnEventRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        if (@event == null)return HookResult.Continue;

        if (Configs.GetConfigData().Locally_Enable == 2)
        {
            Helper.DelayLogLocally();
        }

        if (Configs.GetConfigData().MySql_Enable == 2)
        {
            Helper.DelayLogMySql();
        }
        return HookResult.Continue;
    }

    public void OnMapEnd()
    {
        Helper.ClearVariables();

        if (Configs.GetConfigData().Locally_Enable == 3)
        {
            Helper.DelayLogLocally();
        }

        if (Configs.GetConfigData().MySql_Enable == 3)
        {
            Helper.DelayLogMySql();
        }
    }
    public override void Unload(bool hotReload)
    {
        Helper.ClearVariables();
    }


    /* [ConsoleCommand("css_test", "test")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void test(CCSPlayerController? player, CommandInfo commandInfo)
    {
        if (!player.IsValid())return;
    } */
}