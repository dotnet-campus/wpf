# BT：BR fill 专用整数实现独立审查

## 1. 裁决与所有权

**存在阻断缺陷：当前补丁不能作为全部 fill 路径的已验收修复交付。** 普通 PathGeometry 的分阶段 float 与设备整数细分有真实有限改善证据，值得保留为修复基础；但 rounded/ellipse/矩形本地变换没有进入相同 float 合同，见第3节精确反例。不是判定整个整数 HFD 已被证伪，也不是要求穷尽未来输入。建议 BR 修正这些 fill 专属入口后再作有界验收；BT 不改代码、不替 BR 撤回或提交补丁。

BT 仅使用专用读取、目录、搜索和本报告写入，无终端、进程执行、直接/间接脚本、构建发布或新测试运行。主代理只监督；未改生产、测试、BR 报告或共享交接。根为用户指定 WpfGfxShape/WpfGfxShape；下文生产/证据路径相对此根，官方源码路径相对上一层工作区。报告是读取期间快照，不绑定另一个 BR 后续修改。

已亲读执行规则、完整替换方案1–120（截断处分段补齐）、BQ最新曲线裁决1–22、交接最新节及 BR implementation 报告全文；该报告仍写进行中、禁止交付。AGENTS 名称搜索未找到文件。读取 net10.0/Nullable、两层 global.json、Directory.Build.props/targets，不改SDK/配置。

## 2. fill/stroke隔离与实际调用

GeneratedNativeStroke.cs:23–32：TryCaptureFill 显式 fillFloat:true；TryCapture、SoftwareGroupStrokeContours.TryRead、SoftwareStrokeContours.TryRead 的新参数均默认 false。GeneratedVisualRenderer.cs:846 确实使用 TryCaptureFill，而不是只修改备用覆盖入口。SoftwareStrokeContours.cs:17–21、38–44 的 float 变换只在显式 fill 分支启用；普通 stroke 默认仍走原 double Apply 后转 float。

真实消费者为 GeneratedVisualRenderer → GeneratedNativeStroke → NativeGeneratedPath → BitmapRenderTargetExports.Drawing.cs:284–289 → NativeShapeReader.Capture。NativeShapeReader.cs:28–58 对起点、line、cubic 都先 TryDevice；cubic 调新 Flatten，而不是预量化后交旧 double flattener。两个 NativeShapeReader 调用位置已文本核对。按符号查 SoftwareFillBezier、TryCaptureFill 的索引均返回空，未冒称获得完整 Roslyn 引用闭包；以实际名称搜索与逐文件阅读补足上述调用链。

这证明默认参数隔离成立，不证明所有 stroke 字节不变。NativeShapeReader 是 fill 消费者，且 line 也改变为设备量化，不能声称“生产只改 cubic”。当前 sibling 的 stroke 与官方仍不同，不能仅凭默认 false 认定回归，也不能据该差异认定 BR 导致：还缺相同场景的补丁前候选对照。

## 3. 阻断：曲线兄弟与矩形本地变换没有遵守float阶段

精确位置：SoftwareGroupStrokeContours.cs:87–95 的 rectangle/ellipse 分支未传 fillFloat；SoftwareCurvedStrokeContours.cs:20–25、61–65、79–83 仍调用 GeneratedImageTransform.Apply，再将结果转 float。Apply 在 GeneratedImageTransform.cs:35–36 全为 double；不同于 SoftwareFillBezier.TransformLocal:14–17 先将矩阵转 float、逐乘加以 float 保存。group parent 的54/62行合成和103/116行点变换也仍为 double，不能把一次 geometry local + drawing outer 分离器推广到多层 GeometryGroup。

官方证据：core/resources/ellipsegeometry.cpp:81–90 获取 CMILMatrix 后变换 shape；core/geometry/shape.cpp:787–795 → figure.cpp:754–759 → core/common/BaseMatrix.cpp:252–278，对 MilPoint2F 用 float 矩阵/float乘加。PathGeometryWrapper.cpp:1098–1100 与 geometry/utils.cpp:164–178 同样在本地保存 float 点。rounded 构点 figure.cpp:1038–1070 还使用 float rect、半径与逐项加减，而候选 SoftwareCurvedStrokeContours.cs:70–83 在 double 中生成后才转 float。这些是明确算术合同不同，不是泛泛怀疑 FMA。

最小可精确推导的入口反例（**源码数值反例，BT未实跑像素，不伪称官方运行**）：零圆角 RectangleGeometry，Rect=(1,1,2^-20,1)；本地矩阵 M11=1+2^-24、M22=1、Dx=-1，其余0；drawing父矩阵为identity仅将 M11改为2^20。IEEE float 中 1+2^-24 正好落在1与下一float的中点，舍入到1。官方本地左右点保存为0和2^-20；候选61–65行的 double Apply 后转float，左点保存2^-24，右点保存2^-20+2^-24。因此父设备坐标分别为官方[0,1]、候选[1/16,17/16]；全为有限合法小设备坐标。之后28.4左坐标由-8变为-7，右坐标由8变为9。它已证明输入整数边不同；精确最终BGRA差多少须由 BR 独立官方对照，不在此虚构。

同一矩阵用 ellipse 的右端点(1,0)即可得到本地0与2^-24的差；group parent double同样不应被目前D/F证据认证。建议在 fillFloat 分支统一这些入口的本地float保存和每层矩阵阶段，保留默认stroke路径，针对上述窄矩形及ellipse/rounded非整数矩阵增加真实官方零阈值回归。rounded构点的double/float差另需缩减样本，不把它直接称为现有像素失败根因。

## 4. 28.4量化

SoftwareFillBezier.cs:20–32 对应官方 sw/swlib/aarasterizer.cpp:2955–2963：float父矩阵，先translation减0.5，再整矩阵乘16；1273–1333 对 float 点乘加、比较范围、Round。aacoverage.h:25 的 c_nShift=3，所以官方范围恰为[-8388608,+8388608]，候选不是随意cap。否定式有序比较拒绝NaN和正负无穷，范围内int转换安全。

官方 common/shared/real.h:686–700 规定 half integers up；候选 floor((double)rx+0.5) 负半格-0.5→0、-1.5→-1，与该合同一致，不是AwayFromZero。用double做加0.5避免float在整数舍入边界附近再次丢精度；极小float相加未必逐位精确，但不改变这里的整数舍入结果。未发现此处明确舍入缺陷；x86 FPU细节不由表达式相似即作全输入运行认证。

NativeShapeReader 遇超界返回 InvalidArgumentHResult；官方 BADNUMBER/overflow后上层是否绘制空及是否继续后续drawing，尚未亲核完整对照，不能宣布已等价，也不单因HRESULT数值差独立阻塞。应补边界内/边界外和随后有效绘制的有界行为检查，而非任意无效输入穷举。

## 5. 整数HFD逐项审查

SoftwareFillBezier.cs:38–55 的基变换、带符号long右移、/18向零截断，与 bezier.h:87–166、bezier.cpp:377–462 相应公式一致。C# long的 >> 为算术右移；不存在将负数误用逻辑右移的证据。narrow实际用long承载，但65–66行的空间<16384、误差<1536匹配官方bezier.cpp:86–89、189–248的32位选路。初始分数10、lazy shift每次+2、steady +3、Value(13)匹配官方，不应误改成28。

narrow首步在输出前执行，输出后先判断剩余步数，再halve/double并step；对应官方264–318。wide用28分数、highTolerance=196608<<28、lowTolerance=3<<31；前者等于官方bezier.h:273的表达式，后者等于287行。高层反变换重建low、high先推进、low逐步输出的顺序对应bezier.cpp:495–620。Parent返回误差/4，Adjust的tolerance/4与官方64位vParentError完整乘4后比较等价。官方CMILBezier为32失败才64，候选同样分流。

端点123行补d对应bezier.h:344–348明确提示，不能据它指控多画一条错误线。NativeShapeReader把各figure交TryCreateFigures，fill的隐式闭合不依赖stroke Closed；未发现本次新增开放路径闭合错误的具体证据。

溢出/终止：真实入口先限制每坐标±2^23，所以offset差≤2^24，基础误差≤12*2^24，移28后≤3*2^54，离long溢出仍有空间；Controls中的6/12倍E1也有相应余量。narrow误差初始<1536，lazy不到无界shift。wide初始误差/high阈值≤1024，每次减半参数会至少将误差约除4，高层初始分裂至多约5级；low再次局部降误差，不是按设备坐标逐像素死循环。Adjust仅在正剩余步数调用，故未见steps=0时doubling循环的实际路径。这里是入口范围下的算术检查，不是完整形式证明，也不把IEnumerable裸传任意int当已支持输入。未发现可证实的checked/unchecked溢出根因；不得仅因未显式unchecked就指控异常。没有新增人为work cap；缺官方clip外跳过细分优化只记工作量/覆盖差异，不无证上升为卡死或性能验收门。

## 6. 原始运行证据亲读：不以源码代实跑

共同根 R=artifacts/br-fill-integer/。x64父 R/trial-run-x64/TestResults/br-trial-x64.trx 全26行亲读：1执行/1通过/0失败，Completed，error/timeout/aborted/notExecuted等0；task cf015b94bd184cbe8eb43c4eb0c66dc7，TestRun a58e6a90-251a-4848-84e5-d9b3d1b66b8f。此TRX无数值进程退出字段，BT不填退出0。

业务 X=R/trial-run-x64/Results/curve-an/9b1fa19a674945d59127c22954983441/。comparison.json全文：十帧每帧count0、threshold0、exact=true、hardwareVerified=false。be-device-summary.json:1–84亲读：drawing fill双方5模式全0且非空。separator-summary.jsonl:65–71及133–134亲读：B/E仍有30的判别力，geometry匹配E、drawing/visual匹配B，real candidate fill=0、line=0；D/F仍差8，nested real candidate=0、对D0/对F8。stroke real分别可见110/140/146/118差，保留而不冒判BR根因。不是全部160行审查或全部BGRA重新解码。

X双方identity与trial-run-x64/Results/aot-identity全文亲读：.NET10.0.13，X64/8，官方10,0,1326,50504 commit4099f83977f4d29604a9aeb971fb0a7bc1d5f699；实际官方SHA AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1；候选SHA 2C7529F5BC1DB2BA851DFA58703F2FEDC0815E6FBD96604BE9050B009ACAB57C。requested=actual path，均RTB软件，非HW。aot Arguments是candidate角色，不当父测试过滤器证明。

独立oracle：R/accept-oracle-x64/Results/br-x64-oracle/{supervision.json,TestResults/child.trx}和x86对应已亲读supervision全文、TRX结果段；各5执行/5通过/0失败，其余0；监督均ExitCode0、非timeout/cancel/kill。x64 task eb8bff94758b4cb4a8e72d3d8d203987，测试SHA 447C1AB7D5D380C385DF9B881D0DCD62F235F520293CAE203AA1DF792B70CE8F、MVID5659e331-fffc-4da0-8444-60d6c1dcb3da；x86 task3ccfdc140f884eb991d16b164e84c81a，测试SHA4A5A8AA986C0791833D61B1B565A7038D9FD67B966B20EE9D37B2C219FE6DA73、MVIDad21e91b-2b33-4d87-b746-ccdb85268101。这是受监督oracle进程结果，不累加为生产fill父矩阵。

较早R/oracle-run-final-x64/Results/br-integer-oracle/的5个evidence均搜索到differences=[]/nonempty=true，并读取0号85–99与3号控制点。其测试SHA C5FB92163740811ED75EA0353F478937B847B66702FD8A35487D9B26BD02466F、MVID695bc5a1-44c6-48c5-85f6-7bc74fa19e4f、task8618e72c3fd2475ea7e64f2058ecf2b3，与accept不同，不能混绑。

## 7. x86与兄弟证据最新已到达范围

不等待、不重跑：读取时x86已出现业务结果，不再写完全未运行。Y=R/accept-curve-x86/Results/curve-an/d8425666dd6749c38ab7f15b3e94e163/，comparison全文十帧全0，双方identity全文：taskdcf7f0909cf14639b09d767b86d15e2c，X86/.NET10.0.13；候选SHA6A681983165861B1B5C2596EBC35A7C8A4832F5A938C41E4A6C74152185C38F4，官方SHA5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834。BT未取得此项父完成TRX/数值退出，不把comparison代父验收；x86 B/E/D/F切片本包未回读。

兄弟业务根：R/accept-siblings-x64/Results/bm-siblings/8223cfc98c174a718c67465af55a7fd5/，x86为accept-siblings-x86/Results/bm-siblings/5cd2614966da4154bebf7a769a596f4a/。两comparison及四角色identity全文亲读：arc/cubic/ellipse/rounded各identity/nested共8 fill均0且非空；8 stroke差依次128/122、108/120、190/200、104/94，双架构相同，exact=false/threshold0。实际模块SHA为上述各架构候选/官方，tests为accept-oracle各架构SHA/MVID；task分别9f6f6310975e4201bf8fd4b0318be4f0、546f30b05d164a228a43778597ba022d。BT未亲核兄弟父退出，不填值；也未取得补丁前候选相同8 stroke缓冲，故stroke“不回归”运行验收仍未闭。

这些有限identity/nested兄弟样本没有第3节可区分float与double的矩阵，零差不能消除该缺陷。

## 8. 新默认测试与显式实验边界

BrFillIntegerTests.cs全文已读：public TestClass/TestMethod五DataRow，默认发现；官方RTB cubic与测试侧整数折线作零阈值比较，无skip、golden或放宽阈值。测试侧复制整数算法，不调用生产SoftwareFillBezier，因此支持独立官方oracle，但不直接证明生产挂接及每处公式同步；真实生产需前述curve父测试互补。nonempty只落盘69行，78行仅断差数0：建议补非空断言，避免以后空白双绿。其STA Join超时不取消线程的遗留工作是测试生命周期边界，不据此指控本次5例挂死。

BmFillControlPointTests仍internal、无发现属性，NativeRun.cs:16–19显式同exe --bm-fill-control-points入口，保留16场景的exact/非空与最终Assert，不是默认skip；结果确实仍false，没有通过删除stroke制造Green。五oracle和原十帧既不是一般整数HFD证明，也不是全fill/未来runtime等价证明。

## 9. 可保留范围与尚缺验收

可保留的证据结论：当前x64普通PathGeometry阶段分离器与原十帧有界改善；新整数选路、移位、阈值及端点处理未发现具体反例。不能交付结论：所有fill入口精度已一致、stroke回归闭合、边界/多层group/rounded构点已验收、最终两正式项目双架构full已过。

第3节是具体有效输入算术阻断，不是要求穷尽输入；先用小型独立对照修复并回归即可。剩余边界为x86生产父TRX/退出及分离器回读、补丁前stroke对照、边界行为及多层矩阵阶段、真正大跨度HFD/clip路径。有限5例、十帧和8个兄弟fill零差不足关闭这些范围，但也不得无证声称整数算法错误、overflow或长期卡死已经发生。

BT不重新计算全部二进制hash/像素，也没有完整版本控制dirty清单；上述SHA均来自实际进程落盘identity，非本轮独立重算。最终源码与候选二进制绑定及后续BR修复验收由执行者补齐。本报告写入后分段全文回读；文档完成不代表完整替换完成。
