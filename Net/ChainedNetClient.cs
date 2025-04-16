using System;
using System.IO;
using Chained.Common;
using Terraria;
using Terraria.ModLoader;

namespace Chained.Net;

public class ChainedNetClient
{
    private static Mod _mod = null;
    private static Mod Mod => _mod ??= ModLoader.GetMod("Chained");

    public static void SendHeal(int target, int amount)
    {
        ModPacket packet = Mod.GetPacket();
        packet.Write((byte)ServerCode.Heal);
        packet.Write((byte)target);
        packet.Write((short)amount);
        packet.Send();
    }

    internal static void HandlePacket(BinaryReader reader, int whoAmI, ClientCode code)
    {
        switch (code)
        {
            case ClientCode.Heal:
                short amount = reader.ReadInt16();
                HandleHeal(amount);
                break;
            default:
                throw new Exception($"Unknown client packet code: {code}");
        }
    }

    private static void HandleHeal(short amount)
    {
        Main.player[Main.myPlayer].GetModPlayer<ChainedPlayer>().NoShareHeal(amount);
    }
}

