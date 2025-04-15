using System;
using System.Linq;
using Chained.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace Chained.Common;

public class PlayerChainDrawLayer : PlayerDrawLayer
{
    public override Position GetDefaultPosition() => PlayerDrawLayers.BeforeFirstVanillaLayer;

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        if (drawInfo.shadow != 0f || drawInfo.drawPlayer.dead)
            return;

        ServerConfig config = ModContent.GetInstance<ServerConfig>();
        ChainedPlayer me = drawInfo.drawPlayer.GetModPlayer<ChainedPlayer>();
        Texture2D tex = TextureAssets.Chain.Value;

        foreach (IJointEntity chainedTo in me.ChainedPlayers.Where(chainedTo =>
            chainedTo.IsActive && me.Player.whoAmI >= chainedTo.Player.whoAmI))
        {
            float distance = me.Player.Distance(chainedTo.Center);
            Vector2 center = chainedTo.Center + (me.Player.Center - chainedTo.Center) / 2f;
            int totalSegments = (int)(config.ChainLength / tex.Width);

            for (int i = 0; i < totalSegments; i++)
            {
                float dangleAmount = 1f - distance / config.ChainLength;
                Vector2 p0 = me.Player.Center;
                Vector2 p1 = center + Vector2.UnitY * 140f * dangleAmount;
                Vector2 p2 = chainedTo.Center;

                Vector2 position = Bezier(p0, p1, p2, (float)i / totalSegments);
                float rotation = BezierAngle(p0, p1, p2, (float)i / totalSegments) + float.Pi / 2;
                Color color;
                if (Lighting.Mode == Terraria.Graphics.Light.LightMode.Color)
                    color = new(Lighting.GetSubLight(position));
                else
                    color = Lighting.GetColor(position.ToTileCoordinates());

                drawInfo.DrawDataCache.Add(
                    new DrawData(tex, position - Main.screenPosition, null, color, rotation, new Vector2(0.5f, 0.5f), 1f, SpriteEffects.None, 0f));
            }
        }
    }

    private Vector2 Bezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        return Vector2.Lerp(Vector2.Lerp(p0, p1, t), Vector2.Lerp(p1, p2, t), t);
    }

    private float BezierAngle(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        Vector2 ip0 = Vector2.Lerp(p0, p1, t);
        Vector2 ip1 = Vector2.Lerp(p1, p2, t);
        return (float)Math.Atan2(ip1.Y - ip0.Y, ip1.X - ip0.X);
    }
}