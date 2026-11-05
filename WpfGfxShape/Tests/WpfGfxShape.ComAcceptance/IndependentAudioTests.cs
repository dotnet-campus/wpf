extern alias Stage3Production;

namespace WpfGfxShape.ComAcceptance;

[TestClass]
public sealed class IndependentAudioTests
{
    [TestMethod]
    public Task WhenCompleteWaveRendersThenRealEndpointClockAndDrainAreRequired()
    {
        TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Thread thread = new(() =>
        {
            try
            {
                new Stage3Production::WpfGfxShape.Tests.IndependentAudioTests().WhenCompleteWaveIsRenderedThenEndpointClockAndDrainAreRequired();
                completion.SetResult();
            }
            catch (Exception error) { completion.SetException(error); }
        });
        thread.SetApartmentState(ApartmentState.MTA);
        thread.Start();
        return completion.Task;
    }

    [TestMethod]
    public Task WhenAudioDecodesThenFixedOraclePassesOnDedicatedMta()
    {
        TaskCompletionSource completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        Thread thread = new(() =>
        {
            try
            {
                new Stage3Production::WpfGfxShape.Tests.MediaFoundationSampleAbiTests().WhenSystemSampleDurationIsAssignedThenProductionGetterPreservesIt();
                new Stage3Production::WpfGfxShape.Tests.IndependentAudioTests().WhenWaveAndAviAreComparedThenBothFixedOraclesAreRequired();
                completion.SetResult();
            }
            catch (Exception error) { completion.SetException(error); }
        });
        thread.SetApartmentState(ApartmentState.MTA);
        thread.Start();
        return completion.Task;
    }
}
