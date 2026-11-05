# S-20261008-155：路径锁消费者与恢复基线

## 交付边界

本轮是完整 ImageBrush 遮罩映射、层消费与 Compose 模块内部进展，不是模块交付。未降低原完成条件，没有外部环境阻塞；有画刷的非空路径、形状/画刷原生 ABI 和完整生命周期仍未闭合。

## 原生依据与生产差集

- `core/sw/swlib/swsurfrt.cpp:796-901`：空裁剪提前返回；非空裁剪先 LockInternalSurface(Read|Write)，再按 fill、pen+stroke 条件绘制，最后无论失败或成功均 UnlockInternalSurface。
- `core/sw/swlib/swsurfrt.cpp:913-957`：有限/无限路径共用 DrawPathInternal。
- `core/sw/swlib/swsurfrt.cpp:1182-1204`：需要修正的原生软件层拒绝 alpha mask brush 或有 alpha 的目标。不得把 generated opacity 中间层实现直接当作这个内部目标合同。
- `core/geometry/ShapeBase.h`、`core/resources/BrushRealizer.h`：形状与画刷为原生 C++ 方法族，不能假设为三槽 IUnknown 或以 generated resource handle 代替指针。

`Abi/BitmapRenderTargetExports.Drawing.cs/Internal.cs`：两槽共用目标锁消费者。空裁剪仍提前成功；非空裁剪取得读写锁，按有效画刷条件决定是否消费绘制。无 fill 且无 pen+stroke 时成功，不访问未使用形状/画刷；有效画刷仍明确 E_NOTIMPL。锁错误按首错传播，finally 释放目标锁。未新增接口、导出或修改原 WPF 源码。

## 本轮逐次发布流水

目标配置为 Release，RID 为 win-x64；成功产物统一为 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。

1. `dotnet publish WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`：失败，MSB1009 项目文件不存在，无新产物。此命令发生在读取目录之前；之后按实际路径纠正。
2. `dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`：成功。恢复全量222/222、0失败/跳过、256ms；TRX `0c908e6e-41bb-41be-b2da-da96b3111325`；实际DLL SHA256 `1F9998D25B8E4E7340CBB15C00F2E16C825470A6B927404C5DBE3290BC7ED5A7`。
3. `dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`：生产修复后成功，正常分析器设置。发布工具报告完整日志59行，没有独立保存日志。

## TDD 与最终验证

唯一 `Tests/WpfGfxShape.ComAcceptance` 直接加载生产 Native AOT DLL，无 ProjectReference/Host/测试专用导出。

新增 `BitmapRenderTargetTests.PathNoBrush.cs` 八项用例：PBGRA32/PRGBA128Float × 有限/无限路径 × 目标锁冲突/无冲突。发布2的DLL定向运行取得8项 E_NOTIMPL Red；期望分别为成功或0x88982F0D。修复后发布3：内部目标55/55、58ms；最终全量230/230、0失败/跳过、258ms。用例同时核对锁释放后重试和后续写锁取得，防止失败路径遗留锁。

最终TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，ID `167595e3-5f3d-46e8-a203-d95b984d2821`。实际加载发布3 DLL的SHA256：`71D59B9FA9B286493AD61F53DAE8127AB83F1782EF69F634CE24101D856941C3`。恢复/Red/定向TRX被后续运行覆盖，未独立保存，不宣称存在完整逐次TRX归档。未运行覆盖率或原生差分。

## 补记前次未归档结果

续接上下文报告上一会话已将内部位图接入共同 GetInternalClip；六项两种格式的NaN、负无穷和越界且缺 RenderState 裁剪测试取得 E_INVALIDARG Red，修复后内部47/47、全量222/222、249ms，TRX `36ff5c21-5276-4974-8b1a-b7040b5b4ce7`，DLL SHA256 `1F9998D25B8E4E7340CBB15C00F2E16C825470A6B927404C5DBE3290BC7ED5A7`。本轮恢复发布再次核实222项及相同SHA256；旧TRX没有重新读取，历史数字仅来自续接上下文。

前次上下文报告四次发布：首次正常成功，中间正常失败退出码1且显示日志被CA1416截断，禁用分析器诊断失败取得CS0841局部变量冲突，修复后最终正常成功。前次各次完整命令和日志没有在上下文保存，不能补造；SHA/TRX只有最终发布关联。此处明确记录证据缺口，不冒称完成逐次发布归档。

## 剩余风险与下轮合并要求

有画刷路径仍E_NOTIMPL；无画刷成功不证明真实IShapeData/CBrushRealizer/CPlainPen消费完成。需要先沿生产调用者建立形状、画刷、上下文所有权与方法族，再联合嵌套层、失败逆序清理、CombinedGeometry、映射矩阵和真实读取/效果回调验收。alpha软件目标的内部层原生拒绝分支与generated中间层必须区分，不扩展未获原生授权的成功语义。

阶段1至3关闭、阶段4Active、后三阶段Pending；Ledger仍RepairRequiredPartial，当前不能替换原wpfgfx_cor3.dll。不报告整体百分比。
