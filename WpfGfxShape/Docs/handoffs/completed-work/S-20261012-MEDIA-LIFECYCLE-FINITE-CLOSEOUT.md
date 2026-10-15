# Media 生命周期有限范围收口与阶段4裁定

## 实际回读的产物身份

- 生产产物：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
- 校准报告：`Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-b2452d7de1c34e01b695616af05623f3.json`。
- 报告 AotDllSha256：`47DB2B69098DCA6BE0629F22FEDC704A98350B368E0274D567CD9F24A360AF1B`。
- TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`。
- 实际 TestRun ID：`4d753480-8dc6-4307-8b44-ee98143eb01d`；2026-10-12 19:50:26 至 19:55:11（+08:00）。
- TRX counters：total/executed 1294，passed 1293，failed 1；aborted、timeout、notExecuted 均0。
- 报告：FLOAT投影57/450；直接Pbgra32 0/180；AotBitmapMismatches 0。不能称全量全绿。
- 用户交接：Media定向48/48，普通4182/4182；原版/候选并发创建两轮均2/2。上述独立运行计数此处与本次已回读的COM全量分开标注，不编造各自RunID。
- 本归档没有执行发布；SHA来自上述报告，不补造历史发布命令或序号。

## 本轮实际复测（无生产代码修改/无新发布）

- 定向MediaExportsTests加PublishedArtifactTests：49/49，0失败/跳过，RunID `d5ce245c-14ae-4249-a6ac-3e6077ce010d`。即媒体48项及产物SHA记录1项。
- 随后COM全量：1293通过/1既有失败/1294，RunID `3dfa7504-ee23-432d-8cc0-2e3154c94b54`，2026-10-12 20:09:42至20:14:16（+08:00）。固定com-acceptance.trx已覆盖为此运行，上节4d753480为覆盖前实际回读的历史身份。
- 新独立报告：`wpf-calibration-5ceda4865cc24c73868a5f814014617a.json`，已回读SHA仍 `47DB2B69098DCA6BE0629F22FEDC704A98350B368E0274D567CD9F24A360AF1B`；FLOAT57/450、direct0/180、AotBitmapMismatches0。
- 普通全量实际重跑4182/4182，0失败/跳过，专用测试退出码0。
- 本轮没有新增shader生产代码、公开shader像素验收或新发布，不能称shader已交付或全阶段4完成。

## 权威定义与有限验收表

权威定义为 `remaining-gap-closure-plan.md` 阶段4：消费副本/布局/镜像可追溯，代表更新具有字节/layout/失败码证据，dispatch/factory/消费者支持阶段5。必须区分此定义与完整原生AV/媒体硬件后端兼容；不得不断增加媒体条件阻止转入shader。

| 授权子项 | 当前证据及判定 | 真实剩余必需项 |
|---|---|---|
| 固定命令、MediaPlayer/VideoDrawing/render-data typed接收、factory、失败引用清理 | 已有生产实现与既有协议回归 | 本授权软件消费者范围无新增项 |
| 公开创建、真实系统provider/sample→composition→Visual像素 | 三色AVI真实来源、替换、通知及公开像素既有验收 | 不再重复增加颜色/像素边界 |
| 同源重绑订阅顺序 | 旧订阅先注销再订阅新源，三色替换及parallel已验收 | 无 |
| target锁首错、sample归还及原像素重试 | 公开0x88982F0D与解锁重试已验收 | 无 |
| caller Release后generated resource存活消费 | 既有公开像素验收 | 无 |
| provider独立所有权及归零 | `MediaExportsTests.Lifetime.cs`公开QI/renderer/identity/Release至0 | 无 |
| callback内close、最后caller Release | `MediaExportsTests.Lifetime.cs`真实回调与剩余引用1验收 | 无；不扩展为所有任意线程排列均已证明 |
| callback内rebind/unsubscribe/channel销毁 | `MediaExportsTests.CallbackRebind.cs`真实回调公开命令路径验收 | 无 |
| 系统版本binding及排除self | `SystemMediaModule.cs`从已加载PresentationCore目录绑定、文件identity排除自身；RuntimeBinding公开版本测试 | 无；本路线依赖已加载系统PresentationCore，不等于可替换原库 |
| callback返回后的模块寿命 | 系统模块明确进程拥有，永不FreeLibrary；provider/sample/异步原生返回帧不受最后player引用影响 | 无；不要求不存在的逐player卸载合同 |
| 创建线程合同 | 已验收限于原生实际创建线程及上述公开回调使用范围 | 任意跨创建线程媒体控制不在本次范围；不得宣称全线程兼容 |
| 偶发E_UNEXPECTED | 用户交接原版/候选并发两轮均2/2，未复现 | 未证明根因；不能标记根因修复。作为观察记录而非无限扩展验收表 |
| 原生HW BeginRender/CD3DDeviceLevel1、完整AV族兼容 | 不在此次generated软件消费者收口范围 | 后续真实原生HW路径单独验收，不冒称已有 |

裁定：上述已授权generated软件媒体消费者及列明生命周期子项已满足，转入shader/effects真实消费者族；不是完整原生AV文件族Complete，不晋升整个阶段4。源17/18严格FLOAT以及D3DImage剩余门禁仍开放。

## Shader是否已闭合

未闭合。阶段2的shader资源加载/设备释放已完成，不等于阶段4的GeneratedPixelShaderResource/GeneratedShaderEffectResource消费者完成。当前两类资源只在 `GeneratedEffectResources.cs` 及factory中存在，公开Visual命令分派未含shader effect接线。已有 `Direct3D9PixelShaderEffect.TryCreate` 为真实D3D9 bytecode入口；必须复用真实compiler/consumer并依据原生ShaderEffect.cpp/PixelShader.cpp核对register、参数、首错及生命周期，补公开像素/失败验收，不能以协议状态测试替代。

机器Ledger仍RepairRequiredPartial；本次仅增加可追溯证据，不修补缺失机器分母或声称整体Complete。
