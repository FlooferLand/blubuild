using Godot;
using Godot.Collections;

namespace Project;

// TODO: Replace all these external services with https://blubuild.flooferland.com/ services
public partial class MultiplayerStarter : Control {
    [Export] public required HttpRequest IpRequest;
    [Export] public required HttpRequest PortCheckRequest;
    [Export] public required Button GetJoinIp;
    [Export] public required LineEdit IpAddress;
    [Export] public required Label IpInfoLabel;

    public override void _Ready() {
        IpAddress.FocusEntered += () => {
            IpAddress.SelectAll();
            DisplayServer.ClipboardSet(IpAddress.Text);
        };
        GetJoinIp.Pressed += () => {
            var status = IpRequest.Request("https://api.ipify.org");
            if (status != Error.Ok) IpAddress.Text = $"Error getting IP:\n{status}";
        };
        IpRequest.RequestCompleted += (result, code, headers, body) => {
            if (code != 200) {
                IpInfoLabel.Visible = true;
                IpInfoLabel.Text = $"Error getting IP:\nHTTP code {code}";
                return;
            }

            string ip = body.GetStringFromUtf8();
            IpAddress.Text = $"IP: {ip}";
            IpAddress.Visible = true;
            IpAddress.GrabFocus();

            // Checking if the port is accessible
            var status = PortCheckRequest.Request($"https://portchecker.io/api/me/{NetworkManager.ServerPort}");
            if (status != Error.Ok) GD.PushError($"Error port checking:\n{status}");
        };

        // TODO: Use a stun server instead of this crap. Currently always reports false since its only checking UDP.
        PortCheckRequest.RequestCompleted += (result, code, headers, body) => {
            if (code != 200) {
                GD.PushError($"Failed to port check: Code {code}");
                return;
            }
            bool isOpen = body.GetStringFromUtf8().Equals("true", System.StringComparison.CurrentCultureIgnoreCase);

            IpInfoLabel.Visible = true;
            IpInfoLabel.Text = isOpen
                ? $"Port {NetworkManager.ServerPort} was found open!\nPlayers should be able to join."
                : $"Your server may be inaccessible.\nMake sure to open port {NetworkManager.ServerPort} on your router!";
        };
    }
}
