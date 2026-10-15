# SetMilPerfInstrumentationFlags：真实呈现语义 Open

最终同SHA与原始报告见[集成档](Four-exports-integration-closeout.md)。最后解绑/重绑定测试修改已实际重发布、对候选/独立原版各运行：flags0/2/0对应候选WM_USER计数0/1/0，原版0/0/0，HRESULT均0；候选消息payload123/456正确。原版中间Passed=false，不因顶层Passed=true宣称一致。

原生实读：`uce/apifunc.cpp:952–956`仅赋共享flags；`partitionmanager.h:415–424` bit1禁throttle，bit2 SignalPresent；`hwndtarget.cpp:265–289`必须m_fNeedsPresent，且render target存在、rendering enabled、无no-screen-access、非zombie，底层Present成功后才PostMessage(WM_USER,123,456)。成功SameThreadPresent返回值不等于必然进入该分支。

本次真实触发窗口：topmost可见32x16 STATIC、target46/visual39、create-target0x31/root0x35；每轮清空WM_USER，设flags，解绑再绑定、Close/Commit、SameThreadPresent后立即读取消息。未Dispatch窗口消息，也无内部needs-present/可访问屏幕状态观测，不能证明原版实际执行同一次底层呈现。不能猜flags位错误或以多等待自动判Green。

候选实读 `Abi/PerformanceInstrumentation.cs`：result>=0且bit2便PostMessage；`HardwareRenderTargetExports.Render`的软件/硬件Present出口均调用Presented。与原生条件是否等价尚无运行证明；不能仅S_OK推导原版必须发消息。

结论Open；不无限扩测试、不未经合同改perf生产。若后续需生产修复，先记录最小明确合同并通知父级，再协调共享所有者，最终同SHA复验；本阶段不改player/HWND或生产。禁throttle、并发flags与其它调度消费也未完整验收。
