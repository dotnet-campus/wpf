# 当前源码x86 source-rebuild有限收尾

## 最终裁决

本收尾阶段源码/build/test/publish/沙箱/终端脚本/再委派新增全部0，仅读取原件和写文档。证据根R=`WpfGfxShape/artifacts/postfailure-source-rebuild-x86-r1/`，旧根O=`WpfGfxShape/artifacts/postfailure-new-native-x86/`，均相对工作区d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape。

**本次新来源链在限定范围闭合，两个x86有限合同通过，可保留当前管理完成/业务失败分流及注册所有权修正。** 不再把“当前源码发布来源未闭”作为本次子门阻塞。依据为执行前source-before、持久构建发布记录、显式Native→COM publish→正式clean复制链，以及两个新进程的actual/test身份和业务正常清理。允许增量发布，不能声称本轮强制重编ILC/link或全仓库加密冻结。新SHA与旧15A545相同不倒签旧构建输入；本次成功调用不补造历史日志。完整替换Open，整体基线不升。

## 本次执行记录与来源

|独立账|本轮原有执行|可认证结果|
|---|---:|---|
|正常COM Release win-x86 build|1|01-com-build及两段持久日志：Exit0，133warnings/0errors；原stdout104998字符仅末20000可见，前84998缺失|
|生产Native Release win-x86 publish|1|02-native-publish及11行日志：Exit0，新独立native输出；无warning/error汇总，不造0/0，不声称重新运行ILC/link|
|正式COM self-contained publish|1|03-com-publish及两段日志：Exit0；显式本轮native/AotRuntimeIdentifier/clean，两框架10.0.13；stdout52792字符仅末20000可见，前32792缺失|
|新candidate postfailure|1|task7b82e615cdd54a7495c99f889a7bb60b，TRX1/1/1、监督Exit0、正常清理；05完整亲读记录保留，本阶段复核监督身份/退出|
|新candidate isolation|1|task1be8ef6479c04c3a9cf37b6a20b8110e，TRX1/1/1、监督Exit0、正常清理；06完整亲读记录|
|本收尾新增上述执行|0|无重试/发现/full/x64/单元/显示/KN17|

顺序以持久记录为准：normal COM build→Native publish→COM publish→postfailure→isolation，不按旧计划文字倒改顺序。前代理沙箱2，收尾新增0；不能用两份回收证明全盘不存在未归档执行。第二工具原返回已补入R/06-isolation-tool-return.md，明确引用前代理交接并经实际文件核验；原文字无数字exit，Exit0来自监督而非工具完成消息。

source-before记录net10.0、net10.0-windows、global.json声明SDK10.0.107、本地props/targets/Package.targets、三个生产修正关键片段、四单元和两个COM partial合同。它是执行前限定可读快照，非全repo字节冻结；不把声明SDK当实际SDK认证。沿用现有包规则，执行期间源码/项目/测试/资源/SDK/依赖零改。新clean/aot.sha256及actual同15A545，test身份由新真实进程认证，而非静态猜测。原Native/normal build历史原日志与第一门日志缺失、截断边界继续保留于旧审计，不改写成已恢复。

## 新candidate身份与合同

两task actual均requested=actual=各task/Native/wpfgfx_cor3.dll，moduleHandle=actualBaseAddress=1851981824，SHA `15A545E6C5DC3A19BDFB1B4B3178EA3A54B62D49301C983A648FA17CC6E2B620`，version1.0.0.0。测试SHA `1C56E8CB2F0679FFF67EB217708EFD1DAF969200201E4973802D6927963759BD`，MVID `cc7cfbfe-6b94-4400-814c-a299f138970d`，.NET10.0.13/X86/pointer4。初始化候选文件认证不是actual角色认证。

postfailure：沿R/05-postfailure-readback.md已完整亲读记录，A失败88980406之后Create L0/nonzero且不同A，L关闭source前后flush88980406；两真实Destroy/disconnect/deinitialize0，unknownRegistration=false，stillKnown/cleanupErrors空，cleaned=true。

isolation：本阶段实际完整读取402行business、40行监督、26行TRX、12行stdout、空stderr及identity。A2/S3 flush88980406；B4在关闭A/S前后分别真实新alpha0.5/0.25的Send/CloseBatch/Commit/Flush全0；三个Destroy/disconnect/deinitialize均0，created=destroyed=[2,3,4]，completed/cleaned/disconnected/deinitialized=true，primary=null、cleanupErrors=[]。子PID872；UTC08:31:00.0039383至08:31:00.97009；SupervisorPid6560，120秒，exited/Exit0/OutputCollected=true，无timeout/cancel/kill/drain异常。TRX run bc393760-5189-46e5-966d-4cdf13cba15a，testId18b58946-d629-8139-8233-224beea22724，executionId83259109-a21a-48cd-8d5e-3f4dd5ce4d2c；total/executed/passed1/1/1，失败等0。business UID68a17d3734bb4491ae5268dd4af0c0b9不混为testId。详见R/06-isolation-readback.md。

## 旧official具体引用条件与限制

旧四绿独立保留，不计为本轮执行：O/readback/postfailure-official task6edf1e54cb4d4d33acc0ea81ee5b78c6、postfailure-candidate task06db04ddad8f4f149f42368e8449d05f、isolation-official task9e6ff003bce14ec494beb87175f0ec69、isolation-candidate task1d0a27b241834cbd8d1ed759596edb87，各TRX1/1/1且正常清理；完整亲读索引见[独立审计](replacement-postfailure-execution-handoff-independent-audit.md)。本阶段复核两个official监督全文、business actual身份及参数/测试身份结尾。

- 旧official actual SHA `5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834`来自两份原business，不是由新candidate推导；actual是旧task根wpfgfx_cor3.dll，requestedEqualsActual=true、handle=BaseAddress=1854210048，版本10,0,1326,50504/commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。
- 旧official监督/business的test SHA、MVID、.NET10.0.13/X86/pointer4与新两candidate完全对应。同test字节绑定现有两个original/candidate入口与合同；source-before及新旧business参数相同：Async0/manager0、Visual39、Bitmap95、alphaCommand32/16bytes、bitmapCommand12/12bytes/source0；postfailure失败后接入L，isolation A/S/B关系及0.75/0.5/0.25夹具不变。
- R/O clean runtimeconfig全文21行逐项相同：NETCore/WindowsDesktop10.0.13及四configProperties。deps实际比较1–75及935–982：runtimeTarget win-x86、MSTest4.0.1、两runtimepack10.0.13以及官方native资产名称/版本一致；wpfgfx10.0.1326.50504、PresentationNative10.0.726.45202、D3DCompiler10.0.26100.7705、vcruntime14.44.35211.0。两份621行包清单首尾/官方根wpfgfx与Native候选布局对应；wgx_sdk_version.h全文23行同MIL0x200184C0/DWM0xBDDCB2B，wpfgfx.def全文对应依据前代理04-package-preflight完整比较记录。
- **引用限于同测试/合同/架构/runtime包版本与可读根资产布局条件下的历史官方对照。** 只读文本不能重新认证新clean根官方DLL的SHA；deps未全篇/全文件hash比较，未认证所有依赖二进制逐字相同。新两task只加载candidate，没有本轮actual official实测，不把历史5EBD转写为新根实测，也不声称新包所有官方资产字节全同。此限制不否定新candidate两个有限合同及已有同test历史对照，不追求无必要全repo密码学证明；若后续要求新根官方逐字身份则须独立授权，不在本阶段补跑。

## 其它历史账与覆盖不迁移

第一门显式Release build1/隐式Debug build1/定向测试调用1实际4通过，为历史独立账，原日志审阅边界不补造。旧postfailure COM build1/publish1/official1通过/candidate20B3一次失败（unknownRegistration=true/cleaned=false）完整保留。旧isolation双架构成功、direct build4（1败3成）/publish3/official3失败/candidate0及public/WGC/KS/KN/Ledger等账不冲销、不并入本轮。旧20B3/40D0不含本修正。

仅当前源码x86 postfailure与isolation两个有限ABI合同可保留修正；旧KN17 resize证据不转给15A545，HW/device-loss/media/full及真实异常回滚、构造OOM/诊断I/O/COM释放异常、composition线程重入Destroy不新增覆盖。新x64尚未含修正并未验证，显示线继续冻结，完整替换Open。

## 唯一下一门（建议，不执行）

另授权**x64同冻结源码的postfailure与isolation两个有限合同验证**：不追加生产修正，建立独立x64发布/正式包及actual/test身份链，按对应官方条件作对照，逐合同核真实清理。具体执行预算由主代理/用户另授；本报告不授予执行权，不自动full/显示/KN17或扩错误矩阵。
