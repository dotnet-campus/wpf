# EventProxy 测试先行覆盖扩展

仅修改单项目 Tests/WpfGfxShape.ComAcceptance 中的测试和测试方法文档；未修改生产实现、未创建 Host、未创建或修改 slnx。

原生依据 core/av/eventproxy.cpp:155-267：RaiseEvent 原样调用 descriptor callback 并保留 HRESULT；QI 先清空输出，成功 AddRef；Init 复制 descriptor；最终 Release 析构。

从 8 扩展到 23 个参数化展开后的用例：创建失败所有权、双空参数、两种 QI 引用在原创建引用释放后存活、失败 QI 无泄漏及后续可用、四类 HRESULT、零长 payload、含零/高位字节的逐字节快照及输入不变、失败后继续派发、多个对象独立、未使用对象销毁。回调仅采集数据，不在 unmanaged 边界断言。

验证：测试 csproj Debug 构建成功；运行 23 项、0 通过、23 失败，全部因现有 AOT 产物缺少 MILCreateEventProxy 在初始化处失败。SHA256 8CBF2F6EDF636728CF6B1F9C4736B705147F0036F5C2E4398ADFB8182F9E7A64。本轮没有 publish，不能证明现有产物对应当前源码。

这不是 23 个行为缺陷，也不表示方法体断言已运行；仅是共同导出前置的 Red。生产实现之后必须运行原测试得到 Green，不能降低断言。并发/重入、原生差分、真实媒体关闭与播放仍未覆盖，不声称测试全面完成。
