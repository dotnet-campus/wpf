# S-20261009-209：private CShape Widen尝试及安全撤回（未交付）

## 用户问题与结论

没有确认外部环境阻塞。多轮未完成既有真实MSVC输出对象构造依赖，也有此前绕开核心分支推进其他小项的执行问题。本轮直接尝试输出ABI，不把边界writer通过报告为真实创建者成功。

证据common/shared/dynarrayimpl.h:30–128、dynarray.h:568–629：数组含当前/初始指针、初始容量/容量/计数和内嵌缓冲。geometry/figure.h:455–520：CFigureData多重继承IFigureData/IFigureBuilder，points/types数组和遍历状态。shape.cpp:140–247、30–70：cached figure复用、新figure所有权及析构。strokefigure.cpp:2920–2983、3216–3275：一般sink新建CRail并AddAndTakeOwnership转移。ShapeBase.h:40–155：Widen虚方法真实CShape引用。原生未修改。

## 尝试和撤回原因

尝试Release/x64字段存储、动态数组初始化、输出轮廓读取、逆序清理；private填充成功后调用shape Widen槽，再描边消费。新增边界writer测试取得Red→Green并产生像素。

但cached CFigureData两个vtable均未真实构造，矩形优化/其他原生非虚路径可调用它们；writer直接填字段不覆盖这些调用，不能证明兼容。输出新figure还涉及原生WPFFree/删除析构的分配器合同；只由源码推算大小/偏移未取得MSVC布局验证。继续保留会有原生崩溃风险，故撤回全部生产接线、输出实现和writer测试。新增两文件内容清空（工具不支持删除），没有保留可执行试验。最终private stroke仍E_NOTIMPL，本轮没有生产完成进度，不宣布模块交付。

## 逐次发布

四次完整命令均`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`。项目WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 成功，11行日志全部已读。启动全量工具成功，统计/TRX/SHA未读取，不借用旧值。writer定向旧DLL失败39行，未读取完整错误/身份。
2. 失败57行输出截断，后续Release/win-x64 build明确CS0119 NativeWidenedShape.cs:71，enum cast与指针解引用括号问题；修复后继续。无新产物/SHA/TRX。
3. 尝试代码发布成功60行日志未读取；writer定向工具返回成功，统计/TRX/SHA未读取，不能称真实创建者验收。随后发现vtable构造缺失并撤回。
4. 撤回后发布成功60行日志未读取。全量353/353、436ms、0失败/跳过，TRX46b4d494-de8c-4f34-8975-cc6fd908893f，SHA3B5CF56F4D574D401591784E1FEE19615AB08C3EFA9BBEDD74484376981CE15B。是撤回产物，不是private Widen Green。

固定TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx，历史完整文件被覆盖未备份，只保留已读身份。没有原生创建者差分、覆盖率/E2E/Ledger验证。阶段与替换资格不变。

## 精确后续合并计划

唯一任务仍是private完整fill→Widen→stroke和真实ABI/层Compose闭环。必须先为CShapeBase、CFigureData双基类、DynArrayIA建立真实可用对象构造/方法族，确认Release/x64布局及原生分配器/删除析构合同；验收必须由真正原生方法触发cached矩形优化和一般CRail转移，不再以字段writer代替。然后消费输出并实现失败逆序清理，联合真实brush creator、Begin/End/Abort首错及Compose生命周期。不能只完成布局调查或writer测试即结束，也不能以风险伪称外部阻塞。
