# wpfgfx 迁移分轮路线图

> 用途：把总计划转换成后续对话可领取的唯一主工作包  
> 规则：轮次编号只是交接顺序，不是时间估计；实际下一轮始终以 `next-session-handoff.md` 为准

## 1. 分轮原则

- 每轮只有一个主工作包；
- 每轮开始先读取章程、交接和相关专题；
- 每轮结束必须更新交接；
- 机制 spike 与生产翻译分开；
- 纯逻辑叶子每轮 2-5 个紧密文件，有状态文件 1-3 个，高风险枢纽 1 个；
- 发现未满足前置时，当前包保持 `Blocked`，下一轮先领取前置，不扩大范围；
- 下表中的“候选后续轮”必须在前一轮证据更新后重新确认，不自动领取。

## 2. 已锁定的前五轮

### 第 1 轮：`WP-00A-BUILD-ISOLATION-AND-PROBE-SCAFFOLD`

只创建：

- 局部 `Directory.Build.props/targets`；
- 独立 solution；
- 一个 .NET 10 Native AOT 生产项目；
- 一个独立测试项目；
- `common/core/shared` 空目录边界；
- 非生产 ABI probe 与 native caller/test 承载；
- 隔离输出目录。

允许验证：restore、普通 build、托管内部测试、项目图和 MSBuild 导入隔离。

不做：任何 Native AOT publish、PE/export 检查、动态/静态 native call、Silk.NET 引用、生产导出、任何 wpfgfx 文件翻译。

停止点：隔离项目结构、probe/caller 承载和普通 build/test/导入证据已创建并验证到本轮环境允许的程度；下一唯一工作包固定为 `WP-00B-NATIVEAOT-ABI-PROBE-X64`。

### 第 2 轮：`WP-00B-NATIVEAOT-ABI-PROBE-X64`

- x64 Debug/Release publish；
- PE、DLL/LIB/EXP/PDB 和精确 export；
- 动态/静态 native caller；
- success/error/managed-exception/post-exception；
- 并发首次/重复调用；
- analyzer warning 清零，不压制。

停止点：`E2E-00` x64 通过，且明确只证明 probe。

### 第 3 轮：`WP-00C-ARCHITECTURE-AND-X86-DECISION`

- 官方/目标 SDK Native AOT 平台支持证据；
- ARM64 Debug/Release 在真实 ARM64 Windows 运行；
- x86 restore/publish/PE/stdcall 支持裁决；
- 不支持时形成用户决策请求。

停止点：架构矩阵状态明确，不继续 linker/COM 工作。

### 第 4 轮：`WP-00H-ORIGINAL-BINARY-BASELINE`

- 冻结原 wpfgfx 各可用架构/配置工件；
- export/import/resource/version/symbol/hash；
- 106/107/99/8/7 与 `g_fNoMeterChecks` 实际状态；
- loader、版本和最小生命周期基线。

停止点：candidate 的所有后续差分有权威原工件与 manifest。

### 第 5 轮：`WP-00I-MACHINE-READABLE-LEDGER`

- 建立机器可读 Ledger；
- 合并 24 项目/461 direct ClCompile 下限与磁盘、PCH、生成、资源、ABI、构建、外部供应视图；
- 创建项目哨兵、生成族、循环组和首批文件映射；
- 不翻译生产代码。

停止点：迁移分母可校验，首批批次 1-3 文件工作包可以由 Ledger 派生。

## 3. 批次 0 其它独立轮次

推荐在首个生产文件之前完成或明确阻塞：

- `WP-00D-LINKER-RESOURCE-VERSION-SPIKE`；
- `WP-00E-COM-VTABLE-SPIKE`；
- `WP-00F-CALLBACK-LIFECYCLE-UNLOAD-SPIKE`；
- `WP-00G-SILKNET-ADOPTION-SPIKE`。

具体顺序由 P0 结果和首批生产文件依赖决定。不要把四者合并。

## 4. 批次 1-3 分轮模式

### 批次 1

候选轮次按类型族：

1. HRESULT/WGX error 与版本常量；
2. 固定宽度标量、BOOL/bool 与 enum；
3. 三类 handle 和 pointer/UInt64 槽；
4. GUID/RECT/基础数学/矩阵布局；
5. stream/event descriptor；
6. AV packet；
7. command 基础 header。

每轮必须包含 native/managed layout probe。

### 批次 2

按物理文件/生命周期职责：

1. UtilLib 内存/进程堆；
2. 断言/字符串/registry/timer 等必要文件；
3. DebugLib；
4. DllUtil data；
5. `dllmain.cxx`；
6. `dllmainimpl.cxx` 与生命周期差分。

### 批次 3

按 1-3 文件小包：COM/refcount、锁、数组/region、pixel format、CPU feature、resource cache、DynamicCall。每包都需原生差分。

## 5. 首个真实源码轮次

`WP-04A-EXACT-ARITHMETIC` 已锁定为首个真实实现候选，但只有：

- 与其直接相关的批次 0 门通过；
- 批次 1 固定宽度类型可用；
- 批次 2-3 的断言/内存承载可用；
- 原生差分入口存在；

才可领取。

该轮只迁移 `ExactArithmetic.cpp` 及配对声明映射，不迁移 `LineSegmentIntersection` 或其它 geometry 文件，不使用 `BigInteger`，不优化循环。

## 6. 后续大域分轮

### 批次 4

按叶子算法族逐包；完成一个包后重新查询 Ledger，不自动按目录顺序吞并。

### 批次 5

Platform 能力/锁/内存 → Compiler 小文件 → Collector 值族 → PixelShader。任何 FXJIT 可行性阻塞先升级用户裁决。

### 批次 6

core math、loader/display/RenderOptions、control、targets、glyph 分开；生命周期参与者每次接入都扩展关闭测试。

### 批次 7

以生成族为工作包，不能按单个输出文件领取。每包至少含模型、emitter、原/托管输出、producer、consumer 和字节测试。

### 批次 8

严格按 `8A` 到 `8F`；resources/UCE 不允许一次整目录提交。

### 批次 9-12

SW、HW、AV、meta/API 均按 Ledger 的物理文件和高风险边界拆包。HW 前必须有 Silk.NET 裁决；AV 不能把 DXVA 覆盖误当媒体栈覆盖。

### 批次 13-14

只做完整 ABI/资源/生命周期和 E2E 收口，不在此时首次补写早期应有的测试或 startup/shutdown。

## 7. 每轮结束模板

每轮必须在 `next-session-handoff.md` 写清：

- 当前/已完成工作包；
- Ledger ID 和状态变化；
- 修改文件；
- 已运行证据与结果；
- 未运行证据及原因；
- 新依赖/阻塞/用户裁决；
- 下一唯一工作包；
- 下一轮第一动作；
- 下一轮停止点；
- 逐文件直译、禁止重构提醒。
