# Native publish1

完整参数：dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64 -f net10.0 --self-contained true -p:WpfGfxShapeRuntimeHeapDiagnostic=true -p:RuntimeFrameworkVersion=10.0.13 -p:IntermediateOutputPath=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/runtime-heap-owner-r1/obj/native/ -p:OutputPath=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/runtime-heap-owner-r1/bin/native/ -o WpfGfxShape/artifacts/runtime-heap-owner-r1/native

原返回：执行成功，完整日志58行。亲读1–8/52–58，Exit0、Generating native code、输出runtime-heap-owner-r1/native。stdout615336字符前595336截断，warnings聚合及耗时Unknown，未全读可见warning中段，不冒完整原日志归档。

下一登记COM publish1/4，RuntimeHeapOwnerDiagnostic win-x64 selfcontained NETCore/Desktop10.0.13，显式AotDllPath绑定本诊断Native；独立publish目录。余normal3/tests3/Native2/COM3/sandbox8。目的执行既有固定生命周期及结束后的DLL本堆诊断，不是生产验收。
