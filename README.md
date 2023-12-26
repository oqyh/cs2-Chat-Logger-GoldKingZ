# [CS2] Chat-Logger (1.0.0)

### Log Any Chat Discord Or Log Text

![chat](https://github.com/oqyh/cs2-Chat-Logger/assets/48490385/1a57cca9-5892-4014-9587-6ab4f21480bb)
![log](https://github.com/oqyh/cs2-Chat-Logger/assets/48490385/b745d3e6-e78d-4d91-ab6e-e8cbd7864413)
![discord](https://github.com/oqyh/cs2-Chat-Logger/assets/48490385/61e35453-8cc5-440b-a457-51c468fd2f39)


## .:[ Dependencies ]:.
[Metamod:Source (2.x)](https://www.sourcemm.net/downloads.php/?branch=master)

[CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp/releases)

[Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json) (Discord WebHook)


## .:[ Configuration ]:.
```json
{
  // If Its Enabled Logs Will Located in ../addons/counterstrikesharp/plugins/Chat_Logger/logs/
  "CLModeLogs": false,

  // you can use these in LogChatFormat and LogDiscordChatFormat
  //{TIME} == Time
  //{DATE} == Date
  //{MESSAGE} == Player Message
  //{PLAYERNAME} == Player Name Who Type In Chat
  //{TEAM} == Check If Player Wrote In Chat Team Or Public Chat
  //{STEAMID} = STEAM_0:1:122910632
  //{STEAMID64} = 76561198206086993
  //{IP} = 127.0.0.0
  "LogChatFormat": "[{TIME}] {PLAYERNAME} {MESSAGE} (SteamID: {STEAMID})",

  // Log File Format .txt or .pdf ect...
  "LogFileFormat": ".txt",
  // Date and Time Formate
  "LogFileDateFormat": "MM-dd-yyyy",
  "LogInsideFileTimeFormat": "HH:mm:ss",

  //Send Log To Discord Via WebHookURL
  "SendLogToWebHook": false,

  // you can use these in Connect or Disconnect Message
  //{TIME} == Time
  //{DATE} == Date
  //{MESSAGE} == Player Message
  //{PLAYERNAME} == Player Name Who Type In Chat
  //{TEAM} == Check If Player Wrote In Chat Team Or Public Chat
  //{STEAMID} = STEAM_0:1:122910632
  //{STEAMID64} = 76561198206086993
  //{IP} = 127.0.0.0
  "LogDiscordChatFormat": "[{DATE} - {TIME}] {TEAM} {PLAYERNAME} {MESSAGE} (IpAddress: {IP})",
  "WebHookURL": "https://discord.com/api/webhooks/XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
  "ConfigVersion": 1
}
```


## .:[ Change Log ]:.
```
(1.0.0)
-Initial Release
```

## .:[ Donation ]:.

If this project help you reduce time to develop, you can give me a cup of coffee :)

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://paypal.me/oQYh)
