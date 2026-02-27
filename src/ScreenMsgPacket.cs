using ProtoBuf;

namespace VSScreenAlerts;

[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
public class ScreenMsgPacket
{
    public string Text = "";
    public int DurationSeconds = 5;
    public bool Clear = false;
}