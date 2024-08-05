## .:[ Join Our Discord For Support ]:.

<a href="https://discord.com/invite/U7AuQhu"><img src="https://discord.com/api/guilds/651838917687115806/widget.png?style=banner2"></a>

***
# [CS2] Chat-Logger-GoldKingZ (1.0.8)

### Log InGame Chat And Send It To Log Text Or Discord WebHook

![ingame](https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/assets/48490385/c9f6012b-06f2-4bd5-a215-2f49128d1cba)

![text](https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/assets/48490385/fade3be6-54a9-49e9-82e5-1b9c6cf55280)

![Mode1](https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/assets/48490385/fdae2251-9aea-45a8-a37c-5ec2de8bfbdd)

![Mode5](https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/assets/48490385/7811046a-2b76-4758-8298-f701b45c28a7)


## .:[ Dependencies ]:.
[Metamod:Source (2.x)](https://www.sourcemm.net/downloads.php/?branch=master)

[CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/releases)

[Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json)

## .:[ Configuration ]:.

> [!CAUTION]
> Config Located In ..\addons\counterstrikesharp\plugins\Chat-Logger-GoldKingZ\config\config.json                                           
>

```json
{
  //---------------------------------vvv [ Text Local Save In (Chat-Logger-GoldKingZ/logs/)  ] vvv---------------------------------
  //Enable Logging Text Located In Chat-Logger-GoldKingZ/logs/ ?
  "Text_EnableLoggingMessages": false,

  //Log Only Team Chat Messages?
  "Text_PrivateTeamMessagesOnly": false,

  //Include These Group From Logging Only "" Means Everyone
  "Text_IncludeFlagsMessagesOnly": "",

  //Exclude These Group From Logging Only "" Means Everyone
  "Text_ExcludeFlagsMessages": "@css/exclude,#css/exclude",

  //Exclude Message if begin "!" or "." or "/"
  "Text_ExcludeMessageContains": "!./",

  //Exclude Message If Contains Less Than X Letters
  "Text_ExcludeMessageContainsLessThanXLetters": 0,

  //Exclude Dublicated Messages
  "Text_ExcludeMessageDuplicate": false,

  //Log Message Format
  //{TIME} == Time
  //{DATE} == Date
  //{MESSAGE} == Player Message
  //{PLAYERNAME} == Player Name Who Type In Chat
  //{TEAM} == Check If Player Wrote In Chat Team Or Public Chat [TEAM] [ALL]
  //{STEAMID} = STEAM_0:1:122910632
  //{STEAMID3} = U:1:245821265
  //{STEAMID32} = 245821265
  //{STEAMID64} = 76561198206086993
  //{IP} = 127.0.0.0
  "Text_MessageFormat": "[{TIME}] [{STEAMID}] {TEAM} ({PLAYERNAME}) {MESSAGE}",

  //Date and Time Formate
  "Text_DateFormat": "MM-dd-yyyy",
  "Text_TimeFormat": "HH:mm:ss",

  //Auto Delete Logs If More Than X (Days) Old
  "Text_AutoDeleteLogsMoreThanXdaysOld": 0,

//------------------------------------------------------vvv [ Discord ] vvv------------------------------------------------------

  //Send Log To Discord Via WebHookURL
  // (0) = Disable
  // (1) = Text Only (Result Image : https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/blob/main/Resources/Mode1.png?raw=true)
  // (2) = Text With + Name + Hyperlink To Steam Profile (Result Image : https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/blob/main/Resources/Mode2.png?raw=true)
  // (3) = Text With + Name + Hyperlink To Steam Profile + Profile Picture (Result Image : https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/blob/main/Resources/Mode3.png?raw=true)
  // (4) = Text With + Name + Hyperlink To Steam Profile + Profile Picture + Saparate Date And Time From Message (Result Image : https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/blob/main/Resources/Mode4.png?raw=true)
  // (5) = Text With + Name + Hyperlink To Steam Profile + Profile Picture + Saparate Date And Time From Message + Server Ip In Footer (Result Image : https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/blob/main/Resources/Mode5.png?raw=true)
  "Discord_EnableLoggingMessagesOnMode": 0,

  //Send Only Team Chat Messages?
  "Discord_PrivateTeamMessagesOnly": false,

  //Include These Group From Logging Only "" Means Everyone
  "Discord_IncludeFlagsMessagesOnly": "",

  //Exclude These Group From Logging Only "" Means Everyone
  "Discord_ExcludeFlagsMessages": "@css/exclude,#css/exclude",

  //Exclude Message if begin "!" or "." or "/"
  "Discord_ExcludeMessageContains": "!./",

  //Exclude Message If Contains Less Than X Letters
  "Discord_ExcludeMessageContainsLessThanXLetters": 0,

  //Exclude Dublicated Messages
  "Discord_ExcludeMessageDuplicate": false,

  //Discord Message Format
  "Discord_MessageFormat": "[{TIME}] [{STEAMID}] {TEAM} ({PLAYERNAME}) {MESSAGE}",

  //Date and Time Formate
  "Discord_DateFormat": "MM-dd-yyyy",
  "Discord_TimeFormat": "HH:mm:ss",

  //If Discord_EnableLoggingMessagesOnMode (2) or (3) or  (4) or (5) How Would You Side Color Message To Be Check (https://www.color-hex.com/) For Colors
  "Discord_SideColor": "00FFFF",

  //Discord WebHookURL
  "Discord_WebHookURL": "https://discord.com/api/webhooks/XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",

  ////If Discord_EnableLoggingMessagesOnMode (3) or  (4) or (5) And Player Doesn't Have Profile Picture Which Picture Do You Like To Be Replaced
  "Discord_UsersWithNoAvatarImage": "https://steamcdn-a.akamaihd.net/steamcommunity/public/images/avatars/b5/b5bd56c1aa4644a474a2e4972be27ef9e82e517e_full.jpg",

  //If Discord_EnableLoggingMessagesOnMode (5) Image Url Footer
  "Discord_FooterImage": "https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/blob/main/Resources/serverip.png?raw=true",

}
```

## .:[ Change Log ]:.
```
(1.0.8)
-Fix Some Bugs
-Fixed Text_IncludeFlagsMessagesOnly
-Fixed Text_ExcludeFlagsMessages
-Fixed Discord_IncludeFlagsMessagesOnly
-Fixed Discord_ExcludeFlagsMessages

(1.0.7)
-Upgrade Net.7 To Net.8
-Fix Some Bugs
-Rework Chat Logger
-Added Modes To SendLogToWebHook 4 and 5
-Added Discord_FooterImage
-Saparate Discord Log From Text Log 

(1.0.6)
-Fix Some Bugs
-Fix AutoDeleteLogsMoreThanXdaysOld
-Fix SendLogToWebHook (3) For No Avatar Users
-Added IncludeMessageGroups
-Added UsersWithNoAvatarImage

(1.0.5)
-Fix Some Bugs
-Remove "SteamApi"

(1.0.4)
-Added "ExcludeMessageGroups" 
-Added "ExcludeMessageContainsLessThanXLetters"
-Added "ExcludeMessageDuplicate"
-Added "AutoDeleteLogsMoreThanXdaysOld"
-Added "SendLogToWebHook" Mode 1/2/3
-Added "SideColorMessage"
-Added "SteamApi"
-Fix Some Bugs

(1.0.3)
-Added "ExcludeMessage" 
-Added "ExcludeMessageContains"
-Fix "LogChatFormat" and "LogDiscordChatFormat" not log if other plugin touch "say" and "say_team" 

(1.0.2)
-Fix "LogDiscordChatFormat"

(1.0.1)
-Added {STEAMID3} {STEAMID32} To
"LogChatFormat"
"LogInsideFileTimeFormat"
-Fix Some Bugs
-Fix Discord message now better style with link to steam

(1.0.0)
-Initial Release
```

## .:[ Donation ]:.

If this project help you reduce time to develop, you can give me a cup of coffee :)

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://paypal.me/oQYh)
