# AG：真实 Present 序列与销毁通知对照

## AL 最终只读回读结论

**AG 当前异步场景限定通过；不是一般刷新率、VSync、同步或销毁合同验收。** 本节替代原初稿“进行中/尚无计数”。AL只读现成证据并更新文档，主代理仅监督；没有修改代码、构建、发布、运行或使用终端/shell/任何直接间接脚本。本次不是新实跑，也不代签AG的源码修改清单。

先读执行规则、完整替换方案、当前基线、交接及问题状态。实证根为项目内 `artifacts/replacement-present-ag-2026/final-readback/`。

## 最终正式 TRX（AL亲核）

|项目/架构|相对上述根的最终文件|执行/通过/失败|
|---|---|---|
|COM x64|`com-x64/TestResults/ag-final-x64.trx:72`|9/9/0|
|COM x86|`com-x86/TestResults/ag-final-x86.trx:72`|9/9/0|
|Managed x64|`managed-x64/TestResults/ag-managed-x64.trx:70`|6/6/0|
|Managed x86|`managed-x86/TestResults/ag-managed-x86.trx:70`|6/6/0|

四份Counters均error/timeout/aborted/notExecuted=0。这是定向包，不是两项目双架构full。

## 六组实际64字节响应（AL逐份亲读双架构12份comparison）

下表目录均位于 `com-<arch>/Results/present-contract-y/<id>/comparison.json`。每份threshold=0，Original和Candidate在compose阶段各两条相同完整64字节消息；before-close、compose-again、replacement-channel阶段的响应Messages均为空。

|FrameTime First/Second|实际time@16|x64 id|x86 id|
|---|---:|---|---|
|123/123|123|b70106b20108461f99ad7c97ce2f6c88|482162f31ab440cd87ff4d74ed83103f|
|0/0|0|b9d0a7383861497ab0339c03dc809523|e095c00507f848ea9ea2fad64e992009|
|0/123|123|a3c35e03eb934922839738e4230fbc4a|8dcfe536316a40a68276be43e91b577e|
|123/456|123|6f898af3e2c8433a8362d002ef183228|331aab5f280343efb562d87e89c261c6|
|456/789|456|335a1666a19e4ec7a7b550b45b725f0a|e977fb21adf049069d2609aadae747b6|
|456/123|123|b16b2d8691e04ff89f03246d2739b6b7|f96084a3170f41a6a977d43a69e535a3|

实际字节结构：type@0=0x0A，Reserved@4=0，result@8=2，refresh@12=60，time@16为表中64位值，byte24–63全0。三个实际前24字节分别为 `0A00000000000000020000003C0000000000000000000000`、`0A00000000000000020000003C0000007B00000000000000`、`0A00000000000000020000003C000000C801000000000000`；comparison保存双方完整hex，不只比较字段或忽略time。此表仅描述本次观测，不推导通用时间算法。

## 实际身份与Present触发边界

AL抽核x64 `335a1666a19e4ec7a7b550b45b725f0a/{candidate,official}/identity.json`、candidate/samples.json及双方events.json；x86抽核 `e977fb21adf049069d2609aadae747b6/candidate/identity.json`。实际加载.NET10.0.13，指针8/4；候选x64 SHA `2BD7C2B9207ACF1E75182A8EC5C8B13B7B1B84E525045272733B158E8F069D5C`，x86 `255E699B643E0A6F5250F80C28AE7086038E5FDA6D9959F3EEF5AE1F0C40DA1F`；官方x64 SHA `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`，版本10,0,1326,50504。SHA仅作实际产物归属，不是下一步工作前置阻塞；未重算文件hash，未把本包绑定后续AJ/AK源码。

场景是UCE CrossThread channel + real STATIC HWND的异步Present，不是初稿所写SameThread验收。x64抽样双方events中compose均有 `window:compose:0x400:123:456`、present-signals:compose:1，说明不只观察空队列。后续官方compose-again信号1、replacement-channel信号2，候选分别0和1；消息队列响应相同不等于所有窗口事件次数/全序等价，不能隐藏这一观测。注销、在途提交、destroy-pending及replacement-channel均有记录，但替代通道空不能证明旧通道无迟到回调或销毁后安全。

## 仍开放与回读界限

仅限定当前异步场景的响应内容及必要阶段观测通过；refresh字段60不证明一般60Hz刷新、真实硬件VSync/QPC算法、有效target同步、非零SyncModeStatus HRESULT或完整注销/并发销毁合同。无屏幕像素/HW内容成功声明。AL未逐份重读全部角色events/packets/identity，未解码原始bin或重新执行比较器；完整hex来自落盘comparison，samples/events为上述抽核，不能声称全部二进制独立复算。原始compose-0.bin/compose-1.bin在抽核目录存在，无须杜撰补包。

完整替换未完成；最后源码两个正式项目×x86/x64 Release全量尚未跑，不是无需跑。本轮集成稳定后应准备完整适用矩阵，保留失败、超时和未覆盖，不无限累加局部Green。
