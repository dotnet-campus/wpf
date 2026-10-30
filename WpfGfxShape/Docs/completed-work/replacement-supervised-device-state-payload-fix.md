# HY 类型5设备状态 payload 修复与双架构验收

## HZ终态：选择精确安全撤回，未验收生产补丁不保留

HZ已接管并实际执行；不是新建重复报告。最终两生产文件的HY新增行为已逐项撤回并回读；默认独立payload回归及旧红证据保留。完整替换未完成。

### HZ实跑与证据
- 先复用HY现有x64包，未拿HA旧DLL冒充新候选。首次启动误传不支持的`--blame-hang-timeout`，runner拒绝参数，没有执行测试；这是设施尝试，不计业务通过。随后改用正式`--timeout 120s`、外层180秒。
- 有效task `df657c32839a45b6b0ecb702cfcfd6fe`，.NET10.0.13 X64；过滤`FullyQualifiedName~HyDeviceStatePayloadTests`，父TRX实数2执行2通过，独立official/candidate/official-repeat各1执行1通过。普通无role默认候选也实际执行，非空返回；角色过滤不递归父测试。三子Exit0；外层工具没有提供数值Exit、kill/drain监督字段，不补造这些值。
- 父绝对TRX `C:\CodingAgentSandbox\Tasks\df657c32839a45b6b0ecb702cfcfd6fe\Results\hz-payload\hz-payload.trx`；三个role位于同task `Results/hy-payload/{official,candidate,official-repeat}/role.trx`。父32行/三个role各26行、三个business各137行已完整读取；本地完整回收`artifacts/hz-device-state/readback/payload-x64`。candidate exit.json已读取。
- 实际候选Native模块SHA `FEA2BEECA0A755D88096CAC2548F8AB01A905FB14EA433B7CBAA8545FDB0F613`；test SHA `8999ABE84F43D3EBA65B912DF7969D1B4A923A9264CB85E480ECA4F1D83C5AC7`，MVID `8e219e09-7e50-40d0-a771-6faec27fa51d`。官方两角色实际SHA `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`，10,0,1326,50504 commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699。均按实际加载BaseAddress获取路径。
- 三角色各保存两条64字节完整消息；type5、old/new偏移8/12均1/1，count精确[1,0,0,1,0]，全部业务HRESULT0、destroy/disconnect/deinitialize0、cleaned=true。64是输入/保存长度及独立结构合同，不虚称peek返回长度。此绿只属于撤回前HY现包未渲染初态，不绑定撤回后源码，不宣称双架构验收。
- 旧红沿用下方HY task7389…原始登记，本轮不重复。HZ x86、CompositionNotification/UceChannelAsyncDrain/HW/HU兄弟实跑均0；选择撤回，不以旧HA兄弟结果绑定HY候选。

### 状态所有权审计及撤回明细
- 官方CComposition构造初态NoDevice；Normal=0/NoDevice=1/Occluded=2。`composition.cpp:2225–2274`由partition拥有旧/新状态，接受RENDERING_STATUS，lost/released同时失效VisualCache，occluded安排后续compose；状态变化才发old/new。`RenderingStatusFromHr:884–911`是有上下文的转换，不证明可在任意target返回后直接改全partition。
- `rendertargetmanager.cpp:408–420`在Render错误处理使用状态，成功还可能等待Present层；HY每target.Render结果(含CanPresent成功)即更新，以及最后任意target移除/ReleaseAll直接DeviceLost，未证等价官方聚合/设备释放路径。故不把初态绿当此状态机已验收。双listener广播只有静态证据、无合法官方双listener控制，本轮也不保留它。
- 精确撤回SameThreadChannel的Targets回调构造、DeviceState枚举/字段、ProcessRenderingStatus、NotifyDeviceState及注册广播，恢复`if (_notifications.Add(channel)) channel.PostMessage(0x05)`。撤回GeneratedTargetRegistry新增Action字段/构造、Remove最后target回调、Render canPresent映射及ReleaseAll回调；原AddRef/Release、finally、渲染返回及所有其他行为保留。队列锁、空到非空唤醒、注销在途代码未改。生产COM/fill/DRDS/NativeRun/ExitDiagnostics/VisualCache/公共捕获/打包/版本未改。
- 撤回前新增行为文字快照`artifacts/hz-device-state/hy-production-behavior-snapshot.txt`；HY双架构NativeAOT及x64验收包原目录不覆盖、不清理；完整x64实际回收保留。此文字快照不是完整源码版本库快照，勿冒称逐字完整源码备份。
- 最终源码回读：GeneratedTargetRegistry53行全读；SameThreadPartition构造和注册/通知区定点核对。默认回归139行尾断言仍验证两条完整非空64字节官方独立NoDevice合同，无Ignore/skip/阈值改动，默认无role走candidate。撤回后预期仍旧红，未重发未重跑，不以现包绿冒当前产品已修。
- 撤回后Release方案normalbuild Exit0，3768warnings/0errors，24.91秒；随后无额外属性的default normalbuild Exit0，3768warnings/0errors，23.51秒。分析器正常启用，无新增NoWarn/禁规则；警告未在本轮修理。构建不是运行验收。

### 下一唯一合同门与未覆盖
下一门仍是权威partition状态来源及最小统一序列化闭包：初态、注册快照、已证明合法触发的状态变更共用序列化；不能硬写环境1/1，不能从单target HRESULT推全局。不实现未经官方双listener控制的广播分支；若实现必须补合法控制。最终保留修复须新准确候选双架构payload和必要通知/关闭兄弟实跑。HWtarget、非零SyncModeStatus、target/HW失败映射本轮未覆盖。Available/HWloss/mediaresources/full四格/x64Event0独立线仍开。最后补记：本节及三共享最新HZ节写后回读；下方HY/HX初始化和旧下一门均作历史保留。

## HY历史进行中登记
已读取执行规则、完整替换方案、最新交接入口和HW/HX根因报告。先增加默认启用的独立内容回归，实际旧HA负控后才修改生产。旧HA x64 8F74588B…、x86 75D136…只作为修前基线，不绑定新源码。

## 阶段登记
- 初始化历史：当时尚无HY构建、发布、沙箱task或测试结果；生产修改0。
- 旧红阶段已实跑：x64 task7389a0c29c444cc18e9d6db2c3c38fd3；父1执行1失败/Exit2，official与official-repeat各1执行1通过，candidate 1执行1失败。候选实际HA SHA8F74588B081AD575EC1ED046E99CCC21182BA2E6BC47932F3DE9CBEEC915BEC0；原始字节offset8 expected1 actual0，消息数[1,0,0,1,0]及cleanup均正常，非缺输入或设施失败。两官方均NoDevice1/1。四TRX、监督/stdout和三business137行已全文读取；证据artifacts/hy-device-state/readback/baseline-x64。
- 基线test SHA F2ACCBCB561FDA777984374633E59C01EEA821BF92ABA01594FE9AD8A1945DA2/MVID15be4547-903e-469d-a035-8a8fe80c371c，实际.NET10.0.13 X64；官方AD1C9DBB…10,0,1326,50504 commit4099f839…。无timeout/kill/drain失败，不编造外层工具Exit。此后测试去掉无role空返回，默认直接角色也执行候选内容合同，须再验证最终源码。
- HX既有Passed仅证明有限注销交错，不证明payload exact；官方未渲染partition快照NoDevice(1/1)，候选旧消息Normal(0/0)。多listener广播缺口目前仅静态发现。
- 计划边界：仅相关partition/channel、独立测试及本报告/三共享交接；不动COM、显示链、打包实现或版本。不虚构HWtarget/非零SyncModeStatus。
- 若关键验收不能成立，撤回本轮生产修改，保留独立失败回归和证据。Available/HWbackend/loss/mediaresources/full四格/x64Event0仍开放，完整替换未完成。
