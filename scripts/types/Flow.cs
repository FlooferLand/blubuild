namespace Project;

public class Flow(float @in = 1.0f, float @out = 1.0f) {
    public float In = @in;
    public float Out = @out;

    public void Set(Flow flow) => Set(flow.In, flow.Out);
    public void Set(float @in, float @out) {
        In = @in;
        Out = @out;
    }
}
