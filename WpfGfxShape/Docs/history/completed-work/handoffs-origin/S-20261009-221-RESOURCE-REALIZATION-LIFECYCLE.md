# S-20261009-221：软件资源 realization 与 Compose 重入生命周期

## 原生证据与生产差集

- `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/resources/BrushRealizer.cpp:548-640`：resource Ensure 更新上下文、释放此前 primitive realization、获取当前 brush、设置实现并进行 meta fixup。
- 同文件 `711-726`：FreeRealizationResources 释放 realizer 与资源中不能跨 primitive 的实现。
- 本轮续接工作区已有的 ImageMask.RealizeBrush、session.RealizeBrush 和14项 geometry ImageBrush 重入测试。首次发布后396/396通过，不能将这些遗留修改冒称本轮新增。
- `SoftwareBrushRealization.Draw` 不再绕过资源 realization；接通 Ensure→session.RealizeBrush→共同消费者→finally Free。中间位图已在设备空间实现，消费用 identity/nearest/no-prefilter，保留 coverage、clip、compositing、alpha 上下文。
- Ensure 使用候选所有权转移，失败不保留候选；primitive 完成后立即释放 bitmap immediate。最终释放先清空拥有字段、释放 immediate，再释放捕获的图像源。
- `_realizing` 接通：拒绝同一帧 realization 的重入 Draw，Dispose 先标记失效，消费期间延迟最终释放；Ensure 返回后再次检查失效，不消费已撤销的实现。
- 增加14项 ImageBrush fill/stroke 与图像 opacity mask 联合真实命令测试；覆盖嵌套 present、通道/连接销毁、资源替换/删除、外部目标释放以及第二目标锁失败后的恢复。已有帧快照、Compose首错Abort和逆序帧清理共同保留当前像素与源最终释放。

## 验证与发布流水

两次均实际调用发布：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。

| 发布 | 项目/配置/RID | 结果与诊断 | 对应验证 |
|---|---|---|---|
| 1：开始前 | Code/WpfGfxShape/WpfGfxShape.csproj / Release / win-x64 | 成功，60行日志 | 基线396/396，373ms，0失败/跳过；SHA256和初始TRX id未另行保留 |
| 2：生产接线后 | 同上 | 成功，59行日志，仍有CA1416平台警告，无发布错误 | 定向308/308，199ms；全量410/410，351ms，0失败/跳过 |

产物（两次同一路径）：`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64\wpfgfx_cor3.dll`。

发布2产物由全量测试记录的SHA256：`5FA92A7DEA258F9CA3E81B056219DA211A036E50EEE50FD9A1D1562B6C374C81`。

全量TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`；run id `19116436-4155-4449-acb2-26fe8234ea62`。

首次测试误用 Code 下的测试路径，工具返回目标不存在；定位唯一 Tests 项目后运行基线成功。没有创建测试Host或方案。

新增组合测试在发布1上先运行44项，40通过、4失败；核对夹具：geometry brush和opacity mask同时引用替换后的图像，第二像素应为白色，第二目标第一像素受透明mask约束。修正这两处共享预期后发布1也通过44项，不能报告生产行为Red。测试构建仍有既有MSTest分析器警告。发布2再进行定向与全量验收。

## 完成边界与未关闭项

已关闭 generated 软件 solid/null/ImageBrush 的帧快照→resource realization→共同fill/stroke消费→primitive释放，以及已授权 Compose 重入销毁/替换/失败恢复窄路径。状态保护本身没有对外测试专用入口；不将间接覆盖宣称穷举内部状态。

这不是完整原生CBrushResourceRealizer对象或MSVC布局/QI。generated仍调用与private FillInternalShape共用的消费者，而非完整private DrawPath入口。软件中间位图无需meta target选择，不能据此宣称meta-fixup实现。完整实际创建者/private消费者ABI、meta恢复、非圆dash/marker与完整层栈仍待关闭。阶段4 Active，替换资格不变。

下一轮按 current-work-item.md 完成真实private DrawPath创建者与消费的整块生产闭环，而非再单独补重入测试。
