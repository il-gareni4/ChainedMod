using System.IO;
using Chained.Common;
using Chained.Net;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Chained;

public class Chained : Mod
{
    public override void Load()
    {
        On_Player.Spawn += OnSpawn;
        On_Player.Heal += OnHeal;
    }

    public override void Unload()
    {
        On_Player.Spawn -= OnSpawn;
        On_Player.Heal -= OnHeal;
    }

    private void OnSpawn(On_Player.orig_Spawn orig, Player self, PlayerSpawnContext context)
    {
        orig(self, context);
        if (context == PlayerSpawnContext.RecallFromItem)
            self.GetModPlayer<ChainedPlayer>().PostRecall(context);
        else if (context == PlayerSpawnContext.ReviveFromDeath)
            self.GetModPlayer<ChainedPlayer>().PostRespawn(context);
    }

    private void OnHeal(On_Player.orig_Heal orig, Player self, int amount)
    {
        ChainedPlayer player = self.GetModPlayer<ChainedPlayer>();
        amount = player.ModifyHeal(amount);
        player.PreHeal(amount);
        orig(self, amount);
        player.PostHeal(amount);
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        if (Main.netMode == NetmodeID.Server)
            ChainedNetServer.HandlePacket(reader, whoAmI, (ServerCode)reader.ReadByte());
        else
            ChainedNetClient.HandlePacket(reader, whoAmI, (ClientCode)reader.ReadByte());
    }
}
