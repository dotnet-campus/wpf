# BitmapCache 公开 packet ABI 修正正式收尾

状态：生产修正与双架构公开合同 ClosedLimited；完整替换 Open。此终态替代原执行中草稿。

## 修正与验证
MilBitmapCacheCommand exact struct 28 bytes / payload 20，Pack=1，拒绝旧32尾padding；字段完整保留，0x84 sibling 36不改。生产真实decoder修正，不是仅诊断null-safe。Release build 3721 warnings/0 errors；50 Debug单元全部通过，不冒Release测试运行。

## 四角色实际结果
执行原件：../../artifacts/public-cache-contract/packet-fix-execution.md 及六份 packet-fix-*-receipt.md、四个 packet-fix-*-readback/Results/Results/{original,candidate}/result.json。
- x86 official task3e281b8909714f50bf442d1133eec29e，actual 5EBD18E019D7D82144EE068A7D56F47200E2D82A2121E3F3A066DAA6AC36B834。
- x86 candidate task973754123a164ba489c541d1dfce0948，actual B07C9073F75278ED61C44638EB3644547B413A281E2859B25211CEAAC3D1466C。
- x64 official taske36ab0a8301143f295add446077587d4，actual AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1。
- x64 candidate task83146a2ab92e4a498c2e31767f23cfcf，actual 6BDF1EAED4C2548FC8EECCEF205F8006B5AF81A1D44E66595CF2896E6C48E403。
四角色 VisualBrush/BitmapCacheBrush 四帧分别512/512、业务Green/JSON exit0、sceneDetached及cleanup完整。JSON exit0不等于独立OS exit；工具返回无数值exit。comparisonPendingIndependentComparator保留，不称独立diff通过；公开角色程序集SHA/MVID未知，不为补identity重跑。

## 历史与边界
旧 official472863886f344f16be92faf80d98f626 actual5EBD四512/512、candidate81f254a861ec42dba517ac99e1e775f5 actual147B的0x8D malformed/shutdown异常/cleanup未完原样保留。新公开场景不再出现原0x8D拒包，不能推断整个full无malformed。
前轮预算：normalbuild1/3、tests1/2、Nativepublish2/2、COMpublish2/3（implicit build2）、sandbox4/5；旧剩一次不并入本轮新增授权。
本轮新增默认full已执行终态：normalbuild0、Nativepublish0、源码修改0、COMpublish0（复用现clean）、sandbox2/2，各架构full一次内1500/外1800，先x64后x86无重试。x64 task41e93ce4468949a48d345f9f1cded100=1823/1783/40，x86 edf40a74d42f4f9bb38e8e0f517ea024=1823/1786/37，child均2、无timeout/kill/drain/error。详见 replacement-bitmapcache-fix-default-full-closeout.md。COM手写0x8D仍发旧32，下一直接修合法writer28，不回退生产exact20；新full仍malformed，不冒整体消除。
官方full cache FF80/actual0不经过candidate decoder，不能自动宣布修复。consumer HW/backend/真实target loss/audio/自然资源/final matrix/整体仍Open，旧有限绿不整体迁移新hash。
