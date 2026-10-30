# 阶段A 变更后/构建前冻结快照

代码已冻结：collector 356→422行（+66）、SwapChainObservation 63→80行（+17）、csproj 109→123行（+14），合计新增97行，无删除；实际较原100–160行建议略小，远低于350行停止线。只改三处授权文件；资源/生产/SDK/TFM/库/Host/Native/离线decoder无修改。新增OriginalPresentDxgKrnlDiagnostic默认关闭，独立obj/bin，原collector符号加ORIGINAL_PRESENT_DXGKRNL，win-x86 Native路径不变，framework reference pin10.0.13。

静态审阅：没有新event-ID filter结构/指针转换，因此不涉及该x86 ABI；既有TraceWnode48/TraceProperties120/x86字段offset108/logger104/TracePrivilege16运行守卫原样保留，不以编译代运行认证。WNODE_FLAG_TRACED_GUID0x20000、clock1、sequential1、Enable control1、timeout500、Stop control1保持。D3D9原level6/any0xf不变；DxgKrnl level4/any0x08000000/all0。provider Guid局部ref传参，无addressTaken变量被闭包捕获；Timer只捕获this，不捕获原生地址。

锁gate串行Start/Enable/After/CompleteExit/watchdog/Dispose。两秒一次性watchdog不续期；After仅内存返回QPC/结果；新符号外层finally记录Complete退出（或原调用/assert提前异常）并Stop。单次owned&&!stopped门，Stop只传自有handle与null名称，不查找/停止别的session，不重试。Stop失败仍如实保留状态，不声称session实际已停止。Complete早于Stop请求即正常边界；更早Stop请求标TruncatedBeforeComplete；watchdog因锁/调度不保证硬实时2秒。

临时权限提升仍Start内部try/finally恢复及关闭token，保留Bool/LastError/previous属性及bytes；无恢复后token快照，不能称权限状态运行认证。设置异常仍Stop，原Present/assert/Complete原异常不catch、不替换；finally无throw，新Stop复用DiagnosticError过滤记录，cleanup错误在Errors/Apis独列；灾难性非诊断异常未承诺吞没。DTO均string/数值/bool/Guid/list，无裸nint；Start前原JsonSerializer预检保留。窗口owner查询置Start try内，失败记录，不改变原业务参数。窗口创建时间未伪造，记录实际观察QPC及既有VisualCache生命周期边界；事件header PID/TID/rawQPC只在ETL保留，阶段A不解码进JSON。

原Clear、Present参数/线程、Assert、Complete内部query/消息泵/两次DwmFlush、颜色/前置及原FQN不改；无额外等待。旧D3D9-only配置After Stop和历史轨迹不变。descriptor来自决策中已核PresentMon可变main，只存选择来源，不认证运行机schema。宽keyword其它原始事件保留，不冒三事件专采。

接下来仅一次normal build，调用参数：{"targetPath":"WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj","configuration":"OriginalPresentDxgKrnlDiagnostic","runtimeIdentifier":"win-x86","targetFramework":"net10.0-windows"}。结果无论成败代码不尾改、不重试、不抑制warning。publish/Native/沙箱/测试/发现/再委派/终端脚本全部0。GPU完成/DWM/scanout/完整替换均未证明，整体Open。
