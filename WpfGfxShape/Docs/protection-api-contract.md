# Bounds protection：代码映射与定义行为边界

## 本轮裁决
“没有读取 Disabled 就必定行为不兼容”不是有效验收门，现撤销。应比较对应数据结构、可达条件、返回映射，而不是要求参数必有可观察差异。也不能反向由托管数组安全证明全部 nativepaths 兼容。当前无新增生产 consumer：没有证据支持为了 flag 生效削弱基础检查、制造 unsafe 访问或恢复 1 MiB cap。整体门 Open。

## 来源及完整调用点索引
本轮读取已下载 main：`.coding-agent/sandbox-results/b407d27d575c497f8f36e4c636c5810f/Results/official-protection-source/main/wpf-main/src/Microsoft.DotNet.Wpf/src/WpfGfx/`。归档 SHA `72FA5438CEBBECB33F920A9E45EA086793F1BAC75187681C1ECFA123A9074CFE`，不是当前 servicing 的源码证明。以下列出该下载快照搜索得到的全部 WpfGfx getter 调用点；不是全部 native 调用图已审声明。

| 官方文件/行 | 条件与开启保护行为 | 候选对应与裁决 |
|---|---|---|
| common/shared/WpfGfxSwitches.h:14、cpp:14 | 默认 false；BOOL !! 非零归一 | BoundsCheckProtection 保存 int/Volatile；状态本身不是安全实现 |
| common/effects/effectlist.cpp:324 | GetParameters 先验 index/null/size；UIntAdd(offset,size) 失败或超数据块 E_INVALIDARG | Abi/MilEffectList.cs At/GetParameters/AddCore，详见下节，完整对应方法族已核 |
| common/shared/dynarray.h:92,106 | 空 First/Last 使用 FreAssert，关闭则仅 Assert 后裸访问 | 不存在一对一 DynArray 通用类；不得要求复现空数组裸访问。所有替代容器调用点未逐一审计 |
| core/common/guidelinecollection.cpp:654 | 两次 UIntMult、UIntAdd 验证 snapping frame 尾随分配，IFC 传播溢出 HRESULT | 候选 snapping frame 精确替代及错误映射尚未定位，Unknown；不能标 N/A |
| common/scanop/soconvert.cpp:89,177,197,242,285 | indexed 1/4(含奇尾)/2/8bpp 索引缺 palette 项时输出0；关闭直接读取越界项；void 非 HRESULT 拒绝 | 候选转换路径及缺 palette 项 fallback 未完成精确核验，Unknown，不冒称 CLR 等价 |
| core/fxjit/PixelShader/pstrans.cpp:307 | main 中零/非DWORD/大于1MiB E_FAIL | SoftwarePixelShaderProgram.Compile 使用有界 Span；servicing 实测大于1MiB接受，禁止移植 main cap |
| 同文件:404,421,437,480 | DCL长度、sampler/input/texture寄存器边界 E_FAIL | Compile length/ValidRegister/类型/sampler 验证；支持集并非全部 native shader，完整格式及 HRESULT 等价仍 Open |
| 同文件:501,525 | NOP后错误参数及读取前 token 终界检查 E_FAIL | 单遍 length <= remaining、Slice、NOP length==0；无两遍计数失同步数组 |
| 同文件:628,664,688,707,728,747,764 | 第二遍指令/常量数组界限及 DEF/DEFI/DEFB 错误类型推进一致，E_FAIL | List<Instruction>/Dictionary 与单遍编译，不存在原生按第一遍数量分配后裸写；非法格式候选 Rejected=0x80070057，不宣称内层 HRESULT 相同 |
| 同文件:838,853,868,1124,1171,1181 | predicate/源操作数/相对地址/sampler stage/末条指令及END空间检查 E_FAIL | Compile 固定支持集、operand length/register验证、托管集合；正常shader31差分有限通过，不等于所有 malformed语义 |
| core/hw/d3dglyphbank.cpp:339,367,386,405,425,434,454,470 | locked面末行范围，失败 Unlock/E_FAIL；源/目标pitch乘坐标改INT64避免绕回 | GeneratedGlyphAlpha 有界数组采样不等于完整硬件 glyph bank；HW tank上传路径映射 Unknown |
| core/hw/hwbitmapcolorsource.cpp:3719,3787,4512 | buffer size checked helper；dirty目标offset checked、越界 WGXERR_INTERNALERROR；SelfCopyPixels端点检查 | 候选 color-source/dirty/self-copy 精确对应方法与失败映射 Unknown |
| core/resources/GlyphRunGeometrySink.cpp:487,660 | figure/segment加入后超过INT_MAX，WGXERR_BADNUMBER | Abi/GlyphOutlineExports.Geometry 的 List<byte>/AddRange 无同类裸尾随缓冲；OOM与BADNUMBER边界不同，未证明全异常映射 |
| core/resources/glyphrunslave.cpp:1705,1890,1940 | alpha union/stride*height/扩展*3以UINT64验UINT32_MAX，WGXERR_BADNUMBER | GeneratedGlyphAlpha.Sample long坐标、checked int索引；所有生成/扩展/合并错误映射尚未审完 |

## 完整方法族：effectlist GetParameters
main GetParameters 保护开启时先拒绝 index>=Count、null输出、size<uiSize；再拒绝 cbParamOffset+uiSize 溢出或超过 data block，返回 E_INVALIDARG。关闭路径先取 m_rgParamBlock[idxEffect].cbParamSize，之后才验 index，因此非法 index 的裸解引用不是应复现的正常定义行为。

候选 MilEffectList 的结构不是共享 data block：每个 Entry 含 Data、Size；AddCore 为每个 Entry 按 size 单独 NativeMemory.Alloc(size) 并复制同长数据，之后发布链表；At 在读取条目前检查 index>=self->Count。GetParameters 始终先 At、验 null/size，再 Buffer.MemoryCopy(e->Data,data,size,e->Size)。因此合法生存期、未遭内存破坏的 Entry 满足 allocation length=Size，不存在可由公开 index/size 选择的 offset+size 绕回。非法索引/null/短输出均 E_INVALIDARG。这里的证明依赖构造不变量，不是“全托管天然安全”：该实现明确使用 native 指针，不能笼统适用 CLR bounds。外部野指针、UAF、并发破坏不在此证明内；GetParamRef 暴露参数内容不能合法修改 Entry 元数据。

同族资源创建已核：资源字节数 checked(nuint count*sizeof(nint))，OverflowException/OutOfMemoryException映射E_OUTOFMEMORY，ResourceCount加法预验。这不是 main 新增 switch consumer（本快照 effectlist 仅324一处），不虚构“官方资源溢出开关”。

正常定义输入下，这一完整对应方法族没有必要强制产生 BOOL 差异；开启保护将原生内部损坏/非法索引裸访问转为明确拒绝，不应为了关闭保护复现 OOB。其他表中 Unknown 仍阻止宣称所有保护语义兼容；本轮没有实现 consumer，也没有宣称保护 RedGreen。

## 运行与合同
四字节 BOOL，Winapi/候选 Stdcall int，0=false，任意非零=true；不关闭 CLR bounds。x86 Release NativeAOT 已真实发布并运行官方10.0.9同架构对照，详见[本轮证据](completed-work/bounds-protection-current-validation.md)。main 与实际 servicing commit `901ca941248413c79832d2fdbd709da0c4386353` 的差异未闭，不能以 main 的1MiB限制覆盖实测。
