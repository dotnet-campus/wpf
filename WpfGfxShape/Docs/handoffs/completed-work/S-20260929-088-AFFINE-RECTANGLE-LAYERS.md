# S-20260929-088：仿射图像与矩形几何层联合推进（完整模块未关闭）

## 实际实现

合并前两轮尚未归档的仿射实现与本轮几何层接线：

- GeneratedImageTransform保留六参数仿射矩阵；Visual/render-data快照不再提前丢失非对角分量。负缩放、旋转、斜切进入仿射图像路径。
- SoftwareImageCoverage对矩形四边形执行28.4量化及8×8子扫描覆盖计数；Direct3D9SoftwareImageRenderer.Affine复用WIC预滤波/转换、双线性采样与扫描输出。不是完整路径光栅器；任意曲线、路径填充规则和原生差分未完成。
- 本轮GeneratedImageClip对非整数/仿射RectangleGeometry生成覆盖遮罩；Visual与PushClip通过嵌套中间层，结束时对整组结果应用遮罩。整数正向轴对齐裁剪继续走原快速路径。
- 透明层栈持有不可变遮罩快照，失败时未结束层丢弃；Pop结束几何层并恢复变换/整数裁剪。

## 原生依据

- core/common/InternalRT.h:111-164：BeginLayer几何遮罩/透明度、EndLayer整组合成、EndAndIgnoreAllLayers失败清理。
- core/sw/swlib/aarasterizer.cpp:1250-1690、2950附近：28.4量化、半像素调整、AA缩放及边扫描。
- core/sw/aacoverage.h：8×8覆盖率。
- core/sw/swlib/swrast.cpp:605起、940起：路径及色源消费。

以上不证明实现了IRenderTargetInternal多重继承ABI或CContextState布局。

## 像素验收

既有Composition参数化3项中追加准确断言：负缩放、90度旋转、斜切、半像素图像；非整数Visual裁剪；半覆盖裁剪叠加0.5透明层和重叠图元；层内NaN失败不污染目标；几何自身斜切；非整数PushClip/Pop恢复完整后续绘制。

前轮新增斜切后，NaN失败断言误用了更早的旋转帧。上一轮已改为保持最近斜切帧，未改生产行为。

本轮先添加非整数裁剪测试，旧DLL定向失败；新DLL该断言通过。中间失败日志未独立归档，最终TRX覆盖它们。最终全量124通过、0失败、0跳过，237ms；总数不增加，因为扩展既有用例。

## 发布记录

### 前轮仿射发布（补记）

命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

当轮工具返回成功；中间定向通过后增加测试，后续全量124通过。其独立SHA256/TRX未保存，不能使用本轮哈希替代。

### 本轮发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

项目Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64，结果成功。
产物：`artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
最终TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`。
对应SHA256：`9D5FBEAFB11935E3B93AA80B1615D65443C8E33EFBBB1A22BC11841A87ACD1FA`。

## 未闭合合同

仍未完成用户要求的大模块；没有外部工具阻塞。当前软件消费仍通过位图锁而不是原生内部目标/上下文合同。任意路径/圆角/椭圆/组合几何、动画、IWGX-only反向包装、source-only联合Composition、活动Render真实回调重入、完整资源缓存和原生差分矩阵仍缺。

只更新已证实能力，不提高Ledger完成状态；阶段4Active，不能替换原DLL。下一动作仍为完整消费者/上下文与Compose闭环，不将本矩形层子集作为模块交付。
