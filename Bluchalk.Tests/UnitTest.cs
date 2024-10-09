namespace Bluchalk.Tests;

public class Tests {
    private RshowTransformer? rshow;

    [SetUp]
    public void Setup() {
        // Loading in an rshow
        rshow = new RshowTransformer();
    }

    [Test]
    public void Test1() {
        Assert.Pass();
    }
}