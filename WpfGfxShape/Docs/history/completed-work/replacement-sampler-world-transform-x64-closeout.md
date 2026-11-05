# sampler-world-transform-x64-r1 独立复核与有限闭环

## 当前裁决
x64同一clean的official/candidate同义sampler业务均已认证Passed；不是full通过。旧EvidenceBlocked“无artifacts/原件缺失/official仅用户认证”是前代理错误结论，已由精确路径直接读取与实际执行纠正。旧各文档段落保留为历史，不称用户认证。旧r7不是本次修复源码。

## 六状态（实际回读后更新）
|状态|结论与边界|
|---|---|
|SourceVerified|Passed：Tests/WpfGfxShape.ComAcceptance/D3DImageTests.VisualCache.cs 280-306真实创建TYPE_VISUAL39 contentRoot与TYPE_MATRIXTRANSFORM66 worldTransform，60字节0x77矩阵，DPI/96双精度对角，零初始化animation，0x1c绑定、0x26插child、SetRoot contentRoot；501-543独立release/提交/destroy/disconnect/deinitialize与cleanup断言。未重造修正。|
|PackageVerified|Passed：既有artifacts/sampler-world-transform-x64-r1/clean/aot.sha256直接Read为632FC2436E1C27C345061B73EF5352205E5B9F1C90A0C1D04ECB538084CB3671；package-files.txt直接Read成功，首项入口/测试DLL。实际candidate加载632F，testSHA08AE0DB6F7045FD6CFCF1CFF04D2EAAB415B35D74731CB897D55FEAD482FE56F与official相同。未重建、未逐文件重新hash整包。|
|OfficialVerified|Passed（复核原运行，非新增运行）：task f2fbf9a333134ceebe9cd87408000dae，actual AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1，test08AE，MVID dfd0392d-f53e-4ae9-b3a0-555c3d08f1f7；outer及isolated TRX各1/1/1；监督exit0无kill/timeout/drain错误，parent-role-exit drained=true。stages root12/transform13/child3/target11，M11/M22=1 size60 animation0；各deleted1/S_OK，cleanup complete空错误。原C0000005在此单例未复现，不普遍外推。|
|CandidateVerified|Passed：本轮discovery task8983e60f476749bcb2440cf06cf68c0a同步diag行50真实UID1817ff57-e333-8653-b23b-985784a01e97对应第三DataRow(false,false,0xff00ff00,0x0000ff80)，再--filter-uid单行执行task10ad4e80081047ed85342f436d1451a4；TRX total/executed/passed=1/1/1，fail/error/timeout/skip=0。MTP会话exit0。|
|EvidenceCloseout|Completed（有边界）：official及candidate原始TRX业务输出已读，均前置00563412、初态0000FF80两帧、变更后00FF0080两帧逐项expected=actual。candidate业务通过包含源码强制cleanupFailures为空（各deleted1和HRESULT检查）；没有candidate stages/独立OS监督JSON，不能声称逐项独立日志或OS exit已证，更不证明worker/句柄趋势。|
|Overall|Open：旧x64 1823/1803/20fail与x86 1823/1820/3fail、23红全部保留，不能定向绿相减生成newfull；生产未改，本轮full未跑，memoryGreen=null，backend/HW/resize/loss/音频/资源/worker/final full仍Open。|

## 原件索引（相对嵌套WpfGfxShape目录）
- artifacts/sampler-world-transform-x64-r1/execution-receipt.md：首先按绝对路径直接Read，28行，原文未改。
- artifacts/sampler-world-transform-x64-r1/clean/aot.sha256、clean/package-files.txt：精确直接Read成功。
- artifacts/sampler-world-transform-x64-r1/readback/Results/official-sampler/supervision.json、TestResults/child.trx。
- 同official-sampler/historical-isolation/WhenHistoricalCaptureOfficialSamplerRunsThenExpectedUpdatesRemain/42413be6354b4df5a4490bf6d4f4416f/下的TestResults/child.trx、stages.jsonl、supervision.json、parent-role-exit.json。
- readback/Results/hw-module-f2fbf9a333134ceebe9cd87408000dae-64970dbd9b35421896b46b4d05034d76.json：actual官方身份。
- artifacts/sampler-world-transform-x64-r1/candidate-discovery-readback/Results/candidate-discovery/Diagnostic/log_261029130159372.diag：真实UID发现。
- artifacts/sampler-world-transform-x64-r1/candidate-readback/Results/candidate-sampler/candidate.trx（57行，分段回读）；Diagnostic/log_261029130309323.diag（118行，末行MTP session exit0）。
- artifacts/sampler-world-transform-x64-r1/independent-execution.md：本轮两次实际沙箱原返回和参数独立分层落盘，乱码保留。首次candidate记录替换因未先Read被拒，读取后补记，无业务文件改动。

## 审计差异与预算
根d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/artifacts枚举仅返回log/publish/ref/staging/tmp；这不等于嵌套WpfGfxShape/artifacts不存在，直接Read成功推翻旧缺件结论。本报告路径首次直接Read返回“文件不存在”，本轮新建本报告，不伪称恢复旧报告正文；旧EvidenceBlocked正文保留在各状态文档历史段。

本轮build0/publish0/Native0/offline0/official0/full0/源码改动0；sandbox2（discovery1+candidate1，各外180秒），用户剩余2已用完。没有调用额外COMpublish1或official1，无需恢复重建。历史receipt记载build1/publish1/official sandbox1分开记账，不冒本轮实跑。现有监督器仅支持--filter，故candidate直接--filter-uid加TRX/syncdiag，外180，未改测试/混跑整组。

下一仅另授同修正x86 official sampler及双架构原official/candidate shaderresize；不得重跑本x64 official冒新增证据，也不得将本有限闭环变成full/生产完整验收。
