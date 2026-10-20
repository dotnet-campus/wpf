namespace NativeAcceptance.PublicAbi;
using System.Runtime.InteropServices;

internal static unsafe partial class Program
{
    [DllImport("ole32", ExactSpelling = true)]
    private static extern int CoCreateInstance(Guid* clsid, nint outer, uint context, Guid* iid, nint* output);

    private static void ColorContextContract()
    {
        Guid clsid = new("cacaf262-9370-4615-a13b-9f5539da4c0a");
        Guid iid = new("ec5ec8a9-c395-4314-9c77-54d7a935ff70");
        nint factory = 0, color = 0;
        try
        {
            Check("WIC.Factory", CoCreateInstance(&clsid, 0, 1, &iid, &factory));
            Check("WIC.CreateColorContext", ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)factory)[15])(factory, &color));
            var type = (delegate* unmanaged[Stdcall]<nint, uint*, int>)Export("IWICColorContext_GetType_Proxy");
            var exif = (delegate* unmanaged[Stdcall]<nint, uint*, int>)Export("IWICColorContext_GetExifColorSpace_Proxy");
            var profile = (delegate* unmanaged[Stdcall]<nint, uint, byte*, uint*, int>)Export("IWICColorContext_GetProfileBytes_Proxy");
            uint value = 0;
            Expect("ColorContext.Type.NullContext", type(0, &value), unchecked((int)0x80070057));
            Expect("ColorContext.Type.NullOutput", type(color, null), unchecked((int)0x80070057));
            Expect("ColorContext.Exif.NullContext", exif(0, &value), unchecked((int)0x80070057));
            Expect("ColorContext.Exif.NullOutput", exif(color, null), unchecked((int)0x80070057));
            Expect("ColorContext.Profile.NullContext", profile(0, 0, null, &value), unchecked((int)0x80070057));
            Expect("ColorContext.Profile.NullOutput", profile(color, 0, null, null), unchecked((int)0x80070057));
            uint direct = 0;
            int expected = ((delegate* unmanaged[Stdcall]<nint, uint*, int>)(*(void***)color)[8])(color, &direct);
            Expect("ColorContext.Exif.Uninitialized", exif(color, &value), expected);
            Check("ColorContext.InitializeExif", ((delegate* unmanaged[Stdcall]<nint, uint, int>)(*(void***)color)[5])(color, 1));
            Check("ColorContext.Type.Exif", type(color, &value));
            ExpectValue("ColorContext.Type.Value", value, 2);
            Check("ColorContext.Exif", exif(color, &value));
            ExpectValue("ColorContext.Exif.Value", value, 1);
            expected = ((delegate* unmanaged[Stdcall]<nint, uint, byte*, uint*, int>)(*(void***)color)[7])(color, 0, null, &direct);
            Expect("ColorContext.Profile.ExifContext", profile(color, 0, null, &value), expected);
            Release(color);
            color = 0;
            Check("WIC.CreateProfileContext", ((delegate* unmanaged[Stdcall]<nint, nint*, int>)(*(void***)factory)[15])(factory, &color));
            byte* input = stackalloc byte[4] { 1, 2, 3, 4 };
            Check("ColorContext.InitializeMemory", ((delegate* unmanaged[Stdcall]<nint, byte*, uint, int>)(*(void***)color)[4])(color, input, 4));
            Check("ColorContext.Profile.Size", profile(color, 0, null, &value));
            ExpectValue("ColorContext.Profile.Size.Value", value, 4);
            byte* output = stackalloc byte[4];
            Check("ColorContext.Profile.Bytes", profile(color, 4, output, &value));
            for (int index = 0; index < 4; index++) ExpectValue("ColorContext.Profile.Byte", output[index], input[index]);
            expected = ((delegate* unmanaged[Stdcall]<nint, uint, byte*, uint*, int>)(*(void***)color)[7])(color, 1, output, &direct);
            Expect("ColorContext.Profile.ShortBuffer", profile(color, 1, output, &value), expected);
            uint refs = ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)color)[1])(color);
            for (int index = 0; index < 4; index++) Check("ColorContext.RepeatedType", type(color, &value));
            uint after = ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)color)[2])(color);
            ExpectValue("ColorContext.NoRetainedReference", after, refs - 1);
        }
        finally
        {
            Release(color);
            Release(factory);
        }
    }

    private static void Expect(string operation, int result, int expected)
    {
        Calls.Add(new { Operation = operation, HResult = result, ExpectedHResult = expected });
        if (result != expected) throw new InvalidOperationException(operation);
    }

    private static void ExpectValue(string operation, uint value, uint expected)
    {
        Calls.Add(new { Operation = operation, Value = value, Expected = expected });
        if (value != expected) throw new InvalidOperationException(operation);
    }
}
