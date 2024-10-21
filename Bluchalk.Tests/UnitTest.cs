namespace Bluchalk.Tests;

public class Tests {
    [SetUp]
    public void Setup() { }

    [Test]
    public void TestRaw() {
        var show = new RawMidiTransformer();
        using var fileStream = File.OpenRead("../../../../showtapes/test_raw/signal.mid");
        using (var streamReader = new StreamReader(fileStream)) {
            var result = show.Read(streamReader.BaseStream);
            if (result.LetOk(out var container)) {
                Assert.Pass();
            } else if (result.LetErr(out string err)) {
                Assert.Fail(err);
            }
        }
        Assert.Fail();
    }
}