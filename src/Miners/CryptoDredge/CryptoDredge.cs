﻿using NHM.Common;
using NHM.Common.Enums;
using NHM.MinerPlugin;
using NHM.MinerPluginToolkitV1;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoDredge
{
    public class CryptoDredge : MinerBase
    {
        private string _devices;
        private int _apiPort;

        private static Dictionary<int, int> _acceptedSharesPerDevice = new Dictionary<int, int>();
        private static Dictionary<int, int> _rejectedSharesPerDevice = new Dictionary<int, int>();
        private static Dictionary<int, DateTime> _lastAcceptedSharePerDevice = new Dictionary<int, DateTime>();
        private static Dictionary<int, DateTime> _lastRejectedSharePerDevice = new Dictionary<int, DateTime>();

        public CryptoDredge(string uuid) : base(uuid)
        {}

        private double DevFee => PluginSupportedAlgorithms.DevFee(_algorithmType);

        private struct IdPowerHash
        {
            public int id;
            public int power;
            public double speed;
            public int accepted;
            public int rejected;
            public DateTime lastAccepted;
            public DateTime lastRejected;
        }

        public async override Task<ApiData> GetMinerStatsDataAsync()
        {
            var api = new ApiDataShare(new ApiData());
            var perDeviceSpeedInfo = new Dictionary<string, IReadOnlyList<(AlgorithmType type, double speed)>>();
            var perDevicePowerInfo = new Dictionary<string, int>();
            var totalSpeed = 0d;
            var totalPowerUsage = 0;

            var perDeviceAcceptedShareInfo = new Dictionary<string, (int, DateTime)>();
            var perDeviceRejectedShareInfo = new Dictionary<string, (int, DateTime)>();

            try
            {
                var result = await ApiDataHelpers.GetApiDataAsync(_apiPort, "summary", _logGroup);
                api.ApiResponse = result;
                if (result == "") return api;

                //total speed
                if (!string.IsNullOrEmpty(result))
                {
                    try
                    {
                        var summaryOptvals = result.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var optvalPairs in summaryOptvals)
                        {
                            var pair = optvalPairs.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                            if (pair.Length != 2) continue;
                            if (pair[0] == "KHS")
                            {
                                totalSpeed = double.Parse(pair[1], CultureInfo.InvariantCulture) * 1000; // HPS
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
                    }
                }

                var threadsApiResult = await ApiDataHelpers.GetApiDataAsync(_apiPort, "threads", _logGroup);
                if (!string.IsNullOrEmpty(threadsApiResult))
                {
                    try
                    {
                        var gpus = threadsApiResult.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        var apiDevices = new List<IdPowerHash>();

                        foreach (var gpu in gpus)
                        {
                            var gpuOptvalPairs = gpu.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                            var gpuData = new IdPowerHash();
                            foreach (var optvalPairs in gpuOptvalPairs)
                            {
                                var optval = optvalPairs.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                                if (optval.Length != 2) continue;
                                if (optval[0] == "GPU")
                                {
                                    gpuData.id = int.Parse(optval[1], CultureInfo.InvariantCulture);
                                }
                                if (optval[0] == "POWER")
                                {
                                    gpuData.power = int.Parse(optval[1], CultureInfo.InvariantCulture);
                                }
                                if (optval[0] == "KHS")
                                {
                                    gpuData.speed = double.Parse(optval[1], CultureInfo.InvariantCulture) * 1000; // HPS
                                }
                                if (optval[0] == "ACC")
                                {
                                    gpuData.accepted = int.Parse(optval[1], CultureInfo.InvariantCulture);
                                    if (!_acceptedSharesPerDevice.ContainsKey(gpuData.id)) _acceptedSharesPerDevice.Add(gpuData.id, gpuData.accepted);
                                    if (!_lastAcceptedSharePerDevice.ContainsKey(gpuData.id)) _lastAcceptedSharePerDevice.Add(gpuData.id, new DateTime());
                                    if (_acceptedSharesPerDevice[gpuData.id] != gpuData.accepted)
                                    {
                                        _acceptedSharesPerDevice[gpuData.id] = gpuData.accepted;
                                        _lastAcceptedSharePerDevice[gpuData.id] = DateTime.Now;
                                    }
                                    gpuData.lastAccepted = _lastAcceptedSharePerDevice[gpuData.id];
                                }
                                if (optval[0] == "REJ")
                                {
                                    gpuData.rejected = int.Parse(optval[1], CultureInfo.InvariantCulture);
                                    if (!_rejectedSharesPerDevice.ContainsKey(gpuData.id)) _rejectedSharesPerDevice.Add(gpuData.id, gpuData.rejected);
                                    if (!_lastRejectedSharePerDevice.ContainsKey(gpuData.id)) _lastRejectedSharePerDevice.Add(gpuData.id, new DateTime());
                                    if (_rejectedSharesPerDevice[gpuData.id] != gpuData.rejected)
                                    {
                                        _rejectedSharesPerDevice[gpuData.id] = gpuData.rejected;
                                        _lastRejectedSharePerDevice[gpuData.id] = DateTime.Now;
                                    }
                                    gpuData.lastRejected = _lastRejectedSharePerDevice[gpuData.id];
                                }

                            }
                            apiDevices.Add(gpuData);
                        }

                        foreach (var miningPair in _miningPairs)
                        {
                            var deviceUUID = miningPair.Device.UUID;
                            var deviceID = miningPair.Device.ID;

                            var apiDevice = apiDevices.Find(apiDev => apiDev.id == deviceID);
                            if (apiDevice.Equals(default(IdPowerHash))) continue;
                            perDeviceSpeedInfo.Add(deviceUUID, new List<(AlgorithmType type, double speed)>() { (_algorithmType, apiDevice.speed * (1 - DevFee * 0.01)) });
                            perDevicePowerInfo.Add(deviceUUID, apiDevice.power);
                            totalPowerUsage += apiDevice.power;

                            perDeviceAcceptedShareInfo.Add(deviceUUID, (apiDevice.accepted, apiDevice.lastAccepted));
                            perDeviceRejectedShareInfo.Add(deviceUUID, (apiDevice.rejected, apiDevice.lastRejected));
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(_logGroup, $"Error occured while getting API stats: {e.Message}");
            }

            api.PowerUsageTotal = totalPowerUsage;
            api.AlgorithmSpeedsPerDevice = perDeviceSpeedInfo;
            api.PowerUsagePerDevice = perDevicePowerInfo;

            api.AcceptedShareInfoPerDevice = perDeviceAcceptedShareInfo;
            api.RejectedShareInfoPerDevice = perDeviceRejectedShareInfo;

            return api;
        }

        protected override void Init()
        {
            _devices = string.Join(",", _miningPairs.Select(p => p.Device.ID));
        }

        protected override string MiningCreateCommandLine()
        {
            // API port function might be blocking
            _apiPort = GetAvaliablePort();
            // instant non blocking
            var url = StratumServiceHelpers.GetLocationUrl(_algorithmType, _miningLocation, NhmConectionType.STRATUM_TCP);
            var algo = PluginSupportedAlgorithms.AlgorithmName(_algorithmType);

            var commandLine = $"--algo {algo} --url {url} --user {_username} -b 127.0.0.1:{_apiPort} --device {_devices} --no-watchdog {_extraLaunchParameters}";
            return commandLine;
        }
    }
}
