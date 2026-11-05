# S-UNDATED-068：软件 bitmap render-target ABI 部分实现

## 范围

新增 Abi/BitmapRenderTargetExports.cs：MILCreateFactory、MILFactoryCreateBitmapRenderTarget、MILRenderTargetBitmapGetBitmap、MILRenderTargetBitmapClear，factory/目标 Stdcall COM 表与原子引用计数。复用 SoftwareBitmap 分配真实位图和锁；支持 PBGRA32/PRGBA128Float 目标创建、初始透明、全目标透明清除、返回位图引用。目标释放不影响已返回位图。

这是完整 UCE/CopyForward/Compose 模块的部分源码，不是模块关闭。没有新的外部阻塞。

## 原生依据

- core/api/api_factory.cpp:30-62、251-313：SDK、尺寸、DPI、flags、格式和 HardwareOnly HRESULT。
- include/wgx_render.h:197-323：factory/目标接口槽及签名。
- core/sw/swlib/swsurfrt.cpp:1959-2034、2198-2252：清零位图分配、分辨率、目标接口、GetBitmap AddRef、queued presents 为0。
- core/api/exports.cpp:140-196：目标 GetBitmap 和透明 Clear 导出。

## 已知差集，必须保留为 Partial

- factory 尚未接入 device-manager/display-set 生命周期；UpdateDisplayState 返回 E_NOTIMPL，caps 仅提供无显示集能力回退，不能声称硬件能力查询等价。
- CreateSWRenderTargetForBitmap/CreateMediaPlayer、Begin3D/End3D 返回 E_NOTIMPL。
- 普通 Clear 只接受无 clip 的全透明色，其余明确 E_NOTIMPL；透明导出通过真实 IWIC 锁清零，不是已接通完整软件绘制管线。
- GetBitmap 导出通过目标 GetBitmap 槽及 IWGX → IWIC QI 返回本库已有双接口位图；尚未翻译原生 CWGXWrapperBitmap 的通用外部 IWGX 包装，因此不能声称对任意外部目标等价。
- 尚无 IRenderTargetInternal、render-target manager、DrawingContext、Compose、SameThreadPresent 或双缓冲前缓冲像素消费接线。
- 不能把当前部分 factory 作为完整原生 factory 替代。

## 验证

先新增 BitmapRenderTargetTests：缺 MILCreateFactory 的前置 Red。发布后7项通过，随后补充 IWGX 格式槽和目标 QI 用例。

最终 Release/win-x64 AOT 发布成功；全量101项100通过/1失败/0跳过。新增8项通过；唯一失败仍缺 WgxConnection_SameThreadPresent。存在既有/分析器警告，不报告零警告。

DLL SHA256：225EB8F31417F44D8EEC857142B3D30C04DAB074EE8B55187BBC3D863CD6EABD。
DLL：artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。
TRX：Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。

## 下一轮

保持完整模块验收不变，先消除上述 ABI/绘制差集并复用实际软件管线，再合并目标 manager 注册、Visual/render-data/双缓冲前缓冲消费和真实 Compose，同轮完成多帧脏区、替换、失败和释放验收。不能继续用导出计数或全透明清除测试代替前缓冲像素E2E。
