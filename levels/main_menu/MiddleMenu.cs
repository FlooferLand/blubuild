using System.Net;
using Godot;

namespace Project;

public partial class MiddleMenu : Control {
    [Export] public required PackedScene GameScene;
    [Export] public required PackedScene CharacterEditorScene;

    [Export] public required Button StartPlaytest;
    [Export] public required Button CharacterEditor;
    [Export] public required LineEdit JoinByIP;
    [Export] public required LineEdit UsernameEdit;
    [Export] public required Transition SceneTransition;

    string defaultUsername = "User123";
    PackedScene? nextScene = null;

    public override void _Ready() {
        defaultUsername = "User" + GD.RandRange(0, 999);
        UsernameEdit.PlaceholderText = $"Username (default: {defaultUsername})";

        StartPlaytest.Pressed += () => {
            NetworkManager.Username = (UsernameEdit.Text.Length > 0) ? UsernameEdit.Text : defaultUsername;
            nextScene = GameScene;
            SceneTransition.FadeIn();
        };
        CharacterEditor.Pressed += () => {
            nextScene = CharacterEditorScene;
            SceneTransition.FadeIn();
        };
        SceneTransition.Finished += () => {
            if (nextScene != null)
                GetTree().ChangeSceneToPacked(nextScene);
        };

        JoinByIP.TextSubmitted += text => {
            if (IPAddress.TryParse(text, out var ip)) {
                NetworkManager.Username = (UsernameEdit.Text.Length > 0) ? UsernameEdit.Text : defaultUsername;
                NetworkManager.JoinAddress = ip.ToString();
                GetTree().ChangeSceneToPacked(GameScene);
                return;
            }
            JoinByIP.Text = "Invalid IP address";
        };
    }
}
