# Stage4 A interop pin 与发布 full 身份回读

状态：Stage4 Active；本记录不是整块 A Closed，也不是 B/C 完成。

## 本次生产修改

Core/GeneratedD3DImageResource.cs 的 D3DImagePresent 复用 FrozenBitmapLease/PinInteropSource，不在取得 interop 指针后执行外部 AddRef。using 租约覆盖 Present、availability callback 与 fallback 获取；保留回调后的当前 identity/IsReleased 检查和外层事件完成 finally。不扩展 device future/loss/Reset 模型。最终源码已发布并完成下列验证；未新增测试。

## 已回读的四例及 expected

修改前既有四例 TRX 完整44行分段回读：RunID 74471370-8c6f-49c1-b43a-4afc6f1c1111，total/executed/passed=4，failed/notExecuted=0。

CompositionTests.Stage4LazyWicDiagnostic.cs 的 DataRow 1/2/3/4 分别调用真实 ManagedStream/system decoder 生产消费路径，改变后续 child offset、geometry、brush、删除 image handle。CompositionTests.ActiveRender.cs:320–365、685–715、803–832 实际回读：单次 armed callback=1，提交 HRESULT=0；1/2/3 当前帧 FFFF0000,FFFF0000，1/2 下一帧 FFFF0000,FF00FF00，3 下一帧 FFFF0000,FF0000FF；4 当前及下一帧 FF0000FF,FFFFFFFF。回调次数不是 CopyPixels 次数。

修改前另运行 decoder lifecycle + Stage4LazyWic 组合，实际246/246而不是13；日志18行完整回读，TRX RunID 258ea664-18eb-4776-bde2-84e52bd21fd4，Counters 与 Stage4 输出实际回读。源修改 active FFFF0000,FF00FF00 / next FF0000FF,FFFFFFFF；nested present 8000FFFF。不能将此组合冒称用户指定13专项。

## 本次发布 P1

完整命令：dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64

项目 Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64，退出0。日志59行回读；Generating native code 与最终发布目录成功。日志工具已截断前530844字符，不能称原始完整发布输出无警告；保留的输出含 CA1416。

产物：WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

- 发布后 ordinary full：4212/4212，0失败/0跳过；结果日志50–60行实际回读。
- 发布后 COM full：1351/1351，0失败/0跳过；17行日志完整回读。
- COM TRX：Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx；RunID 135de490-3b02-4243-9151-a32b6e630a95；start 2026-10-13T18:36:51.6517869+08:00 / finish 2026-10-13T18:41:26.0742524+08:00。
- SHA256：AEFDC5008373993541CE818E7423F58A6D14EE163F222F5755763F853232565E，TRX生产DLL加载输出实际回读。
- report：Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-056c64ca2ebf4d11b8467f6a464f572f.json；TRX report路径与JSON AotDllSha256一致，Compared=450，实际回读。Cross-format diagnostic不是float golden。

## 精确未完成边界

本轮没有完成真实 decoder forwarding slot8 的 CopyPixels cachehit/miss/sourceinvalid/realizationfail retry 计数验证，也未确认指定13专项的完整集合；不得拿246或1351全绿替代。A中上述有限必要证据仍开放；不能据此将A全部关闭。

未同步晋升 mapping/ledger/stable/workitem/remaining-gap 状态；旧身份不代表本产物，本归档保存本次确证身份。B shader generation/per-device-cache、HW nonOOM fallback/BadShader/OOM有限合同及C GENPROTO required evidence尚未实现收口。没有启动终端、脚本、自定义程序或沙盒执行器。
