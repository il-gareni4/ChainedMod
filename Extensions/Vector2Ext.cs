using Microsoft.Xna.Framework;

namespace Chained.Extensions;

public static class Vector2Ext {
    public static Vector2 Slide(this Vector2 vec, Vector2 n) {
        return vec - Vector2.Dot(vec, n) * n;
    }
}


