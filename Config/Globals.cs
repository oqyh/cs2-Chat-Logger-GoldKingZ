namespace Chat_Logger_GoldKingZ;

public class Globals
{
    public static Dictionary<ulong, string> Client_Text1 = new Dictionary<ulong, string>();
    public static Dictionary<ulong, string> Client_Text2 = new Dictionary<ulong, string>();
    public static Dictionary<ulong, bool> TeamChat = new Dictionary<ulong, bool>();
    public static Dictionary<ulong, bool> TextExclude = new Dictionary<ulong, bool>();
    public static Dictionary<ulong, bool> TextIncude = new Dictionary<ulong, bool>();
    public static Dictionary<ulong, bool> DiscordExclude = new Dictionary<ulong, bool>();
    public static Dictionary<ulong, bool> DiscordIncude = new Dictionary<ulong, bool>();
}