# S-20261009-210：撤回基线同步与Widen实现路线待决（未交付）

本轮没有新增生产功能，不作为完整模块进度。恢复读取S-209，重新发布并验收撤回后的产物。原生核对compactshapes.cpp:506–600、figure.cpp:40–145、shape.cpp:1040–1165：矩形优化直接调用输出非虚AddRoundedRectangle/AddBeveledRectangle/Transform，内部又依赖完整figure对象。一般sink转移CRail。不能把字段writer通过等同真实MSVC对象兼容。

## 发布

#1完整命令 `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，目标同路径，Release/win-x64，成功；11行发布日志未读取，工具成功可核实，完整诊断未保存。产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。全量353/353、366ms、0失败/跳过；TRX c85302d7-8202-4ad4-9493-cb9f7673dead，SHA256 3B5CF56F4D574D401591784E1FEE19615AB08C3EFA9BBEDD74484376981CE15B，从实际DLL加载测试输出取得。固定TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx；完整历史副本未保存。没有定向新增功能验收、覆盖率、原生差分/E2E/Ledger验证。

## 路线

默认应保持项目纯C#翻译目标，联合翻译CShape/CFigureData构造、必要方法族、Widen sink及所有权，而非把不完整对象交给外部MSVC函数。另一方案是新增生产C++适配层，负责原生对象构造/销毁，属于新增语言/构建依赖，必须先获用户选择，不自行引入。两条路线都不能只提供测试writer或bounds替代。

待用户明确是否允许生产C++适配层；不把此决策描述为外部环境阻塞。private stroke仍E_NOTIMPL，完整模块未关闭，阶段和替换资格不变。下一动作在路线选择后联合实现private完整fill→Widen→stroke与真实ABI/生命周期，不继续外围小切片。
