using System.Runtime.InteropServices;
using WpfGfxShape.Core.Av;

namespace WpfGfxShape.Tests;

[TestClass]
public sealed unsafe class MediaFoundationSampleAbiTests
{
    [TestMethod]
    public void WhenSampleSlotsAreComparedWithSdkThenDurationIsGetter()
    {
        // Independent SDK order: mfobjects.h 996–1275; not derived from production constants.
        string[] methods = ["GetSampleFlags", "SetSampleFlags", "GetSampleTime", "SetSampleTime", "GetSampleDuration", "SetSampleDuration", "GetBufferCount", "GetBufferByIndex", "ConvertToContiguousBuffer", "AddBuffer", "RemoveBufferByIndex", "RemoveAllBuffers", "GetTotalLength", "CopyToBuffer"];
        foreach (var field in typeof(MediaFoundationSampleSlots).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic))
            Assert.AreEqual(33 + Array.IndexOf(methods, field.Name), (int)field.GetRawConstantValue()!, field.Name);
    }

    [TestMethod]
    public void WhenSystemSampleDurationIsAssignedThenProductionGetterPreservesIt()
    {
        nint sample = 0;
        Assert.AreEqual(0, MFStartup(0x20070, 0));
        try
        {
            Assert.AreEqual(0, MFCreateSample(&sample));
            var contract = (ISdkSample)Marshal.GetObjectForIUnknown(sample);
            try
            {
                Assert.AreEqual(0, contract.SetSampleFlags(7));
                Assert.AreEqual(0, contract.SetSampleTime(123456));
                Assert.AreEqual(0, contract.SetSampleDuration(987654));
                long duration = -1;
                Assert.AreEqual(0, ((delegate* unmanaged[Stdcall]<nint, long*, int>)MediaFoundationSample.Table(sample)[MediaFoundationSampleSlots.GetSampleDuration])(sample, &duration));
                Assert.AreEqual(987654L, duration);
                Assert.AreEqual(0, contract.GetSampleDuration(out long after));
                Assert.AreEqual(987654L, after);
                Assert.AreEqual(0, contract.GetSampleTime(out long time));
                Assert.AreEqual(123456L, time);
                Assert.AreEqual(0, contract.GetSampleFlags(out uint flags));
                Assert.AreEqual(7u, flags);
            }
            finally { Marshal.ReleaseComObject(contract); }
        }
        finally
        {
            if (sample != 0) MediaFoundationSample.Release(sample);
            Assert.AreEqual(0, MFShutdown());
        }
    }

    // CLR supplies IUnknown. These 30 entries follow IMFAttributes in the installed SDK,
    // independently of the production function-pointer table; unused entries are never called.
    [ComImport, Guid("c40a00f2-b93a-4d80-ae8c-5a1c634f58e4"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface ISdkSample
    {
        void GetItem(); void GetItemType(); void CompareItem(); void Compare();
        void GetUINT32(); void GetUINT64(); void GetDouble(); void GetGUID();
        void GetStringLength(); void GetString(); void GetAllocatedString();
        void GetBlobSize(); void GetBlob(); void GetAllocatedBlob(); void GetUnknown();
        void SetItem(); void DeleteItem(); void DeleteAllItems(); void SetUINT32();
        void SetUINT64(); void SetDouble(); void SetGUID(); void SetString();
        void SetBlob(); void SetUnknown(); void LockStore(); void UnlockStore();
        void GetCount(); void GetItemByIndex(); void CopyAllItems();
        [PreserveSig] int GetSampleFlags(out uint flags);
        [PreserveSig] int SetSampleFlags(uint flags);
        [PreserveSig] int GetSampleTime(out long time);
        [PreserveSig] int SetSampleTime(long time);
        [PreserveSig] int GetSampleDuration(out long duration);
        [PreserveSig] int SetSampleDuration(long duration);
    }
    [DllImport("mfplat.dll", ExactSpelling = true)] private static extern int MFStartup(uint version, uint flags);
    [DllImport("mfplat.dll", ExactSpelling = true)] private static extern int MFCreateSample(nint* sample);
    [DllImport("mfplat.dll", ExactSpelling = true)] private static extern int MFShutdown();
}
