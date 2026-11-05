# DV：NativeAOT 内 device QueryInterface 最小分支对照

## 1. 裁决与阻塞
**未完成；分支对照没有执行，未证实因果，没有生产修复。** 当前会话可读写文件、构建、发布及运行 Windows 沙箱，但没有可用的子代理委派入口。用户明确要求“主代理只监督”，因此没有由主代理代替执行者修改实现、构建或运行实验，也未模拟委派。需要具备实际子代理委派能力的会话继续执行。

## 2. 已完成的监督准备
首先创建本报告，随后读取当前执行规则、current-work-item、完整替换实施与验证方案全部 120 行、DU device-queryinterface-diagnosis 全文、DT x86-interop-create-fix 全文、DR interop-bitmap-abi 全文；长响应截断尾段已补读。工作区 AGENTS 名称搜索无结果。读取了解决方案项目列表，未检查源码、项目配置或当前二进制身份，不声称完成实现审查。

专属产物根已写入 artifacts/dv-aot-device-qi/status.md。未使用终端、直接或间接脚本、原生代码、新 Host 或新工程。

## 3. 既有证据与因果边界
以下仅是本轮亲读报告中的历史记录，未重新读取其原始运行产物，不算 DV 实跑或原始身份认证。

- DU：JIT x86/x64 在同一合法 surface.GetDevice 输出上，独立 Stdcall vtable[0] 与 Silk generated QI 均返回 E_NOINTERFACE/null，最终引用释放有记录。绑定 IL calli 签名含 Cdecl 元数据，不能直接认定 NativeAOT 根因。
- DT：历史两次候选 x86 运行退出 0xC0000005；合法 surface.QI/GetDesc/GetDevice 返回成功，最后定位到 CheckDevice.QI.before，未取得 after、QI 返回或完成释放。该边界不是故障指令或根因。
- DR：历史官方 x86/x64 与候选 x64 各完成一个测试、三次 Create 生命周期；候选 x86 首次 Create 崩溃。不同历史 SHA 不绑定本轮源码，更不当作新 AOT 候选。

待判定的最小分支仍是：候选 NativeAOT 内，同一合法 GetDevice 输出、同一 this/IID/out，direct Stdcall vtable[0] 与 generated QI 是否出现差异。当前没有足够证据声明 Silk 或 NativeAOT 库缺陷，亦未排除前序内存破坏或 lifetime 差异。

## 4. 本轮实际计数、退出及身份
|项目|DV 实际结果|
|---|---|
|构建 / 发布 / 沙箱任务|0 / 0 / 0|
|x86 direct Stdcall 分支运行|0|
|x64 必要对照|0|
|官方/候选双架构 DRCreate 循环|0|
|相关生命周期 / BW / CK 验证|0 / 0 / 0|
|完成测试与父 TRX|0；无本轮 parentTRX|
|ExitCode / timeout / kill|无进程，未产生；不得填 Exit0 或 false|
|actual testSHA / MVID|未产生、未认证|
|native / official SHA 与 runtime 身份|未产生、未认证|

未创建 obj/bin/publish/package；未上传、回收或清理包源，没有将旧 SHA 装作新产物。没有新证据可支持 10.0.13 双架构正式验收。

## 5. 代码保留与撤回
本轮仅创建/更新本报告和专属 artifacts 状态文件，没有编辑 InteropDeviceBitmapExports 或任何测试。没有添加 DV_INTEROP_DIRECT_QI 编译分支，也没有临时默认修复，故没有本轮生产行为需要撤回。既有 DT trace 未动；未核查其源码状态，不把报告记载当成本轮源码审计。

未改 DR/DS、NativeRun、ExitDiagnostics、VisualCache、historical、捕获、项目打包或共享交接。没有修改 CF/CX/CY fill、Ex 检测、HRESULT 处理、引用计数或分配行为。

## 6. 继续执行的门槛与未闭项
需先提供能实际委派执行者的运行环境，保持主代理仅监督。执行者应仅在 InteropDeviceBitmapExports 的 DT 条件诊断中添加显式 direct Stdcall 分支，先 x86，用相同 this/IID/out 记录前后、HRESULT、输出及引用释放，复用 DR 合法 surface/无 Present/Create probe。其余业务序列不变，按 BL 正式监督获取 ExitCode、timeout/kill、绝对 task parentTRX、阶段与实际身份；正常依赖图独有 obj/bin/publish/package，清洁包源与回收分离。

第一阶段成功后再决定 x64 对照与循环。只有证实 direct Stdcall 修复且 generated 唯一崩溃，才实施不依赖诊断符号的最小默认修复并审查同因 siblings；随后必须官方/候选双架构 DRCreate 三次 QI/AddRef/GC/dirty/Detach/Release、生命周期及 BW/CK 小定向验证。不足证据不得替换默认实现，不盲跑 full。

Available 实际调用与通知、真实 HW 消费、恢复均未覆盖；文档更新不关闭这些缺口。本次交付是能力阻塞记录，不是成功实验或完成验收。
