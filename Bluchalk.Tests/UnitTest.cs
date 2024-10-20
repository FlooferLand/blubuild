namespace Bluchalk.Tests;

public class Tests {
    private RshowTransformer? rshow;

    [SetUp]
    public void Setup() {
        // Loading in an rshow
        rshow = new RshowTransformer();
    }

    [Test]
    public void TestRshow() {
        using var fileStream = File.OpenRead("../../../../tapes/test_raw/signal.mid");
        using (var streamReader = new StreamReader(fileStream)) {
            rshow?.Read(streamReader.BaseStream);
            Assert.Pass();
        }
        Assert.Fail();
    }
}