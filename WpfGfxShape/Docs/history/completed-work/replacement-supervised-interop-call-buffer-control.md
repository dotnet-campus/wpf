# EB：direct Stdcall 与原 stack/local 输出对照

## ED 最终核证与撤回（最新优先）

ED亲读B task333d6746b5764d9b8d99fe0d06134756监督39行及业务39行全文：X86/.NET10.0.13，Exit0，无timeout/cancel/kill，OutputCollected=true；三次Create Hr0、12×3、IUnknown身份、AddRef2/Release1、GC后dirty、Detach、bitmap refs0，最终surface/device/factory refs0、callback root释放、completed。Available Count0，NativeInvocationCovered=false、ConsumerCovered=false，不称通知或HW消费通过。nativeSHA 20F6E494907C1FAF6AF78EAE19E9BAB6F45E5F6FE11DFCC73001F8FD1CAD811A；testSHA 9434326B92C62170D657D3729769F7CFDC67A1A6A5C9033A300279D74714ACB1；MVID 2b643b2b-db73-40f1-a66b-8b3aab52f28f。未取得TRX，不造通过计数。

全direct+stack成功；EC恢复generated device QI组后失败，两处QI未单拆。不能证明全部生成方法有问题或前序无损坏。EA/EB全部行为分支及专属辅助已由ED精确撤除，原生成调用与Ex/HRESULT/null/refs语义保留，DT/DX只读trace默认关闭。默认Release方案构建Exit0、1482警告/0错误，无新增分析器压制；有效实验关闭分析器的限制仍保留，不能追溯认证无压制。限定两处device QI独立Stdcall修复，双架构DR及生命周期回归后才能保留。详见[ED](replacement-supervised-interop-isolation-closeout.md)。以下为历史记录。


## 执行状态
已接管；只用专用工具与 Windows 沙箱正式纯 C# exe，不使用终端或脚本，不委派。不改 DR/DS、NativeRun、ExitDiagnostics、打包、共享交接及其它禁止区域。专属产物 artifacts/eb-call-buffer/。

## 已核原件
EA A2 task 43e1e4da2ac546399fdd321a2aceeaa0：supervision.json 39 行全文、dr-candidate.jsonl 6 行全文、ea-buffers.jsonl 4 行全文已回读。X86/.NET10.0.13，Exit C0000005，无 timeout/cancel/kill，输出已回收。native SHA 09821BE4FB91AEB91B22F0A77EFAB69D0172A3368858B0657ABB6EA94909D3BB；test SHA 3626403856A7C794ED16A17AFD0DC9CA53FFF834EC20CAEDC66153712DDAB284，MVID a98d8f0a-8830-494a-bf2c-ce1426b5e9da。已达四个 heap 输出块，两侧 A5 canary 未变；不证明不存在其它破坏。只有第一轮 Create before，无 Create after 或成功生命周期。

DZ all2 task 3e0678a3994e4a67af6a8af796cacea1：监督全文确认 Exit0、无 timeout/cancel/kill、输出回收；业务末段明确 refs0、Available0，不称回调验证。caps task fc6b8c311c7442e2b37d06f5724a636f 成功是 generated Caps + 其余 direct + heap，不是单因素约定证明。父 TRX 尚未取得，不冒称已读。

## B 合同
完整 Create 链使用 direct Stdcall，但各输出恢复原始 local 地址，不使用 EA heap relocation。保留 HRESULT/null/ownership/Release 和 GetUpdateMethod/Ex 分支。成功后至多再作一次生成 QI vs 其它调用的有信息二分。本轮不保留默认行为修复，不声明 full 或硬件恢复。
