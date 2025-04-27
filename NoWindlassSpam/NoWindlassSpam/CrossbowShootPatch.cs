using System;
using System.Linq;
using CombatOverhaul.Inputs;
using CombatOverhaul.RangedSystems;
using Crossbows;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;

namespace NoWindlassSpam;

public static class CrossbowShootPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CrossbowClient), "Shoot")]
    public static bool ShootClient(
        CrossbowClient __instance,
        ref bool __result,
        ItemSlot slot,
        EntityPlayer player,
        ref int state,
        ActionEventData eventData,
        bool mainHand,
        AttackDirection direction)
    {
        if (!NoWindlassSpamModSystem.Config.ApplicableCrossbowIds.Contains(slot.Itemstack.Collectible.Code.ToString()))
        {
            return true;
        }


        var cooldown = player.WatchedAttributes.GetLong("crossbowcooldown");
        var nowSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (cooldown == 0L || nowSeconds > cooldown)
        {
            return true;
        }

        (player.Api as ICoreClientAPI).TriggerIngameError(__instance, "crossbowOnCooldown",
            $"Cannot shoot for another {cooldown - nowSeconds:0} second(s)");

        __result = false;
        return false;
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(CrossbowServer), "Shoot")]
    public static void ShootServer(
        CrossbowServer __instance, ref bool __result, IServerPlayer player, ItemSlot slot, ShotPacket packet,
        Entity shooter)
    {
        if (__result)
        {
            var nowSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            player.Entity.WatchedAttributes.SetLong("crossbowcooldown", nowSeconds + 12);
        }
    }
}