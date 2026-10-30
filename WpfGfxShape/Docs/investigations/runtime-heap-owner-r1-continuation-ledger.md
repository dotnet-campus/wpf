# Runtime heap owner r1 continuation 实时执行账

授权剩余起点 normalbuild3 / tests3 / Nativepublish2 / COMpublish3 / sandbox7。禁止脚本、终端、再委派。所有次数在调用前登记。

## 持久失败账
前 task792db958965f4236a903f4ca40938c45 原件已亲读：artifacts/runtime-heap-owner-r1/sandbox-results/Results/runtime-heap-owner-r1/supervision.json 全40行；stderr空。监督2026-10-29T08:24:57.3604465至08:24:58.0190225，X64 .NET10.0.13，840秒精确趋势FQN，退出2，无timeout/kill，outputCollected=true。testSHA2460BACFE9E0E25FD15E0F69CE9D4114106B834B334B30CD20BE8E15ECC72880，MVID9306b09a-b0f3-4a44-bf9d-b56eaee95ce2。错误根使用publish而非正式package，缺aot.sha256导致初始化失败；不计业务失败、不计完成16轮。保留全部原件。

## 静态核验与本次登记
实际csproj启用RESOURCE_LIFECYCLE_TREND/HANDLE_TYPES/RUNTIME_HEAP_OWNER_DIAGNOSTIC；ExitDiagnostics精确SupervisionFilter允许840，NativeRun趋势role入口及ValidateOptIn均同符号。正式PackageSelfContainedComAcceptance目标自动生成com-acceptance-package，aot.sha256=F4A67D183B8BF9DDD176DA9BD94A3457E5BA197278E666CF89C00131B0C55B74，runtimeconfig NETCore/Desktop10.0.13。不重发、不手写hash。正式生产默认WpfGfxShapeRuntimeHeapDiagnostic=false，诊断Native独立目录。Track仅List<(WeakReference,WeakReference)>，Observe只IsAlive无Target，无scheduler/Thread局部强根跨GC；Track构造帧在末点前已返回。官方无导出合法，仅candidate进入诊断。

登记本次sandbox1/7，sourceDirectory=WpfGfxShape/artifacts/com-acceptance-package，executableRelativePath=WpfGfxShape.ComAcceptance.exe，arguments=[--supervise-exit,Results/runtime-heap-owner-r1-clean,840,FullyQualifiedName=WpfGfxShape.ComAcceptance.UceChannelTests.WhenRepeatedAsyncSharedWicLifecyclesCompleteThenEachRoundReleasesOwnershipAsync]，timeout900，outputRelativePath=null，localOutputDirectory=WpfGfxShape/artifacts/runtime-heap-owner-r1/clean-sandbox-results。原16轮65sample和硬门不改，三末点一次诊断，不加轮/延时追绿。余normal3/tests3/Native2/COM3/sandbox6。结果task52865bb56c344c6e8d5ec0c97011aa7c exit0/36.451602s，TRX1/1、O/C各16轮，弱ref16/16→16/16→0/0；已读完整监督/TRX/comparison/三个heap JSON。但末点句柄Append被IsPoint白名单跳过，只有原10点，此缺口保留。

已最小修复测试侧IsPoint，仅RUNTIME_HEAP_OWNER_DIAGNOSTIC允许round15三个既定末点。Native来源未改。登记COMpublish1/3（隐式编译1不混计normal），RuntimeHeapOwnerDiagnostic/win-x64/net10.0-windows/selfcontained/NETCore+Desktop10.0.13，绑定原F4A诊断Native，正式目标显式package=runtime-heap-owner-r1/complete-clean，publish=publish-complete。余normal3/tests3/Native2/COM2/sandbox6。此次补齐缺失观测，不改变GC次数/轮数/等待/硬门，不追绿。
