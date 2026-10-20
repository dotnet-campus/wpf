# 两测试项目最终清理

仅用专用.NET发布与MSBuild接口，无终端、脚本、Exec。Cleanup.cs为.NET10文件应用，Main接收工作区根目录；真实工作区由.NET MSBuild Roslyn任务字节加载编译DLL后调用Cleanup.Run(root)。不是生产能力或第三测试项目。

最终删除清单（Tests下）：GenProtoC5Audit.Diagnostics、GenProtoNativeLayoutDiagnostics、GenProtoX86Clr.Diagnostics、NativeCaller、Stage4Layout.Diagnostics、Stage4Layout.X86.Diagnostics、Tools、WpfGfxShape.AbiIntegration、WpfGfxShape.AbiIntegration.Host、WpfGfxShape.ComAcceptance.Host、WpfGfxShape.GenProto.Tests、WpfGfxShape.LegacyHash.Tests、WpfGfxShape.PlayerHwndAcceptance、WpfGfxShape.PresentationCoreE2E、WpfGfxShape.PublicAbi、WpfGfxShape.WpfReference、X86SharedSupport.Diagnostics。

既有授权账为30项目/17目录，不随幂等重跑累计。原账artifacts/test-results/consolidation/cleanup.json保留；行为迁移映射见../../WpfGfxShape/Docs/completed-work/two-test-projects-consolidation.md。迁移已经完成，清理器不再复制旧源码或改测试断言。

校验Tests根恰为WpfGfxShape.Tests与WpfGfxShape.ComAcceptance，递归只有两个项目。完整解析冻结原生TRX的Counters、RunID、全部失败以及PublicAbi两份JSON，摘要写ignored WpfGfxShape/artifacts/test-results/consolidation/final-evidence.md。保留原始TRX/JSON、两正式源、生产发布物和核心Ledger。

回收临时Apply.proj、eng内生成文件/目录（仅保留本README与Cleanup.cs）、WpfGfxShape/artifacts下cleanup-tool、cleanup-publishobj、obj/Cleanup、bin/Cleanup及根artifacts/bin/Cleanup.cs、artifacts/obj/Cleanup.cs。MSBuild使用已加载字节，不锁待删DLL。运行只输出六行摘要，不倾倒JSON，不写Tests/ReferenceData。本轮最终源码专用AOT发布退出0，日志含Generating native code，无IL3050/IL2026，样式警告保留。随后.NET MSBuild字节加载该DLL连续两次真实执行Cleanup.Run，退出0；两次各0文件/0目录/0字节、迁移0（仅业务删除数，不将临时编译回收算业务）。旧30项目/17目录不累计。Apply与发布编译产物已自清，eng仅本README与Cleanup.cs。未重跑生产发布或任何full；生产失败不改成通过。
