# S-20260929-094：跨目标活动回调与最终释放验收

## 反思与事实

此前多轮把模块内部小步骤当作停止点，持续推迟通用内部目标/上下文，未遵守完整模块执行要求；工具不可用的说法无依据。本次仍未完成整个模块，不能把测试增加作为完整交付。

## 原生依据

core/uce/rendertargetmanager.cpp:325-353按目标顺序逐个Render并处理错误，不存在整帧所有目标冻结旧资源的合同。因此验收当前目标保活、后续目标观察已提交更新，不添加全局不可变帧语义。

## 实际变更

只修改测试：CompositionTests.ActiveRender新增两个场景：
- 第一个目标读取WIC像素时回调提交源替换，第一个目标保持旧像素，第二个目标使用新像素。
- 同样回调提交后，第二目标写锁冲突返回原HRESULT；其像素保持空，解锁重试成功。

ManagedStreamTests.Lifecycle的调用方描述符增加可选最终释放通知，七个活动Render场景均在全部清理后验证底层生产流恰好释放一次。不是测试专用生产导出或fake COM。

## 验证

定向七项通过；全量131通过、0失败、0跳过，247ms。使用S-093已发布DLL，本轮未修改生产代码、未发布。
TRX：Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx，最终回归覆盖中间记录。

## 完成门禁

源码查询IRenderTargetInternal/CContextState仅找到SoftwareImageRenderSession中明确声明未实现ABI的注释。DrawPath接线并没有补齐通用内部目标合同。因此整个模块继续未完成；还缺完整内部目标/上下文、通用路径/曲线/组填充和IWGX-only反向包装等。阶段与替换资格不变。不再把跨目标回调全部描述为没有任何证据，但不能将这两个场景扩大为所有交错验收。
