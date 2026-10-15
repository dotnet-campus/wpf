# S-20261009-219：生产solid/null immediate槽合同与共享消费者（模块未关闭）

保持纯C#。NativeImmediateSolidRealization建立软件solid/null窄合同：realizer完整12槽，brush完整6槽，没有空槽；引用构造0→创建引用1，最终Release释放实例。嵌入brush AddRef/Release按LocalMILObject retail合同返回1且不销毁。SoftwareBrushRealization由真实generated solid资源创建该生产实例，通过NativeBrushRealizerReader消费Ensure→Brush→Effects；private FillInternalShape也复用该读取器。帧清理先detach实例再释放owner引用。

原生证据resources/BrushRealizer.h:40–130、BrushRealizer.cpp:880–1035、common/shared/RefCountBase.h:20–48、api/api_basebrushes.h:27–58、api/api_base.h:25–140。实现不是完整MSVC对象布局，不允许原生非虚方法直接操作；仅按已有virtual合同调用。当前IUnknown仅支持IUnknown，未验证CMILBrushSolid其他QI身份；hardware cache/meta fixup不支持。null工厂有实现但真实generated null几何仍可提前跳过，不能称null端到端验收。

## 逐次发布

两次完整命令均`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`，目标同路径Release/win-x64；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 成功11行日志未读，启动全量工具成功，统计/身份/SHA未读取，不借用旧值。
2. 成功59行日志未读，完整诊断未保存。定向258/258、187ms，TRX7a3ed20e-4017-4111-9c39-cd4e01343cf4；全量379/379、436ms、0失败/跳过，TRXe3d596e5-6bc7-4e34-9be6-b2ecec7517dd；SHA3E531C81F962EA6E5C4A7C8D065EC9D5E008CEB6550DE728A6801EBCE46B2E51。

测试项目Tests/WpfGfxShape.ComAcceptance/WpfGfxShape.ComAcceptance.csproj；筛选FullyQualifiedName~CompositionEffectsTests|FullyQualifiedName~BitmapRenderTargetTests。没有新增测试，不声称TDD Red。真实generated solid绘制消费生产创建实例，private边界回归共享读取器。历史TRX完整副本覆盖未保存，仅已读取身份摘要；最终固定文件同项目TestResults/com-acceptance.trx。SHA来自实际加载DLL。覆盖率/原生差分/E2E/Ledger未运行。

## 未满足条件与唯一下一动作

bitmap仍是帧快照，没有private bitmap realizer生产工厂；资源/即时带effect的完整合同及meta intermediates未实现。生产solid实例由generated路径创建，但尚未由真实命令进入private DrawPath target整体消费，不能将共享读取器称为完整创建者→private闭环。完整对象QI身份、ABI布局差分、释放重入/多引用及全部槽行为仍未联合验收。

下一轮继续已批准真实solid/bitmap生产命令→private消费者→首错/逆序释放/删除替换重入大闭环，优先bitmap创建者与整体private消费接线，不转向仅槽测试或再次重构。无确认外部阻塞。本轮是实际窄合同推进，不是完整模块交付；阶段与替换资格不变，不能替换原DLL。
