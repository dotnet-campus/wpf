# 第四批 K：独立 BitmapRealizer x86 根因调查

## 状态

**未关闭，保留失败。** 未可靠确定x86布局，不猜偏移、不加skip、不把夹具失败认定为候选缺陷。本批只修改两份报告，没有测试源码或生产变更，没有生产重发；未声明当前源码构建验收通过。

D独立双架构各5/5、pcore各1/1；H普通成员Thiscall及Brush=10、Effects=11修正后，明确选取D归档DLL双架构各188/188。历史183/114/69由H限定归档回归关闭，不外推最新集成。详细回读边界见 `replacement-supervised-abi-content.md`。

## 官方结构证据

以下均为仓库官方源文件，只读取，未修改：

1. `src/Microsoft.DotNet.Wpf/src/WpfGfx/core/api/api_brush.h:245-247`：CMILBrushBitmap继承CMILObject及CMILBrushWithCache；不是只有单个vptr的平坦结构。
2. `api_base.h:24-44`：CMILObject继承CMILCOMBase，含factory指针及虚析构等；不能用单指针头推导派生字段。
3. `api_basebrushes.h:55-59`：CMILBrushWithCache多重继承CMILBrush和CMILResourceCache。`common/shared/ResourceCache.h:123-130,155-163`：缓存包含IMILResourceCache接口、DynArrayIA及m_cInCall等，布局受指针宽度及构建配置影响。
4. `api_brush.h:420-445`：字段依次为XSpaceDefinition、CMultiOutSpaceMatrix、CParallelogram、两个BOOL、IWGXBitmapSource指针、wrap enum、MilColorF、opacity。官方声明支持字段顺序，但本批没有建立完整的x86基类/对齐/子对象指针调整证据，不能据此直接给出数字偏移。
5. `api_brush.cpp:362-385`：GetBitmapToSampleSpaceTransform读取采样空间定义及矩阵；`core/sw/swlib/swrast.cpp:857-877`：将CMILBrush指针static_cast为CMILBrushBitmap，读取矩阵、texture、wrap、border再调用GetCS_PrefilterAndResample。CMILBrush子对象相对完整对象的调整亦必须验证。

现夹具 `Tests/WpfGfxShape.ComAcceptance/IndependentBitmapRealizerAbiTests.cs:78-91` 分配256字节刷缓冲区，将vptr放在缓冲区起始，再硬编码matrix=80、texture=208。这两个数只有x64实跑支持，尚无x86布局验证。上述结构证据表明不能简单按指针大小缩放猜值，也不足以证明某一个字段就是实际故障点。

## K实际执行与身份

唯一新增ignored目录：`artifacts/supervised-abi-k/`。不清理其他任务。通过现有正式ComAcceptance exe执行，未新增工程、脚本、shell或原生探针。

### x86：复现失败

- 明确选取H归档 `artifacts/supervised-abi-h/readback/bitmap-final-x86`，不是最新集成包。
- 沙箱任务 `1b0b9f9f40a4499b9f3e0213195496c6`；MSTest4.0.1，win-x86，.NET10.0.13；过滤IndependentBitmapRealizerAbiTests。
- 实际输出：`Fatal error. 0xC0000005`，托管顶层为 `IndependentBitmapRealizerAbiTests.WhenBitmapRealizerSucceedsThenRepeatedMemberCallsProduceRed(Boolean)`。
- 回收 `readback/x86-bitmap/Results/aot-identity.json`；候选归档SHA为 `DD10C795FE66535B8D8079781D3DE301BF4DCA81F1A1A9A9227515DCFDAF1EAB`。此初始化identity不是候选分支已执行的证明。
- ResultsK只有部署目录，没有最终TRX；不能给出两例通过率。历史交接已标为官方x86失败，本次输出没有参数False/True及原生指令地址，不能声称本次独立重新定位了官方故障指令。

### x64：2/2

- 明确选取H归档 `artifacts/supervised-abi-h/readback/bitmap-x64`；任务 `1b61662dfc13429b9d91048d593af883`；.NET10.0.13。
- 最终TRX：`readback/x64-bitmap/ResultsK/Administrator_EN-DEV-01_2026-10-19_04_40_05.6096136.trx`，已回读Counters：2执行、2通过、0失败。
- 官方业务JSON：`readback/x64-bitmap/Results/independent-bitmap-realizer-False-bb88fc8fe018415189a0df46a0ba0d4b.json`；官方实际路径为任务根wpfgfx_cor3.dll，SHA `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`，版本 `10,0,1326,50504 @Commit:4099f83977f4d29604a9aeb971fb0a7bc1d5f699`。
- 候选业务JSON：同目录 `independent-bitmap-realizer-True-3bea7b4e83ab49ff94449c442c32d9d4.json`；实际Native路径，SHA `FDF0A6D91EF121ABD399F2B902738FB688629784EED9D0305307D5C9E7774FBE`。
- 两例各16个HRESULT=0，各Ensure/Brush/Effects=16，ContextMatches=true，原始BGRA均为 `[0,0,255,255,0,0,255,255]`。matrix80/texture208只在此x64场景得到行为支持。

## 精确定位边界与待办

本批能够给出的精确故障位置仅为上述测试方法；源码高风险调用边界是第107行循环通过view槽11发起的独立调用，但没有原生故障地址、回调前后持久标记或带行号堆栈，**尚不能证明崩溃恰发生于该语句，更不能命名某个官方内部字段解引用为已确认根因**。

后续应仅在允许领域测试内增加阶段及callback前后持久记录，并将官方/候选各自隔离，以获得明确分支及最后成功边界；根据完整官方基类、DynArrayIA、矩阵/平行四边形布局与CMILBrush子对象调整建立独立x86声明，再验证实际调用，不能以候选类型作唯一预期。可靠修复源码后，必须重新构建并实跑最后源码x86/x64 Release受影响矩阵，回读实际DLL身份/TRX/原始像素。当前未做到该闭环，因此保留原失败，不宣布根因修复。

原始实跑产物与摘要：`artifacts/supervised-abi-k/execution-summary.json`、`readback/x86-bitmap`、`readback/x64-bitmap`。摘要内x86致命输出摘录是本批工具结果的人工记录，不冒充已回收的原生dump或最终TRX。
