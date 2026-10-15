# S-20261008-166：Compose 调用者引用释放与目标快照清理

## 本轮交付及证据

本轮仅推进完整 ImageBrush/层/Compose 模块的内部生命周期，不关闭模块。新增真实 WIC 解码 IStream 回调场景（普通图像及 ImageBrush 遮罩）：回调释放目标及工厂的调用者引用，销毁通道并断开连接；活动帧依靠既有目标、root、来源和锁引用继续输出原始红/绿像素，退出后来源流恰好释放一次。输出位图仍由测试持有以读取结果，不声称所有外部引用均已释放。

生产 `Core/GeneratedTargetRegistry.cs` 将 Render 快照引用由正序改为逆序释放；首错返回仍经过 finally，引用获取与清理顺序对应。已有目标替换、通道销毁、多目标失败及下一帧恢复一起全量回归。新增场景在旧 DLL 已通过，不是生产 Red，也不证明外部可观察的目标释放顺序。

原生读取：`core/uce/connectioncontext.cpp:448-530` 确认仅 root 通道 Compose 与失败退出；`core/resources/BrushRealizer.h:1-130`、`brush.cpp:30-85`、`BrushRealizer.cpp:30-140` 确认真实 realizer 创建/AddRef、借用接口和非IUnknown继承。没有从这些证据推导未验证的 MSVC 槽位。

## 发布与验证

两次完整命令均为 `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，目标 WpfGfxShape.csproj，Release，win-x64。

1. 开始发布成功，工具报告11行，日志未在测试覆盖前独立读取。基线279/279、0失败/跳过、285ms，TRX `5900a836-3112-4e01-9a27-19a0ac810ebe`，开始2026-10-08T18:42:43.2953442+08:00。本轮未提取该发布的SHA；不得以历史SHA代替。首次测试传目录被拒绝，改为csproj成功。
2. 测试修改首次编译CS1686，转交独立回调引用后修复；发布1定向16/16、44ms，既有MSTEST0037/MSTEST0044可见。随后生产修改并第二次发布成功、退出码0；CA1416可见。409681字符输出仅保留末尾20000字符，读取保存摘要于 `S-20261008-166-validation-evidence.txt`，不是完整原始日志。产物 `WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
3. 发布2全量281/281、0失败/跳过、276ms，TRX `58c89e18-055a-4530-baf1-8324c4e00feb`，开始2026-10-08T18:47:23.3445166+08:00，SHA256 `6AA7D9154744CC25E2133CDF441B96684B9469FACEBCCA4E2F0980CFE45A65B7`。最终完整TRX保留于测试项目TestResults/com-acceptance.trx；基线及定向完整TRX已覆盖，身份/摘要不等同于原始副本。

## 剩余与下一轮

唯一下一动作保持完整生产模块，不再安排此场景补测轮：从真实shape/pen/context和resource/immediate realizer创建者落实MSVC布局及借用/持有，保留全部有限/无限路径参数并同链接通EnsureRealization、空画刷、fill/Widen/stroke与共同扫描；合并层Begin/End/Abort、mask/clip/opacity/Pop及Compose首错逆序清理；合并精确曲线组合、非轴保持操作数、Visual/作用域动画、tile/DPI/stretch/alignment/浮点/预滤和原生差分；联合来源重入、删除/替换/销毁、帧快照、调用者释放、失败恢复及最终释放；逐次归档发布诊断/SHA/TRX和映射/Ledger差集后统一验收。

当前私有路径仍将参数折叠为bool，有画刷路径E_NOTIMPL，真实私有创建者和消费者尚未实现；不是确认的外部阻塞。没有新增导出、Host或修改原WPF/.slnx。阶段1–3关闭、阶段4 Active、阶段5–7 Pending，Ledger RepairRequiredPartial；不能替换原DLL。未运行覆盖率、原生差分、PresentationCore E2E或Ledger修复/验证。
