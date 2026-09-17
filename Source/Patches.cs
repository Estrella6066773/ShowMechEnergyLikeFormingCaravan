using HarmonyLib;
using RimWorld;

namespace ShowMechEnergyInTransporters
{
    /// <summary>
    /// 挂钩「装载运输仓 / 穿梭机」界面 Dialog_LoadTransporters。
    ///
    /// 联动关系：该界面在打开时（PostOpen）和点「重置」按钮时会调用
    /// CalculateAndRecacheTransferables 重建装载列表，并在其中构造 pawnsTransfer 组件——
    /// 原版就在这里把 drawMechEnergy 传成了 false，所以列表里能看到机械族却没有电量。
    /// 本补丁用后缀（Postfix）在重建结束后介入，把该开关重新打开。
    ///
    /// 注意：
    /// 1. 不能改用前缀（Prefix）：那时 pawnsTransfer 字段还是旧值或 null。
    /// 2. 该方法同时服务运输仓与奥德赛资料片的穿梭机，二者都会一并生效。
    /// 3. 「组建远征队」（Dialog_FormCaravan）不在挂钩范围内——原版那里本来就传了 true。
    /// 4. 只是丢弃返回值、不改变原方法行为，因此与其它改同一界面的模组一般不冲突。
    /// </summary>
    [HarmonyPatch(typeof(Dialog_LoadTransporters), "CalculateAndRecacheTransferables")]
    internal static class Patch_Dialog_LoadTransporters
    {
        [HarmonyPostfix]
        private static void Postfix(Dialog_LoadTransporters __instance)
        {
            MechEnergyColumn.EnableOn(__instance, "pawnsTransfer");
        }
    }

    /// <summary>
    /// 挂钩口袋地图的「进入」装载界面 Dialog_EnterPortal。
    ///
    /// 联动关系：该界面由 MapPortal 及其子类打开（异象资料片的坑洞 PitGate、虫巢入口
    /// InsectLairEntrance、远古舱门 AncientHatch、口袋地图出口 PocketMapExit 都走它），
    /// 同样是打开时与点「重置」时调用 CalculateAndRecacheTransferables 重建列表。
    /// 原版在这里把 drawMechEnergy 与 drawNutritionEatenPerDay 都留成了默认的 false。
    ///
    /// 注意：
    /// 1. 本补丁只打开电量列，不动原版的「每日进食量」列——所以非机械族的行会留出一格空白，
    ///    这与原版组建远征队界面表现一致（那里也是先看进食量、没有进食量才看电量）。
    /// 2. 若希望这里也显示进食量，可另加一个同款 Postfix 把 drawNutritionEatenPerDay 也置为 true。
    /// 3. 该界面本身由资料片内容触发，但类本身在核心程序集中，未启用资料片时挂补丁也不会出错。
    /// </summary>
    [HarmonyPatch(typeof(Dialog_EnterPortal), "CalculateAndRecacheTransferables")]
    internal static class Patch_Dialog_EnterPortal
    {
        [HarmonyPostfix]
        private static void Postfix(Dialog_EnterPortal __instance)
        {
            MechEnergyColumn.EnableOn(__instance, "pawnsTransfer");
        }
    }
}
