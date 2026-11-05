# DZ：x86 Create 链逐 slot 认证与分组对照

## ED 最终安全收口（最新优先）

DZ历史all2/caps成功仅属其各自产物，不定单因素因果。后续EA heap-only仍失败；EB全direct+原stack成功，EC恢复generated device QI组失败，组内两处未单拆。ED亲核范围及完整身份见[ED报告](replacement-supervised-interop-isolation-closeout.md)，不把历史报告转述称为本轮原件亲核。

ED已精确撤除Interop中的EA_BUFFER_ISOLATION、EB_INTEROP_DIRECT_STACK、EB_GENERATED_DEVICE_QI及专属OutputBuffer/EbRelease；未覆盖生产整文件，DT/DX默认关闭只读trace、原生成调用、Ex/HRESULT/null/refs语义保留，其它生产及测试文件未改。默认Release方案构建Exit0、1482警告/0错误，无新增分析器压制，不是运行验收。后续仅限定两处device QI独立Stdcall修复并经双架构DR及生命周期回归才可保留；当前无生产修复，Available0/native callback/HW消费/full未验收。以下历史保留。


## 状态

EA接管补正：原初稿的“尚未构建、发布或执行”已过期。DZ完整Stdcall+原生堆输出缓冲canary组 task 3e0678a3994e4a67af6a8af796cacea1 已完成三次Create/引用/释放并Exit0（EB已亲读监督39行及业务尾段27–39行，确认Exit0、无timeout/cancel/kill、OutputCollected=true及最终refs0；Available0未覆盖回调）；保持缓冲并恢复generated GetDeviceCaps组 task fc6b8c311c7442e2b37d06f5724a636f 也Exit0。后者监督39行、业务39行已由EA全文亲读，三次Create/QI/AddRef/GC/dirty/Detach/Release及最终bitmap/surface/device/factory refs0明确。stdout为1执行1通过，但回收目录缺child.trx，不能冒称父TRX已亲读。actual nativeSHA 911B527DB27EF95C706C8D8499753A20CB4DCC1A9F5878CD3207D6FD98A4830A，testSHA 4472138F527B8196E7AE3F3B46BEBB0352FA8B5FDE9022F6909E75CB1A055641，MVID b755ada9-df88-412d-82ad-ec672821df3f，.NET10.0.13/X86。未证调用约定与缓冲位置各自因果，不以双因素成功定根因。DZ行为分支已撤且final默认包已发布（接管记录）；EA将先运行该包验证当前控制。持续结果见 replacement-supervised-interop-buffer-isolation.md。

## 执行边界

只用专用文件、构建、发布、测试工具及 Windows 沙箱纯 C# exe；不使用终端、脚本、原生探针或第三工程。不改 DR/DS、共享交接、打包或 CF/CX/CY。默认行为保持不变。先核官方合同和实际生成 API，再决定显式 DZ_INTEROP_STD_COM 诊断组；未验收不能保留行为修复。

## 读取记录

已读 current-execution-rules（27行）、Docs/README（35行）；方案结构为一个生产工程及两个测试工程。DY首次读取响应截断，须补读，不视为全文已读。
