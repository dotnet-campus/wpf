# S-20260929-090：变换动画值消费（完整模块未完成）

## 实际变更

GeneratedTransformResources保留Translate/Scale/Skew/Rotate的nullable动画属性槽位，不再仅依赖压缩后的非空Animations列表。CurrentValue按槽读取DoubleResource值，未绑定属性保留静态值。GeneratedImageTransform在捕获时消费当前值，MatrixTransform读取绑定的MatrixResource。已有依赖监听/引用管理不变，没有新增调度器或时间线。

原生依据：core/resources/translate.cpp:64-88先SynchronizeAnimatedFields再构造矩阵；marshal_generated.cpp:4203-4215逐属性读取m_pXAnimation/m_pYAnimation当前值。原生文件只读，未修改。

## 验收

新增Composition场景：X为静态1，仅Y动画槽绑定DoubleResource，静态Y=8，动画Y=0；准确输出右侧红像素。仅更新动画资源Y=1、无需重发变换命令，绘制移出1行目标，准确输出全空。该场景区分了空X槽和非空Y槽，不能通过压缩依赖列表替代。

旧DLL定向失败，新DLL全量124通过、0失败、0跳过，240ms。扩展既有3个参数用例，总数不增加。失败日志/TRX被最终回归覆盖，未独立存档。不将此平移验收泛称Scale/Skew/Rotate/Matrix各动画槽都已准确像素验收。

## 发布1

完整命令：`dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64`

目标Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64，结果成功。
产物：`artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
TRX：`Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`。
对应SHA256：`EA64B7D3AA6474D678DD68E478DB1EF16A63F501E182FD095AE7ABDF2C63D707`。

## 未完成

仍未达到用户要求的大模块交付，没有外部工具阻塞。原生内部目标/上下文、完整路径/曲线、GeometryGroup填充规则、几何/render-data动画、IWGX-only反向包装及活动Render回调重入未闭合。当前只消费已提交动画值，不实现时间线求值。完整模块及阶段4不关闭，不具备原DLL替换资格。
