﻿using NHM.MinerPluginToolkitV1;
using NHM.MinerPluginToolkitV1.Configs;
using NHM.MinerPluginToolkitV1.Interfaces;
using NHM.Common;
using NHM.Common.Algorithm;
using NHM.Common.Device;
using NHM.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniZ
{
    public partial class MiniZPlugin : PluginBase, IDevicesCrossReference
    {
        public MiniZPlugin()
        {
            // mandatory init
            InitInsideConstuctorPluginSupportedAlgorithmsSettings();
            // set default internal settings
            MinerOptionsPackage = PluginInternalSettings.MinerOptionsPackage;
            // https://miniz.ch/usage/#command-line-arguments | https://miniz.ch/download/#latest-version
            MinersBinsUrlsSettings = new MinersBinsUrlsSettings
            {
                BinVersion = "v1.6v6",
                ExePath = new List<string> { "miniZ.exe" },
                Urls = new List<string>
                {
                    "https://github.com/nicehash/MinerDownloads/releases/download/3.0.0.2/miniZ_v1.6v6_cuda10_win-x64.zip",
                    "https://miniz.ch/?smd_process_download=1&download_id=3839", // original (pwd protected)
                }
            };
            PluginMetaInfo = new PluginMetaInfo
            {
                PluginDescription = "miniZ is a fast and friendly Equihash miner.",
                SupportedDevicesAlgorithms = SupportedDevicesAlgorithmsDict()
            };
        }
        public override string PluginUUID => "eda6abd0-94eb-11ea-a64d-17be303ea466";

        public override Version Version => new Version(14, 0);

        public override string Name => "MiniZ";

        public override string Author => "info@nicehash.com";

        protected readonly Dictionary<string, int> _mappedDeviceIds = new Dictionary<string, int>();

        protected override MinerBase CreateMinerBase()
        {
            return new MiniZ(PluginUUID, _mappedDeviceIds);
        }

        public override Dictionary<BaseDevice, IReadOnlyList<Algorithm>> GetSupportedAlgorithms(IEnumerable<BaseDevice> devices)
        {
            var supported = new Dictionary<BaseDevice, IReadOnlyList<Algorithm>>();

#warning TEMP disable NVIDIA driver check
            // Require 411.31 - CUDA 10.0
            //var minDrivers = new Version(411, 31);
            //if (CUDADevice.INSTALLED_NVIDIA_DRIVERS < minDrivers) return supported;

            var cudaGpus = devices
                .Where(dev => dev is CUDADevice)
                .Cast<CUDADevice>();

            var pcieId = 0;
            foreach (var gpu in cudaGpus)
            {
                _mappedDeviceIds[gpu.UUID] = pcieId;
                ++pcieId;
                var algos = GetSupportedAlgorithmsForDevice(gpu).ToList();
                if (algos.Count > 0) supported.Add(gpu, algos);
            }

            return supported;
        }

        public async Task DevicesCrossReference(IEnumerable<BaseDevice> devices)
        {
            if (_mappedDeviceIds.Count == 0) return;
            // will block
            var minerBinPath = GetBinAndCwdPaths().Item1;
            var output = await DevicesCrossReferenceHelpers.MinerOutput(minerBinPath, "-ci");
            var mappedDevs = DevicesListParser.ParseMiniZOutput(output, devices.ToList());

            foreach (var kvp in mappedDevs)
            {
                var uuid = kvp.Key;
                var indexID = kvp.Value;
                _mappedDeviceIds[uuid] = indexID;
            }
        }

        public override IEnumerable<string> CheckBinaryPackageMissingFiles()
        {
            var pluginRootBinsPath = GetBinAndCwdPaths().Item2;
            return BinaryPackageMissingFilesCheckerHelpers.ReturnMissingFiles(pluginRootBinsPath, new List<string> { "miniZ.exe" });
        }

        public override bool ShouldReBenchmarkAlgorithmOnDevice(BaseDevice device, Version benchmarkedPluginVersion, params AlgorithmType[] ids)
        {
            try
            {
                if (benchmarkedPluginVersion.Major < 13 && ids.First() == AlgorithmType.ZHash) return true;
                if (ids.First() == AlgorithmType.BeamV3 && benchmarkedPluginVersion.Major == 13 && benchmarkedPluginVersion.Minor < 2) return true;
            }
            catch (Exception e)
            {
                Logger.Error(PluginUUID, $"ShouldReBenchmarkAlgorithmOnDevice {e.Message}");
            }
            return false;
        }
    }
}
