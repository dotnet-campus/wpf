# 单测试项目直接加载生产 AOT DLL

用户撤销 ComAcceptance.Host/独立 slnx 做法，要求一个单元测试项目直接使用 wpfgfx AOT DLL，并禁止创建或修改 slnx。本轮未创建或修改任何 slnx，未恢复 Host，也未改动残留旧逻辑测试文件。

目录核查时 ComAcceptance/ComAcceptance.Host 为空，WpfGfxShape.Tests 仍可见旧 cs 文件；没有假定这些文件已经全部删除，也没有将它们包含进新项目。

只创建 Tests/WpfGfxShape.ComAcceptance 内 csproj、EventProxyTests.cs、TestSettings.cs。无 ProjectReference，无 Process.Start，无 Host 文件复制，无环境变量要求。默认直接加载 artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll，可通过 AotDllPath 属性指定。

验证：八项测试均成功走到 NativeLibrary.GetExport 检查，因缺 MILCreateEventProxy 失败。产物 SHA256：8CBF2F6EDF636728CF6B1F9C4736B705147F0036F5C2E4398ADFB8182F9E7A64。是现有产物的具体缺导出证据，本轮未发布，不能称当前源码发布验收。最终按 csproj Debug 构建 0 警告、0 错误（修正程序集级非并行化声明）。

独立测试方法、执行规则及当前工作项已同步为单项目直接加载。旧 Host 历史不再作为实施依据。生产实现未改，真实 COM Green 尚未取得。
