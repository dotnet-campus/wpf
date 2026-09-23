# Mesh 3D shader/fixed-function 生产集成闭环

## 原生证据

- `hwsurfrt.cpp` 的 `CHwSurfaceRenderTarget::DrawMesh3D` 要求已经处于 3D 区间；空 bounds 或禁用 3D 时直接成功。
- 原生顺序为 brush realization、EnsureState、current clip/projected mesh state、visible 判定、derive hardware shader、shader `DrawMesh3D`，最后释放 hardware shader；非可逆矩阵归一化为成功。
- projected mesh 可见时才创建 brush context/shader。mesh bounds debug draw 不覆盖主绘制结果。
- 当前托管 `Direct3D9DerivedMeshShader` 和 `Direct3D9MeshShaderRenderer` 已承载 begin、shader capability、shader draw、fixed-function fallback、finish 与 release 生命周期。

## 托管实现

- 新增 `Direct3D9ProductionMeshDrawOperations`，集中保存 brush realization、projected mesh state、derived shader、可选 state sender 与 debug bounds 操作。
- 新增 `Direct3D9SurfaceRenderTarget.ProductionDrawMesh3D`，显式要求 `_in3D`；不在 Begin3D/End3D 区间时返回 `InvalidCallHResult`，且不触发任何生产依赖。
- 抽取 `DrawMesh3DCore`，让既有入口继续使用真实 `EnsureState`，production adapter 可注入同契约 state sender 用于强类型组合和隔离测试，不改变默认生产行为。
- production 入口复用既有 bounds/3D-disabled、brush realization、clip、projected state、visible、shader derive、shader/fixed-function renderer、debug bounds、non-invertible normalization 和 display completion 逻辑。
- derived shader 创建成功后由 `using` 确定性释放；shader draw 失败时按现有 mesh renderer 进入 fixed-function，finish 始终执行且不覆盖已有失败。

## 所有权与错误语义

- brush/state/projected-state 任一步失败均传播首错并阻止 shader 创建。
- invisible mesh 成功且不 derive shader、不 draw。
- derive 失败时释放可能返回的部分 shader；成功但空 shader 映射为 internal error。
- shader/fixed-function 执行结束后始终 finish，再释放 derived shader；debug bounds draw 使用忽略失败语义。
- 非可逆矩阵继续按原生规则归一化为成功。

## 验证

- 新增 production mesh 定向测试 4 个：shader success、shader failure 后 fixed-function、invisible mesh、非 3D 状态短路。
- production mesh 定向测试：4/4。
- 全量主测试：3954/3954。
- ABI 测试：8/8。
- Debug 解决方案构建：0 警告、0 错误。

## 未扩展边界

未新增 production ABI/导出，未推测 mesh、shader、material 或 brush 私有布局；真实 material、geometry renderer、vertex/index buffer 与 shader bytecode 继续由已有强类型组件和委托边界提供。未隐式开启 scene/Begin3D，未扩展 UCE、完整 effects 或普通 scene 生命周期。