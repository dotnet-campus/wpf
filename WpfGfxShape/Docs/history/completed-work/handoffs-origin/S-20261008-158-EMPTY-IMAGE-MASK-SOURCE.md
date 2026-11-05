# S-20261008-158：ImageBrush 空来源与恢复

## 范围与原生证据

完整ImageBrush遮罩映射、层消费与Compose生命周期模块的内部进展，不是模块关闭，也不授权将后续工作缩成补测小轮。没有确认外部阻塞。

读取 `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/imagebrush.cpp:120-177`：DoesContainContent 对未设置 ImageSource 返回 FALSE/S_OK，后续内容bounds与base tile消费者不执行。读取 BrushRealizer.h:1-134 再次确认 CBrushRealizer 继承 CMILRefCountBase，不可当IUnknown或generated handle调用。

## 修改与生命周期

`Core/GeneratedImageMask.cs`：未设置 Source 时捕获成功、mask为空；既有 CaptureMaskLayer 将其消费为alpha=0的中间层。其他来源类型及未支持能力继续返回失败，不把原生alpha目标修正层的E_NOTIMPL改成成功。

`CompositionEffectsTests.ImageMask.cs`：两项直接加载生产DLL的真实通道用例覆盖绝对/相对单位下清空来源、透明输出、恢复来源与下一帧映射；沿既有源锁失败、层丢弃、重试、scope/Pop链验收。初次相对单位像素预期误写为全不透明，修正为既有0x80800000模型后重新执行，最终两项同断言均取得Present E_NOTIMPL Red。没有新增导出、Host、slnx或修改原WPF源码。

## 发布流水

两次完整命令均为：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`。

1. 修改前恢复发布：成功，随后csproj恢复测试256/256、0失败/跳过、259ms。首次测试误传项目目录，工具报告目标不存在，改为完整csproj后成功；该次不是测试失败。恢复发布诊断、SHA/TRX未独立保存。
2. 修改后发布：成功，退出码0；日志60行，含CA1416诊断。产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。遮罩定向50/50、0失败/跳过、62ms；最终全量258/258、0失败/跳过、264ms。TRX `73348cd1-5b51-46bc-a73c-c7508e4d7ef9`，路径 `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`。该TRX实际加载DLL SHA256：`B768E0A167F7356A59616CE4F8723B0D4EBAF58280CA8D54BB589A6A0D5866DA`。

最终Red定向48通过/2失败、68ms，失败值为0x80004001。早期预期修正前也为48通过/2失败，但一个失败是错误预期，不算同断言Red。测试TRX固定文件被后续测试覆盖，恢复/Red/定向TRX及逐次发布原始日志未独立保留；以上摘要来自当轮工具日志，不声称完整持久化证据链。

## 未完成与续接

非空有限/无限fill/stroke真实IShapeData/IFigureData、CBrushRealizer/CMILBrush、CPlainPen/CContextState ABI消费者仍缺；Begin/End/Abort、嵌套mask/clip/opacity、失败逆序清理与Compose生命周期尚须联合闭合。一般曲线组合/非轴保持操作数、曲线原生边缘差分、动画、tile/DPI/浮点/预滤矩阵及效果回调释放引用仍未完成。未修复或验证Ledger，未执行覆盖率、原生差分或PresentationCore E2E。阶段1至3关闭、阶段4 Active、阶段5至7 Pending，Ledger RepairRequiredPartial，不能替换原DLL。下一轮唯一任务继续完整模块，详见 current-work-item.md。
