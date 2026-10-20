# BO：present event 合同探索事实保全（BP 补记）

## 状态

BO 未创建报告，由 BP 在进一步修改前补记。仅实验，未验收修复；不能把 E_HANDLE 消失或同步 Commit 成功视为堆损坏修复。

## 已有源码与证据

BO 修改 D3DImageTests.VisualCache.cs 和 D3DImageTests.CaptureHistoricalControls.cs：新增 asynchronousPresent 分支、复制并转交事件、异步 flush 及默认可发现的四数据合同测试。历史 control 改为异步，但未初始化 partition manager。BN/BL 阶段诊断和退出监督先于 BO，必须保留。

产物保留于 artifacts/bo-present-event/：contract-x86、contract-x64、diagnosed-contract-x86、official-x86 以及 x86/x64 发布目录。

亲读 diagnosed-contract-x86/bo-contract/supervision.json 全文38行：task05b570c35c494c0ab7df55fc97c7f7c1，监督9344、子10048，.NET10.0.13 X86，测试SHA E9F516DDE46EB723D76F845DAC4BFD5A4717763F1763F0AAD8BAA8A491DE0E71，MVID8ee5692c-3437-4af9-8eaa-c261ab12deca；子退出-1073741819/0xC0000005，非超时/取消/kill，双流已收集、SupervisorError=null。

BP 随后亲读 diagnosed-contract-x86/bo-contract/stages.jsonl 全文24行，确认官方同步 update-only 三次 Commit S_OK、cleanup 完整，异步最后 connect after/channel before。亲读 contract-x64/bo-contract/supervision.json 全文38行：task2920170b5c4d4f63948cea0c6cd60c0e，监督9960/子2872，测试SHA AF5489379EFA6F007996839E2EDD82E6367C9C1DA1E560085E7C8D1A3C506C5D，退出0xC0000005，非超时/取消/kill，双流已收集、SupervisorError=null。此为失败证据，不是验收。

## 后续

BP 独有产物根 artifacts/bp-safe-closeout/。先核对真实初始化/反初始化与连接/通道所有权，最小有界验证；若未完成双架构内容重复更新、cleanup 与监督正常退出验收，精确撤回 BO 未验收行为及默认合同测试，保留失败证据及 BN/BL 诊断。最终状态见 BP 报告；本文件不宣布修复。
