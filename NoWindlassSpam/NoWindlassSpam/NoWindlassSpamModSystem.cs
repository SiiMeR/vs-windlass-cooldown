using Crossbows;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace NoWindlassSpam;

public class NoWindlassSpamModSystem : ModSystem
{
    public override void StartClientSide(ICoreClientAPI api)
    {
        var harmony = new Harmony(Mod.Info.ModID);

        var original =
            AccessTools.Method(typeof(CrossbowClient), "Shoot");
        var patch = AccessTools.Method(typeof(CrossbowShootPatch), nameof(CrossbowShootPatch.ShootClient));

        harmony.Patch(original, new HarmonyMethod(patch));
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        var harmony = new Harmony(Mod.Info.ModID);


        var original =
            AccessTools.Method(typeof(CrossbowServer), "Shoot");
        var patch = AccessTools.Method(typeof(CrossbowShootPatch), nameof(CrossbowShootPatch.ShootServer));

        harmony.Patch(original, postfix: new HarmonyMethod(patch));
    }
}