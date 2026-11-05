# 原 Present 系统轨迹门：编译前停止

## 实际终态

本次已获实施授权；实际停在静态可实施性审查，未形成可安全提交的有限实现。Implementation=Blocked.PreBuildSafetyReview，Build/Publish/Sandbox/Test=NotRun，Readback/Closeout=Completed。不是编译失败、测试 Failed、权限失败或 ETW API 失败。没有创建 session、调整 token、执行 Present 或产生 ETL。

按授权“无法安全有限实现则编译前停”分支结束本门，不启动替代实现、额外 metadata 门、provider/等待/paint/format/capture 变体或静态报告接力。显示内容根因 Unknown；完整替换 Open。

## 实施前范围、预分类及来源快照

本文件在任何诊断源码/配置修改及编译之前落盘；它是本次范围和来源登记，不是运行机 schema 快照。

允许范围原为默认关闭诊断 partial、VisualCache 原 native 前置外围 scope、SwapChainObservation 实际 native Present 前后内存 QPC hook、独有非默认诊断配置及必要资源。用户本次指定配置名称 OriginalPresentSystemTraceDiagnostic，优先于历史设计的 NativePresentSystemTraceDiagnostic；本次二者均未新增。若实施，原方法 FQN、shader、expected、阈值、等待、Present 参数、线程、SDK/TFM、入口、公共捕获及其他诊断均不得改动，禁止同时启用 NativeObservation/paint。

预分类：只可能认证 runtime Start/Stop、精确 HWND kernel 提交及有字段实证的 token/queue 连接；DWM 合成、scanout、实际 UMD 归属不得升级。无事件不能反证没有 Present。关联未实现，所有系统层均为 Unknown，而不是 NotObserved 的运行结论。

已读最新 decision 共104行，长返回截断后补读1–24、27–38、39–74、73–97、98–104；25–26在首次可见返回内。已读当前恢复入口可见前41行、执行规则完整27行、current-work-item前45行、current-stable-baseline前32行、连续性恢复/收尾规则完整66行及计划状态前22行。入口和工作项后续历史未全文重审，不冒完整历史原件审计。

来源为工作区现有文件，不假定等于上游：

- `Tests/WpfGfxShape.ComAcceptance/HpGraphicsTraceTests.Control.cs`：实际读取1–150、151–179、180–275、276–329、330–406及400–426；330起返回在工具字符边界截断，未冒完整426行逐字审计。亲见 x64 显式布局、固定指针偏移、实时 consumer/stopper、TDH 标量读取和 finally 中断言。
- `HpGraphicsTraceTests.cs`：实际读取可见1–59及190–252，未全文审计。
- `D3DImageTests.HrTraceTarget.cs`：完整81行。历史 HQ/HT 运行结论仅引用 decision，不绑定当前新任务。
- `D3DImageTests.SwapChainObservation.cs`：完整47行；原 slot17/121、参数和 result 断言未修改。
- `D3DImageTests.VisualCache.cs`、项目 csproj：仅可见前56/60行，返回截断；未进入编辑，不声称已完整读全部待修改文件。
- 本机 SDK `C:/Program Files (x86)/Windows Kits/10/Include/10.0.26100.0/shared/evntrace.h`：亲读1238–1265 EVENT_TRACE_PROPERTIES、1611–1644 EVENT_TRACE_LOGFILEW、1073–1115 TRACE_LOGFILE_HEADER。首次在 um 目录查找 evntrace.h 返回不存在，随后名称查询找到 shared 正确路径；未以错误路径推出 SDK 不存在。
- SDK evntcons.h 已定位，未完成 EVENT_RECORD 及嵌套类型完整读取/核对；TDH 契约也未完成本次全文核对。没有伪造 x86 SizeOf/OffsetOf 数值或运行机 TDH 元数据。
- 上游 PresentMon/官方网页链接仅为 decision 内已登记参考，本次未重新联网固定 commit，不冒本次抓取快照或运行机 schema。

## 明确范围阻碍与静态审核

1. 旧 collector 是实时 x64 专用：TraceProperties120、LogFile448/callback424、EventRecord112/UserData96，并断言 IntPtr.Size=8。SDK 亲读契约含 HANDLE/LPWSTR、嵌套 EVENT_TRACE/TRACE_LOGFILE_HEADER 和 union；只把指针换为 IntPtr 或改几个数字不构成 x86 认证。本次没有完成自然布局及全部嵌套契约的可靠实现/审核，不能安全进入唯一构建门。
2. 旧 ReadScalarProperty 接受任意1/2/4/8字节并按无符号值报告，只检查部分 flags/count；它不等于本门要求的完整 descriptor/version、运行机 schema、预期类型/长度和截短 payload 边界验证。旧 Verify 仅核部分 descriptor，不能直接作为完整白名单认证。复用此代码而不补齐会把“TDH 调用成功”误称为“目标字段合同已验证”。
3. 旧控制流为 realtime consumer/stopper，finally 内存在 Assert，可能遮蔽原业务异常。不能原样复用为本门文件模式、唯一所有权且名称身份匹配的 STOP、token previous 恢复和无 finally throw 合同。需重新落实同步停止及离线消费的取消/截止清理，而不是拷贝旧线程再加超时。
4. 本次没有建立同时覆盖上述 ABI、schema、session/token ownership、异常分离、稳定 DTO/序列化自检、阶段落盘、离线预算及负控的可审查有限实现。不能以压缩代码行数或扩大平台来绕过约550行新增诊断限制。因此停在编译前。此为本次实现未建立的具体安全阻碍，不是证明这些 Windows API 不可用，也不是证明约550行内绝对不可能实现。

没有新增诊断代码，故未发生 address-taken 变量闭包捕获或 CA2219 修改；也没有编译器/分析器通过证据。没有实现关联器或 TDH decoder，错 PID/TID/HWND/QPC/token/sequence、未知 version、截短 payload、lost 降级负控全部 NotRun，不编造通过。没有采用同步 scope 或2秒线程计划，不能报告任何采集时限/清理已认证。

## 执行预算与证据边界

|项目|本次实际|结果|
|---|---:|---|
|诊断/生产源码、csproj、资源修改|0|全部保持原样|
|normal COM build|0/1|NotRun，编译前停止|
|COM publish|0/1|NotRun|
|Windows 沙箱/原测试体|0/1、0/1|NotRun，无 task ID|
|Native、发现、official、x64、full、重试、终端/脚本、再委派|0|未执行|

没有 build/publish/sandbox 工具调用，因此没有对应参数、真实 exit、warnings/errors、TRX、监督报告、stdout/stderr、新 test SHA/MVID、actualNative 加载证据、adapter/module 快照或 ETL 可归档。未跑旧包。SDK/源码读工具存在字符截断，上文保留实际可见边界；不声称保存不可见返回原文。没有 ETL，故不存在已全字节解码审计的主张。

新增文件仅本 closeout；最小更新恢复入口、current-work-item、计划状态及当前基线，并回读修改。全部历史文档/预算及双架构候选有限通过保留，不转绑当前身份。原 shader/resize 本次未执行，既有前置真实失败不被更改。

## 最窄结论与唯一主线

本门没有新增系统运行证据，显示内容根因仍 Unknown，本次显示诊断路线结束。唯一收敛主线：回归原产品真实 HW 内容与 resize 合同，只在形成可验证的产品缺陷依据后另授权产品修复及原最终矩阵；不以本次未实现、历史有限绿或缺少 ETW 事件宣称产品通过/驱动故障，不建议追加同义诊断门。完整替换 Open。
