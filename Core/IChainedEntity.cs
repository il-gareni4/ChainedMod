using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Chained.Core;
public interface IJointEntity
{
    public Vector2 Center { get; set; }
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public float Rotation { get; set; }
    public IEnumerable<IJointEntity> ChainedTo { get; }
    public Vector2 TileCollision(Vector2 position, Vector2 velocity);
}