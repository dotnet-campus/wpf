# S-20260929-073：根通道 Present 选择修正（完整模块未完成）

## 已完成事实

原生证据：`src/Microsoft.DotNet.Wpf/src/WpfGfx/core/uce/apifunc.cpp:129-139`、`core/uce/connectioncontext.cpp:448-525`。原生按通道表遍历，只对 hSourceChannel 为 NULL 的存活条目 Compose，并首错返回。

`Abi/UceChannelExports.cs` 的 Channel 保存 IsRoot，Present 按身份升序选择存活根通道，不再将共享 partition 去重视为根通道规则。没有改动 ABI、Commit、资源计数或显示恢复。当前身份单调分配排序不代表原生可复用句柄表的全部时序；Present 期间重入删除仍需处理。

真实 DLL 测试先增加根通道删除/共享通道保留/输出位图持写锁的场景。修改前定向 2 通过、1 失败：预期 Present 返回 0，实际 -2003292403（目标锁错误）。修复后全量 113 通过、0 失败、0 跳过；输出像素及释放后位图保活断言保留。

## 发布流水

### 发布 1：失败

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape`

目标为目录，未显式指定配置/RID。退出码 1；日志截断前 1257941 个字符，保留部分出现旧 WpfGfxShape.Tests 的 CA1416/MSTEST0044 警告与测试程序集发布路径，没有保留下具体失败错误，不能推断根因。未取得可归属本次的生产 AOT 产物、SHA256 或 TRX。不作为生产发布成功证据。

### 发布 2：修改前成功

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

项目 Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64，退出码 0。产物 `artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。基线全量 112/112；随后新增用例定向 2/3。未提取此次 SHA256，不借用旧轮次哈希。TRX 为 `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，随后已被发布 3 的回归覆盖；Red 诊断见上文。

### 发布 3：修复后成功

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

同一生产项目、Release/win-x64，工具返回成功，共 60 行发布日志；未单独归档完整诊断。产物 `artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。

全量真实 DLL 回归 113 通过/0 失败/0 跳过。TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，记录 SHA256 `C69A33ECE324372075EB873B35EC0F004CE156A39EAE5F2CADA023532A4197EF`。测试项目为 Debug/net10.0，加载的是上述 Release/win-x64 原生 DLL。

## 未完成与技术风险

本轮未交付“通用软件 Visual/DrawImage 绘制与 Compose 生命周期”完整模块，不将本修正解释为允许连续小切片或关闭阶段。

已确认 `Direct3D9SoftwareScanPipeline.cs:41-48` 默认初始化成功但 Run 为空回调；`Direct3D9SoftwareRenderTargetSurface` 当前委托使用 byte[]，专用消费者使用原生锁指针。需要实现真实生产扫描操作及内存/锁适配，不能只改接口名称。此为必须解决的生产依赖，并非外部停工阻塞。重入快照、批次边界、通用采样/裁剪/组透明度及相应像素验收尚未实现。

下一轮仍按 current-work-item.md 完成同一大模块。阶段 4 保持 Active，不能替换 wpfgfx；Ledger 不升级完成状态。
