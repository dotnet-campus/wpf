using System.Buffers.Binary;
using System.Runtime.InteropServices;

namespace WpfGfxShape.ComAcceptance;

public sealed unsafe partial class D3DImageTests
{
#if IF_PARTITION_RENDER_STATE
    /// <summary>Requires real consumer content and a real partition transition from the official HWND presentation path.</summary>
    [TestMethod]
    [Timeout(90000)]
    public void WhenIfOfficialHwndPresentsThenPartitionTransitionAndSnapshotsShareState() => RunHbConsumer(true, true);

    /// <summary>Applies the same real presentation contract to the preserved candidate.</summary>
    [TestMethod]
    [Timeout(90000)]
    public void WhenIfCandidateHwndPresentsThenPartitionTransitionAndSnapshotsShareState() => RunHbConsumer(false, true);
#endif

    private sealed class IfPartitionObservation : IDisposable
    {
        private readonly nint _module;
        private readonly nint _connection;
        private readonly nint _first;
        private readonly Action<string, object?> _record;
        private nint _second;
        private uint? _current;
        private int _transitions;

        internal IfPartitionObservation(nint module, nint connection, nint first, Action<string, object?> record)
        {
            _module = module;
            _connection = connection;
            _first = first;
            _record = record;
        }

        private nint Export(string name) => NativeLibrary.GetExport(_module, name);
        private void Check(string stage, int hr)
        {
            _record("IF " + stage, new { HResult = hr });
            Marshal.ThrowExceptionForHR(hr);
        }

        private void Register(nint channel, uint enabled, string stage)
        {
            uint* packet = stackalloc uint[] { 3, enabled };
            Check(stage + " send", ((delegate* unmanaged[Stdcall]<void*, uint, byte, nint, int>)Export("MilResource_SendCommand"))(packet, 8, 0, channel));
            Check(stage + " flush", ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilComposition_SyncFlush"))(channel));
        }

        private List<(uint Old, uint New)> Drain(nint channel, string stage, int index)
        {
            var states = new List<(uint Old, uint New)>();
            byte* buffer = stackalloc byte[64];
            for (int count = 0; count < 64; count++)
            {
                new Span<byte>(buffer, 64).Clear();
                int retrieved = 0;
                Check(stage + " peek", ((delegate* unmanaged[Stdcall]<nint, void*, nuint, int*, int>)Export("MilComposition_PeekNextMessage"))(channel, buffer, 64, &retrieved));
                if (retrieved == 0)
                {
                    _record("IF " + stage + " drained", new { Channel = index, StateMessages = states.Count });
                    return states;
                }
                Assert.AreEqual(1, retrieved);
                ReadOnlySpan<byte> bytes = new(buffer, 64);
                uint type = BinaryPrimitives.ReadUInt32LittleEndian(bytes);
                uint oldState = BinaryPrimitives.ReadUInt32LittleEndian(bytes[8..]);
                uint newState = BinaryPrimitives.ReadUInt32LittleEndian(bytes[12..]);
                _record("IF message", new { Stage = stage, Channel = index, Type = type, Old = oldState, New = newState, Hex = Convert.ToHexString(bytes) });
                if (type == 5)
                {
                    byte[] expected = new byte[64];
                    BinaryPrimitives.WriteUInt32LittleEndian(expected, 5);
                    BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(8), oldState);
                    BinaryPrimitives.WriteUInt32LittleEndian(expected.AsSpan(12), newState);
                    CollectionAssert.AreEqual(expected, bytes.ToArray());
                    states.Add((oldState, newState));
                }
            }
            throw new InvalidOperationException("IF bounded message drain did not converge.");
        }

        internal void RegisterInitial()
        {
            Register(_first, 1, "A initial registration");
            var states = Drain(_first, "A initial snapshot", 0);
            Assert.AreEqual(1, states.Count);
            Assert.AreEqual((1u, 1u), states[0]);
            _current = states[0].New;
        }

        internal void ObserveFrame(int frame)
        {
            var messages = Drain(_first, "content accepted frame " + frame, 0);
            Assert.AreEqual(frame == 0 ? 1 : 0, messages.Count);
            if (frame == 0) Assert.AreEqual((1u, 0u), messages[0]);
            foreach (var state in messages)
            {
                Assert.AreEqual(_current, (uint?)state.Old);
                if (state.Old != state.New) _transitions++;
                _current = state.New;
            }
            if (frame != 0)
            {
                Assert.AreEqual(0, Drain(_second, "B content accepted frame " + frame, 1).Count);
                return;
            }
            nint sibling = 0;
            Check("create B from A", ((delegate* unmanaged[Stdcall]<nint, nint, nint*, int>)Export("MilConnection_CreateChannel"))(_connection, _first, &sibling));
            _second = sibling;
            _record("IF sibling source", new { A = (long)_first, B = (long)_second, Source = (long)_first });
            Register(_second, 1, "B registration after real content");
            AssertSnapshot(Drain(_first, "B registration A broadcast", 0));
            AssertSnapshot(Drain(_second, "B registration B snapshot", 1));
            Register(_first, 0, "A disable");
            Assert.AreEqual(0, Drain(_first, "A disable", 0).Count);
            Assert.AreEqual(0, Drain(_second, "A disable B", 1).Count);
            Register(_first, 1, "A re-registration");
            AssertSnapshot(Drain(_first, "A re-registration A snapshot", 0));
            AssertSnapshot(Drain(_second, "A re-registration B broadcast", 1));
        }

        private void AssertSnapshot(List<(uint Old, uint New)> states)
        {
            Assert.AreEqual(1, states.Count);
            Assert.AreEqual(_current, (uint?)states[0].Old);
            Assert.AreEqual(_current, (uint?)states[0].New);
        }

        internal void AssertTransition()
        {
            _record("IF transition verdict", new { RealTransitions = _transitions, Current = _current, ConsumerContentAccepted = true, ConsumerActualBackend = "not independently observed" });
            Assert.AreEqual(1, _transitions);
            Assert.AreEqual((uint?)0, _current);
        }

        public void Dispose()
        {
            Register(_first, 0, "A final disable");
            if (_second != 0)
            {
                Register(_second, 0, "B final disable");
                Check("B destroy", ((delegate* unmanaged[Stdcall]<nint, int>)Export("MilConnection_DestroyChannel"))(_second));
                _second = 0;
            }
            _record("IF listener cleanup complete", null);
        }
    }
}
