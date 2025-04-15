using Chained.Common;
using Terraria;
using Terraria.ModLoader;

namespace Chained;

public class Chained : Mod
{

    public override void Load()
    {
        On_Player.Spawn += PostSpawn;
    }

    public override void Unload()
    {
        On_Player.Spawn -= PostSpawn;
    }

    private void PostSpawn(On_Player.orig_Spawn orig, Player self, PlayerSpawnContext context)
    {
        orig(self, context);
        self.GetModPlayer<ChainedPlayer>()?.PostSpawn(context);
    }
}
