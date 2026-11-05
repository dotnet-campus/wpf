# 原 Present 回收 ETL 离线 TDH x86 r1

## 实施前范围、关联预期和预算快照

已亲读 raw-etl stage1 全文、r1 closeout（截断段补读）、恢复入口当前头部、当前规则全文、连续性收尾全文、工作项/基线/plan当前头部，以及原 controller 1–119 全文。直接列目录确认输入位于 WpfGfxShape/artifacts/original-present-raw-etl-x86-r1/readback/Results/WpfGfxShape-OriginalPresent-8b2f6187eae44b42b5943845c2b10127/original-present.etl；不以文本读取 ETL。controller 原件：PID2748/TID4736/QPC频率10000000，Present before39911863244854/after39911863255850/result0；采后长度131072/SHA4085C964310A28A8FCEB686326F401B4405F8B8512C408307B89F81F050214E6。当前仅复核 JSON，未复算二进制。

仅新增 ComAcceptance 默认关闭离线 TDH partial、OriginalPresentOfflineTdhDiagnostic 配置及诊断限定自有输入 Content/clean复制项。生产、原产品测试、collector、NativeRun、SDK/TFM/包及公共入口零改。正式同 exe 唯一无参 FQN：WpfGfxShape.ComAcceptance.D3DImageTests.WhenRecoveredOriginalPresentEtlIsDecodedOfflineThenRuntimeBoundaryEvidenceIsRecorded。默认输入 inputs/original-present.etl，附原controller；不上传整个repo、不用文本搬运ETL。实际二进制复制只由MSBuild Content/Copy执行，clean白名单只在新诊断配置含这两个自有输入。

关联先以运行机 TDH schema/完整 descriptor 为权威，保留所有事件及未知/错误；仅实际 schema 明示 Present Start/Stop 的事件可关联。RAW_TIMESTAMP 必须先核 logfile ReservedFlags clock1及PerfFreq=原controller频率才比较原QPC。PID/TID/闭区间均须匹配。错PID、错TID、错QPC和未知descriptor拒绝为静态元数据负控，不注入系统故障。不把0事件说成无Present；不推kernel提交/DWM/scanout/颜色正确。

执行预算：normal COM build最多1（OriginalPresentOfflineTdhDiagnostic/win-x86/net10.0-windows）；成功后COM publish最多1（selfcontained10.0.13，显式既有15A545 Aot输入，独立publish和clean）；成功后Windows离线沙箱1/精确体1，监督120秒/外层180秒/整目录回收。失败即停，无修改后重试、分析器压制、独立发现、Native、原测试重跑、x64/full/显示/重采/provider扩展/终端/脚本/再委派。

官方SDK 10.0.26100.0 evntrace.h 1073–1115/1611–1650、evntcons.h EVENT_HEADER/EVENT_RECORD、tdh.h 466–645/651–660已直接读取。实现将采用Winapi回调、x86自然Pack8布局守卫、OpenTraceW FILE|RAW_TIMESTAMP|EVENT_RECORD→同步ProcessTrace→finally CloseTrace一次；delegate活至ProcessTrace返回，无后台线程。稳定DTO不序列化原生指针；回调异常保存后回到托管侧传播，关闭错误独立记录不覆盖主异常。

## 执行账（随门更新）

Implementation=Delivered.BuildFailed（未认证）；Build=ExecutedFailed；Publish/Sandbox/Test=NotRun；Closeout=Completed。新增normal COM build1，publish0/沙箱0/体0/Native0/独立发现0/原业务重跑0/重试0/终端0/脚本0/再委派0。全部历史账原样保留，完整替换Open。

## 唯一真实构建门与停止

实际 run_build 参数：targetPath=WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj，configuration=OriginalPresentOfflineTdhDiagnostic，runtimeIdentifier=win-x86，targetFramework=net10.0-windows；无额外分析器开关。exit1，133 warnings/1 error，3.03秒。新增partial第157行 `if (failure is null) throw;` 位于finally内，引发CA2219。没有修复后重试、没有压制分析器。失败即停止源码编辑和后续执行。

工具返回失败及64行日志；read_last_log_lines(1,32)/(33,64)完整读取所有可见存储日志。stdout105587字符，仅尾20000可见，前85587不可见。可见日志回读不等于完整原始stdout归档；元数据/真实参数/错误摘要已即时保存至 artifacts/original-present-offline-tdh-x86-r1/build-result.json。未将转述摘要冒原始日志全文。

交付文件：D3DImageTests.OriginalPresentOfflineTdh.cs（默认条件编译partial）、WpfGfxShape.ComAcceptance.csproj（新非默认配置、runtime pin、自有输入Content）、ComAcceptance.Package.targets（仅新诊断配置两个输入clean白名单）。SDK/TFM/包版本、生产/原产品测试/collector/NativeRun均未改。配置文件变更不等于已发布clean包；没有成功发布物、task、TRX、decoded.json或新运行身份。不得执行旧exe冒本次解析。

实现尚未通过编译门，x86布局/回调/CloseTrace/TDH及负控都未运行认证；无事件数量、实际descriptor、schema字段或header clock证据。本次只能确认原controller记录值与交接一致，不能确认ETL结构/目标Present/关联；更不能证明kernel提交、DWM、scanout或像素正确。

唯一后续若另授权：只修复新partial的CA2219异常保存边界（将写盘失败保存后在finally外传播），重新静态审核后另给单次构建/发布/离线沙箱预算；本轮不执行。该动作可由编译器直接证伪修复，仍不是产品根因证据；不自动扩大DxgKrnl/重采/显示探针或解析平台。

阅读边界更正：上文SDK范围为所参考结构的合并范围，不是连续每行全文审核；TRACE_EVENT_INFO 586–599及尾645后本轮未连续读到。旧closeout部分长行工具截断，已读取的原controller1–119和stage1完整，不把旧报告被截断部分称完整审核。本轮未满足成功运行所要求的完整回收；失败分支据实收尾。
