﻿using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.ExtraLaunchParameters;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;

namespace TRex
{
    internal static class PluginInternalSettings
    {
        internal static TimeSpan DefaultTimeout = new TimeSpan(0, 1, 0);

        internal static MinerApiMaxTimeoutSetting GetApiMaxTimeoutConfig = new MinerApiMaxTimeoutSetting
        {
            GeneralTimeout = DefaultTimeout,
        };

        internal static MinerBenchmarkTimeSettings BenchmarkTimeSettings = new MinerBenchmarkTimeSettings
        {
            PerAlgorithm = new Dictionary<BenchmarkPerformanceType, Dictionary<string, int>>(){
                { BenchmarkPerformanceType.Quick, new Dictionary<string, int>(){ { "KAWPOW", 160 } } },
                { BenchmarkPerformanceType.Standard, new Dictionary<string, int>(){ { "KAWPOW", 180 } } },
                { BenchmarkPerformanceType.Precise, new Dictionary<string, int>(){ { "KAWPOW", 260 } } }
            }
        };

        internal static MinerOptionsPackage MinerOptionsPackage = new MinerOptionsPackage
        {
            GeneralOptions = new List<MinerOption>
            {
                /// <summary>
                /// GPU intensity 8-25 (default: auto).
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_intensity",
                    ShortName = "-i",
                    LongName = "--intensity",
                    DefaultValue = "auto"
                },
                /// <summary>
                /// Set process priority (default: 2) 0 idle, 2 normal to 5 highest.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_priority",
                    ShortName = "--cpu-priority",
                    DefaultValue = "2"
                },
                /// <summary>
                /// Forces miner to immediately reconnect to pool on N successively failed shares (default: 10).
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_reconectFailed",
                    ShortName = "--reconnect-on-fail-shares",
                    DefaultValue = "10"
                },
                /// <summary>
                /// Sliding window length in seconds used to compute average hashrate (default: 60).
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_avgHashrate",
                    ShortName = "-N",
                    LongName = "--hashrate-avr",
                    DefaultValue = "60"
                },
                /// <summary>
                /// Sliding window length in seconds used to compute sharerate (default: 600).
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_avgSharerate",
                    ShortName = "--sharerate-avr",
                    DefaultValue = "600"
                },
                /// <summary>
                /// Set temperature color for GPUs stat. Example: 55,65 - it means that
                /// temperatures above 55 will have yellow color, above 65 - red color. (default: 67,77)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_tempColor",
                    ShortName = "--temperature-color",
                    DefaultValue = "67,77"
                },
                /// <summary>
                /// GPU stats report frequency. (default: 5. every 5th share)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_reportInterval",
                    ShortName = "--gpu-report-interval",
                    DefaultValue = "5"
                },
                /// <summary>
                /// Quiet mode. No GPU stats at all.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "trex_quiet",
                    ShortName = "-q",
                    LongName = "--quiet"
                },
                /// <summary>
                /// Don't show date in console.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "trex_hideDate",
                    ShortName = "--hide-date"
                },
                /// <summary>
                /// Disable color output for console.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "trex_noColor",
                    ShortName = "--no-color"
                },
                /// <summary>
                /// Disable NVML GPU stats.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionIsParameter,
                    ID = "trex_noNvml",
                    ShortName = "--no-nvml"
                },
                /// <summary>
                /// Full path of the log file.
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_logPath",
                    ShortName = "-l",
                    LongName = "--log-path"
                },
                /// <summary>
                /// Memory tweak mode (default: 0 - disabled). Range from 0 to 6. General recommendation
                /// is to start with 1, and then increase only if the GPU is stable.
                /// The effect is similar to that of ETHlargementPill.
                /// Supported on graphics cards with GDDR5 or GDDR5X memory only.
                /// Requires running the miner with administrative privileges.
                /// Can be set to a comma separated list to apply different values to different cards.
                /// Example: --mt 4 (applies tweak mode #4 to all cards that support this functionality)
                /// --mt 3,3,3,0 (applies tweak mode #3 to all cards except the last one)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "trex_memoryTweak",
                    ShortName = "--mt",
                    Delimiter = ","
                },
                /// <summary>
                ///  Low load mode (default: 0). 1 - enabled, 0 - disabled.
                ///  Reduces the load on the GPUs if possible. Can be set to a comma separated string to enable
                ///  the mode for a subset of the GPU list (eg: --low-load 0,0,1,0)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithMultipleParameters,
                    ID = "trex_lowLoad",
                    ShortName = "--low-load",
                }
            },
            TemperatureOptions = new List<MinerOption>
            {
                /// <summary>
                /// GPU shutdown temperature. (default: 0 - disabled)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_tempLimit",
                    LongName = "--temperature-limit",
                    DefaultValue = "0"
                },
                /// <summary>
                /// GPU temperature to enable card after disable. (default: 0 - disabled)
                /// </summary>
                new MinerOption
                {
                    Type = MinerOptionType.OptionWithSingleParameter,
                    ID = "trex_tempStart",
                    LongName = "--temperature-start",
                    DefaultValue = "0"
                }
            }
        };
    }
}
