# NativePaintBaseline 授权前置快照

## 授权与固定合同

本次用户已授权一次实现和执行。权威设计 replacement-native-present-paint-baseline-decision.md 共122行已分段读取；规则 current-execution-rules.md 全文、恢复入口最新节、current-work-item/实施方案最新节已读。无AGENTS命中。仅新增诊断partial、OutputWindow精确HWND默认关闭hook、csproj新非默认配置及必要文档。不改生产、原VisualCache、公共捕获、WindowThread、入口、SDK/TFM/包规则、shader顺序/expected/阈值。

固定A/B独立窗口同owner合法创建调用销毁；32×16、requested format0、count1、DISCARD1、windowed1、HAL1、behavior0x26，FF123456、普通9 Present(NULL,NULL,同HWND,NULL)。A直接，B真实非空更新区WM_PAINT内一次Clear/Present后ValidateRect。各一次提交，总Clear/Present各<=2。不因A像素失败跳过B；基础API/身份/参数/清理异常立即停止。WndProc固定64事件内存，异常保存不跨边界，不I/O/等待/完成泵，防重入、溢出标无效，测试保存强引用直到销毁。提交返回之后同样EVENT query、5秒截止、两次DwmFlush；每次观察GetDC/GetPixel/ReleaseDC紧邻、无屏幕额外读/延长/重发/按色循环。基线目标色不救绿；实际参数/DPI/owner漂移无效。

## 预定义裁决

expected COLORREF00563412，每通道差<=1沿原合同。A错B对仅支持有限事务敏感性；A错B错否定该形式充分性并停止paint/owner/override/format/DPI/capture变体；A对B对仅当前最小能力；A对B错反对修正。基础API/参数/清理无效独立记录。基线未知或已目标色属于证据不足。无frame token/scanout证明，不认证DWM内部因果。任何结果停止，不改原用例；根因未定，完整替换Open而非ExternalBlocked。

## 独立预算和执行输入

正常COM build<=1，直接Configuration=NativePaintBaselineDiagnostic/runtime=win-x86/TFM=net10.0-windows实际编译新分支；失败即停、不修改重试、不抑分析器。成功后COM publish<=1，同配置self-contained，两框架专属FrameworkReference RuntimeFrameworkVersion=10.0.13，仿原合法模式，不启用NATIVE_OBSERVATION_CONTRACT。Native publish0。

权威Native源已读postfailure-source-rebuild-x86-r1/03-com-publish.md：d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/postfailure-source-rebuild-x86-r1/native/wpfgfx_cor3.dll。预期SHA15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620，须由包和运行身份另核，不用旧test SHA/MVID。

独立输出WpfGfxShape/artifacts/native-paint-baseline-x86-r1/{obj,bin,publish,clean}；沿现有PackageSelfContainedComAcceptance，不改包规则。build/publish成功后读clean包依赖/身份/精确新FQN，唯一沙箱1，测试体1，发现0/official额外0/x64/full/原shader0/重试0。正式clean根exe，--supervise-exit，Results/native-paint-baseline-x86独有，内部120秒、外180秒、准确FullyQualifiedName等值WpfGfxShape.ComAcceptance.D3DImageTests.WhenPaintScopedNativePresentIsComparedThenOriginalClientPixelIsRequired。

工具返回立即归档可见参数/返回/截断/原件位置；逐段亲读TRX、监督、全部JSON/stages/identity/stdoutstderr。区分候选文件校验与实际加载；native-only可能仅初始化候选身份，不认证产品运行。

## 实施次序与当前账

1. 快照：完成。
2. 诊断partial、精确hook、新配置：NotStarted。
3. 唯一诊断build：NotStarted，已用0。
4. 成功后唯一publish及clean核验：NotStarted，已用0。
5. 成功后唯一沙箱/体及完整回读：NotStarted，已用0。
6. 独立closeout及恢复入口/current-work-item/plan更新回读：待完成。

所有历史预算独立保留，尤其87d673efe07345f9b0133d2c3287e0f9的build1/publish1/沙箱1/体1不冲销。
