# MILMedia生产导出与真实系统来源前置（未完成media闭环）

## 实际交付

新增 Abi/MediaExports.cs，近似直译 core/api/exports.cpp:198–419 的21个MILMedia导出。按 include/wgx_render.h:728–836 的IUnknown后三槽开始的21个方法顺序，使用Stdcall、byte bool、long媒体时间；空media返回E_INVALIDARG，非空参数原样传递、HRESULT原样返回，不AddRef/Release借用media，不保存provider，不伪造S_OK。

新增唯一COM验收项目内MediaExportsTests.cs，31个实例。21项空对象/输出不改写合同，10项使用固定参考记录的系统WindowsDesktop 10.0.12原版wpfgfx DLL创建真实factory/EventProxy/WMP IMILMedia，随后用候选DLL公开MILMedia导出消费，和真实COM槽比较bool、uint、long、double输出及HRESULT。descriptor是合法事件回调，不是媒体COM替身。测试直接运行器进程执行，不启动Host/脚本/自定义程序。真实来源测试仅覆盖未Open媒体的查询，不是播放/sample/Visual像素验收。

## 原生合同与未完成边界

core/av/internal.h:33–91：provider RegisterResource/UnregisterResource接受CMilSlaveVideo*；renderer BeginComposition接受CMilSlaveVideo*，BeginRender接受CD3DDeviceLevel1*并返回IWGXBitmapSource。原版CompositionNotifier.cpp:125–202直接调用视频成员。不能把GCHandle/托管地址传给旧provider；不能以系统IWMPPlayer替代IMILMedia或保存provider返回S_OK。

GeneratedMediaPlayerResource.cs QI后的E_NOTIMPL仍存在，本轮未将其隐藏。Core/Av已有托管VideoSlave/VideoComposition/Notifier并非真实provider桥。MILFactoryCreateMediaPlayer的候选实现及真实媒体来源创建、provider注册注销、sample BeginRender/EndRender、VideoDrawing/RenderData/Visual消费者、首错失败重试、回调锁外重入和替换关闭仍未完成。无证据认定不可实现或外部阻塞。本轮只是生产前置，未满足用户要求的完整大模块，不以31项增长计media完成。

## 发布与验证

发布1完整命令：dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -p:NoWarn=CA1416。
目标net10.0 NativeAOT Shared、Release/win-x64；退出码0，13行发布日志。此前专用Release/win-x64 build退出码0。产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

SHA256 D491C68EBE5C74EB4A59A24A62B5CE31D63F0C5C829949C7F23A4740528BA3A4，由本轮全量校准报告实际读取。

专用定向：FullyQualifiedName~MediaExportsTests，31通过/0失败/0跳过，退出码0；TRX RunID b4e23a2f-838e-4cd0-a26a-2e73beec1e30。覆盖前读取TRX第2行；固定TRX随后被全量覆盖，不声称另有定向TRX存档。

全量：1275通过/1失败/0跳过/1276，退出码1；RunID 29fb3fdc-cd54-4afc-b6cc-9e37c71df72c。固定TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。唯一失败WhenTileParametersAreCalibratedThenPixelsMatchOriginalWpfMeasurements；报告wpf-calibration-d7f1faf8960c4ac0bd6a758525947c1f.json：float投影57/450、Pbgra32 0/180、格式判定差57；固定参考SHA98D740A895372A31DAF4EB234F3942E31A8C99A15D278F121DA84C36B585CE58、原版SHA501A1CD67D51BB1663870B12AF0F8C0BA29B3D37B8B509BE896D2AAF32DB214A。新增回归0，不宣称全绿。已有MSTest分析器警告。

测试未先于本轮生产实现，未取得TDD Red；明确记录偏离既有工作流，不补造历史。

## 状态

阶段1–3 Closed、4 Active、5–7 Pending；Ledger RepairRequiredPartial，不可替换原库。shader、source17/18 FLOAT57及D3DImage剩余bounds/loss依赖未关闭。下一动作仍须合并真实media来源、provider、composition、公开Visual消费为完整生产模块，不转回零散D3DImage补测。
