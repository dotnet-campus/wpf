using System.Globalization;
using System.Reflection;

namespace WpfGfxShape.Core;

internal static class Direct3D9TextPixelShaderResources
{
    private const string ResourceName = "WpfGfxShape.Resources.TextPixelShaders.rc";

    private static readonly Dictionary<string, uint> ResourceIds = new(StringComparer.Ordinal)
    {
        ["g_PixelShader_Text11A_CTSB_P0"] = 100,
        ["g_PixelShader_Text11A_GSSB_P0"] = 101,
        ["g_PixelShader_Text11A_CTTB_P0"] = 102,
        ["g_PixelShader_Text11A_GSTB_P0"] = 103,
        ["g_PixelShader_Text11L_CTSB_P0"] = 104,
        ["g_PixelShader_Text11L_GSSB_P0"] = 105,
        ["g_PixelShader_Text11L_CTTB_P0"] = 106,
        ["g_PixelShader_Text11L_GSTB_P0"] = 107,
        ["g_PixelShader_Text20A_CTSB_P0"] = 108,
        ["g_PixelShader_Text20A_GSSB_P0"] = 109,
        ["g_PixelShader_Text20A_CTTB_P0"] = 110,
        ["g_PixelShader_Text20A_GSTB_P0"] = 111,
        ["g_PixelShader_Text20L_CTSB_P0"] = 112,
        ["g_PixelShader_Text20L_GSSB_P0"] = 113,
        ["g_PixelShader_Text20L_CTTB_P0"] = 114,
        ["g_PixelShader_Text20L_GSTB_P0"] = 115
    };

    private static readonly Lazy<Dictionary<uint, uint[]>> ShaderBytecode = new(LoadShaderBytecode);

    internal static bool TryGetShaderBytecode(uint resourceId, out ReadOnlySpan<uint> bytecode)
    {
        if (ShaderBytecode.Value.TryGetValue(resourceId, out uint[]? shaderBytecode))
        {
            bytecode = shaderBytecode;
            return true;
        }

        bytecode = default;
        return false;
    }

    private static Dictionary<uint, uint[]> LoadShaderBytecode()
    {
        Assembly assembly = typeof(Direct3D9TextPixelShaderResources).Assembly;
        using Stream? stream = assembly.GetManifestResourceStream(ResourceName);
        if (stream is null)
        {
            return new Dictionary<uint, uint[]>();
        }

        using StreamReader reader = new(stream);
        Dictionary<uint, uint[]> shaders = [];
        uint? currentResourceId = null;
        List<uint>? currentBytecode = null;

        while (reader.ReadLine() is { } line)
        {
            string trimmedLine = line.Trim();
            if (currentResourceId is null)
            {
                int nameEnd = trimmedLine.IndexOf(' ');
                string resourceName = nameEnd > 0 ? trimmedLine[..nameEnd] : trimmedLine;
                if (ResourceIds.TryGetValue(resourceName, out uint resourceId))
                {
                    currentResourceId = resourceId;
                }

                continue;
            }

            if (trimmedLine == "{")
            {
                currentBytecode = [];
                continue;
            }

            if (trimmedLine == "};")
            {
                if (currentBytecode is not null && currentBytecode.Count > 0)
                {
                    shaders.Add(currentResourceId.Value, currentBytecode.ToArray());
                }

                currentResourceId = null;
                currentBytecode = null;
                continue;
            }

            if (currentBytecode is null)
            {
                continue;
            }

            foreach (string token in trimmedLine.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                ReadOnlySpan<char> value = token.AsSpan();
                if (value.EndsWith('L'))
                {
                    value = value[..^1];
                }

                if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                    && uint.TryParse(value[2..], NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out uint instruction))
                {
                    currentBytecode.Add(instruction);
                }
            }
        }

        return shaders;
    }
}
