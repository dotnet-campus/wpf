# 严格双进程首个渲染用例：同冻结包重复与边界

## 当前结论（串行任务5）

**run2 留存完整发布快照可用，同一冻结包两次新增独立双进程重复通过现有严格 RTB 门；总体仍为 OverallOpen，不是所有行为等价认证。** 未覆盖或失败的历史门不关闭，run1 不计入同包重复。

实际入口为 `artifacts/strict-render-r28/run2/StrictRenderSupervisorR28.exe`，无参数；从冻结 `Sources/StrictRenderSupervisorR28.csproj`、`Program.cs` 与现有文件核定，不重编该监督器或业务包。监督器顺序启动 `Package/StrictRenderR28.exe`，参数为选定gfx绝对路径、`geometry-public-r27`、输出目录、gfx SHA256。双方业务输入和顺序相同，仅gfx身份、取证路径及进程标识不同。

## 新证据与实测

证据路径相对仓库根 `WpfGfxShape/`：

- [首次重复](../../artifacts/strict-render-r28/task5-repeat/Evidence/comparison.json)：直接执行原run2完整快照。父PID5660，original PID10880、candidate PID8756。
- [带父退出收据的重复](../../artifacts/strict-render-r28/task5-supervised-repeat/Evidence/summary.json)：原快照作为 `Frozen/` 内容复制后执行同一入口；父PID804，original PID12080、candidate PID824。父[真实进程收据](../../artifacts/strict-render-r28/task5-supervised-repeat/Evidence/parent-process.json) exit0、timeout=false、reaped/drained=true；[stdout](../../artifacts/strict-render-r28/task5-supervised-repeat/Evidence/parent-stdout.txt)为 passed=true、differences=0、first=-1、original/translated=0，stderr空。
- [独立回读](../../artifacts/strict-render-r28/task5-supervised-repeat/Evidence/readback.json)审计两次新增重复，包含各子真实process/stdout/stderr、唯一gfx路径/SHA、runtime逐模块哈希、业务契约与历史run2比较。四个子进程均exit0、timeout=false、reaped/drained=true、killError=null，stdout/stderr均空；PID与共同父身份成立。两次parentClean、moduleGate及采样检查均true。
- [运行前manifest](../../artifacts/strict-render-r28/task5-supervised-repeat/Evidence/manifest-before.json)、[运行后manifest](../../artifacts/strict-render-r28/task5-supervised-repeat/Evidence/manifest-after.json)、[首次重复manifest](../../artifacts/strict-render-r28/task5-supervised-repeat/Evidence/manifest-first-repeat.json)：599个非根Evidence文件相同，含监督器、runtime、Package及Sources；第二次运行前后不变。历史run2的402个业务包清单项逐项长度/SHA复核全部匹配，见[检查结果](../../artifacts/strict-render-r28/task5-supervised-repeat/Evidence/historical-manifest-checks.json)。原run2目录与原Evidence未修改。

四份新增raw均4096字节；每轮双方完整逐字节差异0，且分别与历史run2对应raw逐字节相同；SHA256均 `7F55E138C9605464AE90811BD28B5477871BB3E1AA71F5FD3A1C8B052E0C308B`。32×32、stride128、Pbgra32、96×96dpi，caseId `E2E-05-fixed-geometry-v1`。业务事件序列、imageMetadata、caseId、consumer、cleanup、stage、failureStage、exception、exitCode、originalAssertionsPassed、pixelByteCount这11项均与历史run2相同，轮内也相等，threshold=0。cleanup是“Dispatcher shutdown; finalizers drained; native module retained until process exit”的业务收据，结合真实退出，不等于证明每个资源均释放或卸载。

## 冻结身份

| 对象 | SHA256 / MVID |
| --- | --- |
| 业务exe `Package/StrictRenderR28.exe` | `F39CF9ADBFB01835B7D9C5368813EFD509F98C64CD376B3DFCA856D6E08364CA` |
| 业务managed DLL | `F276AA34FBF25A69C2B9B498188961BB1F03E96BA18941FD39803AD29992D3CD`；MVID `11b0489d-8044-4f33-8f86-4b5277a65587` |
| 监督exe | `236A8F28268B72495F7B70B1A65F56DE51251ED583066E13B7D59165BB577891` |
| 监督managed DLL | `54E99B8332670D684670A1CE84AADFFA36D8017E36F2AFA6A59F8812B14339D3`；MVID `221b16fb-d224-48ea-9bd9-9fdf276b0d8d` |
| official `Package/Reference/wpfgfx_cor3.dll` | `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`，1956648字节 |
| candidate `Package/Native/wpfgfx_cor3.dll` | `8EE1C6895313DB7AB9F95B90D45EF1D0249695229ABE227D1F73C725A8D9FC6D`，4895744字节 |

固定self-contained win-x64，Microsoft.NETCore.App / Microsoft.WindowsDesktop.App 10.0.13；runtimeconfig字符串与历史run2相同，各侧采样到的包内模块SHA与相同冻结文件匹配。完整文件哈希见上述manifest。固定输入实现 `Sources/GeometryR27.cs` SHA `5D9C9D098A8CB2CC33B60800FD873FA1A545B07A8DB554AF099C7D5A403F997C`；consumer SHA `D20A2C0C2448A036AB3F7A7A4099F482FCB9EA4172A026CFC8BB4AEB64ACB5BB`。源码清单随每轮Evidence保留，不引入新场景或外部输入。

## 未认证与真实缺口

- 各业务进程采样均只有自己选定的一个gfx（Reference或Native路径与上述SHA），父监督采样零gfx且无PresentationCore；这不是全生命周期连续拦截，不证明没有瞬态加载或所有路径无fallback。未进行同进程双版加载；未改fallback、补偿或阈值。
- 比较器只比较上述11个业务字段及raw。`requests`、`processArchitecture`、`WindowsBase`/`PresentationCore`/`PresentationFramework`描述对象、`backgroundActual`/`redActual`、`pixelsSha256`字段本身没有完整语义逐项比较认证；raw哈希独立重算，模块/harness/MVID/PID/handle另作身份门，不能把身份门当成全部字段行为相等。完整未直接比较字段列表保留于readback的 `unComparedResultFields`。事件为程序记录的有限事件，不是所有WPF事件；PNG编码文件、桌面HWND/resize、错误注入、资源计数与全生命周期均未认证。
- 首次直接重复的父真实exit/分离stderr没有独立进程收据，仅工具返回摘要；不能用第二次收据追认第一次父退出。第二次父/子退出及输出已补齐。审计辅助程序最外层真实exit仍Unknown，不为该未知再造通用监督框架。
- 官方来源为 `C:\Users\lindexi\.nuget\packages\microsoft.windowsdesktop.app.runtime.win-x64\10.0.13\runtimes\win-x64\native\wpfgfx_cor3.dll`。任务4的[SHA收据](../../artifacts/strict-render-r28/task4-audit/Evidence/official-provenance.json)已核发布副本与run2 officialSHA相等；不是宿主缓存原位hash、原始nupkg或签名全链验证。证书提取的Microsoft主体和thumbprint不等于Authenticode链/吊销/NuGet签名信任验证。
- 历史桌面白像素失败、full可信参考缺口、x86等仍开放。

## 历史账（不计入任务5）

- 原run2：父4860，original7652、candidate9900，子exit0、timeout=false、reaped/drained=true；四份子stdout/stderr空，parentGate/moduleGate成立。原run1/run2各4096字节差异0，但run1缺module/runtime证据。
- run1业务DLL SHA `618D9FCC39D8C90E860E888E90EAF4BF15FC59B21998002BE52DD28241A63AD3`、MVID `1a8bb18a-20ba-4087-aec1-447088c8e267`，与run2不同，不能把run1/run2称为同包重复。历史[审计summary](../../artifacts/strict-render-r28/task4-audit/Evidence/summary.json)整体false源于run1缺证，不是run2像素失败。
- 任务4：文档3份、只读审计helper源码1份、publish1成功、sandbox1；生产/业务修改0；曾有SYSLIB0057警告。更早交接另账为helper publish2、sandbox3，非本任务亲自执行。

## 任务5自身账与回读

生产/业务源码及冻结业务/监督包修改0；独立build/test0、业务publish0、辅助审计publish1成功、Windows sandbox2次（每次同一RTB用例两个独立业务子进程）。新辅助文件2份位于 `artifacts/strict-render-r28/task5-auditor-source/`，不是通用外层框架；现有编译审计器固定Input/run1/run2且不能接收本轮目录，因此未将其旧结果冒称本轮审计。辅助程序只复制既有冻结快照，不编译业务代码；发布日志15行完整亲读，exit0，有4条CA2007警告，未隐藏。文档3份更新，不改合同；无终端/脚本，无新矩阵，无继续分派。

亲读本次summary、父process/stdout/stderr、readback的四个子进程身份/输出、四组raw重算及11项业务契约、未比较列表；对全部动态失败值检索，仅见预期timeout=false/runtime配置false。大manifest与runtime长表由审计逐项核算并保存，不声称逐行人工全文审完。文档写后回读。结论限于同冻结包有限RTB门可重复通过。
