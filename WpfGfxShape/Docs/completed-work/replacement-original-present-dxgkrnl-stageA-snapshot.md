# DxgKrnl 阶段A：变更前与预算快照

授权：只在现有 ComAcceptance 实施 collector/hook/新默认关闭配置；normal COM build 上限1，失败即停，成功亦停。publish/Native/沙箱/测试/发现/再委派/终端脚本全部0。历史账独立保留，完整替换Open。

变更前：OriginalPresentSystemTrace.cs 356行，仅 D3D9 level6/any0xf；After原Present返回即Stop；Start成功后2秒一次性watchdog；唯一自有session handle STOP；临时SeSystemProfilePrivilege启用及finally恢复；稳定DTO及原ETL/hash。SwapChainObservation.cs 63行，原参数/线程/Clear/assert/Complete不改。旧OriginalPresentSystemTraceDiagnostic及离线r2配置不改。

范围：只改上述collector、SwapChainObservation.cs、WpfGfxShape.ComAcceptance.csproj。预计新增约110–160行；超过约350行或需要复杂解码则编译前停。文档仅独立报告/快照及恢复状态最小前置更新，不改写历史。

明确选择用户本次允许的宽keyword短session分支：不新增EVENT_ID_FILTER/ENABLE_TRACE_PARAMETERS结构，无新x86 filter指针ABI；D3D9沿level6/any0xf/all0，DxgKrnl GUID802ec45a-1e99-4b83-9920-87c98277ba9d level4/any0x08000000/all0/timeout500ms。ETL保留所有原始其它事件；目标descriptor166v0/171v2/172v2仅来源元数据，不作目标解析或版本认证。与前决策ID filter建议的差异在此显式登记，不冒仅三事件采集。

实现后/冻结前另写快照；唯一build参数预定：targetPath=WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj，configuration=OriginalPresentDxgKrnlDiagnostic，runtimeIdentifier=win-x86，targetFramework=net10.0-windows。代码冻结后不再编辑代码；结果及日志边界即时单独保存。阶段C才解码/同机TDH认证，构建成功不冒结构运行通过。
