# EventProxy 当前源码 Native AOT 发布与 COM Green

## 本轮范围

按用户要求发布现有生产实现，再运行唯一 COM 验收项目。没有修改生产代码、测试断言、项目依赖或解决方案，没有创建 Host。原生合同沿用 S-UNDATED-054：core/api/exports.cpp:895-910、core/av/eventproxy.cpp:209-267、include/wgx_render.h:82。

## 实际验证

- 发布目标：`WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj`，Release / win-x64；项目设置 PublishAot=true、NativeLib=Shared、SelfContained=true。
- 发布返回退出码 0；日志包含 `Generating native code`、创建 native lib/exp 和发布目录。发布有 CA1416 警告，输出被工具截断，不能声称零警告或给出完整警告计数。
- 发布 DLL：`d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\artifacts\publish\WpfGfxShape\Release\win-x64\wpfgfx_cor3.dll`。使用项目约定默认目录覆盖旧产物，未使用独立版本目录。
- 随后运行 `WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj`；测试程序集 Debug/net10.0，待测 DLL Release/win-x64。
- 测试退出码 0：24 通过、0 失败、0 跳过，共 24，报告持续时间 86 ms。测试构建成功。
- 初始化实际检查 Windows、DLL 存在、PE 无 CLR header、架构匹配及 MILCreateEventProxy 导出，再直接调用真实 COM vtable。没有生产托管程序集引用或内部替身。
- 测试将路径和 SHA256 写入 TestContext，但成功测试的摘要日志没有展开该输出；本轮未取得可独立归档的新哈希或 TRX，不复制旧哈希充作新产物身份。未取得源码提交号。

## TDD 结论与边界

已有测试先于实现编写，旧 DLL 的共同前置 Red → 生产实现 → 本轮当前源码 AOT 发布 Green 已有证据。24 项覆盖创建参数、QI/identity/引用保留、HRESULT、二进制 payload、descriptor 副本、多对象隔离、callback 内 QI/平衡 Release 和最终 dispose。

实现前没有当前源码新发布 Red，这是历史证据缺口；本轮不能追溯补造，也不回退生产代码来制造 Red。旧 DLL 的初始化失败不代表 24 个行为断言曾分别失败。

EVID-EVENTPROXY-COM-EXPORT 在现有 win-x64 测试范围内 Passed。完整 exports.cpp 文件族仍为 Partial；并发关闭、低资源、原生差分、进程退出 SEH、其他架构、真实媒体创建和 PresentationCore E2E 未验收。阶段 4 Active、替换资格不变。

## 后续

下一轮按 current-work-item.md 完成受控并发 EventProxy 引用—回调—最终释放生命周期验收；新增契约测试先于任何生产修复。保留现有 24 项回归，不恢复旧测试项目。
