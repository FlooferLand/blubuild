using System.Net;
using Godot;

namespace Project;

public partial class MiddleMenu : Control {
    [Export] public required PackedScene GameScene;
    [Export] public required Button StartPlaytest;
    [Export] public required LineEdit JoinByIP;
    [Export] public required LineEdit UsernameEdit;

    string defaultUsername = "User123";

    public override void _Ready() {
        defaultUsername = "User" + GD.RandRange(0, 999);
        UsernameEdit.PlaceholderText = $"Username (default: {defaultUsername})";

        StartPlaytest.Pressed += () => GetTree().ChangeSceneToPacked(GameScene);
        JoinByIP.TextSubmitted += text => {
            if (IPAddress.TryParse(text, out var ip)) {
                NetworkManager.RemoteJoinAddress = ip.ToString();
                GetTree().ChangeSceneToPacked(GameScene);
                return;
            }
            JoinByIP.Text = "Invalid IP address";
        };
    }
}
