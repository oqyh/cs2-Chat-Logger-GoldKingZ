using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using Chat_Logger_GoldKingZ.Config;

namespace Chat_Logger_GoldKingZ;

public class ChatLoggerGoldKingZ : BasePlugin
{
    public override string ModuleName => "Chat Logger (Log Chat To Locally/Discord WebHook/MySql)";
    public override string ModuleVersion => "1.1.1";
    public override string ModuleAuthor => "Gold KingZ";
    public override string ModuleDescription => "https://github.com/oqyh";
    public static ChatLoggerGoldKingZ Instance { get; set; } = new();
    private readonly PlayerChat _PlayerChat = new();
    public Globals g_Main = new();
    public override void Load(bool hotReload)
    {
        Instance = this;
        Configs.Load(ModuleDirectory);
        Configs.Shared.CookiesModule = ModuleDirectory;

        RegisterListener<Listeners.OnMapStart>(OnMapStart);
        RegisterEventHandler<EventPlayerConnectFull>(OnEventPlayerConnectFull);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        RegisterEventHandler<EventRoundEnd>(OnEventRoundEnd);
        RegisterListener<Listeners.OnMapEnd>(OnMapEnd);

        AddCommandListener("say", OnPlayerChat, HookMode.Post);
		AddCommandListener("say_team", OnPlayerChatTeam, HookMode.Post);
        
        if(hotReload)
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

            if(Configs.GetConfigData().MySql_Enable == 1 || Configs.GetConfigData().MySql_Enable == 2 || Configs.GetConfigData().MySql_Enable == 3)
            {
                MySqlDataManager.InitializeDatabase();

                if(Configs.GetConfigData().MySql_AutoDeleteLogsMoreThanXdaysOld > 0)
                {
                    _ = Task.Run(async () => 
                    {
                        try 
                        {
                            await MySqlDataManager.DeleteOldMessagesAsync();
                        }
                        catch (Exception ex)
                        {
                            Helper.DebugMessage($"Cleanup Error: {ex.Message}");
                        }
                    });
                }
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

        if(Configs.GetConfigData().MySql_Enable == 1 || Configs.GetConfigData().MySql_Enable == 2 || Configs.GetConfigData().MySql_Enable == 3)
        {
            MySqlDataManager.InitializeDatabase();

            if(Configs.GetConfigData().MySql_AutoDeleteLogsMoreThanXdaysOld > 0)
            {
                _ = Task.Run(async () => 
                {
                    try 
                    {
                        await MySqlDataManager.DeleteOldMessagesAsync();
                    }
                    catch (Exception ex)
                    {
                        Helper.DebugMessage($"Cleanup Error: {ex.Message}");
                    }
                });
            }
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
    
    private HookResult OnPlayerChat(CCSPlayerController? player, CommandInfo info)
	{
        if (!player.IsValid())return HookResult.Continue;

        _PlayerChat.OnPlayerChat(player, info, false);

        return HookResult.Continue;
    }
    private HookResult OnPlayerChatTeam(CCSPlayerController? player, CommandInfo info)
	{
        if (!player.IsValid())return HookResult.Continue;

        _PlayerChat.OnPlayerChat(player, info, true);

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