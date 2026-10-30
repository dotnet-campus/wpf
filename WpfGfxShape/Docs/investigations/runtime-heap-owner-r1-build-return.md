# normalbuild1 原返回与读取边界

工具参数：targetPath=WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj，configuration=RuntimeHeapOwnerDiagnostic，runtimeIdentifier=win-x64，targetFramework=net10.0-windows。
原返回：执行成功。完整日志共64行，可使用read_last_log_lines按行读取。已亲读1–64：命令dotnet build，退出码0，145warnings/0errors，耗时00:00:04.45。stdout114359字符，前94359不可见，保留末20000；不能声称完整stdout归档。尾warnings为既存MSTEST/CA类，本文件不逐字重存末20000。日志原件为当时最后日志（会被下次执行覆盖），本文件只保留准确参数、结果、截断与摘要。源码编译成功不等于执行。预算normal1/4，余3；tests0/3、Native0/3、COM0/4、sandbox0/8。

下一登记Native publish1/3：独立runtime-heap-owner-r1/native，Release win-x64 net10.0，WpfGfxShapeRuntimeHeapDiagnostic=true，NETCore10.0.13，隔离IntermediateOutputPath/OutputPath防生产产物混用；目的仅诊断DLL本堆。余Native2。
