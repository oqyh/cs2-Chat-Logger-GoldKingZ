using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Text;

namespace Chat_Logger_GoldKingZ.Config
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RangeAttribute : Attribute
    {
        public int Min { get; }
        public int Max { get; }
        public int Default { get; }
        public string Message { get; }

        public RangeAttribute(int min, int max, int defaultValue, string message)
        {
            Min = min;
            Max = max;
            Default = defaultValue;
            Message = message;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class CommentAttribute : Attribute
    {
        public string Comment { get; }

        public CommentAttribute(string comment)
        {
            Comment = comment;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class BreakLineAttribute : Attribute
    {
        public string BreakLine { get; }

        public BreakLineAttribute(string breakLine)
        {
            BreakLine = breakLine;
        }
    }
    public static class Configs
    {
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
            var defaultConfig = new ConfigData();
            if (File.Exists(_configFilePath))
            {
                try
                {
                    _configData = JsonSerializer.Deserialize<ConfigData>(File.ReadAllText(_configFilePath), SerializationOptions);
                }
                catch (JsonException)
                {
                    _configData = MergeConfigWithDefaults(_configFilePath, defaultConfig);
                }
                
                _configData!.Validate();
            }
            else
            {
                _configData = defaultConfig;
                _configData.Validate();
            }

            SaveConfigData(_configData);
            return _configData;
        }

        private static ConfigData MergeConfigWithDefaults(string path, ConfigData defaults)
        {
            var mergedConfig = new ConfigData();
            var jsonText = File.ReadAllText(path);
            
            var readerOptions = new JsonReaderOptions 
            { 
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip 
            };

            using var doc = JsonDocument.Parse(jsonText, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            });
            
            foreach (var jsonProp in doc.RootElement.EnumerateObject())
            {
                var propInfo = typeof(ConfigData).GetProperty(jsonProp.Name);
                if (propInfo == null) continue;

                try
                {
                    var jsonValue = JsonSerializer.Deserialize(
                        jsonProp.Value.GetRawText(), 
                        propInfo.PropertyType,
                        new JsonSerializerOptions
                        {
                            Converters = { new JsonStringEnumConverter() },
                            ReadCommentHandling = JsonCommentHandling.Skip
                        }
                    );
                    propInfo.SetValue(mergedConfig, jsonValue);
                }
                catch (JsonException)
                {
                    propInfo.SetValue(mergedConfig, propInfo.GetValue(defaults));
                }
            }
            
            return mergedConfig;
        }

        private static void SaveConfigData(ConfigData configData)
        {
            if (_configFilePath is null)
                throw new Exception("Config not yet loaded.");

            var json = JsonSerializer.Serialize(configData, SerializationOptions);
            
            var lines = json.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var newLines = new List<string>();

            foreach (var line in lines)
            {
                var match = Regex.Match(line, @"^\s*""(\w+)""\s*:.*");
                bool isPropertyLine = false;
                PropertyInfo? propInfo = null;

                if (match.Success)
                {
                    string propName = match.Groups[1].Value;
                    propInfo = typeof(ConfigData).GetProperty(propName);

                    var breakLineAttr = propInfo?.GetCustomAttribute<BreakLineAttribute>();
                    if (breakLineAttr != null)
                    {
                        string breakLine = breakLineAttr.BreakLine;

                        if (breakLine.Contains("{space}"))
                        {
                            breakLine = breakLine.Replace("{space}", "").Trim();

                            if (breakLineAttr.BreakLine.StartsWith("{space}"))
                            {
                                newLines.Add("");
                            }

                            newLines.Add("// " + breakLine);
                            newLines.Add("");
                        }
                        else
                        {
                            newLines.Add("// " + breakLine);
                        }
                    }

                    var commentAttr = propInfo?.GetCustomAttribute<CommentAttribute>();
                    if (commentAttr != null)
                    {
                        var commentLines = commentAttr.Comment.Split('\n');
                        foreach (var commentLine in commentLines)
                        {
                            newLines.Add("// " + commentLine.Trim());
                        }
                    }

                    isPropertyLine = true;
                }

                newLines.Add(line);

                if (isPropertyLine && propInfo?.GetCustomAttribute<CommentAttribute>() != null)
                {
                    newLines.Add("");
                }
            }

            var adjustedLines = new List<string>();
            foreach (var line in newLines)
            {
                adjustedLines.Add(line);
                if (Regex.IsMatch(line, @"^\s*\],?\s*$"))
                {
                    adjustedLines.Add("");
                }
            }

            File.WriteAllText(_configFilePath, string.Join(Environment.NewLine, adjustedLines), Encoding.UTF8);
        }

        public class ConfigData
        {
            private string? _Version;
            private string? _Link;
            [BreakLine("----------------------------[ ↓ Plugin Info ↓ ]----------------------------{space}")]
            public string Version
            {
                get => _Version!;
                set
                {
                    _Version = value;
                    if (_Version != MainPlugin.Instance.ModuleVersion)
                    {
                        Version = MainPlugin.Instance.ModuleVersion;
                    }
                }
            }

            public string Link
            {
                get => _Link!;
                set
                {
                    _Link = value;
                    if (_Link != "https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ")
                    {
                        Link = "https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ";
                    }
                }
            }

            [BreakLine("{space}----------------------------[ ↓ Locally Config ↓ ]----------------------------{space}")]
            [Comment("Save Chat Messages Locally (In ../Chat-Logger-GoldKingZ/logs/)?\n0 = No, Disable\n1 = Yes, But Log When Player Chat Direct\n2 = Yes, But Log And Send All Messages When Round End (Recommended For Performance)\n3 = Yes, But Log And Send All Messages When Map End (Recommended For Performance)")]
            [Range(0, 3, 1, "[Chat Logger] Locally_Enable: is invalid, setting to default value (1) Please Choose From 0 To 3.\n[Chat Logger] 0 = No, Disable This Feature\n[Chat Logger] 1 = Yes, But Log When Player Chat Direct\n[Chat Logger] 2 = Yes, But Log And Send All Messages When Round End (Recommended For Performance)\n[Chat Logger] 3 = Yes, But Log And Send All Messages When Map End (Recommended For Performance)")]
            public int Locally_Enable { get; set; }

            [Comment("Required [Locally_Enable = 1/2/3]\nLog Messages Only:\n1 = Both Public Chat And Team Chat\n2 = Public Chat Only\n3 = Team Chat Only")]
            [Range(1, 3, 1, "[Chat Logger] Locally_LogMessagesOnly: is invalid, setting to default value (1) Please Choose From 1 To 3.\n[Chat Logger] 1 = Both Public Chat And Team Chat\n[Chat Logger] 2 = Public Chat Only\n[Chat Logger] 3 = Team Chat Only")]
            public int Locally_LogMessagesOnly { get; set; }

            [Comment("Required [Locally_Enable = 1/2/3]\nLog These Flags Messages Only And Ignore Log Others\nExample:\n\"SteamID: 76561198206086993,76561198974936845 | Flag: @css/vips,@css/admins | Group: #css/vips,#css/admins\"\n\"\" = To Log Everyone")]
            public string Locally_IncludeTheseFlagsMessagesOnly { get; set; }

            [Comment("Required [Locally_Enable = 1/2/3]\nDont Log These Flags Messages And Log Others\nExample:\n\"SteamID: 76561198206086993,76561198974936845 | Flag: @css/vips,@css/admins | Group: #css/vips,#css/admins\"\n\"\" = To Exclude Everyone")]
            public string Locally_ExcludeFlagsMessages { get; set; }
            
            [Comment("Required [Locally_Enable = 1/2/3]\nDont Log Messages If It Start With\n\"\" = Disable This Feature")]
            public string Locally_ExcludeMessagesStartWith { get; set; }

            [Comment("Required [Locally_Enable = 1/2/3]\nDont Log Messages If It Contains Less Than X Letters\n0 = Disable This Feature")]
            public int Locally_ExcludeMessagesContainsLessThanXLetters { get; set; }

            [Comment("Required [Locally_Enable = 1/2/3]\nDont Log Messages If It Duplicates Previous Message?\ntrue = Yes\nfalse = No")]
            public bool Locally_ExcludeMessagesDuplicate { get; set; }

            [Comment("Required [Locally_Enable = 1/2/3]\nHow Do You Like The Message Format\n{DATE} = [Locally_DateFormat]\n{TIME} = [Locally_TimeFormat]\n{PLAYER_NAME} = Player Name\n{PLAYER_MESSAGE} = Player Message\n{PLAYER_TEAM} = Check If Player Wrote In Chat Team Or Public Chat [TEAM]\n{PLAYER_STEAMID} = STEAM_0:1:122910632\n{PLAYER_STEAMID3} = U:1:245821265\n{PLAYER_STEAMID32} = 245821265\n{PLAYER_STEAMID64} = 76561198206086993\n{PLAYER_IP} = 123.45.67.89\n\"\" = Disable This Feature")]
            public string Locally_MessageFormat { get; set; }

            [Comment("Required [Locally_Enable = 1/2/3]\nHow Do You Like Date Format\nExamples:\ndd MM yyyy = 25 12 2023\nMM/dd/yy = 12/25/23\nMM-dd-yyyy = 12-25-2025")]
            public string Locally_DateFormat { get; set; }

            [Comment("Required [Locally_Enable = 1/2/3]\nHow Do You Like Time Format\nExamples:\nHH:mm = 14:30\nhh:mm a = 02:30 PM\nHH:mm:ss = 14:30:45")]
            public string Locally_TimeFormat { get; set; }

            [Comment("Required [Locally_Enable = 1/2/3]\nAuto Delete File Logs That Pass Than X Old Days\n0 = Disable This Feature")]
            public int Locally_AutoDeleteLogsMoreThanXdaysOld { get; set; }

            
            [BreakLine("{space}----------------------------[ ↓ Discord Config ↓ ]----------------------------{space}")]


            [Comment("Discord WebHook\nExample: https://discord.com/api/webhooks/XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX\n\"\" = Disable This Feature")]
            public string Discord_WebHook { get; set; }

            [Comment("Required [Discord_WebHook]\nHow Do You Like Message Look Like\n1 = Text Only (Result Image : https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/blob/main/Resources/Mode1.png?raw=true)\n2 = Text With + Name + Hyperlink To Steam Profile (Result Image : https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/blob/main/Resources/Mode2.png?raw=true)\n3 = Text With + Name + Hyperlink To Steam Profile + Profile Picture (Result Image : https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/blob/main/Resources/Mode3.png?raw=true)\n4 = Text With + Name + Hyperlink To Steam Profile + Profile Picture + Saparate Date And Time From Message (Result Image : https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/blob/main/Resources/Mode4.png?raw=true)\n5 = Text With + Name + Hyperlink To Steam Profile + Profile Picture + Saparate Date And Time From Message + Server Ip In Footer (Result Image : https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/blob/main/Resources/Mode5.png?raw=true)\n0 = Disable")]
            [Range(1, 5, 3, "[Chat Logger] Discord_Style: is invalid, setting to default value (3) Please Choose From 1 To 5.\n[Chat Logger] 1 = Text Only (Result Image : https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/blob/main/Resources/Mode1.png?raw=true)\n[Chat Logger] 2 = Text With + Name + Hyperlink To Steam Profile (Result Image : https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/blob/main/Resources/Mode2.png?raw=true)\n[Chat Logger] 3 = Text With + Name + Hyperlink To Steam Profile + Profile Picture (Result Image : https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/blob/main/Resources/Mode3.png?raw=true)\n[Chat Logger] 4 = Text With + Name + Hyperlink To Steam Profile + Profile Picture + Saparate Date And Time From Message (Result Image : https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/blob/main/Resources/Mode4.png?raw=true)\n[Chat Logger] 5 = Text With + Name + Hyperlink To Steam Profile + Profile Picture + Saparate Date And Time From Message + Server Ip In Footer (Result Image : https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/blob/main/Resources/Mode5.png?raw=true)")]
            public int Discord_Style { get; set; }

            [Comment("Required [Discord_Style 2/3/4/5]\nHow Would You Side Color Message To Be Use This Site (https://htmlcolorcodes.com/color-picker) For Color Pick")]
            public string Discord_SideColor { get; set; }

            [Comment("Required [Discord_Style 3/4/5]\nFooter Image")]
            public string Discord_FooterImage { get; set; }

            [Comment("Required [Discord_Style 5]\nIf Player Doest Have Avatar What Should We Replace It")]
            public string Discord_UsersWithNoAvatarImage { get; set; }

            [Comment("Required [Discord_WebHook]\nLog Messages Only:\n1 = Both Public Chat And Team Chat\n2 = Public Chat Only\n3 = Team Chat Only")]
            [Range(1, 3, 1, "[Chat Logger] Discord_LogMessagesOnly: is invalid, setting to default value (1) Please Choose From 1 To 3.\n[Chat Logger] 1 = Both Public Chat And Team Chat\n[Chat Logger] 2 = Public Chat Only\n[Chat Logger] 3 = Team Chat Only")]
            public int Discord_LogMessagesOnly { get; set; }

            [Comment("Required [Discord_WebHook]\nLog These Flags Messages Only And Ignore Log Others\nExample:\n\"SteamID: 76561198206086993,76561198974936845 | Flag: @css/vips,@css/admins | Group: #css/vips,#css/admins\"\n\"\" = To Log Everyone")]
            public string Discord_IncludeTheseFlagsMessagesOnly { get; set; }

            [Comment("Required [Discord_WebHook]\nDont Log These Flags Messages And Log Others\nExample:\n\"SteamID: 76561198206086993,76561198974936845 | Flag: @css/vips,@css/admins | Group: #css/vips,#css/admins\"\n\"\" = To Exclude Everyone")]
            public string Discord_ExcludeFlagsMessages { get; set; }
            
            [Comment("Required [Discord_WebHook]\nDont Log Messages If It Start With\n\"\" = Disable This Feature")]
            public string Discord_ExcludeMessagesStartWith { get; set; }

            [Comment("Required [Discord_WebHook]\nDont Log Messages If It Contains Less Than X Letters\n0 = Disable This Feature")]
            public int Discord_ExcludeMessagesContainsLessThanXLetters { get; set; }

            [Comment("Required [Discord_WebHook]\nDont Log Messages If It Duplicates Previous Message?\ntrue = Yes\nfalse = No")]
            public bool Discord_ExcludeMessagesDuplicate { get; set; }

            [Comment("Required [Discord_WebHook]\nHow Do You Like The Message Format\n{DATE} = [Discord_DateFormat]\n{TIME} = [Discord_TimeFormat]\n{PLAYER_NAME} = Player Name\n{PLAYER_MESSAGE} = Player Message\n{PLAYER_TEAM} = Check If Player Wrote In Chat Team Or Public Chat [TEAM]\n{PLAYER_STEAMID} = STEAM_0:1:122910632\n{PLAYER_STEAMID3} = U:1:245821265\n{PLAYER_STEAMID32} = 245821265\n{PLAYER_STEAMID64} = 76561198206086993\n{PLAYER_IP} = 123.45.67.89\n\"\" = Disable This Feature")]
            public string Discord_MessageFormat { get; set; }

            [Comment("Required [Discord_WebHook]\nHow Do You Like Date Format\nExamples:\ndd MM yyyy = 25 12 2023\nMM/dd/yy = 12/25/23\nMM-dd-yyyy = 12-25-2025")]
            public string Discord_DateFormat { get; set; }

            [Comment("Required [Discord_WebHook]\nHow Do You Like Time Format\nExamples:\nHH:mm = 14:30\nhh:mm a = 02:30 PM\nHH:mm:ss = 14:30:45")]
            public string Discord_TimeFormat { get; set; }
            
            
            [BreakLine("{space}----------------------------[ ↓ MySql Config ↓ ]----------------------------{space}")]
            

            [Comment("Save Chat Messages Into MySql?\n0 = No, Disable\n1 = Yes, But Log When Player Chat Direct\n2 = Yes, But Log And Send All Messages When Round End (Recommended For Performance)\n3 = Yes, But Log And Send All Messages When Map End (Recommended For Performance)")]
            [Range(0, 3, 0, "[Chat Logger] MySql_Enable: is invalid, setting to default value (0) Please Choose From 0 To 3.\n[Chat Logger] 0 = No, Disable\n[Chat Logger] 1 = Yes, But Log When Player Chat Direct\n[Chat Logger] 2 = Yes, But Log And Send All Messages When Round End (Recommended For Performance)\n[Chat Logger] 3 = Yes, But Log And Send All Messages When Map End (Recommended For Performance)")]
            public int MySql_Enable { get; set; }

            [Comment("MySql Host\nExample:\n123.45.67.89")]
            public string MySql_Host { get; set; }

            [Comment("MySql Database\nExample:\nChat_Logs")]
            public string MySql_Database { get; set; }

            [Comment("MySql Username\nExample:\nroot")]
            public string MySql_Username { get; set; }

            [Comment("MySql Password\nExample:\nPassword123123")]
            public string MySql_Password { get; set; }

            [Comment("MySql Password\nExample:\n3306")]
            public uint MySql_Port { get; set; }

            [Comment("Required [MySql_Enable = 1/2/3]\nLog Messages Only:\n1 = Both Public Chat And Team Chat\n2 = Public Chat Only\n3 = Team Chat Only")]
            [Range(1, 3, 1, "[Chat Logger] MySql_LogMessagesOnly: is invalid, setting to default value (1) Please Choose From 1 To 3.\n[Chat Logger] 1 = Both Public Chat And Team Chat\n[Chat Logger] 2 = Public Chat Only\n[Chat Logger] 3 = Team Chat Only")]
            public int MySql_LogMessagesOnly { get; set; }

            [Comment("Required [MySql_Enable = 1/2/3]\nLog These Flags Messages Only And Ignore Log Others\nExample:\n\"SteamID: 76561198206086993,76561198974936845 | Flag: @css/vips,@css/admins | Group: #css/vips,#css/admins\"\n\"\" = To Log Everyone")]
            public string MySql_IncludeTheseFlagsMessagesOnly { get; set; }

            [Comment("Required [MySql_Enable = 1/2/3]\nDont Log These Flags Messages And Log Others\nExample:\n\"SteamID: 76561198206086993,76561198974936845 | Flag: @css/vips,@css/admins | Group: #css/vips,#css/admins\"\n\"\" = To Exclude Everyone")]
            public string MySql_ExcludeFlagsMessages { get; set; }
            
            [Comment("Required [MySql_Enable = 1/2/3]\nDont Log Messages If It Start With\n\"\" = Disable This Feature")]
            public string MySql_ExcludeMessagesStartWith { get; set; }

            [Comment("Required [MySql_Enable = 1/2/3]\nDont Log Messages If It Contains Less Than X Letters\n0 = Disable This Feature")]
            public int MySql_ExcludeMessagesContainsLessThanXLetters { get; set; }

            [Comment("Required [MySql_Enable = 1/2/3]\nDont Log Messages If It Duplicates Previous Message?\ntrue = Yes\nfalse = No")]
            public bool MySql_ExcludeMessagesDuplicate { get; set; }

            [Comment("Required [MySql_Enable = 1/2/3]\nAuto Delete Logs That Pass Than X Old Days\n0 = Disable This Feature")]
            public int MySql_AutoDeleteLogsMoreThanXdaysOld { get; set; }


            [BreakLine("{space}----------------------------[ ↓ Utilities  ↓ ]----------------------------{space}")]

            [Comment("Enable Debug Plugin In Server Console (Helps You To Debug Issues You Facing)?\ntrue = Yes\nfalse = No")]
            public bool EnableDebug { get; set; }
            
            public ConfigData()
            {
                Version = MainPlugin.Instance.ModuleVersion;
                Link = "https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ";

                Locally_Enable = 1;
                Locally_LogMessagesOnly = 1;
                Locally_IncludeTheseFlagsMessagesOnly = "";
                Locally_ExcludeFlagsMessages = "Flags : @css/exclude | Groups : #css/exclude";
                Locally_ExcludeMessagesStartWith = "!./";
                Locally_ExcludeMessagesContainsLessThanXLetters = 0;
                Locally_ExcludeMessagesDuplicate = false;
                Locally_MessageFormat = "[{TIME}] [{PLAYER_STEAMID}] {PLAYER_NAME} {PLAYER_TEAM}{PLAYER_MESSAGE}";
                Locally_DateFormat = "MM-dd-yyyy";
                Locally_TimeFormat = "HH:mm:ss";
                Locally_AutoDeleteLogsMoreThanXdaysOld = 7;
                
                Discord_WebHook = "";
                Discord_Style = 4;
                Discord_SideColor = "00FFFF";
                Discord_FooterImage = "https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/blob/main/Resources/serverip.png?raw=true";
                Discord_UsersWithNoAvatarImage = "https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/blob/main/Resources/avatar.jpg?raw=true";
                Discord_LogMessagesOnly = 1;
                Discord_IncludeTheseFlagsMessagesOnly = "";
                Discord_ExcludeFlagsMessages = "Flags : @css/exclude | Groups : #css/exclude";
                Discord_ExcludeMessagesStartWith = "!./";
                Discord_ExcludeMessagesContainsLessThanXLetters = 0;
                Discord_ExcludeMessagesDuplicate = false;
                Discord_MessageFormat = "[{TIME}] [{PLAYER_STEAMID}] {PLAYER_NAME} {PLAYER_TEAM}{PLAYER_MESSAGE}";
                Discord_DateFormat = "MM-dd-yyyy";
                Discord_TimeFormat = "HH:mm:ss";

                MySql_Enable = 0;
                MySql_Host = "MySql_Host";
                MySql_Database = "MySql_Database";
                MySql_Username = "MySql_Username";
                MySql_Password = "MySql_Password";
                MySql_Port = 3306;
                MySql_LogMessagesOnly = 1;
                MySql_IncludeTheseFlagsMessagesOnly = "";
                MySql_ExcludeFlagsMessages = "Flags : @css/exclude | Groups : #css/exclude";
                MySql_ExcludeMessagesStartWith = "!./";
                MySql_ExcludeMessagesContainsLessThanXLetters = 0;
                MySql_ExcludeMessagesDuplicate = false;
                MySql_AutoDeleteLogsMoreThanXdaysOld = 7;

                EnableDebug = false;
            }
            public void Validate()
            {
                foreach (var prop in GetType().GetProperties())
                {
                    var rangeAttr = prop.GetCustomAttribute<RangeAttribute>();
                    if (rangeAttr != null && prop.PropertyType == typeof(int))
                    {
                        int value = (int)prop.GetValue(this)!;
                        if (value < rangeAttr.Min || value > rangeAttr.Max)
                        {
                            prop.SetValue(this, rangeAttr.Default);
                            Helper.DebugMessage(rangeAttr.Message,false);
                        }
                    }
                }
            }
        }
    }
}