﻿using NHM.MinerPluginToolkitV1.Configs;
using NHM.Common.Enums;
using System.Collections.Generic;

using SAS = NHM.MinerPluginToolkitV1.Configs.PluginSupportedAlgorithmsSettings.SupportedAlgorithmSettings;

namespace TRex
{
    public partial class TRexPlugin
    {
        protected override PluginSupportedAlgorithmsSettings DefaultPluginSupportedAlgorithmsSettings => new PluginSupportedAlgorithmsSettings
        {
            DefaultFee = 1.0,
            Algorithms = new Dictionary<DeviceType, List<SAS>>
            {
                {
                    DeviceType.NVIDIA,
                    new List<SAS>
                    {
                        new SAS(AlgorithmType.KAWPOW){NonDefaultRAMLimit = 4UL << 30 },
                        new SAS(AlgorithmType.DaggerHashimoto){NonDefaultRAMLimit = 3UL << 30 }
                    }
                }
            },
            AlgorithmNames = new Dictionary<AlgorithmType, string>
            {
                { AlgorithmType.KAWPOW, "kawpow" },
                { AlgorithmType.DaggerHashimoto, "ethash" }
            }
        };
    }
}
