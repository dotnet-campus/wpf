# 原版 WPF 行为基准

此项目由用户明确要求新增，独立于唯一生产 COM 验收项目；不是 ComAcceptance.Host，不引用翻译程序集，不修改解决方案，不替换系统 DLL。

## 已实现

- net10.0-windows、UseWPF、x64、MSTest STA 测试，无额外窗口或子进程。
- 根据实际 PresentationCore 所在 WindowsDesktop 目录，用绝对路径解析 DllImport。直接调用原生 MilUtility_GetTileBrushMapping，ABI 依据 core/uce/apifunc.cpp 的原生声明。
- RenderTargetBitmap 经原版 WPF 软件路径渲染。测试验证进程内 wpfgfx_cor3.dll 的加载路径与直接导入一致。
- 450 组输入：负viewbox/透明padding的sourceMode17/18，分别无变换及矩阵(0.8,0.6,-0.6,0.8,3,0)，加上原有sourceMode 22–27 的 viewport（百万平方、横/纵细长、半tile边界及负tile） × None/Tile/FlipX/FlipY/FlipXY × LowQuality/HighQuality/NearestNeighbor × fill/stroke/mask。
- 每次输出独立 TestResults/original-wpf-<GUID>.json：完整输入、绝对 viewport/viewbox、4×4 D3D 映射、4×4 Pbgra32 字节、DLL 路径/版本/SHA256、运行时和渲染模式。输出也附加至测试结果。
- 红色内部像素校准测试，用于验证读取格式与渲染链路。

## 证据边界

原版样本作为独立固定对照，COM 测试核验同格式像素投影，不根据 AOT 实测值回填预期。当前270组全图对比通过，不代表所有COM参数已经校准。

sourceMode 25–27 的 source 使用与现有 fixture 对应的 1×1 GeometryDrawing、ScRGB 红色 1.25、alpha 0.5；stroke 矩形为 (1.5,1.5,1,1)，宽度 2。已校正 viewbox units 为 Absolute；mask 为 Visual.OpacityMask 的红色矩形，与 COM 的 Visual mask 和红色 consumer 对齐。旧 original-wpf-cae82710033d408886b8f8833c9a2219.json 输入不一致，不再用于校准。当前固定证据为 original-wpf-658eb1f21ad34043beef26e8a72dc567.json，附180组不透明红实色目标control：相同目标/变换/描边、取消ImageBrush与Visual mask。control45组不同，全部source17旋转；source18全部control一致但主实验38组不同。实色control用于隔离覆盖，不能证明所有图像差异仅来自覆盖。969d旧样本的sourceMode18目标几何不等价，已停用；新样本显式记录TargetGeometry和PenThickness，COM先核验这些输入再比较像素。sourceMode18所有consumer目标为(-10,-10,20,20)，stroke厚度2。

COM 校准测试从真实 AOT bitmap lock 逐行读回全部16像素，以stride定位，转换为 Pbgra32 后与原版实测字节精确对照。先前24组差异已修复裁剪前限幅顺序；校正sourceMode18目标输入后，当前450组中364组一致、86组不一致；原有270组尺寸矩阵仍全部一致。COM全量890通过1失败，失败为保留严格比较的校准测试；这不是新稳定基线。sourceMode17/18源色为不透明线性红1，Stretch.None、Absolute viewbox，报告包含VisualTransform及分组差异计数。直接导出的Mapping为刷子到局部world映射，VisualTransform单独记录，不能混称最终device映射。报告同时保留原始浮点值；全图一致仅限当前4×4 fixture，不证明原生浮点/HDR精确一致。

RenderTargetBitmap 输出为 Pbgra32；会经历颜色转换、HDR 截断和量化。不得将其字节直接与生产 Vector4 比较，也不得用其确认或替代 float/HDR 距离 <1e-7 的断言。映射导出只证明映射参数，不证明内部中间面采样及生命周期。

后续应逐项审核共享输入描述及可比输出格式，建立 COM 输出的同格式投影对照；float/HDR 需要额外原生浮点绘制契约证据。不可仅凭颜色“看起来一致”确认预期值。

## 原版 float 存储与绘制行为探针

`WhenOriginalFloatTargetDrawsBitmapThenSrgbClampingIsRecorded` 经原版Release/x64内部DrawBitmap绘制6/10宽gradient并直接float读回，返回成功但HDR红色限幅为1、负绿色为0，蓝色呈sRGB量化采样。测试仅确认这条实测路径的限幅行为，不作为生产HDR或DrawingImage参考。现有源码SetSurface按色空间选择scRGB，而当前安装二进制实测不同，尚须核对实际版本、内部目标绑定/颜色源与ABI；不得据此修改生产float要求。

`FloatReadbackTests.WhenOriginalSoftwareFloatBitmapIsWrittenThenHdrReadbackIsExact` 直接加载已安装WindowsDesktop原版DLL，使用原生factory创建格式26软件目标，确认WIC PRGBA128bppFloat，写锁写入HDR/负分量后CopyPixels逐Vector4精确读回。此证据仅证明原版float存储/读回可达，不证明DrawingImage绘制或旋转golden。

原版Clear另行观察：原生MilColorF为D3DCOLORVALUE的RGBA顺序；当前运行时Clear输入(1.25,-0.25,0.1234567,1)读回(1,0,0.122138776,1)，出现限幅/量化，不能把Clear当成HDR精确绘制参考。测试不要求此观察值作为golden。独立报告尝试写入ReferenceResultsPath，不替换固定original-wpf样本；若工作区工具无法枚举该产物，必须标记报告尚未归档，而非声明已取得SHA256/文件身份。
