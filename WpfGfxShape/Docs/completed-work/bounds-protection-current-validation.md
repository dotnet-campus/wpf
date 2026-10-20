# Bounds protection：当前生产验证收口

## 独立只读复核补记
本次仅审阅原物与更新Docs；未执行终端/脚本、构建、测试、发布或修改生产/测试。下文“本轮执行”指此前执行轮，不代表本次重跑。本次完整读取保护导出15行并搜索整个Code目录，确认没有生产消费者。

**KnownGap：保护开关没有可观察的生产安全路径。** setter保存BOOL状态不满足“控制额外保护”的完整目标。正常输入兼容、有限非法结构fallback一致及PE导出闭合可以单独认可，不能升级为保护完整实现或最终兼容交付。下一正式任务须先确定一项有对应实现依据的observable安全路径，明确default/false/true的输入、预期检查及返回/异常，再实现真实consumer并验证；不得空接consumer、随意cap或无限下载。此轮不写生产补丁。

本次实际回读：native-full TRX Counters为1677/1645/32、退出2；Pcore/player TRX Counters为9/9/0；ShaderBounds TRX RunID `991cd914-1c21-4dd9-829a-3dc5a43fe969`为1/1/0。最终双方JSON的`shaderBounds`数组280–440行已全部读取并逐项核对31项一致：default红色；0→1→-1→2→0各三种长度红色、三种非法结构白色。441–454行实际模块SHA不同且均exitCode=0；candidate的PendingIndependentComparator仅为单角色状态。

primitive/default双方实际加载模块已回读：官方1956176字节、SHA `098D9CEFACA7B1E36460EB28F2725D7AFDB8E93855C29F8DD3627A1FB692AE8E`；候选4463616字节、SHA `737CDD5712F0A499FFF1ADB905741DB19ED899C9B0FA4237E0C6A3333A00CA45`。两边runtime字段为.NET 10.0.9/X64；PresentationCore与WindowsBase文件版10.0.926.27113，产品版10.0.9-servicing.26270.113+901ca941248413c79832d2fdbd709da0c4386353；候选产品版1.0.0+b4de881638949d7fdbb653f9650fb21fd07bc685。primitive/default独立比较为byte-exact、threshold=0、双方像素SHA相同。不能把requestedNative或frozenCandidate字段误当实际加载。这里是runtime声明与WPF文件版本实证，不是单独coreclr.dll文件版本核验。

**独立验收尚未完成：**本次未逐文件读完24组全部original/candidate角色JSON及所有scene/comparator数组，也未完整读完retail50 Calls、托管最终原始TRX和全部32失败详情。因此不对“所有Pcore场景zero、所有角色身份/CLR版本均独立核毕”签字；下文原执行轮记录及基线数字保持其原身份，不作为本次完整逐文件复核声明。PublicAbi三份报告搜索均见Missing=[]/Extra=[]，仅支持当前PE闭合。已有实际失败片段包括HW cache期望0x00563412而桌面0x002E2E2E，以及PublicHwnd shared在PresentNativeControl返回141953144；不能全部归环境，也不能据此判候选全部正确。

发现仍有完整依赖副本的精确路径：`WpfGfxShape/artifacts/sandbox-readback/bounds-complete-servicing/`（含coreclr.dll、clrjit.dll及跨平台coverage依赖）。这里只列已见路径，未测量体积、未推断其他目录均已清理，未删除任何原物。

## 裁决
本轮先实际运行已有生产发布包，再修改有限测试并运行回读。生产源码与NativeAOT DLL未改、未重新publish；唯一当前数字及SHA见[基线](../handoffs/current-stable-baseline.md)。不是完整保护实现交付，原生额外硬化及异常触发条件仍Open。

## 源码核实
`Code/WpfGfxShape/Core/BoundsCheckProtection.cs`将四字节BOOL非零归一化，通过Volatile写入进程静态字段。对整个Code目录搜索BoundsCheckProtection仅命中其定义，`Disabled`没有生产消费者。故现状确为无消费者策略状态；不得把该字段、导出存在或有限像素一致称为保护开关完整语义实现。本轮未制造空consumer或任意阈值，也未改变此生产代码以扰动已测SHA。

`SoftwarePixelShaderProgram.Compile`没有1 MiB cap，仍检查DWORD对齐、最小长度、ps_2_0版本、END位置、操作数长度、寄存器/指令及sampler等结构，托管Span/数组访问仍受CLR bounds保护。这是恒定基础安全，不是可切换native hardening。官方设置合同是控制额外bounds保护；不能由BOOL setter本身推导所有原生异常/性能路径，不能以托管天然安全替代对应路径证明。

## 本轮执行及原始证据
所有路径相对`WpfGfxShape/`，无终端脚本、无第三测试工程。

- `artifacts/sandbox-readback/bounds-current/`：第一步真实原版/候选ShaderBounds，通过；完整读取shaderBounds数组、模块身份和退出状态。
- `artifacts/sandbox-readback/bounds-current-pcore-player/`：Pcore primitive/images/nested/blur/shadow/lifetime/media/dispatcher各default/false/true，零阈值独立比较；retail-player.json两边Calls完整读取，HRESULT与输出哨兵一致。TRX RunID `52ed8560-a405-481e-9e73-f86367091cff`。
- `artifacts/sandbox-readback/bounds-current-native-full/`：真实full，已读TRX Counters、具体HW/control失败、identity，以及PublicAbi原版和候选报告中的Missing/Extra；不沿用旧Missing1。完整Results与TestResults均保留。
- 托管第一次实际full发现旧测试仍要求人为1 MiB阈值，失败真实保留于本轮测试日志；只修改`BoundsCheckProtectionTests`使其符合刚取得的官方接受证据，加入非规范BOOL 2，改名避免声称BOOL控制阈值。先定向通过，再full通过；不改golden/阈值/skip。
- `artifacts/sandbox-readback/bounds-complete-servicing/`：最终fixture补default正常shader、每次BOOL后的END尾随及截断操作数，实际运行并完整读取全部shaderBounds观测与actualNativeModules。默认正常和三种NOP资源大小均红色`0xFFFF0000`；非法version、END尾随、截断操作数均白色identity fallback `0xFFFFFFFF`，五次切换双方一致。测试比较的是两边完整数组，不靠单角色Green；候选JSON自身PendingIndependentComparator不是最终差分裁决。

## 发布/runtime偏差的真实保留
补fixture后首次仅框架依赖publish使远端选择.NET 10.0.1，原版缺保护入口，定向测试失败，证据`artifacts/sandbox-readback/bounds-complete/`；没有将其删除或算成功。随后仅验收应用用本次已测10.0.9 self-contained重新发布，恢复同版官方对照并实际通过；该版本仅本次复现输入，不是固定runtime用户合同。包目标先RemoveDir后重建，临时依赖副本不进入生产源码；没有新增eng工程或扩大清理scope。生产SHA保持不变。

## 未闭范围
HW/control失败原样保留，不随机追Green；staging正确不证明桌面正确。本轮desktopduplication两个swap均创建成功但AcquireNextFrame超时，不能沿用旧零像素描述。x86本轮未实跑；既有构建不能替ABI/ESP运行。官方servicing对应源码/SourceLink/PDB、额外硬化消费者精确范围、BOOL对应功能返回/原生异常条件尚未完成，明确Open。有限安全结构差分不证明全部恶意输入或所有hardening路径。

仅两个正式Tests；Windows x86/x64、DLL进程驻留、无固定runtime合同不变。GenProto/GUID/Ledger历史治理不是正式测试门。整体HW/全配置/持续媒体/寿命/E2E仍未完成。
