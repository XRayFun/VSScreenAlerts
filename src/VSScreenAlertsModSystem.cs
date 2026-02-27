using System;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace VSScreenAlerts;

public class VSScreenAlertsModSystem : ModSystem
{
    private const string ChannelName = "vsscreenalerts";

    private ICoreServerAPI? sapi;
    private IServerNetworkChannel? serverChannel;

    private HudElementScreenMsg? hud;

    public override void StartServerSide(ICoreServerAPI api)
    {
        sapi = api;

        serverChannel = api.Network
            .RegisterChannel(ChannelName)
            .RegisterMessageType<ScreenMsgPacket>();

        api.RegisterCommand("screenmsg",
            "Show an on-screen message to all players",
            "/screenmsg <seconds> <text...>",
            CmdScreenAll,
            "controlserver");

        api.RegisterCommand("screenmsgp",
            "Show an on-screen message to one online player",
            "/screenmsgp <playername|uid> <seconds> <text...>",
            CmdScreenPlayer,
            "controlserver");

        api.RegisterCommand("screenmsgclear",
            "Clear on-screen message for all players",
            "/screenmsgclear",
            CmdClearAll,
            "controlserver");

        api.RegisterCommand("screenmsgclearp",
            "Clear on-screen message for one online player",
            "/screenmsgclearp <playername|uid>",
            CmdClearPlayer,
            "controlserver");
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        hud = new HudElementScreenMsg(api);

        api.Network
            .RegisterChannel(ChannelName)
            .RegisterMessageType<ScreenMsgPacket>()
            .SetMessageHandler<ScreenMsgPacket>(OnClientPacket);
    }

    private void OnClientPacket(ScreenMsgPacket pkt)
    {
        if (hud == null) return;

        if (pkt.Clear)
        {
            hud.Clear();
            return;
        }

        int seconds = pkt.DurationSeconds <= 0 ? 1 : pkt.DurationSeconds;
        hud.Show(pkt.Text ?? "", seconds);
    }

    private static string ParseText(CmdArgs args)
    {
        // Allow users to type \n for new lines.
        var text = (args.PopAll() ?? "").Trim();
        return text.Replace("\\n", "\n");
    }

    private IServerPlayer? FindOnlinePlayer(string nameOrUid)
    {
        if (sapi == null) return null;
        if (string.IsNullOrWhiteSpace(nameOrUid)) return null;

        nameOrUid = nameOrUid.Trim();

        return sapi.World.AllOnlinePlayers
            .OfType<IServerPlayer>()
            .FirstOrDefault(p =>
                p.PlayerName.Equals(nameOrUid, StringComparison.OrdinalIgnoreCase) ||
                p.PlayerUID.Equals(nameOrUid, StringComparison.OrdinalIgnoreCase));
    }

    private void CmdScreenAll(IServerPlayer caller, int groupId, CmdArgs args)
    {
        if (sapi == null || serverChannel == null) return;

        int seconds = args.PopInt(5) ?? 5;
        seconds = Math.Clamp(seconds, 1, 24 * 60 * 60);

        string text = ParseText(args);
        if (string.IsNullOrWhiteSpace(text))
        {
            sapi.SendMessage(caller, groupId, "Usage: /screenmsg <seconds> <text...>", EnumChatType.CommandError);
            return;
        }

        serverChannel.BroadcastPacket(new ScreenMsgPacket { Text = text, DurationSeconds = seconds, Clear = false });
    }

    private void CmdScreenPlayer(IServerPlayer caller, int groupId, CmdArgs args)
    {
        if (sapi == null || serverChannel == null) return;

        string playerName = args.PopWord("");
        int seconds = args.PopInt(5) ?? 5;
        seconds = Math.Clamp(seconds, 1, 24 * 60 * 60);

        string text = ParseText(args);

        if (string.IsNullOrWhiteSpace(playerName) || string.IsNullOrWhiteSpace(text))
        {
            sapi.SendMessage(caller, groupId, "Usage: /screenmsgp <playername|uid> <seconds> <text...>", EnumChatType.CommandError);
            return;
        }

        var target = FindOnlinePlayer(playerName);
        if (target == null)
        {
            sapi.SendMessage(caller, groupId, $"Player not found (online): {playerName}", EnumChatType.CommandError);
            return;
        }

        serverChannel.SendPacket(new ScreenMsgPacket { Text = text, DurationSeconds = seconds, Clear = false }, target);
    }

    private void CmdClearAll(IServerPlayer caller, int groupId, CmdArgs args)
    {
        if (serverChannel == null) return;
        serverChannel.BroadcastPacket(new ScreenMsgPacket { Clear = true });
    }

    private void CmdClearPlayer(IServerPlayer caller, int groupId, CmdArgs args)
    {
        if (sapi == null || serverChannel == null) return;

        string playerName = args.PopWord("");

        if (string.IsNullOrWhiteSpace(playerName))
        {
            sapi.SendMessage(caller, groupId, "Usage: /screenmsgclearp <playername|uid>", EnumChatType.CommandError);
            return;
        }

        var target = FindOnlinePlayer(playerName);
        if (target == null)
        {
            sapi.SendMessage(caller, groupId, $"Player not found (online): {playerName}", EnumChatType.CommandError);
            return;
        }

        serverChannel.SendPacket(new ScreenMsgPacket { Clear = true }, target);
    }
}