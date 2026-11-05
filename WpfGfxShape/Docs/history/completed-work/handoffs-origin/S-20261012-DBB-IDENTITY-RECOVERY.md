# 双缓冲全格式候选身份恢复（未关闭方法族）

## 本次实际核验

直接读取当前 `Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx` 第2行及8250–8272行：RunID `624aa86c-854a-41de-9485-7ab108e55143`，1199执行、1198通过、1失败、0未执行。报告 `wpf-calibration-587e5bb812e142138b8d6bff3c82788a.json` 前13行：

- AOT SHA256：`760CB48F8365BA5004BEBDBE891364E20709A56ABF058CDEC7CD37F0451CC988`。
- 原版 SHA256：`501A1CD67D51BB1663870B12AF0F8C0BA29B3D37B8B509BE896D2AAF32DB214A`。
- 固定参考 SHA256：`98D740A895372A31DAF4EB234F3942E31A8C99A15D278F121DA84C36B585CE58`。
- float投影57/450不同，直接Pbgra32 0/180不同。投影不是float golden。

## 源码事实与验证边界

`DoubleBufferedBitmapExports.Create` 已把真实palette传入 `BitmapFormatConverter.Create`。`CompositionTests.DoubleBufferedFormats.cs` 包含28格式的真实系统WIC转换、Visual/DrawImage前缓冲消费、源替换、实际目标读锁首错及重试、销毁通道后后缓冲存活验收。

报告AOT身份与旧稳定基线相同；此事实不能证明上述源码差集已发布至该DLL。本次恢复前的发布命令、发布日志与该RunID的具体关联未取得，不能补造发布序号或把既有测试结果称作当前源码Green。需要重新发布并定向、全量验证。

原生 `doublebufferedbitmapres.cpp:185–323` 确认更新接管调用方已增加的引用；CopyForward未初始化/UseBackBuffer失败也必须通知并关闭重复事件。现有源码在CopyForward前递减dirty数量，失败不能擅自回填。

本归档仅恢复实际证据，不宣布双缓冲族Closed；复制自身失败、丢包联合生命周期和失败释放仍需真实COM验收。glyph、D3DImage、media、shader、旋转float仍未完成；阶段4 Active，不能替换原库。本恢复步骤未执行发布、构建或测试。
