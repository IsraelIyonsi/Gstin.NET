namespace ExternalConsumer.SmokeTests;

using Gstin;

public class ConsumerSmokeTests
{
    [Fact]
    public void Entry_class_resolves_from_a_foreign_namespace()
    {
        var ok = GstinNumber.IsValid("27AAPFU0939F1ZV");

        Assert.True(ok);
        Assert.Equal("27", GstinNumber.Parse("27AAPFU0939F1ZV").StateCode);
    }
}
