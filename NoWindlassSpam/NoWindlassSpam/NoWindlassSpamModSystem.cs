using System;
using Crossbows;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace NoWindlassSpam;

public class NoWindlassSpamModSystem : ModSystem
{
    public static CrossbowsSpamConfig Config;
    private INetworkChannel _networkChannel;

    public override void Start(ICoreAPI api)
    {
        _networkChannel = api.Network.RegisterChannel(Mod.Info.ModID).RegisterMessageType<ConfigPacket>();
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        Config = new CrossbowsSpamConfig();
        (_networkChannel as IClientNetworkChannel).SetMessageHandler<ConfigPacket>(OnConfigPacketReceived);

        var harmony = new Harmony(Mod.Info.ModID);

        var original =
            AccessTools.Method(typeof(CrossbowClient), "Shoot");
        var patch = AccessTools.Method(typeof(CrossbowShootPatch), nameof(CrossbowShootPatch.ShootClient));

        harmony.Patch(original, new HarmonyMethod(patch));
    }

    private void OnConfigPacketReceived(ConfigPacket packet)
    {
        Config.ApplicableCrossbowIds = packet.ApplicableCrossbowIds;
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        TryToLoadConfig(api);

        api.Event.PlayerNowPlaying += player =>
        {
            (_networkChannel as IServerNetworkChannel).SendPacket(new ConfigPacket
                { ApplicableCrossbowIds = Config.ApplicableCrossbowIds }, player);
        };

        var harmony = new Harmony(Mod.Info.ModID);


        var original =
            AccessTools.Method(typeof(CrossbowServer), "Shoot");
        var patch = AccessTools.Method(typeof(CrossbowShootPatch), nameof(CrossbowShootPatch.ShootServer));

        harmony.Patch(original, postfix: new HarmonyMethod(patch));
    }

    private static void TryToLoadConfig(ICoreAPI api)
    {
        //It is important to surround the LoadModConfig function in a try-catch. 
        //If loading the file goes wrong, then the 'catch' block is run.
        try
        {
            Config = api.LoadModConfig<CrossbowsSpamConfig>("CrossbowsSpamConfig.json") ?? new CrossbowsSpamConfig();

            //Save a copy of the mod config.
            api.StoreModConfig(Config, "CrossbowsSpamConfig.json");
        }
        catch (Exception e)
        {
            //Couldn't load the mod config... Create a new one with default settings, but don't save it.
            api.Logger.Error("Could not load config! Loading default settings instead.");
            api.Logger.Error(e);
            Config = new CrossbowsSpamConfig();
        }
    }
}