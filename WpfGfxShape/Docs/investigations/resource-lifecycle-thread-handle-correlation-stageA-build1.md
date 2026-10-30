# Thread correlation 阶段A唯一normalCOMbuild1真实回执

源码已冻结；失败即停止，不修后再试，不尾改源码。构建1/1；exit=1，warnings=0，errors=1。可见日志19行，已完整回读1–19，无可见日志截断。工具未给总墙钟耗时；MSBuild输出Elapsed=00:00:01.07，restore用时5.02秒，二者原样分别保留，不拼造总时长。

参数：targetPath=WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj；configuration=ResourceLifecycleHandleTypesDiagnostic；runtimeIdentifier=win-x64；targetFramework=net10.0-windows。

完整可见日志（来自最后构建日志）：
1: 命令: dotnet build
2: 目标: WpfGfxShape\Tests\WpfGfxShape.ComAcceptance\WpfGfxShape.ComAcceptance.csproj
3: 附加参数: --configuration ResourceLifecycleHandleTypesDiagnostic --runtime win-x64 --framework net10.0-windows
4: 退出码: 1
5: 结果: 失败
6:
7: <标准输出>
8:   正在确定要还原的项目…
9:   已还原 d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Tests\WpfGfxShape.ComAcceptance\WpfGfxShape.ComAcceptance.csproj (用时 5.02 秒)。
10: d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Tests\WpfGfxShape.ComAcceptance\ProcessHandleTypeSnapshot.cs(268,41): error CS1597: 方法或访问器块后面的分号无效 [d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Tests\WpfGfxShape.ComAcceptance\WpfGfxShape.ComAcceptance.csproj]
11:
12: 生成失败。
13:
14: d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Tests\WpfGfxShape.ComAcceptance\ProcessHandleTypeSnapshot.cs(268,41): error CS1597: 方法或访问器块后面的分号无效 [d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Tests\WpfGfxShape.ComAcceptance\WpfGfxShape.ComAcceptance.csproj]
15:     0 个警告
16:     1 个错误
17:
18: 已用时间 00:00:01.07
19: </标准输出>

本次未执行test/publish/Native/沙箱/终端/脚本/再委派，全部0。失败后仅文档收尾；不得把warnings0当分析器完整通过，也不得把编译失败的自检声称已运行。
