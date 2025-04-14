using System;
using System.Collections.Generic;
using System.Linq;
using Chained.Core;
using Chained.Utilities;
using Chained.Extensions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using log4net.Appender;
using Newtonsoft.Json.Bson;

namespace Chained.Common;

public class ChainedPlayer : ModPlayer, IJointEntity
{
    public Vector2 Position { get => Player.position; set => Player.position = value; }
    public Vector2 Center { get => Player.Center; set => Player.Center = value; }
    public Vector2 Velocity { get => Player.velocity; set => Player.velocity = value; }
    public float Rotation { get => Player.bodyRotation; set => Player.bodyRotation = value; }
    public IEnumerable<IJointEntity> ChainedTo =>
        CommonMain.ActivePlayers
        .Where(player => player.whoAmI != Player.whoAmI && player.team == Player.team)
        .Select(player => player.GetModPlayer<ChainedPlayer>());

    public override void OnEnterWorld()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            Main.NewText($"[{Mod.DisplayName}] It is recommended to disable this mod in singleplayer.", Colors.RarityDarkRed);
            return;
        }
    }

    public override void PreUpdateMovement()
    {
        UpdateChain();
    }

    public Vector2 TileCollision(Vector2 position, Vector2 velocity)
    {
        return Collision.TileCollision(position, velocity, Player.width, Player.height);
    }

    private void UpdateChain()
    {
        // When player try to move outside of the chain length, we need to find the intersection point of the vector and the circle.
        // Amount of vector transferred to the other player is the amount of vector that is outside of the circle
        // Multiplied by dot product of normal vector in intersection point and velocity vector.
        // Velocity left (not transferred) will be directed to perform slide along the circle.

        ServerConfig config = ModContent.GetInstance<ServerConfig>();
        float maxLength = config.ChainLength;
        foreach (IJointEntity player in ChainedTo)
        {
            Vector2 directionToThis = Vector2.Normalize(Player.Center - player.Center);
            Vector2 directionToOther = Vector2.Normalize(player.Center - Player.Center);

            float distance = Player.Distance(player.Center);
            // If distance is greater than maxLength, we need to move (editing position directly) players to the end of the chain
            if (distance > maxLength)
            {
                float overLength = distance - maxLength;
                Vector2 thisDesiredVelocity = directionToOther * (overLength / 2f + 0.005f);
                Vector2 otherDesiredVelocity = directionToThis * (overLength / 2f + 0.005f);

                Vector2 thisCollision = TileCollision(Player.position, thisDesiredVelocity);
                Vector2 otherCollision = player.TileCollision(player.Position, otherDesiredVelocity);

                Player.position += thisCollision;
                player.Position += otherCollision;

                Vector2 thisDelta = thisDesiredVelocity - thisCollision;
                Vector2 otherDelta = otherDesiredVelocity - otherCollision;
                if (thisDelta != Vector2.Zero)
                    player.Position += player.TileCollision(player.Position, -thisDelta);
                if (otherDelta != Vector2.Zero)
                    Player.position += TileCollision(Player.position, -otherDelta);
                    
                // Recalculate directions
                directionToThis = Vector2.Normalize(Player.Center - player.Center);
                directionToOther = Vector2.Normalize(player.Center - Player.Center);
            }

            Vector2 chainCenter = player.Center + (Player.Center - player.Center) / 2f;
            float futureDistance = Vector2.Distance(Player.Center + Player.velocity, player.Center + player.Velocity);
            if (futureDistance > maxLength)
            {
                IntersectionResult(this, chainCenter, maxLength / 2f, out Vector2 thisResultVelocity, out float thisTransferVelocity);
                IntersectionResult(player, chainCenter, maxLength / 2f, out Vector2 otherResultVelocity, out float otherTransferVelocity);
                Player.velocity = thisResultVelocity + (directionToOther * otherTransferVelocity);
                player.Velocity = otherResultVelocity + (directionToThis * thisTransferVelocity);
            }
        }
    }

    private Vector2? VectorCircleIntersection(Vector2 pos, Vector2 vec, float rad)
    {
        if (rad <= 0 || pos.Length() > rad || vec == Vector2.Zero || (pos + vec).Length() < rad)
            return null;

        if (vec.X == 0)
        {
            float y = (float)Math.Sin(Math.Acos(pos.X / rad)) * rad * Math.Sign(pos.Y);
            return new Vector2(pos.X, y);
        }


        // line equation: y = kx + y_0
        // circle equation: x^2 + y^2 = rad^2
        // final equation: x^2 + (kx + y_0)^2 = rad^2
        // simplify: x^2 + k^2 * x^2 + 2k * y_0 * x + y_0^2 - rad^2 = 0 =>
        // => (1 + k^2) * x^2 + 2k * y_0 * x + y_0^2 - rad^2 = 0
        //    |_______|         |______|       |___________|
        //    a                 b              c

        Vector2 vecNormal = Vector2.Normalize(vec);
        double k = vecNormal.Y / vecNormal.X;
        double y_0 = pos.Y - pos.X * k;

        double a = 1 + k * k;
        double b = 2 * k * y_0;
        double c = y_0 * y_0 - rad * rad;

        double d = b * b - 4.0 * a * c;
        if (d < 0)
            return null;
        else if (d == 0)
        {
            double x = -b / (2 * a);
            return new Vector2((float)x, (float)(k * x + y_0));
        }
        else
        {
            double x;
            if (Math.Sign(vec.X) > 0)
                x = (-b + Math.Sqrt(d)) / (2 * a);
            else
                x = (-b - Math.Sqrt(d)) / (2 * a);
            return new Vector2((float)x, (float)(k * x + y_0));
        }
    }

    private bool IntersectionResult(IJointEntity player, Vector2 center, float radius, out Vector2 resultVelocity, out float transferVelocity)
    {
        Vector2? intersection = VectorCircleIntersection(player.Center - center, player.Velocity, radius);
        if (intersection.HasValue)
        {
            float restrictedLength = (intersection.Value - (player.Center - center)).Length();
            float overLength = player.Velocity.Length() - restrictedLength;

            // Normal is normalized inversed radius vector
            Vector2 intersectionNormal = Vector2.Normalize(Vector2.Negate(intersection.Value));
            // Basically projection of velocity vector on normal vector
            Vector2 slideVec = (Vector2.Normalize(player.Velocity) * overLength).Slide(intersectionNormal);
            float transferredLength = overLength - slideVec.Length();

            resultVelocity = Vector2.Normalize(player.Velocity) * restrictedLength + slideVec;
            transferVelocity = transferredLength;
            return true;
        }
        else
        {
            resultVelocity = player.Velocity;
            transferVelocity = 0f;
            return false;
        }
    }
}
