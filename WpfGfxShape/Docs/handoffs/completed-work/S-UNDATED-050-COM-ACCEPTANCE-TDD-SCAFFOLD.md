# 独立生产 COM 验收测试先行

用户要求采用 TDD，生产 COM 验收可以要求先 publish AOT DLL，并独立于普通逻辑测试。新增 ComAcceptance/ComAcceptance.Host 两个无生产程序集引用的项目与独立 slnx；未加入普通解决方案，不自动发布。

原生合同核对：MILCreateEventProxy 在 core/api/exports.cpp:895-910；IMILEventProxy IID 与四槽 vtable 在 include/wgx_render.h:82,852-859；导出表 core/dll/wpfgfx.def。Shutdown 不在公开 COM 接口，不虚构槽位。

先行测试包括空参数、QI identity、未知 IID、callback HRESULT/bytes、引用与最终 disposal。读取显式 WPFGFX_COM_AOT_DLL；PE 检查排除托管 DLL，SHA256 在父/子进程复核；每个场景单独 Host，超时终止进程树。不假设动态卸载安全。

验证：独立解决方案构建成功；六场景实跑全部因缺少 WPFGFX_COM_AOT_DLL 失败。该结果仅为发布前置门禁 Red，不是生产导出 Red。未执行 publish、未运行真实 DLL 的 COM 场景、未修改生产实现；不能标记生产 COM 测试通过。

后续先在本次提交的新 AOT 发布产物上取得具体 Red（例如 Host 10 缺导出），再实现 MILCreateEventProxy/IMILEventProxy 并重复发布验证 Green。操作约定、退出码、限制见 Tests/WpfGfxShape.ComAcceptance/README.md。

生产发布来源不能仅由 PE 外形证明，CI 必须保留当前提交的 publish 日志；真实播放/STA/异常封锁/进程退出等后续合同仍未覆盖。此前 StateThread 生命周期待修复项未因本次测试优先级调整而解决。
