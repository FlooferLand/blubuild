using Godot;

namespace Project;

public partial class GraphicsLoader : WorldEnvironment {
    public override void _EnterTree() {
        Environment = GraphicsAutoload.ActiveWorldEnv;

        var viewport = GetViewport();
        GraphicsAutoload.Instance.Change += env => {
            Environment = env;
        };
        GraphicsAutoload.Instance.UpdateViewport += () => {
            GraphicsAutoload.SetupViewport(viewport);
        };
    }
}
