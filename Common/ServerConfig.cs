using System;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Chained.Common
{
    public class ServerConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Header("General")]
        [Range(50f, 1000f)]
        [DefaultValue(250f)]
        public float ChainLength { get; set; }
    }
}