# 当前源码托管生产主要回归：执行与完整收尾

## 终态

ManagedMajorRegression=ExecutedPassed（仅本次筛选的Debug/net10.0配置）；ExplicitReleaseBuild=Passed；Readback/Closeout=Completed。唯一正常Release build exit0/0 warnings/0 errors；唯一run_tests exit0，Total4199/Passed4199/Failed0/Skipped0，1个测试文件，2秒。测试确实隐式构建Debug，不能称Release测试通过，更不能称最终Native/COM full通过。无失败需定位或自动修复，无日志待读/文档待办。

显示根因仍Unknown；不重启ETW/捕获/窗口探针、不再造微ABI门。双架构15A545/54BC62既有有限Native证据、所有旧红、HW/loss/media/长期资源趋势/随机差分/最终full的Open全部保留。本门不闭合完整替换或实施包1–8。

## 执行前静态合同

持久快照先于任何执行写入 `WpfGfxShape/artifacts/managed-major-regression-current-source/pre-execution.md`。唯一目标为工作区下 `WpfGfxShape/Tests/WpfGfxShape.Tests/WpfGfxShape.Tests.csproj`，绝对路径 `d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Tests\WpfGfxShape.Tests\WpfGfxShape.Tests.csproj`；没有运行根solution或ComAcceptance。

Tests为net10.0、nullable enable、MSTest4.0.1、直接ProjectReference生产wpfgfx_cor3。local Directory.Build.props/targets隔离到artifacts；Tests无专有global.json/runsettings/testconfig，不继承COM项目配置。根global.json请求10.0.107，latestFeature，目录仅见.dotnet/sdk/10.0.402；日志未打印实际SDK，故不能认证实际选用版本。生产PublishAot等设置不妨碍本次普通build/test，不需要Aot绑定参数。

三生产修正实际亲读存在：PartitionScheduler管理completion/注册失败worker回滚；UceChannelExports入表/提交/失败移表及断连最后channel清理；SameThreadChannel.Register检查TryAdd。PartitionSchedulerTests全部130行含四个真实scheduler回归，late register、flush保错、source关闭共享存活、独立partition健康，均纳入本次筛选；无逐测试TRX，故不另编造四体独立通过记录。源码/项目/测试/SDK/资源/包均未修改。

已读最新handoff、阶段Ccloseout全63行、完整实施方案全197行（初次截断后分段补齐）、remaining-gap计划全64行、current规则及基线/事项/plan状态/矩阵最新段；历史长导航不当当前授权。未宣称重读全部历史原件。

## 适用集、排除与分母

项目以MSTest TestClass/TestMethod/DataRow组织，含生产路由、调度、软件渲染/几何、状态/cache、资源所有权、delegate/内存vtable设备单元，也含真实系统D3D9测试，故不能无filter全跑。静态搜索实际工厂/导入/初始化入口后保守整类排除：

- `WpfGfxShape.Tests.Direct3D9FactoryTests`：真实系统D3D9工厂/adapter/device/surface等；即使纯常量/空释放sibling也整类排除。
- `WpfGfxShape.Tests.ImageUploadCallContractTests`：真实WIC COM、GPU上传/读回。
- `WpfGfxShape.Tests.DuDeviceQueryInterfaceTests`：DU_DEVICE_QI默认条件关闭，仍防御性排除，含d3d9/user32桌面设备。

精确filter：`FullyQualifiedName!~WpfGfxShape.Tests.Direct3D9FactoryTests&FullyQualifiedName!~WpfGfxShape.Tests.ImageUploadCallContractTests&FullyQualifiedName!~WpfGfxShape.Tests.DuDeviceQueryInterfaceTests`。

其它现有托管生产主要集纳入；这不等于没有任何系统调用（scheduler有ole32初始化），而是不主动运行上述真实显示/硬件测试，不调用COM验收项目。SupportedOSPlatform标签不等于真实HW，不据此排除大量托管delegate测试。未添加Ignore/skip、未改断言/阈值。无独立发现/可用索引，不预猜分母；4199来自本次工具实际汇总，不用旧4256作相减、全量分母或补证。排除类的实际未运行体数未独立发现，Unknown；它们不是本次Skipped。

## 工具执行合同与实际账

1. 唯一run_build：targetPath上述绝对项目，configuration=Release，targetFramework=net10.0，RID省略。实际参数 `--configuration Release --framework net10.0`；正常build1，exit0，0 warnings/0 errors，elapsed1.29秒（还原行另显示5.45秒，原样保留不强行归一）。
2. 仅在build成功后唯一run_tests：同绝对项目，filter如上。schema只有targetPath/filter，不能传configuration/no-build/architecture/logger；没有路径/filter参数注入。
3. 实际测试日志显示生产和Tests输出均 `artifacts/bin/.../Debug/net10.0/`，测试亦从该DLL运行。因此另列测试驱动隐式Debug build1；没有第二次独立normal build，也不隐瞒总计两次构建入口（显式1+测试隐式1）。测试exit0/4199通过/0失败/0跳过，零skip且非零体门满足。
4. 测试日志未提供warning总数：可见warning/error明细均无，warning总数Unknown，不能写0；测试失败0，编译错误未独立总结，不能捏造编译error汇总。历史133 warnings未抑制或冲销。

预算实耗：正常Release build1/≤1，run_tests1/≤1，隐式Debug build1另计；Native/COM发布/沙箱/独立发现/重试/终端脚本/再委派均0；源码/项目/测试/SDK/资源/包修改0。构建restore产生ignored中间产物，不是修改包版本。没有失败后的修生产/断言/skip/disable warning重跑。

## 原始证据、完整性与身份限制

目录 `WpfGfxShape/artifacts/managed-major-regression-current-source/`：
- `pre-execution.md`：执行前合同、身份/适用/排除/预算与停止门。
- `build-evidence.md`：准确请求、工具原返回、最后build日志全部19行；读取1–30返回1–19，测试覆盖前已归档。
- `test-evidence.md`：准确请求、工具原返回、最后test日志全部16行；读取1–25返回1–16，立即归档且未再执行覆盖。

两份日志全部可见、无工具截断；没有独立stderr段，不冒stderr空。工具返回只有成功/行数，数字exit由完整日志第4行确认。未请求/生成额外TRX或diag，也没有日志提供其实际路径；本门权威是该单项目工具汇总，不假称逐体原始结果已认证。

实际输出身份可核至完整路径、net10.0、Release编译/Debug运行、生产程序集名wpfgfx_cor3及直接项目依赖。额外只读产物：Debug runtimeconfig全13行声明Microsoft.NETCore.App10.0.0（请求版本非实际servicing）；GeneratedMSBuildEditorConfig全19行确认net10.0/adapter；project.nuget.cache全35行成功，MSTest/framework/adapter4.0.1、Microsoft.NET.Test.Sdk18.0.0、MTP2.0.1及Silk2.23.0。没有新建执行器计算hash、未认证SHA/MVID/进程arch/指针宽度/实际SDK与运行时servicing，不把AnyCPU/未指定RID说成x86或x64。源码身份是本轮限定可读快照及零源码改动，不是全仓SHA或二进制来源全认证。旧Native15A545/54BC62绝不绑定本次托管DLL。

## 唯一下一门建议

另授权主要能力包3/6的**既存非显示媒体实资源合同选门审查**：只读正式Tests/ComAcceptance现有入口，选一个确能验证真实非显示资源取得、持续更新与最终释放的最小合同，先列精确FQN/参数、真实资源证据及执行预算；没有可用合同则明确缺口，不新造微ABI/显示探针、不自动build/publish/运行。该建议不是当前收尾欠项，亦不以再跑Debug或普通Release build冒最终双架构验收；最终x86/x64 Release Native/COM全矩阵仍须独立授权与真实身份绑定。
