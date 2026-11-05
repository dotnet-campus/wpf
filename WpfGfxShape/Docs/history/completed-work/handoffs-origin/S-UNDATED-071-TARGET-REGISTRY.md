# S-UNDATED-071：共享分区目标注册引用部分实现

## 实现

GeneratedTargetRegistry 接入 SameThreadPartition、GeneratedProtocolHandleTable：GenericTargetCreate执行成功后注册并AddRef，显式删除移除一次注册，最后通道销毁释放分区剩余目标。先预留容量再AddRef以避免OOM泄漏；移除集合项后Release以避免COM回调重入时重复释放。

原生证据：core/uce/rendertargetmanager.cpp:873-933（Add/Remove）、composition.cpp:1380-1436（删除先取消一次注册）、GenericTarget_Create的ProcessCreate后注册调用。注册表是manager所有权子集，不含显示可用性通知、渲染状态通知、遍历渲染或Compose。

新增真实DLL测试：同一目标注册两次，删除资源只移除一次注册，目标应保活到分区销毁。旧DLL取得行为Red：删除后disposals实际1，预期0。

## AOT发布流水

### 发布1

- 命令：`dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`
- 项目：`WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj`；Release/win-x64。
- 结果：成功，工具报告59行发布日志；未保存独立完整日志，不声称零警告。
- 产物：`WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll`。
- 本轮验收补记：发布1后全量110项109通过/1失败/0跳过，新增目标保活用例通过；唯一失败仍缺WgxConnection_SameThreadPresent。本轮仅发布1次。
- 对应DLL SHA256：`4B5823EF3DF26859E6C0166A7024767622B725275C52C18ADEAE65CA8829FBCC`。
- TRX：`WpfGfxShape/Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`（覆盖此前Red结果）。

## 未完成

完整消费者/Compose模块仍未完成；此次只实现manager注册引用子集，不能宣称完整manager或前缓冲像素闭环。未新增SameThreadPresent空壳。下一轮仍须合并IRenderTargetInternal、实际Visual/render-data/前缓冲消费与Compose，并完成多帧像素、失败和生命周期验证。
