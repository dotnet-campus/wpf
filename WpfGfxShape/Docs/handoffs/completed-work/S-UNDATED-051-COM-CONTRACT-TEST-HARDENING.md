# COM 验收合同补强（测试先行）

原生依据：core/av/eventproxy.cpp:225-266，QI 先清空输出、支持 IID 时 AddRef，Init 按值复制 descriptor。

修改独立 ComAcceptance.Host/测试项目：

- unknown-iid 输出由初始零改为 -1 哨兵，避免漏写输出仍通过；不释放未被改写的哨兵。
- 新增 query-null-output，验证 E_POINTER。
- 新增 descriptor-copy，创建后清空调用方 descriptor，要求 callback/dispose 使用对象副本。
- 同步独立测试方法文档的八场景与退出码 28。

实际验证：独立解决方案 Debug 构建成功；八个场景全部在 WPFGFX_COM_AOT_DLL 缺失处失败，0 通过、8 失败。没有进入真实 DLL COM 调用，不能记为具体合同 Red 或 ABI Green。

未修改生产实现、未发布或使用旧缓存 DLL。下一步仍为取得当前源码的发布产物，运行八场景得到具体 Red，再实现生产 MILCreateEventProxy/IMILEventProxy。当前环境门禁阻塞不因测试扩展而解除。普通逻辑测试不触发 publish。
