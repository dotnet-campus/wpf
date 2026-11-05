# ED — interop isolation 安全撤回与独立审查

## 1. 源码安全状态

ED直接执行、主代理仅监督。先创建本报告；已读执行规则、完整方案（截断补读）、DZ/EA/EB/EC及Interop。先核 artifacts/ec-binding-isolation/snapshot/InteropDeviceBitmapExports.cs 可读，实验完整snapshot保留，未覆盖生产整文件。

唯一生产修改为 Code/WpfGfxShape/Abi/InteropDeviceBitmapExports.cs：逐处撤除EA_BUFFER_ISOLATION、EB_INTEROP_DIRECT_STACK、EB_GENERATED_DEVICE_QI及专属OutputBuffer/EbRelease，恢复各处原默认生成调用。修改后627行全文分段回读，Abi目录残留搜索无命中。原surface独立Stdcall QI、DT/DX默认关闭只读trace、全部Ex/HRESULT/null/refs及finally/Release语义保留。

未改DR/DS、入口、ExitDiagnostics、VisualCache/historical、捕获、项目打包、CF/CX/CY及其它生产/测试文件。无终端或直接间接脚本，无新Host、发布、实验或full。本轮是安全撤回与审查，不是生产行为修复。

## 2. 默认配置构建

最终源码修改后专用构建WpfGfxShape/WpfGfxShape.slnx，Release，未指定RID/TFM或额外MSBuild属性：Exit0，1482警告、0错误，8.31秒。未设置EnableNETAnalyzers=false、NoWarn或WarningLevel，未改项目/分析器配置。CA1416警告明确存在，不称零警告。本次未发生DR CA1838阻断，未改DR。

工具日志61行已回读；原stdout1213693字符仅保留尾20000字符，不称全部警告逐条核验。独有记录 artifacts/ed-interop-closeout/build-result.md；构建固定产物沿用项目artifacts/bin和obj，不改输出/打包。构建不是双架构NativeAOT运行回归，不能追溯认证有效实验无压制：有效实验关闭分析器仍是明确限制。

## 3. EC有效原件亲核

artifacts/ec-binding-isolation/run-generated-device-qi-x86-actual/Results/ 下ec-generated-device-qi-x86/supervision.json39行、dr-candidate.jsonl6行及dt-interop-create/stages.jsonl8行全文亲读。

有效task6e9885f1d1f8489a99b6ead5dc6446b5，父11064/子5884，X86/4/.NET10.0.13，filtered/30秒；Exit C0000005，无timeout/cancel/kill，OutputCollected=true。nativeSHA DD1B69F8AA641B6923BEAF5A6184831AD76813D15125F58A0771F0FE968FDF13；testSHA 684C186C721017E7E451DB23A2E85760008069E4A8888522AB4FEA04FCFE757E；MVID 03a25ec6-2257-4f77-a8dc-0eb586bd7e53。

surface QI/GetDesc/GetDevice after均0，最后CheckDevice.generated.QI.before，无after；业务止于首轮Create before，无完整Create或成功生命周期。无异常指令/栈/ESP，不推断首次破坏位置。

漏启DR的task49ba12de774a4e46b0a02f8d6c14f4ac监督39行与stdout12行已亲读：Exit8、Zero tests ran，排除因果对照，不算产品失败。

## 4. B成功原件亲核

artifacts/eb-call-buffer/run-b-x86/Results/eb-b-x86/ 下supervision.json39行与dr-candidate.jsonl39行全文亲读。task333d6746b5764d9b8d99fe0d06134756，父7864/子6932，X86/4/.NET10.0.13，Exit0，无timeout/cancel/kill，OutputCollected=true。

nativeSHA 20F6E494907C1FAF6AF78EAE19E9BAB6F45E5F6FE11DFCC73001F8FD1CAD811A；testSHA 9434326B92C62170D657D3729769F7CFDC67A1A6A5C9033A300279D74714ACB1；MVID 2b643b2b-db73-40f1-a66b-8b3aab52f28f。

三次Create Hr0/12×3、IUnknown身份、AddRef2/Release1、GC后dirty、Detach及bitmap refs0；最终surface/device/factory refs0、callback root freed、completed。Available Count0、NativeInvocationCovered=false、ConsumerCovered=false。未取得TRX，不造通过计数；业务迭代不是用例计数，成功refs0不迁移EC失败组。

## 5. 因果与下一门

已证全direct+原stack成功；恢复generated device QI组失败，组内CheckDevice/GetUpdateMethod两处未单拆。EA heap-only失败、DZ all2/caps成功按历史报告保留，不冒称本轮亲核其全部原件。heap迁移不是B成功必要条件；不能定整个Silk库bug、所有生成调用错误或唯一根因。最小原Create仍属候选x86问题，x64旧Event0/E_HANDLE堆损坏不因此关闭。

实际下一门：限定两处device QI独立Stdcall修复，保持IID/this/out及Ex/HRESULT/null/refs合同；双架构DR及生命周期回归后才能保留。ED不做该修复或新实验，不把只读审查称修复。Available0/native callback/HW消费/full未验收，完整替换未完成。

## 6. 文档收口

补正DZ/EA/EB/EC最终撤回状态并更新三共享文档最新节，历史保留。五报告加三共享共八文档最终按小段全文回读至末；截断须补读，不以部分读取冒称全文。
