# S-20260930-142：子Visual裁剪与嵌套坐标验收

第1步验收补齐：父Visual无自身内容，子Visual图像宽2、矩形clip[1,2]、scaleX=2、offsetX=-3，外层容器offsetX=1。子外边界按clip→transform→offset落在[0,2]，父相对mask准确输出左透明右红；未裁剪边界将为[-2,2]并产生不同采样。现有两项relativeViewport用例均通过。

本轮仅修改测试和方案，未发布；使用S-141产物。定向及全量164通过、0失败、0跳过，220ms，TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。

下一步为render-data旋转图元与矩形PushClip边界，需核对原生bounder使用的保守矩形语义，不能未经证据改成精确几何裁剪。五步计划未全部完成。
