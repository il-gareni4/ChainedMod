using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace Chained.Net;

public class ChainedNetServer
{
    private static Mod _mod = null;
    private static Mod Mod => _mod ??= ModLoader.GetMod("Chained");

    public static void SendHeal(int target, short amount)
    {
        ModPacket packet = Mod.GetPacket();
        packet.Write((byte)ClientCode.Heal);
        packet.Write(amount);
        packet.Send(target);
    }

    internal static void HandlePacket(BinaryReader reader, int whoAmI, ServerCode code)
    {
        switch (code)
        {
            case ServerCode.Heal:
                int target = reader.ReadByte();
                short amount = reader.ReadInt16();
                HandleHeal(target, amount);
                break;
            default:
                throw new Exception($"Unknown server packet code: {code}");
        }
    }

    private static void HandleHeal(int target, short amount)
    {
        Main.player[target].Heal(amount);
        SendHeal(target, amount);
    }
}
