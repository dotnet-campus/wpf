using System.Numerics;

namespace WpfGfxShape.Core.Av;

// Output frames describe queued audio, not media already consumed by the device.
internal sealed class AudioConsumptionTimeline
{
    private readonly Queue<Segment> _segments = new();
    private uint _sampleRate;
    private ulong _clockAnchor, _outputFrames, _sourceFrames;
    private UInt128 _observedFramesNumerator;
    private ulong _observedFrequency = 1;
    private long _mediaAnchor;
    private readonly record struct Segment(ulong OutputStart, uint OutputFrames, ulong SourceStart, uint SourceFrames);

    internal void Reset(long mediaPosition100ns, ulong clockPosition, uint sampleRate)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(mediaPosition100ns);
        ArgumentOutOfRangeException.ThrowIfZero(sampleRate);
        _segments.Clear();
        _clockAnchor = clockPosition;
        _mediaAnchor = mediaPosition100ns;
        _sampleRate = sampleRate;
        _outputFrames = _sourceFrames = 0;
        _observedFramesNumerator = 0;
        _observedFrequency = 1;
    }

    internal void Clear()
    {
        _segments.Clear();
        _sampleRate = 0;
        _outputFrames = _sourceFrames = _clockAnchor = 0;
        _mediaAnchor = 0;
        _observedFramesNumerator = 0;
        _observedFrequency = 1;
    }

    internal void Submit(uint outputFrames, uint sourceFrames)
    {
        if (_sampleRate == 0) throw new InvalidOperationException();
        ArgumentOutOfRangeException.ThrowIfZero(outputFrames);
        ArgumentOutOfRangeException.ThrowIfZero(sourceFrames);
        ulong outputEnd = checked(_outputFrames + outputFrames);
        ulong sourceEnd = checked(_sourceFrames + sourceFrames);
        _segments.Enqueue(new(_outputFrames, outputFrames, _sourceFrames, sourceFrames));
        _outputFrames = outputEnd;
        _sourceFrames = sourceEnd;
    }

    internal long Position100ns(ulong clockPosition, ulong frequency, ulong duration)
    {
        ArgumentOutOfRangeException.ThrowIfZero(frequency);
        if (_sampleRate == 0) return 0;
        ulong elapsed = clockPosition >= _clockAnchor ? clockPosition - _clockAnchor : 0;
        UInt128 numerator = UInt128.Min((UInt128)elapsed * _sampleRate, (UInt128)_outputFrames * frequency);
        // Retiring segments is destructive, so later observations cannot precede consumed audio.
        if ((BigInteger)numerator * _observedFrequency >= (BigInteger)_observedFramesNumerator * frequency)
        {
            _observedFramesNumerator = numerator;
            _observedFrequency = frequency;
        }
        BigInteger consumed = (BigInteger)_observedFramesNumerator;
        BigInteger denominator = _observedFrequency;
        BigInteger sourceNumerator = (BigInteger)_sourceFrames * denominator;
        while (_segments.TryPeek(out Segment segment))
        {
            if (consumed >= ((BigInteger)segment.OutputStart + segment.OutputFrames) * denominator)
            {
                _segments.Dequeue();
                continue;
            }
            sourceNumerator = (BigInteger)segment.SourceStart * denominator * segment.OutputFrames
                + (consumed - (BigInteger)segment.OutputStart * denominator) * segment.SourceFrames;
            denominator *= segment.OutputFrames;
            break;
        }
        BigInteger ticks = _mediaAnchor + sourceNumerator * 10000000 / (denominator * _sampleRate);
        return (long)BigInteger.Min(ticks, Math.Min(duration, (ulong)long.MaxValue));
    }
}
