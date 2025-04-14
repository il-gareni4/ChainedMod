using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace Chained.Utilities;
    public static class CommonMain
    {
        public static IEnumerable<Player> ActivePlayers => Main.player.Where(player => player != null && player.active);  
    }