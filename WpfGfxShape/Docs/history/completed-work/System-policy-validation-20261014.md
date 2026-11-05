# 系统策略及相邻族：已测修复，完整合同仍Open

最终生产SHA `272B8AE53E0A0956937C2983E9D12B1BB1549495403424FCD8D780A7818303DE`。历史4BF5…归档保留，不覆盖历史结果。最后源码修改后已重新production publish、COM包publish、PublicAbi publish并沙箱运行；随后仅更新Docs。

## 本轮修复与实证

- 核对原生 `core/uce/rsapi.cpp:23–25,50–108` 和 `apifunc.cpp:149–175`：实际入口为System32 `dwmapi.dll!DwmGetGraphicsStreamClient`，签名HRESULT WINAPI(UINT, UUID*)。删除错误user32静态导入，使用可选加载/可选export、stdcall函数指针；缺DLL返回0x8007007E，缺export返回0x8007007F，调用后finally释放模块。外部BOOL策略仅Vista且原生S_OK时返回true，其余保持false。现代Windows差分不会执行Vista加载分支，不能声称该分支已运行覆盖。
- `HardwareRenderTargetExports.Target.InitializeBackend` 改为成功后提交PolicyGeneration；失败保持未提交，以便同代重试。软件创建失败统一清理；硬件失败已经逆序释放的字段置空，避免下一轮重复Dispose。这是代码修复与回归已测，不等于已构造政策切换失败的完整生产闭环。
- 最小WpfGfxShape.Tests Release和ComAcceptance Release构建成功。系统5导出及相邻MILLoadResource/GetNextPerfElementId/CopyPixelBuffer已有生产实现，本轮保留新代码，不以旧SHA覆写。
- SystemPolicyTests原版/候选diff覆盖0/1/-1/2策略、跨线程读、update返回、恢复、stream现代OS、ID增量、资源缺失HRESULT及输出保持、位拷贝1094组合/边界；初次2/2通过，最终同SHA全量再次两项通过。独立原版为WindowsDesktop10.0.1，SHA94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F，不替代10.0.12。

## 最终同SHA证据

路径均相对WpfGfxShape；原始证据不改写。

| 证据 | 实读结果 |
|---|---|
| `Investigations/System-policy-family/full-final/Results/aot-identity.json` | Sandbox=true、上述生产SHA |
| `Investigations/System-policy-family/full-final/Results/final-full.trx` | RunID b2e84011-b732-4cdd-90ed-7afd4cece142；1429执行，1397通过/32失败/0跳过，Failed |
| `Investigations/System-policy-family/related-final/TestResults/system-policy-final-related.trx` | 868执行，836通过/32失败/0跳过；组合SystemPolicyTests、D3DImageTests、CompositionTests、GeometryUtilityTests、AcceptancePathsTests，不冒称旧292子集 |
| `Investigations/System-policy-family/strict-final/Results/614f1bcb8a6344ef9964b2c643492f4e/report.json` | 同SHA，Missing11/Extra0，LifecyclePassed=true，PeContractPassed/AllAbiPassed/Passed=false |

最终full通过--results-directory Results把TRX和业务JSON/identity一起限回收Results。初次与related分别只回收Results或TestResults，不能冒称它们各自同时有完整TRX+JSON。沙箱执行入口仍额外回传部分运行依赖，不能声称磁盘证据目录严格仅Results；上传只使用标准小包。未执行终端脚本，未在host修改持久系统设置；未增加stub/NotImpl或调用原版代替候选，未用HW失败转SW的方法隐藏32失败。

## 严格PE剩余11项（实际读取，不由19减8推测）

MilPlayer_Create；MilPlayer_Process；MilUtility_GetTileBrushMapping；SetMilPerfInstrumentationFlags；MilVisualTarget_AttachToHwnd；MilVisualTarget_DetachFromHwnd；MilContent_AttachToHwnd；MilContent_DetachFromHwnd；MIL3DCalcProjected2DBounds；MilGlyphRun_GetGlyphOutline；MilGlyphRun_ReleasePathGeometryData。

## 尚未关闭的精确合同

1. 系统策略5+相邻3仅有限差分已证，不标完整Closed：真实HW→explicit SW→HW呈现与同代失败重试/资源关闭/缓存失效仍缺独立原版对照；RDP与registry软件选择和native策略等价仍须确认，不能推广到全部HW策略。
2. MILLoadResource仅缺失路径差分，尚缺合法IMAGE资源成功路径；GetNextPerfElementId尚缺并发增量合同；Vista optional DLL/export分支缺运行覆盖。
3. 最终32项HW像素失败保留，不能一概归环境；完整ABI、UCE资源、PresentationCore10.0.12 E2E、A/B/C与架构/unload正式门仍Open，无替换资格。本轮未实现剩余11族，不以export存在代替完整方法族生产验证。
4. [唯一外部输入清单](../all-completion-external-prerequisites.md)的历史authority、ARM64工具链+真实机器、合法完整WindowsDesktop10.0.12三类仍未收到；自主未完成项不得转嫁为外部输入。
