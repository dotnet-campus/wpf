# 双输入 bytecode 独立专项实证

完成独立诊断测试项目 Docs/investigations/TwoInputShader.Tests，专用 run_tests 精确过滤 WhenOwnedTwoInputHlslIsCompiledThenHalDeviceAcceptsBytecode。初次 strict flags 0x800 编译失败 X3086，随后采用 legacy backwards-compatible flags0x1000、ps_2_0 系统 D3DCompile，编译和反汇编均S_OK。最后定向回归1/1通过、0失败、0跳过，退出码0；显式DoNotParallelize后不再出现MSTEST0001。

同真实HAL设备（与VisualCache验收adapter0/HAL/behavior0x26/32×16/windowed参数一致）vtable106：原27DWORD HRESULT0x8876086C；自有pass-through HRESULT0；系统编译76DWORD/304bytes双输入 HRESULT0。完整caps、完整tokens、HLSL、flags、profile、SHA和反汇编均保存 Evidence 并回读。SHA256 4B6CCE69D31356ACAF977F9D451E8F358BE4701B0F38FAB2DDF153E3F07DC595。

实际采用原方案0.5*input0+input1，以免改golden。编译产物独立DEF c0.x=0.5，s0/s1分别TEXLD、MAD写r0、MOV写oC0；原ADD直接写oC0为关键非法结构差异。完整可实施替换与原生BadShader通知/fallback/OOM合同见 Docs/investigations/stage4-two-input-shader-bytecode-repair-plan.md，已写回读。

未改生产代码或现有ComAcceptance测试；未发布AOT、无本次生产DLL SHA/生产TRX；未终端/脚本/外部执行器、未全量、未skip、未改expected。仅确认字节码准入，未冒充Visual/Cache最终像素验收。Stage4 Active不变。
