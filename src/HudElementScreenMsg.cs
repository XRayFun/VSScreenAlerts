using System;
using Vintagestory.API.Client;

namespace VSScreenAlerts;

public class HudElementScreenMsg : HudElement
{
    private GuiElementDynamicText? dynText;
    private long expiresAtMs;

    public HudElementScreenMsg(ICoreClientAPI capi) : base(capi)
    {
        Compose();
    }

    private void Compose()
    {
        // Центр экрана, чуть выше прицела/курсора. Подстрой Y при желании.
        var dialogBounds = ElementBounds.Fixed(0, 0, 1100, 140)
            .WithAlignment(EnumDialogArea.CenterMiddle)
            .WithFixedOffset(0, -90);

        var textBounds = ElementBounds.Fixed(0, 0, 1100, 140);

        var font = CairoFont.WhiteDetailText()
            .WithFontSize(36)
            .WithStroke(new double[] { 0, 0, 0, 0.9 }, 3.0);

        SingleComposer = capi.Gui
            .CreateCompo("vsscreenalerts-hud", dialogBounds)
#pragma warning disable CS0618
            .AddDynamicText("", font, EnumTextOrientation.Center, textBounds, "msg")
#pragma warning restore CS0618
            .Compose();

        dynText = SingleComposer.GetDynamicText("msg");
    }

    public void Show(string text, int durationSeconds)
    {
        expiresAtMs = capi.World.ElapsedMilliseconds + Math.Max(1, durationSeconds) * 1000L;
        dynText?.SetNewText(text ?? "", true);
        TryOpen();
    }

    public void Clear()
    {
        expiresAtMs = 0;
        TryClose();
    }

    public override void OnRenderGUI(float deltaTime)
    {
        base.OnRenderGUI(deltaTime);

        if (IsOpened() && expiresAtMs > 0 && capi.World.ElapsedMilliseconds >= expiresAtMs)
        {
            Clear();
        }
    }

    public override bool PrefersUngrabbedMouse => true;
}