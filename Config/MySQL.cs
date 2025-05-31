using MySqlConnector;
using System.Text;
using Chat_Logger_GoldKingZ.Config;

namespace Chat_Logger_GoldKingZ;

public class MySqlDataManager
{
    private static string ConnectionString => new MySqlConnectionStringBuilder
    {
        Server = Configs.GetConfigData().MySql_Host,
        Port = Configs.GetConfigData().MySql_Port,
        Database = Configs.GetConfigData().MySql_Database,
        UserID = Configs.GetConfigData().MySql_Username,
        Password = Configs.GetConfigData().MySql_Password,
        CharacterSet = "utf8mb4",
        Pooling = true,
        MinimumPoolSize = 0,
        MaximumPoolSize = 100
    }.ConnectionString;

    private const string CreateTableQuery = @"
    CREATE TABLE IF NOT EXISTS chat_logs (
        id INT AUTO_INCREMENT PRIMARY KEY,
        date DATETIME NOT NULL,
        map_name VARCHAR(255) NOT NULL,
        steam_id VARCHAR(64) NOT NULL,
        player_name VARCHAR(128) NOT NULL,
        `where` INT NOT NULL,
        message LONGTEXT NOT NULL,
        server_id VARCHAR(36) NOT NULL DEFAULT '' -- Added column
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

    CREATE INDEX IF NOT EXISTS idx_date ON chat_logs(date);";

    private const string InsertQuery = @"
    INSERT INTO chat_logs 
    (date, map_name, steam_id, player_name, `where`, message, server_id)
    VALUES
    (@date, @map_name, @steam_id, @player_name, @where, @message, @server_id);";

    public static async Task CreateTableIfNotExistsAsync()
    {
        try
        {
            await using var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();

            bool tableExists;
            await using (var checkCmd = new MySqlCommand(
                "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = 'chat_logs'", 
                connection))
            {
                tableExists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync()) > 0;
            }

            await using (var cmd = new MySqlCommand(CreateTableQuery, connection))
            {
                await cmd.ExecuteNonQueryAsync();
            }
            
            Helper.DebugMessage(tableExists
                ? "Database table already exists - verified structure"
                : "Database table created successfully");
        }
        catch (Exception ex)
        {
            Helper.DebugMessage($"DB Init Error: {ex.Message}");
        }
    }

    public static async Task DeleteOldMessagesAsync()
    {
        if(Configs.GetConfigData().MySql_AutoDeleteLogsMoreThanXdaysOld < 1) return;
        
        using var connection = new MySqlConnection(ConnectionString);
        try
        {
            await connection.OpenAsync();

            var checkQuery = $@"SELECT COUNT(*) FROM chat_logs 
                            WHERE date < DATE_SUB(NOW(), INTERVAL @days DAY);";
            
            using var checkCmd = new MySqlCommand(checkQuery, connection);
            checkCmd.Parameters.Add("@days", MySqlDbType.Int32).Value = Configs.GetConfigData().MySql_AutoDeleteLogsMoreThanXdaysOld;
            var oldMessagesCount = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());

            if (oldMessagesCount == 0)
            {
                Helper.DebugMessage($"No messages older than {Configs.GetConfigData().MySql_AutoDeleteLogsMoreThanXdaysOld} days found");
                return;
            }

            Helper.DebugMessage($"Found {oldMessagesCount} old messages (older than {Configs.GetConfigData().MySql_AutoDeleteLogsMoreThanXdaysOld} days), starting reorganization...");
            
            await CreateTempTable(connection);
            await CopyDataToTempTable(connection);
            await SwapTables(connection);

            Helper.DebugMessage($"Successfully removed {oldMessagesCount} old messages and reset IDs");
        }
        catch (Exception ex)
        {
            Helper.DebugMessage($"Reorganization Error: {ex.Message}");
        }
    }

    private static async Task CopyDataToTempTable(MySqlConnection connection)
    {
        var copyQuery = $@"INSERT INTO chat_logs_new 
                    (date, map_name, steam_id, player_name, `where`, message, server_id)
                    SELECT date, map_name, steam_id, player_name, `where`, message, server_id 
                    FROM chat_logs 
                    WHERE date >= DATE_SUB(NOW(), INTERVAL @days DAY)
                    ORDER BY date;";

        using var cmd = new MySqlCommand(copyQuery, connection);
        cmd.Parameters.Add("@days", MySqlDbType.Int32).Value = Configs.GetConfigData().MySql_AutoDeleteLogsMoreThanXdaysOld;
        await cmd.ExecuteNonQueryAsync();
    }
    private static async Task CreateTempTable(MySqlConnection connection)
    {
        var createQuery = @"CREATE TABLE chat_logs_new (
        id INT AUTO_INCREMENT PRIMARY KEY,
        date DATETIME NOT NULL,
        map_name VARCHAR(255) NOT NULL,
        steam_id VARCHAR(64) NOT NULL,
        player_name VARCHAR(128) NOT NULL,
        `where` INT NOT NULL,
        message LONGTEXT NOT NULL,
        server_id VARCHAR(36) NOT NULL
    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 AUTO_INCREMENT=1;";

        using var cmd = new MySqlCommand(createQuery, connection);
        await cmd.ExecuteNonQueryAsync();
    }
    
    private static async Task SwapTables(MySqlConnection connection)
    {
        var swapQuery = @"RENAME TABLE chat_logs TO chat_logs_old,
                        chat_logs_new TO chat_logs;
                        DROP TABLE chat_logs_old;";

        using var cmd = new MySqlCommand(swapQuery, connection);
        await cmd.ExecuteNonQueryAsync();
    }

    public static async Task InsertChatLog(DateTime date, string mapName, string steamId, string playerName, int where, string message, string serverId)
    {
        using var connection = new MySqlConnection(ConnectionString);
        try
        {
            await connection.OpenAsync();
            
            using var cmd = new MySqlCommand(InsertQuery, connection);
            cmd.Parameters.Add("@date", MySqlDbType.DateTime).Value = date;
            cmd.Parameters.Add("@map_name", MySqlDbType.VarChar, 255).Value = mapName;
            cmd.Parameters.Add("@steam_id", MySqlDbType.VarChar, 64).Value = steamId;
            cmd.Parameters.Add("@player_name", MySqlDbType.VarChar, 128).Value = playerName;
            cmd.Parameters.Add("@where", MySqlDbType.Int32).Value = where;
            cmd.Parameters.Add("@message", MySqlDbType.LongText).Value = message;
            cmd.Parameters.Add("@server_id", MySqlDbType.VarChar, 36).Value = serverId;

            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Helper.DebugMessage($"Insert Error: {ex.Message}");
        }
    }

    public static async Task BatchInsertMessages(List<Globals.ChatMessageStorage> messages)
    {
        if (messages.Count == 0) return;

        using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();
        
        using var transaction = await connection.BeginTransactionAsync();
        
        try
        {
            var queryBuilder = new StringBuilder();
            queryBuilder.AppendLine("INSERT INTO chat_logs (date, map_name, steam_id, player_name, `where`, message, server_id) VALUES ");
            
            for (int i = 0; i < messages.Count; i++)
            {
                if (i > 0) queryBuilder.Append(", ");
                queryBuilder.Append($"(@date{i}, @map_name{i}, @steam_id{i}, @player_name{i}, @where{i}, @message{i}, @server_id{i})");
            }
            
            using var cmd = new MySqlCommand(queryBuilder.ToString(), connection, transaction);
            
            for (int i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                cmd.Parameters.Add($"@date{i}", MySqlDbType.DateTime).Value = message.Date;
                cmd.Parameters.Add($"@map_name{i}", MySqlDbType.VarChar, 255).Value = message.MapName;
                cmd.Parameters.Add($"@steam_id{i}", MySqlDbType.VarChar, 64).Value = message.SteamID64;
                cmd.Parameters.Add($"@player_name{i}", MySqlDbType.VarChar, 128).Value = message.PlayerName;
                cmd.Parameters.Add($"@where{i}", MySqlDbType.Int32).Value = message.Where;
                cmd.Parameters.Add($"@message{i}", MySqlDbType.LongText).Value = message.Message;
                cmd.Parameters.Add($"@server_id{i}", MySqlDbType.VarChar, 36).Value = message.ServerId;
            }
            
            await cmd.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
            
            Helper.DebugMessage($"Successfully inserted {messages.Count} chat messages");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Helper.DebugMessage($"Batch Insert Error: {ex.Message}");
        }
    }
}