# AQ 曲线差异因果实验与最小修复

## 当前状态

AQ已有ignored实验产物，但此前未完成事实回填。AT亲读x86摘要：原44字节/最小cubic10字节差中，预量化double、整数步长精确求值与整数HFD均记零差；x64原始比较仍为44/10差。此为历史产物读数，不是AT重新计算原始像素。AT未完成双架构全轨迹复核，生产未修改，修复质量门未通过。详见[AT最终报告](replacement-supervised-curve-quantization-fix.md)。

范围仅 SoftwarePathCoverage、紧邻曲线展平、RoundedRectangleContractTests 及独有回归；不修改 producer、通知、上传、捕获、入口、项目或主交接。实际 GPU 圆角未覆盖，不能以软件结果替代。

## 有界执行顺序

1. 阅读当前规则、方案、rounded-rectangle/AN 报告与官方源码，确认现有精确算法及许可。
2. 读取 AN 原始证据与测试，保留同输入官方像素，设计控制点预量化与整数 HFD 的分离实验。
3. 在独有 ignored artifacts 中构建纯 C# 实验，双架构运行并回读原始差异。
4. 仅在因果证据充分时最小修改软件曲线覆盖并增加回归；否则不猜修。
5. 最后源码双架构 Release 构建、自包含正式包运行适用几何/pen/clip 回归，回读 TRX、identity、SHA 与原始像素。
6. 最终更新并回读本报告，明确未闭项和下一最小实验。

## 证据

AQ证据位于 `artifacts/curve-aq/readback/{x86-final,x64-final}/Results/curve-an/`。AT已核对两角色实际identity与frame9预量化轨迹：官方10.0.13，版本10,0,1326,50504；SHA及具体场景目录见AT报告。预期来自真实官方BezierTo曲线像素，不是候选算法自产；干预则是测试侧先flatten成折线，不能冒称官方内部HFD轨迹。仅有正数小曲线、无变换证据，负数半格/变换顺序/大曲线及兄弟stroke尚未闭合。纯C#审计器已编译发布，但沙箱传输遇10054失败，未获得全原始字节重算结果。当前生产双架构Release常规构建各0警告0错误；无新正式包/回归TRX，旧AQ SHA不绑定本轮构建。未改变 golden、skip、阈值或 fallback。GPU圆角仍未覆盖；完整替换未完成。
