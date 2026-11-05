# r9：亲读 owner-thread 实验；一次 staging 决定性对照 / OverallOpen

## 入口与结论

已先亲读 r8 报告及 `artifacts/candidate-resize-r9d/sandbox-readback/Evidence`，再作一次有界 r9e 实验，并亲读其结果。**r9d 原 nativeControl 没有通过；r9e 也没有通过。RunCycle 尚未进入，resize 完成0。** 失败发生在 candidate 渲染之前，不是生产 resize 缺陷已复现。

本次生产修改0、Native重发布0、默认async修改0；原 expected=0x00563412、逐通道容差1、采样点(16,8)/屏幕(56,48)、DPI96、device flags0x26、swapEffect1均保留。无原版加载/复制/fallback，无producer替consumer、跳门或重试刷绿。

## r9d 原件亲读

位置：`artifacts/candidate-resize-r9d/sandbox-readback/Evidence`，task `3f1031021e8a4592af96c99937fb7358`。

- supervisor PID3476 exit1；candidate PID5896 exit1；均 timeout=false/reaped=true/drained=true/killError=null。
- frozen identity：Native SHA256 `2E5DC1BCC9CC7DEF60B0EE05C622954DC9BC7C2C3A57888FEE5767ED5705AFEE`；harness SHA `94AFD6A41699BCDCD29D5936E6514FE3C043060ED0E057F8436A1CB78520614D`，MVID `13b95acd-1f68-49cc-90ab-7d8a3f8d1aca`。
- stdout、stderr、process、identity、result身份段及完整诊断采样和销毁尾部已亲读。currentThread/OwnerTid均6828；窗口、focus、hDeviceWindow、Present override、cover均0x6200020。device creation flags38即0x26，Adapter0/Type1；实际backbuffer32x16、format22，client32x16，bounds(40,40,72,56)。Default桌面、session1、visible1/iconic0/cloaked0，cooperative0。
- Present result0，双DwmFlush后windowDC/screenDC均16777215即0x00FFFFFF，原AssertCombinedPixel逐通道断言失败。栈包括 RunOwnedNativeControl → owner Invoke → RunCandidateNativeControl → Scene.Run；不是只凭源码推断 owner-thread 实验已运行。
- 5条 release refs=0；DestroyWindow结果1，WM_DESTROY/NCDESTROY存在，nativeControl-released。这里记录原始返回，不扩写为所有COM对象绝无引用。
- 失败后GDI SetPixel返回/读取5649426即0x00563412，双DC一致；仅诊断，不是oracle通过。
- candidate-raw仅result、stages、native-control-r9.jsonl，无RunCycle帧/events/resources。

## 呈现语义和共享线程状态审查

亲读共享 OutputWindow、WindowThread、SwapChainObservation、GpuReadback、resize Header/Scene/csproj。owner Invoke在owner线程直接执行；异常用ExceptionDispatchInfo传回，不吞断言；finally清空静态windowThread，using退出时PostThreadMessage(WM_QUIT)/Join。此单业务进程中尚未进入其它RunCycle，没有证据表明并发验收用例抢写windowThread。静态字段可能构成跨用例并发风险，但不能据此解释本次单入口失败或扩大修改范围。

窗口无背景brush，WM_ERASE仅返回1、WM_PAINT仅ValidateRect；r9d Clear到oracle间未见新PAINT/ERASE。实际窗口大小、前景与cover一致。Present以device vtable17调用，source/dest/dirty均NULL，显式override就是目标HWND；未在BeginScene/EndScene内调用。

只读官方文档：
- https://learn.microsoft.com/en-us/windows/win32/api/d3d9/nf-d3d9-idirect3ddevice9-present ：NULL source使用完整源、NULL dest填充完整客户区，override为目标客户区；这些参数符合当前DISCARD语义，不需要改成COPY或另换目标来刷绿。首次抓取504，第二次成功；网页不是运行证据。
- https://learn.microsoft.com/en-us/windows/win32/api/d3d9/nf-d3d9-idirect3ddevice9-getrendertargetdata ：staging必须SYSTEMMEM且大小/格式匹配，multisampled源不支持。现有ReadNativeBackBufferPixel固定format21，不能原样用于本次format22。

## r9e：仅一次最小决定性对照，实际运行后回读

验收改动仅 `Tests/CandidateResize/NativeControlDiagnostic.cs` 和 `CandidateResize.csproj`：在原Clear成功后、原Present前，读取实际backbuffer描述，创建同大小同格式SYSTEMMEM staging，GetRenderTargetData/LockRect读取原点(16,8)，记录所有HRESULT并finally Release。未改共享正式测试源码。该回读只回答Clear是否写入，绝不作为桌面consumer通过依据，也未更换device或swapchain参数。它增加同步，故r9e单独标作instrumented诊断，不能冒充未插桩原验收通过。

专用publish2成功，随后sandbox1；task `b93ec87b7b5b4e7193c83003df01e7ff`，完整回收：

`artifacts/candidate-resize-r9e/sandbox-readback/Evidence`

- supervisor PID9400 exit1；candidate PID6756 exit1；均timeout=false/reaped=true/drained=true/killError=null。外层无参数启动器数值exit未由工具给出，不编造exit0。
- frozen identity：Native仍上述SHA；harness SHA `FE69DAD18B3D2B3E23912274CA6B16A4921A42BAE1135C11468651AB61275D5A`，MVID `a0dc4d74-baa1-46d2-a2f2-4cba7cdd73df`。stdout实际加载任务Package/Native同SHA，result finalModules保留模块清单。
- `candidate-raw/native-control-r9.jsonl` 第16行：stage=clear-before-present-staging-diagnostic-only，buffer/desc/create/copy/lock/unlock全部0，32x16、format22、Pitch128，pixel=4279383126，即0xFF123456。
- 同一owner线程6336、同一目标HWND0x33502C2；Present result0。双DwmFlush后双DC仍0x00FFFFFF，原AssertCombinedPixel再次真实失败。stderr保留完整传播栈，process failures为business-process-failed/consumer-not-completed/reported-business-failure。
- 原oracle失败后的GDI控制仍同点写读0x00563412、双DC一致；不更改退出结果。诊断尾部DestroyWindow=1、WM_DESTROY/NCDESTROY；stages中7条release refs=0，最终nativeControl-released。
- 六项负控assertions逐项亲读：original/full/compare exit64 bootstrap；missingdigest exit1 digest-required；wrongsha exit1 sha-validation；wrongarch exit1 pe-architecture-validation。均passed=true/noNative=true/noRequests=true；原始各exit/result/stdout/stderr均保留，名字不代表执行过original/full真实业务。
- 无截图，本次保存逐阶段双DC采样与GPU同点采样；candidate-raw仍只有三个诊断文件，无帧/events/resources。原RunCycle源码与三轮调用仍保留，但没有执行认证。

## 证据边界与停止条件

此次最小对照排除了“Clear根本未写入GPU backbuffer”这一解释；owner线程迁移也没有解决问题。结果明确为：正确GPU颜色 + Present成功 + 当前可见窗口的桌面consumer仍白。尚未捕获DWM/驱动真实消费/重定向内容，不能把Present返回0当已上屏，也不能凭GDI可读便断言D3D桌面路径完整可见。因此**不宣称已证明环境硬限制或已找到确切验收缺陷**。

停止无目标参数实验。下一步如继续，应仅作一个有界的同参数Present路径跟踪（runtime实际目标/重定向表面/DWM消费），或在独立明确标记的诊断中对比device Present与implicit swapchain Present；后者必须保留原失败门，不能将对照成功冒充原通过。没有原consumer通过证据前，不执行/追认resize。不能改flags、DPI、swapEffect、expected、阈值或跳nativeControl。

## 执行账与前序保留

用户交接前子任务publish8、sandbox4单列，未冒充本次亲见的8条publish日志。首轮 `artifacts/candidate-resize-r9/sandbox-readback/Evidence/candidate-resize-process.json` 已亲读：子PID4224 timeout=true、reaped=true、drained=true、killError=null、exitCode=-1；这是监督记录值，不冒称OS原始exit。对应supervisor PID11036 exit1、timeout=false/reaped=true/drained=true。首逐像素诊断超时回收未遗漏。r9b/c HWND/format/flags及GDI结论按用户交接保留，不伪称本次逐份重读。

本次新增publish2成功、sandbox1、独立build0/run_tests0；生产/Native发布/新视频/full/original真实业务0。r9累计按交接为publish10、sandbox5，含前序失败与超时，不能仅报成功运行。r6有限视频结论不变；r9d/r9e的nativeControl均失败，RunCycle完成0，OverallOpen。
