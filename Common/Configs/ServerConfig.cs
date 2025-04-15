using System;
using System.ComponentModel;
using Chained.Common.Configs.Enums;
using Terraria.ModLoader.Config;

namespace Chained.Common
{
    public class ServerConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Header("ChainSettings")]
        [Range(50f, 1000f)]
        [DefaultValue(250f)]
        public float ChainLength { get; set; }

        [Header("Stats")]
        [DefaultValue(HealthMode.Average)]
        public HealthMode HealthMode { get; set; }
        
        [DefaultValue(ManaMode.Average)]
        public ManaMode ManaMode { get; set; }

        [Header("Actions")]
        [DefaultValue(OnHurtAction.Full)]
        public OnHurtAction OnHurt { get; set; }
    }
}