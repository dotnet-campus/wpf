# S-20260930-139：相对与绝对画刷变换顺序验收

仍处五步计划第1步。核对BrushTypeUtils.cpp:47-115，相对变换先按内容边界换算，再施加绝对变换。新增内容原点x=1、宽1，RelativeTransform水平翻转、Transform平移0.25的非交换组合，显式最近邻隔离采样影响。Visual与PushOpacityMask作用域入口均验证右侧0xbfbf0000、Pop后左红，并保留锁失败恢复及源引用释放后保活。

初始测试使用默认预滤却按最近邻期望，得到0x60600000；明确设置最近邻后通过，未修改生产计算，不作为生产Red→Green。嵌套Visual变换尚未独立新增。

本轮仅测试和文档，无生产修改/发布。定向9项及全量164通过、0失败、0跳过，226ms。TRX Tests/WpfGfxShape.ComAcceptance/TestResults/com-acceptance.trx。方案补入五步实时状态；带clip边界仍缺，第1步未关闭。
