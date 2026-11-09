using System.Text.Json;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class StrictRatePreflightTests
{
    /// <summary>Freezes the operation order for every independent player.</summary>
    [TestMethod]
    [DataRow(0, "Default,Set,Pause,Set,Play,ApplicationIdle,Close,Close-ApplicationIdle")]
    [DataRow(1, "Default,Set,ApplicationIdle,Close,Close-ApplicationIdle")]
    [DataRow(2, "Default,Set,ApplicationIdle,Close,Close-ApplicationIdle")]
    [DataRow(3, "Default,Set,ApplicationIdle,Close,Close-ApplicationIdle")]
    [DataRow(4, "Default,Set,ApplicationIdle,Close,Close-ApplicationIdle")]
    [DataRow(5, "Default,Set,ApplicationIdle,Close,Close-ApplicationIdle")]
    [DataRow(6, "Default,Set,ApplicationIdle,Close,Close-ApplicationIdle")]
    [DataRow(7, "Default,Set,ApplicationIdle,Close,Close-ApplicationIdle")]
    [DataRow(8, "Default,Set,ApplicationIdle,Close,Close-ApplicationIdle")]
    [DataRow(9, "Default,Set,ApplicationIdle,Close,Close-ApplicationIdle")]
    [DataRow(10, "Default,Pause,Set,Play,ApplicationIdle,Close,Close-ApplicationIdle")]
    public void WhenEpisodeRunsThenSequenceIsFrozen(int episode, string expected)
    {
        Assert.AreEqual(expected, string.Join(',', StrictRatePreflight.Steps(episode).Select(step => step.Name)));
    }

    /// <summary>Retains exceptional inputs and signed zero without JSON numeric normalization.</summary>
    [TestMethod]
    [DataRow(1, "BFF0000000000000", "Finite")]
    [DataRow(2, "FFF0000000000000", "NegativeInfinity")]
    [DataRow(3, "8000000000000000", "NegativeZero")]
    [DataRow(4, "0000000000000000", "PositiveZero")]
    [DataRow(5, "3FF0000000000000", "Finite")]
    [DataRow(6, "0000000000000001", "Finite")]
    [DataRow(7, "7FEFFFFFFFFFFFFF", "Finite")]
    [DataRow(8, "7FF0000000000000", "PositiveInfinity")]
    [DataRow(9, "FFF8000000000000", "NaN")]
    public void WhenInputIsEncodedThenBitsAndClassificationAreExact(int episode, string bits, string classification)
    {
        var encoded = StrictRatePreflight.Encode(StrictRatePreflight.Steps(episode)[1].Value!.Value);
        using var json = JsonDocument.Parse(JsonSerializer.Serialize(encoded));
        Assert.AreEqual(bits + ":" + classification,
            json.RootElement.GetProperty("Bits").GetString() + ":" + json.RootElement.GetProperty("Classification").GetString());
    }

    /// <summary>Does not canonicalize a NaN payload.</summary>
    [TestMethod]
    public void WhenNaNHasPayloadThenPayloadIsPreserved()
    {
        Assert.AreEqual("7FF8000000000042", StrictRatePreflight.Encode(BitConverter.UInt64BitsToDouble(0x7FF8000000000042)).Bits);
    }

    /// <summary>Every failing call is observed and cannot suppress later operations.</summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(5)]
    [DataRow(6)]
    [DataRow(7)]
    [DataRow(8)]
    [DataRow(9)]
    [DataRow(10)]
    public void WhenEveryCallThrowsThenAllSubsequentCallsStillRun(int episode)
    {
        var observed = new List<string>();
        StrictRatePreflight.Execute(episode, _ => throw new InvalidOperationException(),
            (step, error) => observed.Add(step.Name + ":" + error?.GetType().Name));
        CollectionAssert.AreEqual(StrictRatePreflight.Steps(episode).Select(step => step.Name + ":InvalidOperationException").ToArray(), observed.ToArray());
    }

    /// <summary>Only episode, invocation and observation are accepted; roles cannot select business steps.</summary>
    [TestMethod]
    public void WhenBusinessSequenceIsSelectedThenRoleIsNotAnInput()
    {
        var method = typeof(StrictRatePreflight).GetMethod("Execute", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
        CollectionAssert.AreEqual(new[] { typeof(int), typeof(Action<StrictRatePreflight.Step>), typeof(Action<StrictRatePreflight.Step, Exception>) },
            method.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
    }

    /// <summary>The preflight remains a media scene without pixel or pure-video routing.</summary>
    [TestMethod]
    public void WhenPreflightIsRegisteredThenItIsKnownMedia()
    {
        Assert.IsTrue(StrictRenderCase.IsKnown(StrictRenderCase.MediaAudioRatePreflight) && StrictRenderCase.IsMedia(StrictRenderCase.MediaAudioRatePreflight));
    }
}
