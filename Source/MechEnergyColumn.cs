using System;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace ShowMechEnergyInTransporters
{
    /// <summary>
    /// 打开「机械族电量」列的共用工具，两个补丁都经由这里改动原版数据。
    ///
    /// 联动关系（自上而下的调用链）：
    ///   Patches.cs 的后缀补丁（界面重建装载列表之后）
    ///     -> EnableOn()：取出界面实例上的 pawnsTransfer 组件
    ///       -> 把该组件的私有字段 drawMechEnergy 置为 true
    ///         -> 原版 TransferableOneWayWidget.DrawRow 每帧读取该字段，为每一行决定「是否画出电量列」
    ///           -> 原版 TransferableOneWayWidget.DrawMechEnergy 完成实际绘制（蓝色百分比 + 鼠标悬停提示）
    ///
    /// 也就是说本模组不添加任何绘制代码，只是把原版自己关掉的开关重新打开，
    /// 因此显示样式与「组建远征队」界面天然完全一致。
    ///
    /// 为什么必须用反射：drawMechEnergy 声明为 private bool，原版没有提供 setter；
    /// 同一类型里的 readOnly、drawIdeo、drawXenotype 都是 public，唯独这个开关没有公开入口。
    /// 与之相对，pawnsTransfer 是界面的私有字段，只能按字段名取。
    ///
    /// 注意：
    /// 1. 字段名一旦被原版改名，这里会失效。因此两处字段都做了「找不到就只报一次错误并跳过」的处理，
    ///    不抛异常，避免把整个装载界面拖垮；见到日志里的报错说明游戏更新后需要同步改字段名。
    /// 2. 该开关同时决定「是否绘制」与「是否占用 75 像素列宽」，两者由原版同一处逻辑控制，
    ///    所以翻动它不会造成列错位。
    /// 3. 只有 Biotech 激活、且该行单位有 needs.energy（即机械族）时才会真正画出内容；
    ///    其余单位只是占一列空白，观感与原版组建远征队界面相同。
    /// 4. 只在游戏的 UI 线程被调用，无需加锁。
    /// </summary>
    internal static class MechEnergyColumn
    {
        private const string WidgetFlagFieldName = "drawMechEnergy";

        private static bool pawnsWidgetFieldReported;
        private static bool widgetFlagFieldReported;

        /// <summary>
        /// 打开指定界面上装载列表的电量列。
        /// </summary>
        /// <param name="dialog">装载界面实例，取值不能为空。</param>
        /// <param name="pawnsWidgetFieldName">该界面上「人员列表」组件的私有字段名，两个界面都叫 pawnsTransfer。</param>
        /// <remarks>
        /// 必须在界面重建装载列表之后调用（后缀补丁），此时字段才不是 null；
        /// 在构造完成前调用会静默什么都不做。
        /// </remarks>
        public static void EnableOn(object dialog, string pawnsWidgetFieldName)
        {
            if (dialog == null)
            {
                return;
            }

            object pawnsWidget = ReadField(dialog, pawnsWidgetFieldName, ref pawnsWidgetFieldReported);
            if (pawnsWidget == null)
            {
                return;
            }

            WriteBoolField(pawnsWidget, WidgetFlagFieldName, value: true, ref widgetFlagFieldReported);
        }

        private static object ReadField(object instance, string fieldName, ref bool alreadyReported)
        {
            FieldInfo field = FindField(instance.GetType(), fieldName);
            if (field == null)
            {
                if (!alreadyReported)
                {
                    alreadyReported = true;
                    Log.Error("[显示机械族电量] 类型 " + instance.GetType().Name + " 上找不到字段 " + fieldName + "，本处界面无法打开电量列。游戏更新后字段改名时会出现此提示。");
                }
                return null;
            }
            return field.GetValue(instance);
        }

        private static void WriteBoolField(object instance, string fieldName, bool value, ref bool alreadyReported)
        {
            FieldInfo field = FindField(instance.GetType(), fieldName);
            if (field == null)
            {
                if (!alreadyReported)
                {
                    alreadyReported = true;
                    Log.Error("[显示机械族电量] 类型 " + instance.GetType().Name + " 上找不到字段 " + fieldName + "，电量列无法打开。游戏更新后字段改名时会出现此提示。");
                }
                return;
            }
            field.SetValue(instance, value);
        }

        private static FieldInfo FindField(Type type, string fieldName)
        {
            try
            {
                return AccessTools.Field(type, fieldName);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
