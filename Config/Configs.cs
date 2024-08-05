using System.Text.Json;
using System.Text.Json.Serialization;

namespace Chat_Logger_GoldKingZ.Config
{
    public static class Configs
    {
        public static class Shared {
            public static string? CookiesModule { get; set; }
        }
        
        private static readonly string ConfigDirectoryName = "config";
        private static readonly string ConfigFileName = "config.json";
        private static string? _configFilePath;
        private static ConfigData? _configData;

        private static readonly JsonSerializerOptions SerializationOptions = new()
        {
            Converters =
            {
                new JsonStringEnumConverter()
            },
            WriteIndented = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
        };

        public static bool IsLoaded()
        {
            return _configData is not null;
        }

        public static ConfigData GetConfigData()
        {
            if (_configData is null)
            {
                throw new Exception("Config not yet loaded.");
            }
            
            return _configData;
        }

        public static ConfigData Load(string modulePath)
        {
            var configFileDirectory = Path.Combine(modulePath, ConfigDirectoryName);
            if(!Directory.Exists(configFileDirectory))
            {
                Directory.CreateDirectory(configFileDirectory);
            }

            _configFilePath = Path.Combine(configFileDirectory, ConfigFileName);
            if (File.Exists(_configFilePath))
            {
                _configData = JsonSerializer.Deserialize<ConfigData>(File.ReadAllText(_configFilePath), SerializationOptions);
            }
            else
            {
                _configData = new ConfigData();
            }

            if (_configData is null)
            {
                throw new Exception("Failed to load configs.");
            }

            SaveConfigData(_configData);
            
            return _configData;
        }

        private static void SaveConfigData(ConfigData configData)
        {
            if (_configFilePath is null)
            {
                throw new Exception("Config not yet loaded.");
            }
            string json = JsonSerializer.Serialize(configData, SerializationOptions);


            File.WriteAllText(_configFilePath, json);
        }

        public class ConfigData
        {
            public string empty { get; set; }
            public bool Text_EnableLoggingMessages { get; set; }
            public bool Text_PrivateTeamMessagesOnly { get; set; }
            public string Text_IncludeFlagsMessagesOnly { get; set; }
            public string Text_ExcludeFlagsMessages { get; set; }
            public string Text_ExcludeMessageContains { get; set; }
            public int Text_ExcludeMessageContainsLessThanXLetters { get; set; }
            public bool Text_ExcludeMessageDuplicate { get; set; }
            public string Text_MessageFormat { get; set; }
            public string Text_DateFormat { get; set; }
            public string Text_TimeFormat { get; set; }
            public int Text_AutoDeleteLogsMoreThanXdaysOld { get; set; }
            public string empty1 { get; set; }
            private int _Discord_EnableLoggingMessagesOnMode;
            public int Discord_EnableLoggingMessagesOnMode
            {
                get => _Discord_EnableLoggingMessagesOnMode;
                set
                {
                    _Discord_EnableLoggingMessagesOnMode = value;
                    if (_Discord_EnableLoggingMessagesOnMode < 0 || _Discord_EnableLoggingMessagesOnMode > 5)
                    {
                        Discord_EnableLoggingMessagesOnMode = 0;
                        Console.WriteLine("|||||||||||||||||||||||||||||||||||||||||||||||| I N V A L I D ||||||||||||||||||||||||||||||||||||||||||||||||");
                        Console.WriteLine("[Chat-Logger-GoldKingZ] Discord_EnableLoggingMessagesOnMode: is invalid, setting to default value (0) Please Choose 0 or 1 or 2 or 3 or 4 or 5.");
                        Console.WriteLine("[Chat-Logger-GoldKingZ] Discord_EnableLoggingMessagesOnMode (0) = Disable");
                        Console.WriteLine("[Chat-Logger-GoldKingZ] Discord_EnableLoggingMessagesOnMode (1) = Text Only");
                        Console.WriteLine("[Chat-Logger-GoldKingZ] Discord_EnableLoggingMessagesOnMode (2) = Text With + Name + Hyperlink To Steam Profile");
                        Console.WriteLine("[Chat-Logger-GoldKingZ] Discord_EnableLoggingMessagesOnMode (3) = Text With + Name + Hyperlink To Steam Profile + Profile Picture");
                        Console.WriteLine("[Chat-Logger-GoldKingZ] Discord_EnableLoggingMessagesOnMode (4) = Text With + Name + Hyperlink To Steam Profile + Profile Picture + Saparate Date And Time From Message");
                        Console.WriteLine("[Chat-Logger-GoldKingZ] Discord_EnableLoggingMessagesOnMode (5) = Text With + Name + Hyperlink To Steam Profile + Profile Picture + Saparate Date And Time From Message + Server Ip In Footer");
                        Console.WriteLine("|||||||||||||||||||||||||||||||||||||||||||||||| I N V A L I D ||||||||||||||||||||||||||||||||||||||||||||||||");
                    }
                }
            }
            public bool Discord_PrivateTeamMessagesOnly { get; set; }
            public string Discord_IncludeFlagsMessagesOnly { get; set; }
            public string Discord_ExcludeFlagsMessages { get; set; }
            public string Discord_ExcludeMessageContains { get; set; }
            public int Discord_ExcludeMessageContainsLessThanXLetters { get; set; }
            public bool Discord_ExcludeMessageDuplicate { get; set; }
            public string Discord_MessageFormat { get; set; }
            public string Discord_DateFormat { get; set; }
            public string Discord_TimeFormat { get; set; }
            private string? _Discord_SideColor;
            public string Discord_SideColor
            {
                get => _Discord_SideColor!;
                set
                {
                    _Discord_SideColor = value;
                    if (_Discord_SideColor.StartsWith("#"))
                    {
                        Discord_SideColor = _Discord_SideColor.Substring(1);
                    }
                }
            }
            public string Discord_WebHookURL { get; set; }
            public string Discord_UsersWithNoAvatarImage { get; set; }
            public string Discord_FooterImage { get; set; }
            public string empty2 { get; set; }
            public string Information_For_You_Dont_Delete_it { get; set; }
            
            public ConfigData()
            {
                empty = "---------------------------------vvv [ Text Local Save In (Chat-Logger-GoldKingZ/logs/)  ] vvv---------------------------------";
                Text_EnableLoggingMessages = false;
                Text_PrivateTeamMessagesOnly = false;
                Text_IncludeFlagsMessagesOnly = "";
                Text_ExcludeFlagsMessages = "@css/exclude,#css/exclude";
                Text_ExcludeMessageContains = "!./";
                Text_ExcludeMessageContainsLessThanXLetters = 0;
                Text_ExcludeMessageDuplicate = false;
                Text_MessageFormat = "[{TIME}] [{STEAMID}] {TEAM} ({PLAYERNAME}) {MESSAGE}";
                Text_DateFormat = "MM-dd-yyyy";
                Text_TimeFormat = "HH:mm:ss";
                Text_AutoDeleteLogsMoreThanXdaysOld = 0;
                empty1 = "------------------------------------------------------vvv [ Discord ] vvv------------------------------------------------------";
                Discord_EnableLoggingMessagesOnMode = 0;
                Discord_PrivateTeamMessagesOnly = false;
                Discord_IncludeFlagsMessagesOnly = "";
                Discord_ExcludeFlagsMessages = "@css/exclude,#css/exclude";
                Discord_ExcludeMessageContains = "!./";
                Discord_ExcludeMessageContainsLessThanXLetters = 0;
                Discord_ExcludeMessageDuplicate = false;
                Discord_MessageFormat = "[{TIME}] [{STEAMID}] {TEAM} ({PLAYERNAME}) {MESSAGE}";
                Discord_DateFormat = "MM-dd-yyyy";
                Discord_TimeFormat = "HH:mm:ss";
                Discord_SideColor = "00FFFF";
                Discord_WebHookURL = "https://discord.com/api/webhooks/XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX";
                Discord_UsersWithNoAvatarImage = "https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/blob/main/Resources/avatar.jpg?raw=true";
                Discord_FooterImage = "https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/blob/main/Resources/serverip.png?raw=true";
                empty2 = "-----------------------------------------------------------------------------------";
                Information_For_You_Dont_Delete_it = " Vist  [https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/tree/main?tab=readme-ov-file#-configuration-] To Understand All Above";
            }
        }
    }
}