# S-20261008-163：组合几何空 LineGeometry 填充操作数

## 交付范围

完整 ImageBrush 遮罩映射、层消费与 Compose 生命周期模块内部生产修复，未关闭完整模块；没有确认外部阻塞。本轮未实现私有 C++ 路径 ABI、笔画或一般曲线布尔组合，不能视作完成交接中的整个模块。

## 原生证据与生产修改

- `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/combinedgeometry.cpp:30-135`：取得当前两个形状，null 作为空形状，调用 CShapeBase::Combine，清理临时形状及 LeaveResource。
- `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/linegeometry.cpp:25-97`：当前端点、动画与矩阵构造线形状。本轮只处理无笔画填充，不将线形状在笔画语义下当空。
- `Core/GeneratedGeometryBounds.cs` 的 Append 已把 LineGeometry 视作无填充区域，而 TryCombinedRectangle 原先拒绝它，导致相对遮罩内容 bounds 解析返回失败。现在仅在组合填充区域提取中将 null 或 LineGeometry 识别为空区域，沿已有精确矩形区域布尔运算处理四种模式，不引入包围框近似。
- `Tests/WpfGfxShape.ComAcceptance/CompositionEffectsTests.ImageMask.cs` 新建真实 generated LineGeometry 命令操作数，覆盖 Union/Intersect/Xor/Exclude；沿既有真实来源写锁失败、层丢弃、解锁重试、相对遮罩、嵌套 Pop 和后续帧恢复联合消费者链验收。

## ABI、错误及所有权

没有新增生产导出、接口或包，没有修改原 WPF 源码、解决方案或创建 Host。使用既有资源命令与生产 Native AOT DLL，不伪造 C++ shape/realizer。未改变 HRESULT 首错、来源引用、目标锁和逆序清理合同。此修改仅适用于无笔画填充区域；没有把 C++ 对象当 IUnknown 或 handle。

## 逐次发布与验收

两次发布均执行：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。
项目 net10.0，配置 Release，RID win-x64，均退出码 0。
产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。

1. 发布 1：先于读取交接及修改执行。初始 271/271、0 失败/跳过、257ms；TRX `0e542b9b-6777-4f67-a5fa-6e8389da2785`；SHA256 `0DE97F7F7F075E2A1B3B54560E6C6B705F3ABC0CF1C1696FC9BB49EAC2FD3267`。完整可见 11 行发布日志独立保存为 `S-20261008-163-publish-01.log`。
2. TDD Red：新增四个组合操作数用例，定向 63 通过/4 失败、72ms；均在相同 Present 断言返回 E_NOTIMPL。测试编译可见 MSTEST0037/MSTEST0044，未修改这些既有警告。
3. 发布 2：生产修复后再次发布成功，确有 Generating native code。可见 CA1416；工具将409681字符输出截断为末尾20000字符，读取可见59行，不能宣称完整日志或无警告，诊断摘要已独立保存于 `S-20261008-163-verification-evidence.md`。
4. 定向 Green：filter `FullyQualifiedName~WhenImageMaskSourceIsLockedThenLayerIsDiscardedAndRetryPreservesMapping`，67/67、0 失败/跳过、68ms，TRX `6ec2f3e1-3cb0-41bd-9803-a31c9ef7f8ad`。
5. 发布 2 全量：275/275、0 失败/跳过、275ms，TRX `637670c4-cf65-4eb1-8683-aaae1a570c3c`，开始2026-10-08T18:14:50.1237732+08:00。测试记录实际 DLL SHA256 `41D2E78397A04FCD5C23C4CF8F3CD1934ACF38835C780C132232BE692D306909`。

TRX 路径 `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`；每次运行覆盖既有文件。初始及定向身份已记录，未独立保存完整历史 TRX；最终全量 TRX 保留在原路径。SHA/TRX 记录对应具体发布，不将历史产物 hash 当新产物。未执行覆盖率、原生边缘差分、PresentationCore E2E 或 Ledger 修复/验证。

## 后续边界

阶段1–3正式关闭，阶段4 Active，阶段5–7 Pending；Ledger RepairRequiredPartial，不能替换原 wpfgfx_cor3.dll。下一轮仍合并生产私有路径真实创建者/共同 fill-stroke 扫描、遮罩与层生命周期、一般组合区域、映射和重入恢复验证，不另立空线操作数或分支补测轮。
