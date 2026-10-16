# GenProto wirecompiler 有限验收

## 证据与执行边界

当前运行身份、计数及所有报告/TRX/程序集完整 SHA 只由[唯一状态源](../handoffs/current-stable-baseline.md)维护。已使用专用测试设施执行定向、full、完成后归档；无终端脚本，无生产修改、publish 或 COM full。最终代码后测试通过，之后仅文档/Ledger同步。

归档测试 `WireCompilerArchiveTests` 完整解析旧 report、旧 summary、当前 report、当前 summary 的每个 JSON 叶值，验证输入源文件、native测量报告、隔离managed程序集、测试程序集SHA，复制并核对原始full TRX。旧报告未覆盖，旧定向RunID保留在状态源；不是把旧错误观测改写为正确数据。归档报告含全部 test source SHA、assembly MVID、RunID、原始 Counters 与逐enum缺失/差异。

## 测量建模修复与分层

- **native container**：复用现有 Win32/x64 原头文件真实编译产物及独立 PE 导出解析设施；sizeof/alignof/offsetof来自compiler数据，不是C#推算。当前轮没有重编译native，复用并校验其报告hash。
- **managed**：原模型按文件枚举先取到 Point/Size/Rect 无字段partial声明，使四个packet被错误测为9。测试侧优先Generated字段声明后，PointResource/SizeResource原24对9、RectResource/Viewport3DVisualSetViewport原40对9差异消失。冻结生产decl、required及expected没有为凑size修改。原始9仍在旧报告。
- **wire**：两组native显式padding无managed同名字段：VisualSetGuidelineCollection的UINT16Padding0/1在10/14，GlyphRunCreate的Padding0/1/2/3在18/66/70/74。它们仍是offset差，不能把null改0或宣称全部字段相同。size相同及有效字段offset一致不证明padding字节内容、端到端传输或消费者执行等价。
- **架构**：Win32数据为native真实compiler值；managed来自X64 CLR固定宽度观测，x86 CLR为推导而非执行。ARM64 compiler/runtime外部缺项维持Blocked。DLL/诊断程序集进程驻留，无unload。

## enum真实对照

复用NativeCoreCoverageTests/NativeEnums独立compiler表，逐宽度和成员原值对比；不是从managed值生成native期望。`enumAudit`逐项保存已比较成员数、missing、differences。已找到native键的成员结果一致，不代表所有enum一致。

具体未证native键/alias包含MILCMD、MILWindowLayerType、ShaderRenderMode、RenderingBias、FillRule、GeometryCombineMode、AlignmentX/Y、PenLineCap/Join等；以归档逐项记录为准。没有native值时保留null/Blocked，不将命名规则猜测当authority，不为本模块扩展无限能力。

## 八项native-only的producer / wire / consumer

全部wire声明源为`include/Generated/wgx_commands.h`，全部XML来源是显式`Command UnmanagedOnly=true`；consumer分派源为`core/uce/generated_process_message.inl`。布局观测不证明这些producer真实执行。

| packet | producer源码定位 | consumer/保留边界 |
|---|---|---|
| TransportSyncFlush | connection.cpp SyncFlush及connectioncontext.cpp cmdFlush | 对应MilCmdTransportSyncFlush分派，生产执行未由本测量证明 |
| TransportDestroyResourcesOnChannel | connectioncontext.cpp cmdDestroy | 同名分派；生产执行未证 |
| ChannelCreateResource | htmaster.cpp及composition.cpp | 同名分派；生产执行未证 |
| ChannelDeleteResource | htmaster.cpp | 同名分派；生产执行未证 |
| ChannelDuplicateHandle | htmaster.cpp | 同名分派；生产执行未证 |
| BitmapSource | apifunc.cpp MilResource_SendCommandBitmapSource | 同名分派；生产执行未证 |
| MediaPlayer | apifunc.cpp MilResource_SendCommandMedia | MilCmdMediaPlayer→TYPE_MEDIAPLAYER→CMilSlaveVideo::ProcessUpdate；生产执行未证 |
| VisualCreate | 全src搜索未找到直接MilCmdVisualCreate producer；普通ChannelCreateResource是另一packet，不能顶替 | 同名分派→CMilVisual::ProcessCreate，node.cpp明确RIP及E_UNEXPECTED；来源缺口具体保留Blocked，不编造producer |

MediaPlayer的真实来源已纠正：Resource.xml显式`Target=MediaPlayer Name=Update UnmanagedOnly=true`，CommandTypeName规则为MilCmd + 空Mil域 + MediaPlayer + 空Update后缀。不存在该资源Generate选择；不能回退猜测resource origin。SendCommandMedia构造`MILCMD_MEDIAPLAYER { MilCmdMediaPlayer }`，赋Handle/pMedia/notifyUceDirect，发送sizeof(player)，随后真实native分派调用CMilSlaveVideo::ProcessUpdate。

## 有限验收与父级

独立compiler观测、差异保全及来源审阅子项 **Satisfied**；Ledger已有pending条目已据实更新，并有独立有限验收记录绑定归档。全部enum对照、wire字节等价、八项生产执行和VisualCreate直接producer均不得称Satisfied。两个MILCMD required及父级保持Blocked，stage4Closed=false，既有consumer/factory有限Satisfied不降级。

[外部唯一页](../all-completion-external-prerequisites.md)中的authority历史binding、ARM64、完整runtime、官方player protocol/oracle及可观测HW会话确实阻止总完成；全required/physical、七阶段、七项正式条件、A/B/C及独立E2E不减。父级继续具名正式差集，不重复领取本有限模块，总门Red。
