# EC — interop binding isolation

## 范围与执行状态

本轮仅执行 EB 已准备的 generated device QI + 其余 direct/原 stack 第二分组；不更改参数、refs 或 Ex 检测，不扩展二分。随后保存实验源快照，精确撤回 EA/EB 实验分支，验证默认项目构建，并补齐 DZ/EA/EB/EC 报告。

上述初始状态已由ED独立核证补正：有效实验task6e9885f1d1f8489a99b6ead5dc6446b5监督39行、DT trace8行与DR业务6行已全文亲读。X86/4/.NET10.0.13，Exit C0000005，无timeout/cancel/kill，OutputCollected=true。surface QI/GetDesc/GetDevice均after=0；最后CheckDevice.generated.QI.before，无after，无完整Create或成功生命周期。nativeSHA DD1B69F8AA641B6923BEAF5A6184831AD76813D15125F58A0771F0FE968FDF13；testSHA 684C186C721017E7E451DB23A2E85760008069E4A8888522AB4FEA04FCFE757E；MVID 03a25ec6-2257-4f77-a8dc-0eb586bd7e53。

漏启DR的task49ba12de774a4e46b0a02f8d6c14f4ac监督Exit8与stdout零测试已亲读，应从因果对照排除；不是产品Create失败。有效实验关闭分析器是明确限制，ED默认构建不能追溯为该实验无压制认证。

B task333d6746b5764d9b8d99fe0d06134756监督及关键业务全文已由ED独立亲核：全direct+原stack完成三次Create和refs0；恢复generated device QI组失败，组内CheckDevice/GetUpdateMethod两处未单拆。最小原Create仍是候选x86问题，不能定整个Silk库bug、首次破坏指令或唯一调用根因。未取得TRX，不造计数，成功refs0不迁移失败组。

实验完整snapshot已在artifacts/ec-binding-isolation/snapshot；ED先核Interop快照可读，再逐处撤除EA/EB行为分支与专属辅助，不用快照覆盖生产整文件。现已恢复原生成调用和全部Ex/HRESULT/null/refs语义，DT/DX默认关闭只读trace保留，其它生产/测试文件未改。默认Release方案构建Exit0、1482警告/0错误，无新增分析器压制；没有新发布、实验或full。Available0/native callback/HW消费/full未验收。下一门限定两处device QI独立Stdcall修复并经双架构DR及生命周期回归才能保留；当前只是安全撤回与审查，不是修复。详见[ED](replacement-supervised-interop-isolation-closeout.md)。
