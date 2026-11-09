using System.Globalization;

namespace WpfGfxShape.ComAcceptance;

internal static class StrictRatePreflight
{
    internal const int EpisodeCount = 11;
    internal sealed record Step(string Name, double? Value = null);
    internal sealed record EncodedDouble(string Bits, string Classification, string? Decimal);

    internal static EncodedDouble Encode(double value) => new(
        $"{BitConverter.DoubleToUInt64Bits(value):X16}",
        double.IsNaN(value) ? "NaN" : double.IsPositiveInfinity(value) ? "PositiveInfinity" :
        double.IsNegativeInfinity(value) ? "NegativeInfinity" : value == 0 ?
        (BitConverter.DoubleToUInt64Bits(value) >> 63 == 1 ? "NegativeZero" : "PositiveZero") : "Finite",
        double.IsFinite(value) ? value.ToString("R", CultureInfo.InvariantCulture) : null);

    internal static Step[] Steps(int episode)
    {
        Step[] body = episode switch
        {
            0 => [new("Default"), new("Set", 0.5), new("Pause"), new("Set", 2), new("Play")],
            10 => [new("Default"), new("Pause"), new("Set", double.NaN), new("Play")],
            >= 1 and <= 9 => [new("Default"), new("Set", new[] { -1d, double.NegativeInfinity,
                BitConverter.UInt64BitsToDouble(0x8000000000000000), 0, 1, double.Epsilon,
                double.MaxValue, double.PositiveInfinity, double.NaN }[episode - 1])],
            _ => throw new ArgumentOutOfRangeException(nameof(episode))
        };
        return [.. body, new("ApplicationIdle"), new("Close"), new("Close-ApplicationIdle")];
    }

    internal static bool Recoverable(Exception error) => error is not OutOfMemoryException and not StackOverflowException;

    internal static void Execute(int episode, Action<Step> invoke, Action<Step, Exception?> observe)
    {
        foreach (Step step in Steps(episode))
        {
            Exception? caught = null;
            try { invoke(step); }
            catch (Exception error) when (Recoverable(error)) { caught = error; }
            observe(step, caught);
        }
    }
}
