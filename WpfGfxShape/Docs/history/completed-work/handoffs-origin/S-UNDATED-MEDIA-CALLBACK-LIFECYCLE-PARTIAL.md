# Media callback/lifecycle partial — stage 4 remains Active

## Changes

- Core/Av/CompositionNotifier.cs snapshots registered notifications under lock and invokes frame/invalidation callbacks outside that lock. A UI-frame request raised by a callback remains pending for the next pass.
- Core/Av/VideoSlave.cs rolls back failed provider registration, preserves the first HRESULT, and releases the temporary provider reference so the next update can retry. Final release detaches the provider before calling unregister and releases its reference in finally.
- Tests/WpfGfxShape.Tests/AvCompositionNotifierTests.cs adds three callback/reentry/request-preservation tests. These are managed notification tests, not media provider or actual-video acceptance evidence.

## Validation

- Targeted AvCompositionNotifierTests: 3 passed, 0 failed, 0 skipped; 18 ms.
- Full WpfGfxShape.Tests run: 3352 passed, 9 failed, 3361 reported before abort. Test host crashed in Direct3D9Device.SetNativeRenderState via Direct3D9DeviceInterfaceQueryFailureTests.WhenBaseDeviceQueryForExtendedInterfaceFailsWithPointerThenTemporaryReferenceIsReleased.
- Reported failures include eight Direct3D9DeviceManagerTests and GeneratedProtocolTests.WhenCommandIsUnknownThenUnknownPacketIsReturned. No baseline comparison was performed; do not classify these as pre-existing or claim no regression.
- This is not the previous COM acceptance suite (1275/1). No new dedicated publish, SHA, COM RunID or actual-surface output evidence was produced.

## Exact native contract evidence

- core/av/internal.h: IAVSurfaceRenderer slots after IUnknown are BeginComposition, BeginRender, EndRender, EndComposition, GetContentRectF, GetContentRect.
- include/InternalGUIDs.h: IAVSurfaceRenderer = 90830AE8-7B8B-46BC-9854-1B256548A928; IMILSurfaceRendererProvider = E6F1CC74-A0EB-4EBE-8241-089D1CB079D7.
- core/resources/VideoSlave.h: CMilSlaveVideo inherits CMilSlaveResource; NewFrame and InvalidateLastCompositionSampleTime are nonvirtual native methods.
- core/av/CompositionNotifier.cpp invokes these native methods directly on the registered CMilSlaveVideo pointer. A COM callback vtable or GCHandle is not a valid substitute.
- core/av/evrpresenter.cpp BeginComposition snaps a sample through GetCompositionSample; BeginRender takes CD3DDeviceLevel1* (null for software), returning IWGXBitmapSource*.

## Remaining scope

GeneratedMediaPlayerResource.ProcessCommand still returns E_NOTIMPL after provider QI. Production native-resource registration bridge, genuine media creation/migration, actual sample acquisition, brush/Visual drawing, replacement ownership and real-system lifecycle acceptance remain incomplete. No fake provider, forged C++ layout, custom host or success-only completion was introduced. Stage 4 is not Closed. Machine ledger/stable workitem/long-term plan were not advanced by this partial change.
