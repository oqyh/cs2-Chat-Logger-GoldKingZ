## .:[ Join Our Discord For Support ]:.

<a href="https://discord.com/invite/U7AuQhu"><img src="https://discord.com/api/guilds/651838917687115806/widget.png?style=banner2"></a>

# [CS2] Chat-Logger-GoldKingZ (1.1.1)

Log Chat To Locally/Discord WebHook/MySql

![webchatlogger](https://github.com/user-attachments/assets/14247d1a-b3c2-4140-a7c3-445c9ac70dc7)

![ingame](https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/assets/48490385/c9f6012b-06f2-4bd5-a215-2f49128d1cba)

![text](https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/assets/48490385/fade3be6-54a9-49e9-82e5-1b9c6cf55280)

![Mode1](https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/assets/48490385/fdae2251-9aea-45a8-a37c-5ec2de8bfbdd)

![Mode5](https://github.com/oqyh/cs2-Chat-Logger-GoldKingZ/assets/48490385/7811046a-2b76-4758-8298-f701b45c28a7)

---

## 📦 Dependencies
[![Metamod:Source](https://img.shields.io/badge/Metamod:Source-2d2d2d?logo=sourceengine)](https://www.sourcemm.net)

[![CounterStrikeSharp](https://img.shields.io/badge/CounterStrikeSharp-83358F)](https://github.com/roflmuffin/CounterStrikeSharp)

[![MySQL](https://img.shields.io/badge/MySQL-4479A1?logo=mysql&logoColor=white)](https://dev.mysql.com/doc/connector-net/en/) [Included in zip]

[![JSON](https://img.shields.io/badge/JSON-000000?logo=json)](https://www.newtonsoft.com/json) [Included in zip]

---

## 📥 Installation

### Plugin Installation
1. Download the latest `Chat-Logger-GoldKingZ.x.x.x.zip` release
2. Extract contents to your `csgo` directory
3. Configure settings in `Chat-Logger-GoldKingZ/config/config.json`
4. Restart your server

### Web Interface Setup
1. Download the latest `webserver.x.x.x.zip` release
2. Extract to your web server directory
3. Configure MySQL connections in `database.php`

---

## ⚙️ Configuration

> [!NOTE]
> Located In ..\Chat-Logger-GoldKingZ\config\config.json                                           
>

<details open>
<summary><b>Locally Config</b> (Click to expand 🔽)</summary>

| Property | Description | Values | Required |  
|----------|-------------|--------|----------|  
| `Locally_Enable` | Save Chat Messages Locally | `0`-Disable<br>`1`-Log when player chats<br>`2`-Log and send at round end<br>`3`-Log and send at map end | - |  
| `Locally_LogMessagesOnly` | Log Messages Only | `1`-Both public and team chat<br>`2`-Public chat only<br>`3`-Team chat only | `Locally_Enable=1/2/3` |  
| `Locally_IncludeTheseFlagsMessagesOnly` | Log These Flags Only | Example: `!76561198206086993,@css/include`<br>`""` = Everyone | `Locally_Enable=1/2/3` |  
| `Locally_ExcludeFlagsMessages` | Don't Log These Flags | Example: `@css/exclude,#css/exclude`<br>`""` = Exclude none | `Locally_Enable=1/2/3` |  
| `Locally_ExcludeMessagesStartWith` | Exclude Messages Starting With | Example: `!./`<br>`""` = Disable | `Locally_Enable=1/2/3` |  
| `Locally_ExcludeMessagesContainsLessThanXLetters` | Exclude Short Messages | Minimum letters<br>`0` = Disable | `Locally_Enable=1/2/3` |  
| `Locally_ExcludeMessagesDuplicate` | Exclude Duplicate Messages | `true`/`false` | `Locally_Enable=1/2/3` |  
| `Locally_MessageFormat` | Message Format | Template with placeholders<br>`""` = Disable | `Locally_Enable=1/2/3` |  
| `Locally_DateFormat` | Date Format | Examples: `MM-dd-yyyy` | `Locally_Enable=1/2/3` |  
| `Locally_TimeFormat` | Time Format | Examples: `HH:mm:ss` | `Locally_Enable=1/2/3` |  
| `Locally_AutoDeleteLogsMoreThanXdaysOld` | Auto Delete Old Logs | Days to keep<br>`0` = Disable | `Locally_Enable=1/2/3` |  

</details>

<details>
<summary><b>Discord Config</b> (Click to expand 🔽)</summary>

| Property | Description | Values | Required |  
|----------|-------------|--------|----------|  
| `Discord_WebHook` | Discord WebHook URL | Example: `https://discord.com/api/webhooks/...`<br>`""` = Disable | - |  
| `Discord_Style` | Message Appearance Style | `0`-Disable<br>`1`-Text only<br>`2`-Text+Name+Link<br>`3`-+Profile Picture<br>`4`-+Separate Date/Time<br>`5`-+Server IP footer | `Discord_WebHook` |  
| `Discord_SideColor` | Message Side Color | Hex color code (e.g. `00FFFF`) | `Discord_Style=2/3/4/5` |  
| `Discord_FooterImage` | Footer Image URL | Image URL | `Discord_Style=3/4/5` |  
| `Discord_UsersWithNoAvatarImage` | Default Avatar Image | Image URL | `Discord_Style=5` |  
| `Discord_LogMessagesOnly` | Log Messages Only | `1`-Both chats<br>`2`-Public only<br>`3`-Team only | `Discord_WebHook` |  
| `Discord_IncludeTheseFlagsMessagesOnly` | Log These Flags Only | Example: `!76561198206086993`<br>`""` = Everyone | `Discord_WebHook` |  
| `Discord_ExcludeFlagsMessages` | Exclude These Flags | Example: `@css/exclude`<br>`""` = Exclude none | `Discord_WebHook` |  
| `Discord_ExcludeMessagesStartWith` | Exclude Messages Starting With | Example: `!./`<br>`""` = Disable | `Discord_WebHook` |  
| `Discord_ExcludeMessagesContainsLessThanXLetters` | Exclude Short Messages | Minimum letters<br>`0` = Disable | `Discord_WebHook` |  
| `Discord_ExcludeMessagesDuplicate` | Exclude Duplicates | `true`/`false` | `Discord_WebHook` |  
| `Discord_MessageFormat` | Message Format | Template with placeholders | `Discord_WebHook` |  
| `Discord_DateFormat` | Date Format | Examples: `MM-dd-yyyy` | `Discord_WebHook` |  
| `Discord_TimeFormat` | Time Format | Examples: `HH:mm:ss` | `Discord_WebHook` |  

</details>

<details>
<summary><b>MySQL Config</b> (Click to expand 🔽)</summary>

| Property | Description | Values | Required |  
|----------|-------------|--------|----------|  
| `MySql_Enable` | Save to MySQL | `0`-Disable<br>`1`-Log immediately<br>`2`-Log at round end<br>`3`-Log at map end | - |  
| `MySql_Host` | MySQL Host | Example: `123.45.67.89` | `MySql_Enable=1/2/3` |  
| `MySql_Database` | Database Name | Example: `Chat_Logs` | `MySql_Enable=1/2/3` |  
| `MySql_Username` | Database Username | Example: `root` | `MySql_Enable=1/2/3` |  
| `MySql_Password` | Database Password | Example: `Password123123` | `MySql_Enable=1/2/3` |  
| `MySql_Port` | Database Port | Default: `3306` | `MySql_Enable=1/2/3` |  
| `MySql_LogMessagesOnly` | Log Messages Only | `1`-Both chats<br>`2`-Public only<br>`3`-Team only | `MySql_Enable=1/2/3` |  
| `MySql_IncludeTheseFlagsMessagesOnly` | Log These Flags Only | Example: `!76561198206086993`<br>`""` = Everyone | `MySql_Enable=1/2/3` |  
| `MySql_ExcludeFlagsMessages` | Exclude These Flags | Example: `@css/exclude`<br>`""` = Exclude none | `MySql_Enable=1/2/3` |  
| `MySql_ExcludeMessagesStartWith` | Exclude Messages Starting With | Example: `!./`<br>`""` = Disable | `MySql_Enable=1/2/3` |  
| `MySql_ExcludeMessagesContainsLessThanXLetters` | Exclude Short Messages | Minimum letters<br>`0` = Disable | `MySql_Enable=1/2/3` |  
| `MySql_ExcludeMessagesDuplicate` | Exclude Duplicates | `true`/`false` | `MySql_Enable=1/2/3` |  
| `MySql_AutoDeleteLogsMoreThanXdaysOld` | Auto Delete Old Logs | Days to keep<br>`0` = Disable | `MySql_Enable=1/2/3` |  

</details>

<details>
<summary><b>Utilities Config</b> (Click to expand 🔽)</summary>

| Property | Description | Values | Required |  
|----------|-------------|--------|----------|  
| `EnableDebug` | Enable Debug Mode | `true`-Yes<br>`false`-No | - |  

</details>

---

## 📜 Changelog

<details>
<summary><b>📋 View Version History</b> (Click to expand 🔽)</summary>

### [1.1.1]
#### **General Changes**  
- Reworked plugin for better stability  
- Fixed Plugin Only Works With css_plugins reload
- Added config descriptions in `config.json`  
- New `EnableDebug` option  

#### **Local Logging (Locally_)**  
- Added `Locally_Enable` (logs at round/map end)  
- `Locally_LogMessagesOnly` filters by chat type (Team/Public/Both)  
- Supports SteamID formats (`!STEAM_0:1:122910632`, `!U:1:245821265`, `!245821265`, `!76561198206086993`)
- Fixed `Locally_MessageFormat`  

#### **Discord Logging (Discord_)**  
- Removed `Discord_EnableLoggingMessagesOnMode`  
- Fixed `Discord_Style` formatting  
- Added `Discord_LogMessagesOnly` (Team/Public/Both)  
- Supports SteamID formats (`!STEAM_0:1:122910632`, `!U:1:245821265`, `!245821265`, `!76561198206086993`)  

#### **MySQL Logging (New Feature)**  
- Added MySql_Enable
- Added MySql_Host
- Added MySql_Database
- Added MySql_Username
- Added MySql_Password
- Added MySql_Port
- Added MySql_LogMessagesOnly
- Added MySql_IncludeTheseFlagsMessagesOnly
- Added MySql_ExcludeFlagsMessages
- Added MySql_ExcludeMessagesStartWith
- Added MySql_ExcludeMessagesContainsLessThanXLetters
- Added MySql_ExcludeMessagesDuplicate
- Added MySql_AutoDeleteLogsMoreThanXdaysOld

#### **Web Interface (New Feature)**  
- Added web-based log viewer  

### [1.1.0]
- Fixed some bugs
- Fixed bind not logging

### [1.0.9]
- Fixed some bugs
- Fixed Text_ExcludeMessageContains
- Fixed Discord_ExcludeMessageContains

### [1.0.8]
- Fixed some bugs
- Fixed Text_IncludeFlagsMessagesOnly
- Fixed Text_ExcludeFlagsMessages
- Fixed Discord_IncludeFlagsMessagesOnly
- Fixed Discord_ExcludeFlagsMessages

### [1.0.7]
- Upgraded from .NET 7 to .NET 8
- Fixed some bugs
- Reworked chat logger
- Added modes 4 and 5 to SendLogToWebHook
- Added Discord_FooterImage
- Separated Discord log from text log

### [1.0.6]
- Fixed some bugs
- Fixed AutoDeleteLogsMoreThanXdaysOld
- Fixed SendLogToWebHook (3) for no avatar users
- Added IncludeMessageGroups
- Added UsersWithNoAvatarImage

### [1.0.5]
- Fixed some bugs
- Removed "SteamApi"

### [1.0.4]
- Added "ExcludeMessageGroups"
- Added "ExcludeMessageContainsLessThanXLetters"
- Added "ExcludeMessageDuplicate"
- Added "AutoDeleteLogsMoreThanXdaysOld"
- Added "SendLogToWebHook" modes 1/2/3
- Added "SideColorMessage"
- Added "SteamApi"
- Fixed some bugs

### [1.0.3]
- Added "ExcludeMessage"
- Added "ExcludeMessageContains"
- Fixed "LogChatFormat" and "LogDiscordChatFormat" not logging when other plugins touch "say" and "say_team"

### [1.0.2]
- Fixed "LogDiscordChatFormat"

### [1.0.1]
- Added {STEAMID3} and {STEAMID32} to:
  - "LogChatFormat"
  - "LogInsideFileTimeFormat"
- Fixed some bugs
- Improved Discord message styling with Steam links

### [1.0.0]
- Initial Release

</details>

---
