# S-20261009-235：单tile可见区域与描边裁剪工作空间

## 生产推进

GeneratedImageMask在capture阶段把device clip反映射到viewport空间，判定可见窗口是否完全落在单个整数tile中。满足条件时，根据tile索引奇偶及FlipX/FlipY/FlipXY建立该tile的独立仿射变换，转为非重复可见实现；DrawingSnapshot仍保留原始PixelToDevice，按目标clip分配source中间面。bitmap source复用相同tile定位路径。窗口跨tile时保持原重复路径，不任意截断周期或修改分辨率。

描边联合修改SoftwareStrokeWidening、NativeStrokeReader和真实private/generated软件path消费者：先按原pen形成端帽/join/dash轮廓，再将轮廓变换到设备空间、按消费者clip裁剪并定点量化。bounds仍从原轮廓计算，不裁中心线替代描边，不改变brush局部bounds合同。复用S-234的闭合轮廓裁剪与非有限输入保护。

## 证据与范围

读取ViewportAlignedIntermediateRealizer.cpp 450–545（工具显示至540）：原生分别计算base tile和render bounds，并保存rendered tile到base tile的映射。当前单tile路径保留整数索引和翻转，不称为原生CalculateSurfaceSizeAndMapping全方法直译。既有AxisAlignedStripClipper证据见S-234，裁剪用于形成后的stroke轮廓。

## TDD与验收

起始710/710。新增3项真实generated→COM用例：1e10基础tile的fill/mask小可见窗口、1e10描边source。旧DLL定向137通过/3失败，均在present返回E_NOTIMPL；联合生产修改发布后140/140通过，最终全量713/713，0失败/跳过（2s）。既有二维flip/负索引、fill/mask/生命周期全部回归通过。

单tile优化没有任意固定尺寸阈值：按可见窗口是否跨周期决定。新增测试当前证明base tile索引0；非零奇偶tile映射主要由既有小tile用例回归，不称为所有超大翻转/分数情况独立证明。

## 发布记录

两次完整命令均为 `dotnet publish WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj -c Release -r win-x64`；目标WpfGfxShape/Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

1. 起始成功，11行日志，全量710/710；起始hash/TRX未独立保留，不能将旧归档Run ID冒称本次发布后测试。
2. 描边工作空间和单tile可见映射联合修改后成功，60行日志，仍有CA1416警告。定向140/140，全量713/713。SHA256 `A693A318E7D358DB75C88C38A74563045A6777878FA34A0B7EAE12954ABDE37E`；TRX `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx`，Run ID `5d2c3ad9-85b2-43f9-a42b-7d6d935ed9b3`，由TestContext确认实际加载产物。定向TRX已被全量覆盖。

## 未关闭合同

可见窗口跨两个或更多超大tile时仍走完整基础tile分配；完整采样空间像素对齐、旋转parallelogram精确裁剪、分数接缝过滤、极端曲线扁平化成本尚未关闭。此次已推进单tile实际生产路径和stroke消费方法族，不等同原批准大模块全部关闭。

下一轮应联合实现多tile可见窗口的source区域映射及像素对齐/过滤边界，合并fill/stroke/mask、奇偶/负tile生命周期验收，不默认再做单参数或纯补测。阶段4Active，Ledger RepairRequiredPartial，不能替换原库。
