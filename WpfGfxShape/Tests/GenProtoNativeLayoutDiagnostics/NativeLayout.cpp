// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#include <windows.h>
#include <stddef.h>
#include <d3d9.h>
#include <wincodec.h>
#include "../../../src/Microsoft.DotNet.Wpf/src/WpfGfx/include/wincodec_private.h"
#include "../../../src/Microsoft.DotNet.Wpf/src/WpfGfx/include/processed/wgx_core_types.h"
#include "../../../src/Microsoft.DotNet.Wpf/src/WpfGfx/include/processed/wgx_av_types.h"

#include "NativeCore.generated.inc"
#include "NativeEnums.inc"

static_assert(sizeof(AVEventData) == 20);
static_assert(alignof(AVEventData) == 4);
static_assert(offsetof(AVEventData, avEvent) == 0);
static_assert(offsetof(AVEventData, errorHResult) == 4);
static_assert(offsetof(AVEventData, typeLength) == 8);
static_assert(offsetof(AVEventData, paramLength) == 12);
static_assert(offsetof(AVEventData, typeAndParamStrings) == 16);
static_assert(sizeof(HMIL_OBJECT) == 4 && sizeof(HMIL_RESOURCE) == 4 && sizeof(HMIL_CHANNEL) == 4);
static_assert(sizeof(MIL_CHANNEL) == sizeof(void*));

// Exported data supports independent PE measurement without executing another architecture.
extern "C" __declspec(dllexport) const unsigned int Stage4Layout[] = {
    0x5347344c, sizeof(void*), sizeof(AVEventData), alignof(AVEventData),
    offsetof(AVEventData, avEvent), offsetof(AVEventData, errorHResult),
    offsetof(AVEventData, typeLength), offsetof(AVEventData, paramLength),
    offsetof(AVEventData, typeAndParamStrings), sizeof(HMIL_OBJECT), sizeof(HMIL_RESOURCE),
    sizeof(HMIL_CHANNEL), sizeof(MIL_CHANNEL), sizeof(HMIL_PLAYER),
    sizeof(HMIL_CONNECTIONMANAGER), sizeof(HMIL_COMMANDTRANSPORT),
    sizeof(HMIL_CROSSTHREADCOMMANDTRANSPORT), sizeof(HMIL_CONNECTION),
    sizeof(MilColorF), alignof(MilColorF), offsetof(MilColorF, r), offsetof(MilColorF, g),
    offsetof(MilColorF, b), offsetof(MilColorF, a), MIL_SDK_VERSION, DWM_SDK_VERSION
};
extern "C" __declspec(dllexport) unsigned int __cdecl Stage4ReadLayout(unsigned int index)
{
    return index < sizeof(Stage4Layout) / sizeof(Stage4Layout[0]) ? Stage4Layout[index] : 0xffffffff;
}
