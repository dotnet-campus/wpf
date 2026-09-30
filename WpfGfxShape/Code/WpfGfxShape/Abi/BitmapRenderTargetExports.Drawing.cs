using System.Numerics;
using System.Runtime.InteropServices;
using WpfGfxShape.Core;

namespace WpfGfxShape.Abi;

internal static unsafe partial class BitmapRenderTargetExports
{
    // Release native layout prefix only. Never allocate this as a full CContextState.
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeContextPrefix
    {
        internal nint DisplaySet, DisplaySettings, DpiProvider;
        internal Matrix4x4 UnitTransform;
        internal uint PageUnit;
        internal Matrix4x4 World3D, View3D, Projection3D, Viewport3D;
        internal float MeshLeft, MeshTop, MeshRight, MeshBottom;
        internal byte In3D;
        internal uint DepthFunction, CullMode;
        internal NativeAliasedClip Clip;
        internal NativeRenderState* RenderState;
        internal Matrix4x4 WorldToDevice;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRenderState
    {
        internal uint Options;
        internal Matrix4x4 LocalTransform;
        internal int SourceX, SourceY, SourceWidth, SourceHeight;
        internal uint Interpolation;
        internal byte Prefilter;
        internal float PrefilterThreshold;
        internal uint AntiAlias;
        internal MilCompositingMode Compositing;
        internal uint TextRendering, TextHinting;
    }

    private static int DrawInternalBitmap(InternalView* self, nint context, nint bitmap, nint effects)
    {
        if (context == 0 || bitmap == 0) return InvalidArgument;

        var native = (NativeContextPrefix*)context;
        if (native->RenderState == null) return InvalidArgument;
        NativeRenderState state = *native->RenderState;
        Matrix4x4 m = native->WorldToDevice;
        if (native->In3D != 0 || (state.Options & ~1u) != 0 || state.AntiAlias > 1
            || state.Interpolation > 5 || state.Compositing is not (MilCompositingMode.SourceOver or MilCompositingMode.SourceCopy)
            || m.M13 != 0 || m.M14 != 0 || m.M23 != 0 || m.M24 != 0
            || m.M31 != 0 || m.M32 != 0 || m.M33 != 1 || m.M34 != 0 || m.M43 != 0 || m.M44 != 1)
            return NotImplemented;
        if (!float.IsFinite(m.M11) || !float.IsFinite(m.M12) || !float.IsFinite(m.M21)
            || !float.IsFinite(m.M22) || !float.IsFinite(m.M41) || !float.IsFinite(m.M42)) return InvalidArgument;
        if (state.Prefilter != 0 && !float.IsFinite(state.PrefilterThreshold)) return InvalidArgument;
        Direct3D9SurfaceRect? clip = null;
        NativeAliasedClip c = native->Clip;
        if (c.IsNull == 0)
        {
            if (!float.IsFinite(c.Left) || !float.IsFinite(c.Top) || !float.IsFinite(c.Right) || !float.IsFinite(c.Bottom)) return InvalidArgument;
            clip = new(ClipCoordinate(c.Left, int.MaxValue), ClipCoordinate(c.Top, int.MaxValue),
                ClipCoordinate(c.Right, int.MaxValue), ClipCoordinate(c.Bottom, int.MaxValue));
        }
        int effectResult = SoftwareBitmapEffects.Capture(effects, out var effectSnapshot);
        if (effectResult < 0) return effectResult;
        using var ownedEffects = effectSnapshot;
        var drawContext = new SoftwareImageDrawingContext(new(m.M11, m.M22, m.M41, m.M42, m.M12, m.M21),
            clip, state.Interpolation == 0 ? 3u : state.Prefilter != 0 ? 2u : 1u, state.Compositing,
            (state.Options & 1) != 0 ? new MilRectD(state.SourceX, state.SourceY, state.SourceWidth, state.SourceHeight) : null,
            state.PrefilterThreshold, state.AntiAlias != 0, state.Prefilter != 0, effectSnapshot);
        nint target = (nint)self->Owner;
        ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)target)[1])(target);
        ((delegate* unmanaged[Stdcall]<nint, uint>)(*(void***)bitmap)[1])(bitmap);
        try
        {
            uint width, height;
            int result = ((delegate* unmanaged[Stdcall]<nint, uint*, uint*, int>)(*(void***)bitmap)[3])(bitmap, &width, &height);
            if (result < 0) return result;
            result = SoftwareImageRenderSession.Open(target, uint.MaxValue, uint.MaxValue, out var session);
            if (result < 0) return result;
            using (session)
                return session is null ? Direct3D9Factory.UnexpectedHResult
                    : session.Draw(bitmap, new(0, 0, width, height), true, drawContext);
        }
        finally { Direct3D9Factory.Release(bitmap); Direct3D9Factory.Release(target); }
    }
}
