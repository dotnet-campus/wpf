# 原版校准：撤回边界和精度候选

更正上一会话结尾：工具实际可用，不能称没有读取/编辑/测试能力。恢复后读取上一轮日志确认DeviceAligned Border→Extend尝试没有减少差异，撤回。另试基向量长度按float运算，亦未改善，撤回；没有保留这两项未经确认的生产修改。

## 发布对应

所有下述发布完整命令均为：dotnet publish d:\lindexi\Code\dotnetcampus\WpfLab\WpfGfxShape\WpfGfxShape\Code\WpfGfxShape\WpfGfxShape.csproj -c Release -r win-x64
目标Code/WpfGfxShape/WpfGfxShape.csproj，Release/win-x64；产物WpfGfxShape/artifacts/publish/WpfGfxShape/Release/win-x64/wpfgfx_cor3.dll。

- 前会话漏归档发布：DeviceAligned排除Border，工具返回成功59行日志；SHA256 F752DDA2CC9FEB4C951E7EAEFFD488E7341446C28D4F8DF1D869ADA7A6341FDF，对应定向报告0e31a7c480b84e55bbc783b6c9185205，86组不同、实色0不同。独立TRX Run ID未保存。
- 本轮发布1：撤回边界试验，tile基向量长度改float，退出码0、60行日志；SHA256 AF9B6B5356AE4D61E22F818EBEC4CA92A3AE0228DDE1793CAEAA6F09A0AB6810，定向报告c5265515c907416789471139ed49a2b4，仍86组不同、实色0不同。定向TRX随后覆盖，Run ID未保存。
- 本轮发布2：撤回float候选，恢复前次矩形轮廓修复后的源码逻辑，退出码0、59行日志；SHA256 2922882594988716E869AE2CE853D397FCDAA29C02C3F785271056781DECCAC8。相同逻辑重新发布hash不同，不能假定二进制可复现。

完整发布诊断未在测试覆盖前保存，不能声明无警告。最终报告Tests/WpfGfxShape.ComAcceptance/TestResults/wpf-calibration-2cbeb1cc6d8f42028d9ab7dd160b7b20.json对应发布2：450组86不同，直接8位180组86不同，格式判定差0，实色180组全部一致。全量890通过1失败0跳过、总891、2秒。
TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx；Run ID494e3469-c716-4d9b-9570-7e207e96c9da。不是新稳定基线。

原生核查：imagebrush.cpp767–801明确绘制DrawingImage到intermediate；drawingimage.cpp明确直接Draw drawing；BrushIntermediateRealizer.cpp120–219创建透明surface及新DrawingContext、clip到surface；tilebrush.cpp263–388设置base tile/mapping/wrap/source clip；swrast.cpp1106–1156先DeriveFromBitmapAndContext再按InterpolationMode采样。这些不足以证明候选边界切换或精度收窄修复，故不保留猜测。

当前实色目标覆盖问题已修复，但源中间面86组仍未定位，不能以重复计数宣布闭环完成。后续应获取原版中间面realization参数或构造独立BitmapSource对照隔离DrawingImage rasterization与最终resampling；保持严格预期、HDR和1e-7约束，不增加无证据绕过分支。
