# DL 官方模块身份选择修复与原 52 项复测

## 状态
已完成本任务有界修复及复测：原52项52 passed/0 failed，加默认双模块回归共53/53 passed，BL Exit0。仅执行同因夹具修复，不修改生产算法、默认打包规则或共享交接。

## 修复前基线（已与DK报告交叉验证）
DK x64 task `7074b35a6fca494bb3a215109943125c`：52 项，40 pass、12 fail，Exit 2。失败为 Br 5 + CK 栅格 7，同名官方/候选模块导致 Single 按 ModuleName 选择抛异常。

## 执行约束
仅专用工具与 Windows 沙箱正式纯 C# exe；不使用终端或脚本，不新增工程。独有产物目录 `artifacts/dl-oracle-identity/`。先读取规则、完整方案及 DK 报告和实际加载实现，再做最小修复与正常依赖图构建、clean 包复测。Native 复用指定 SHA；官方安装目标 .NET/WPF 10.0.1 x64，必须以实际加载模块记录身份，不用期望路径伪装。

## 调查、改动、验证
已读执行规则27行、完整方案120行、DK报告116行及原filter代码。Br/CK仅两处basename Single缺陷；Bm/BW/Bz/Cb/Cd/Cn/Cq/RoundedRectangle按实际resolver句柄定位基址，无需改动。官方来源与WpfReference NativeLibrary resolver均为Visual/PresentationCore所在安装目录。

已实现仅从实际加载模块中按官方安装完整路径Single匹配，不使用First或fallback，保留缺失/重复的精确异常。Br/CK记录实际所选模块路径、基址、SHA，原像素生成与nonempty/threshold0断言不变。默认新增同名双模块回归，断言NativeLibrary.Load两个实际句柄不同、模块同名、选择基址等于官方句柄。诊断运行类仍仅DK_X64_FAMILY显式编译。

正常依赖图Release win-x64 build成功，随后正式framework-dependent publish生成独有clean白名单包；均含既有分析器警告，无生产重发。一次正式沙箱task `95ec6061d59e41b391757a0b9e0a7f7e` 已退出：BL监督Exit0，120秒预算实际约4.79秒，无timeout/kill/drain超时；外预算180秒，父TRX 1/1 pass。原52项族集合逐项沿用DK真实代码，仅追加默认双模块回归。TRX计数、监督全文、实际身份、12份栅格comparison尾段和BW/ABI关键内容均已亲读，详见后文。

实际包24文件9826018字节，比DK增加7168字节，全部来自测试DLL增至1438720字节；新增依赖路径0，Native不变。原24白名单无人为增包，不上传PublishDir/回收根。正式输入 `artifacts/dl-oracle-identity/build/com-acceptance-package`，全任务回收 `artifacts/dl-oracle-identity/readback-x64`。

实际身份：.NET10.0.1 X64；测试SHA B119B0C7A4CBF80B42833E82AC744E489725639F4BD6649775016604C4D5FAE8，MVID81734b43-bec3-4d17-b494-a01001832cc2。双模块回归实际SameNameCount2；官方路径C:/Program Files/dotnet/shared/Microsoft.WindowsDesktop.App/10.0.1/wpfgfx_cor3.dll，基址/实际句柄7FFC1C2D0000，SHA94C06484D001F33EE651F993ACB257DEBA59F06B4D924C06100B3AFF3ECA680F；候选Native/wpfgfx_cor3.dll基址/实际句柄7FFC0C9E0000，SHA F34DEAB06AF47E9D79E5C1CBA06ECE8113C85C7BCF8DC5923A6DA2DB952C20A7。不是哈希期望路径冒充实际加载。

## 最终文件清单与机制
- `Tests/WpfGfxShape.ComAcceptance/BrFillIntegerTests.cs`：仅替换身份定位并增加实际基址字段。
- `Tests/WpfGfxShape.ComAcceptance/CkFillIntegerTests.cs`：兄弟同因修复，内容生成和断言未动。
- `Tests/WpfGfxShape.ComAcceptance/OfficialRasterModuleTests.cs`：默认编译的内部唯一定位函数和最小双模块回归；实际模块完整路径匹配而非构造路径直接返回，回归通过实际NativeLibrary句柄交叉验证。
- `Tests/WpfGfxShape.ComAcceptance/DlOracleIdentityRunTests.cs`：仅DK_X64_FAMILY显式编译的本轮监督测试，原DK类未修改；仅在原11个筛选片段后增加回归片段。
- 本独有报告。未改NativeRun/ExitDiagnostics、capture/VisualCache/historical/WindowQ、生产、项目版本、默认打包、共享交接或并行设置。无新增资源字符串、异常吞噬或fallback，沿用Single精确异常。

## 正式运行与精确证据
任务绝对根 `C:\CodingAgentSandbox\Tasks\95ec6061d59e41b391757a0b9e0a7f7e`。本地以下路径均相对 `artifacts/dl-oracle-identity/readback-x64/`，全任务完整回收，不把回收目录再作输入。

- 父TRX `Results/dl-parent/parent.trx` 1–26行全文：1 executed/1 passed，仅监督测试，不与内层重复累计。
- 原52+1正式TRX `Results/dl-oracle-identity/TestResults/child.trx` 500–503行：53 executed/53 passed/0 failed/0 timeout/0 notExecuted。远端同构绝对路径在上述任务根。
- `Results/dl-oracle-identity/supervision.json` 1–39行全文：filtered，TimeoutSeconds120，Exit0/0x00000000，OutputCollected=true，TimedOut/Cancelled/KillAttempted/OutputDrainTimedOut=false，SupervisorError=null。09:14:41.1830182Z–09:14:45.9723953Z。外180秒；工具未单独给外层进程Exit字段，不伪填。
- `Results/dl-oracle-identity/dual-modules.json` 1–17行全文：同名模块2，官方实际句柄与所选基址一致，实际SHA如前。
- `Results/dk-package-identity.json` 1–126行分段全文：24文件实际大小，测试SHA/MVID、runtime、WPF及官方/候选身份。PresentationCore SHA605FD26A2D1DF0D81B3DE3CB233365DC6091EC7C72C48B22FB805819C8F57654，版本10.0.125.57005；官方文件版本10,0,125,57005，commit fad253f51b461736dfd3cd9c15977bb7493becef。未引入10.0.13 golden或ReferenceData。

## 12项真实栅格复测
12项本轮均已完成实际RenderTargetBitmap像素生成、两份原始pbgra32落盘、逐字节零阈值比较及原有双方非空断言，不再止于身份异常。

| 族/样本 | comparison原件位置（Results下） | 亲读尾段行号 | 结果 |
|---|---|---|---|
| Br0 | br-integer-oracle/0/178c565f84b14890af817b406bcb508b/evidence.json | 87–100 | differences=[]、nonempty=true |
| Br1 | br-integer-oracle/1/08ab853e88dc438d9863b4cbf0af4bc4/evidence.json | 39–52 | 同上 |
| Br2 | br-integer-oracle/2/cf5dcc639ce647648749bf7c9b4dbd80/evidence.json | 87–100 | 同上 |
| Br3 | br-integer-oracle/3/71821b669ada4a3eab7a476c3e84a798/evidence.json | 595–608 | 同上 |
| Br4 | br-integer-oracle/4/0918eecc53a549a69bcdef209b746881/evidence.json | 87–100 | 同上 |
| CK0 | ck-fill-integer/oracle/0/evidence.json | 87–105 | differences=0、双方nonempty=true |
| CK1 | ck-fill-integer/oracle/1/evidence.json | 39–57 | 同上 |
| CK2 | ck-fill-integer/oracle/2/evidence.json | 87–105 | 同上 |
| CK3 | ck-fill-integer/oracle/3/evidence.json | 595–613 | 同上 |
| CK4 | ck-fill-integer/oracle/4/evidence.json | 87–105 | 同上 |
| CK5 | ck-fill-integer/oracle/5/evidence.json | 35–53 | 同上 |
| CK6 | ck-fill-integer/oracle/6/evidence.json | 3439–3457 | 同上 |

上述12份均threshold0，实际官方SHA94C06484…及基址7FFC1C2D0000一致；测试SHA/MVID亦与本轮一致。Br JSON仅一个nonempty字段，但源码保留lines的第二非空断言且TRX通过。原始24份像素文件完整回收，由原比较器执行逐字节比较；未声称用文本工具逐字节另验二进制。本轮无新真像素差；修复前是身份夹具失败，不能把旧12fail说成生产算法缺陷。CK链接生产SoftwareFillBezier但没有生产入口hookup，Br独立整数控制，两者不冒称Native候选消费。

## 原52项分账与关键内容
原52项：BW1、CK10（栅格7+round3）、Br5、Geometry12、IndependentRealizerABI2、IndependentBitmapRealizerABI4、Bitmap短缓冲2/无brush8/空clip6/opaque清除1、DK身份1；本轮全部通过。新增默认双模块回归1，因此53。原52包含身份诊断1，业务分母51；不是53项全为新增业务。

BW `artifacts/bw-curved-fill/precision/comparison.json` 31行全文：threshold0/exact=true；rectangle/ellipse × candidate/official-repeat四项count0、nonempty=true。三个角色identity各14行与supervision各17行全文已读：实际路径=requested，实际SHA/测试SHA/MVID与本轮一致；Exit0、timedOut=false，backend=RenderTargetBitmap software、hardwareVerified=false。六份原始像素完整回收。

ABI `Results/independent-bitmap-realizer-*` 四份各45行全文已读：官方/候选 × translated false/true，各16次返回0，Ensured/Brushes/Effects16、ContextMatches=true，MatrixOffset80/TextureOffset208。未平移像素[0,0,255,255,0,0,255,255]，平移[0,0,0,0,0,0,255,255]，官方/候选相同且非空。`independent-realizer-{False,True}-*.json`两份各34行全文：各16次E_ACCESSDENIED(-2147024891)、Calls16、ContextMatches=true、Adapter0、Display4294967295，随后两绿色像素AP8A/wD/AP8=。不声称寄存器/ESP直接观测。Geometry12原件和bitmap锁/哨兵/像素合同结果完整回收，未宣称逐份大型轮廓全文另审。

## 实际24文件清单
| 相对路径 | 字节 |
|---|---:|
| WpfGfxShape.ComAcceptance.exe | 162816 |
| WpfGfxShape.ComAcceptance.dll | 1438720 |
| WpfGfxShape.ComAcceptance.deps.json | 40043 |
| WpfGfxShape.ComAcceptance.runtimeconfig.json | 567 |
| Microsoft.Testing.Platform.dll | 1137192 |
| Microsoft.Testing.Extensions.TrxReport.dll | 144928 |
| Microsoft.Testing.Extensions.TrxReport.Abstractions.dll | 27168 |
| Microsoft.Testing.Extensions.VSTestBridge.dll | 117280 |
| Microsoft.VisualStudio.TestPlatform.ObjectModel.dll | 335408 |
| Microsoft.TestPlatform.AdapterUtilities.dll | 43048 |
| MSTest.TestAdapter.dll | 55344 |
| MSTestAdapter.PlatformServices.dll | 373800 |
| MSTest.TestFramework.dll | 400416 |
| MSTest.TestFramework.Extensions.dll | 32800 |
| Microsoft.TestPlatform.CoreUtilities.dll | 99360 |
| Microsoft.TestPlatform.PlatformAbstractions.dll | 51744 |
| Microsoft.TestPlatform.Utilities.dll | 67104 |
| Newtonsoft.Json.dll | 712464 |
| Microsoft.Testing.Extensions.MSBuild.dll | 59936 |
| Contracts/wgx_sdk_version.h | 641 |
| Contracts/wpfgfx.def | 3881 |
| Native/wpfgfx_cor3.dll | 4520448 |
| aot.sha256 | 66 |
| package-files.txt | 844 |
合计9826018字节（文件长度和，不是HTTP字节）。新增依赖0。原生直接复用DE正式DLL，没有重发。首次publish后仅补齐本轮条件诊断监督类并重新publish，尚未执行；最终源码对应包仅一次正式沙箱执行，不择绿重跑。

## 剩余与下一步
本轮剩余失败0，身份错误已消除，没有新真像素差需要隐藏或改算法。下一扩族建议按真实输入闭包逐步加入更多BitmapRenderTarget/Geometry方法；需要静态参考的CQ206先由当前10.0.1同版同架构官方正式机制生成独立输入并记录actual身份，不搬旧10.0.13。x86环境/同名模块回归仍需单独适用矩阵；本轮不移绑任何历史full，不宣称full、GPU/device-loss/resize恢复或完整替换。整体完整替换未完成。

报告最终全文按段回读后提交。

