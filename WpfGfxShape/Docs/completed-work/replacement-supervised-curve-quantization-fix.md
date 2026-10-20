# AT 曲线预量化有界生产修复质量门

## 最终裁决

**质量门未通过；生产与正式测试源码均未修改，未批准预量化修复。** 已读执行规则与完整方案（含90—120行）。本轮没有完成双架构全轨迹及原始像素的独立重算，不能以 AQ 的零差摘要宣布根因闭合。整体替换未完成，新 GPU 圆角仍未覆盖。

## 已亲读的证据及其边界

证据相对于项目根 `WpfGfxShape`：

- x86：`artifacts/curve-aq/readback/x86-final/Results/curve-an/fca4190516f1409e9537a8d5b4637d9f/`。
- x64：`artifacts/curve-aq/readback/x64-final/Results/curve-an/04266b612c1340bf9a5a6244684db709/`。
- x86 `aq-summary.json` 确有两角色 frame5 double=44、frame9 double=10，三种预量化干预均0；frame6—8均0。此处是历史摘要读数，不是本轮重新比较二进制。
- x64 `comparison.json` 为 exact=false：frame1/3/5各44，frame9为10；frame0/2/4/6/7/8为0。frame5首差offset696，官方36/候选40；frame9首差offset2432，官方171/候选179。另亲读 `difference-9.json` 前65行，offset2588为116/128，offset2744为36/48；未声称读完整原始差异文件。
- 双架构 original `aq-9-prequantized-double.json` 已全文读：输入(4,28),(4.04,4),(36,4),(36,28)，记录的16个量化输出一致，schedule为空。其余完整轨迹未全部回读，不能宣称双架构干预全通过。

### 实际身份（两角色 identity.json 均亲读）

官方均 .NET 10.0.13、版本 `10,0,1326,50504`，commit `4099f83977f4d29604a9aeb971fb0a7bc1d5f699`。

| 架构 | 官方 SHA256 | 候选 SHA256 |
|---|---|---|
| X86 | 5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834 | 0FCA4287C3E44CB7A7B0801D297C6D4E85F4AD84402D8B9580D0725E8449741B |
| X64 | AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1 | C8C35CE4872FA659B02F8731F34F1F0492775D30879DEC17E54779F06228BE01 |

x86实际模块位于任务 `c50ee70e49bb45e285374eb6451c371a`，x64位于 `2b68ca94e10e4c54b8c05ad15848cde1`：官方为任务根 `wpfgfx_cor3.dll`，候选为 `Native/wpfgfx_cor3.dll`。均明确 hardwareVerified=false。以上SHA仅归属历史AQ运行，不绑定本轮常规构建。

## 独立性与单变量审查

已全文读取 `CurveCausalityExperiment.cs`，并读 `RoundedRectangleContractTests.cs` 的官方/候选加载、比较与原始cubic构造相关段。

1. Compare 的预期读自 `original/{frame}.pbgra32`；原始 frame9 是 WPF BezierTo 提交的真实曲线。没有发现使用候选算法自产官方预期。
2. 四种干预均由同一测试侧算法先生成折线，再分别提交官方/候选 RenderTargetBitmap。官方干预输出不是官方内部HFD轨迹，三种模式互相也不是三份独立实现证明。
3. double 与 prequantized-double 复用 Floating，后者只预量化控制点；首点也预量化，但当前样本首点本来就在格点。对这组正数、无变换、小曲线，可作为样本级单变量干预。
4. 测试量化为 `floor((v-0.5)*16+0.5)` 后逆映射；未覆盖负数半格、非整数变换顺序、clipping与大控制多边形。整数实验明确限制轴跨度<1024、禁止高阶细分；不能外推全算法。
5. 生产 `SoftwareBezierFlattener` 有共享stroke调用、deviceTransform误差计算、collinear快捷路径；实验Floating没有这些完整行为。因此不能直接向共享Flatten入口加入预量化并宣称pen/clip安全。

## 本轮实际执行

- 新增独有纯 C# 审计程序 `.coding-agent/at-curve-audit/{CurveAudit.csproj,Program.cs}`，只读取原始像素/完整轨迹/identity/TRX并输出JSON，不执行脚本。
- 通过发布专用操作将该审计程序自包含发布到 `artifacts/curve-aq/at-audit`，成功。该程序不是正式验收第三Host，只是ignored离线证据审计器。
- 推送整个 `artifacts/curve-aq` 并启动审计时，**文件传输失败**：客户端退出 `-1073740791`，HTTP传输 SocketException 10054。没有取得程序运行或审计输出，不能算已重算像素。未清理任何他人目录。
- 生产项目 `Code/WpfGfxShape/WpfGfxShape.csproj` 最终现有源码分别常规 Release/win-x86、Release/win-x64 构建成功，退出0、0警告、0错误；两次完整17行日志均已亲读。构建不等于 NativeAOT发布或实际回归。
- 未改变生产，因此未重发正式生产包扰动SHA。未执行新的44/10差、边界曲线、兄弟几何/pen/clip运行；没有新的TRX。递归查找AQ readback的“.trx”只发现同名报告组件DLL，未发现TRX文件，不能拿组件存在替代测试结果。

## 精确下一步

1. 由纯C#沙箱打包程序只复制两架构 `Results/curve-an` 的原始证据及必要运行结果到独有小目录，避免推送多份完整runtime；然后运行已编译审计器，回读完整audit.json及全部40组每架构差分、实际identity与文件SHA。当前程序相对路径约定为审计器目录的兄弟 `readback`。
2. 补查官方扫描转换入口的变换→28.4控制点舍入→HFD→输出量化顺序，明确负数半格规则与合法范围，不凭样本推定。
3. 使用固定seed有界加入负/非整数控制点、半格两侧、非整数平移/缩放的独立官方cubic；同一Floating仅切换预量化，保存未量化输出与量化输出、步长和原始像素。发现新差异保留，不调参数或阈值。
4. 达到上述证据门后才在fill设备空间路径最小修改，避免无证据影响共享stroke；补几何/pen/clip回归。最后源码双架构Release构建，必要发布自包含正式包，实际运行44/10、边界和兄弟回归并回读TRX/identity/SHA/原始像素。

本轮交付为有界审查及明确阻塞，不是修复完成；未改资源bitmap、通知、图像、捕获、入口、项目、golden、skip或阈值。
