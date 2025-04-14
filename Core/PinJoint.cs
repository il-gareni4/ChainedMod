using Microsoft.Xna.Framework;

namespace Chained.Core;
public class PinJoint
{
    public IJointEntity Entity1 { get; }
    public Vector2 Origin1 { get; }
    public IJointEntity Entity2 { get; }
    public Vector2 Origin2 { get; }

    public PinJoint(IJointEntity entity1, Vector2 origin1, IJointEntity entity2, Vector2 origin2)
    {
        Entity1 = entity1;
        Origin1 = origin1;
        Entity2 = entity2;
        Origin2 = origin2;
    }
}