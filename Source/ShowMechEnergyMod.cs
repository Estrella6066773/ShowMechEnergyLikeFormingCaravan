using System.Reflection;
using HarmonyLib;
using Verse;

namespace ShowMechEnergyInTransporters
{
    /// <summary>
    /// 模组入口。RimWorld 会在启动时执行带 StaticConstructorOnStartup 标记的类型静态构造函数。
    ///
    /// 联动关系：本类只负责挂载补丁；真正的改动在 Patches.cs 的两个后缀补丁里，
    /// 它们再调用 MechEnergyColumn 去打开原版的电量列开关。
    ///
    /// 注意：PatchAll 会扫描本程序集内所有带 HarmonyPatch 的类型，新增补丁类时无需在这里登记。
    /// HarmonyId 必须与 About/About.xml 里的 packageId 区分开且全局唯一，
    /// 否则会与其它模组的 Harmony 实例冲突。
    /// </summary>
    [StaticConstructorOnStartup]
    public static class ShowMechEnergyMod
    {
        public const string HarmonyId = "local.showMechEnergyLikeFormingCaravan";

        static ShowMechEnergyMod()
        {
            new Harmony(HarmonyId).PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}
