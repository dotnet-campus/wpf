# 原版实验：相对刷子mask与null pen输入核对

新增WpfReference.WhenRelativeBitmapStrokeIsMaskedThenOriginalHalfAlphaIsRecorded，2×1 Pbgra32源第一像素透明、第二不透明/半透明红，96DPI，relative viewport/viewbox(0,0,1,1)、Stretch.Fill、Left/Top、None、Unspecified scaling，目标Rect(1,0,1,1)，PushOpacity(1)，pen2/miter10、有brush或null brush，Visual mask或scoped mask。录制8组完整输入输出。

定向1/1通过，原版证据Tests/WpfGfxShape.WpfReference/TestResults/relative-half-alpha-stroke-0571c8d831dc44af9b45d1992f30472d.json。原版DLL SHA256501A1CD67D51BB1663870B12AF0F8C0BA29B3D37B8B509BE896D2AAF32DB214A。

明确事实：不透明源+fill+null pen brush+Visual mask第二pixel红/alpha64，第一pixel0，即既有0x40400000预期成立；不是半透明stroke，null pen brush不产生stroke。相同不透明源有brush pen输出96；半透明源fill+stroke输出28，三种输入不可混用。上一轮将两个失败笼统称半透明stroke不准确，应逐参数核对。

初版b666d0604996472aa44667add1f2fefc只录制半透明源有brush pen得28，输入不对应旧null-pen失败，不能用于改旧预期。修正版每sample明确SourcePixelsBgra、NullPenBrush、TranslucentSource、HasFill和ScopedMask；顶层SourcePixelsBgra为初始源，不应用它覆盖sample独立源。

本轮未发布Native AOT、未改生产或既有断言，仅运行新原版定向实验；没有COM全量新结果，最后COM基线仍896通过1失败。至少null pen失败已证明直连128是实际偏差，不可改预期为128。另一项需完整核对，尤其mask sizing bounds与fill+stroke原生调用顺序。

下一闭环针对relative bounds：对null pen branch记录fill bounds和mask scope bounds，核对直连改变primitive bounder的影响；联动scene realization、独立source clip几何裁剪、mask层级后以原版64/实际严格比较验收，保持HDR和1e-7约束。
