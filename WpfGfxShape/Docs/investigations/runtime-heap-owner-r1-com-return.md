# COM publish1 返回 / 沙箱1登记

参数：dotnet publish WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj -c RuntimeHeapOwnerDiagnostic -r win-x64 -f net10.0-windows --self-contained true -p:ComAcceptanceSandbox=true -p:AotRuntimeIdentifier=win-x64 -p:AotDllPath=d:/lindexi/Code/dotnetcampus/WpfLab/WpfGfxShape/WpfGfxShape/artifacts/runtime-heap-owner-r1/native/wpfgfx_cor3.dll -p:RuntimeFrameworkVersion=10.0.13 -p:WindowsDesktopRuntimeFrameworkVersion=10.0.13 -o WpfGfxShape/artifacts/runtime-heap-owner-r1/publish
原返回成功，60行，亲读1–8/56–60，Exit0，stdout57470字符/前37470截断，warnings总数及耗时Unknown。实际隐式编译1，normalbuild不混计。

登记沙箱1/8：sourceDirectory=WpfGfxShape/artifacts/runtime-heap-owner-r1/publish；exe=WpfGfxShape.ComAcceptance.exe；args=[--supervise-exit, Results/runtime-heap-owner-r1, 840, FullyQualifiedName=WpfGfxShape.ComAcceptance.UceChannelTests.WhenRepeatedAsyncSharedWicLifecyclesCompleteThenEachRoundReleasesOwnershipAsync]；timeout900，outputRelativePath=null；localOutputDirectory=WpfGfxShape/artifacts/runtime-heap-owner-r1/sandbox-results。唯一父体1，两role各1各16轮；目的固定生命周期及三末点heap观测。余sandbox7/normal3/tests3/Native2/COM3。
