﻿using Crypto.Core.Helpers;
using CryptoMarketClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crypto.Core.Exchanges.Base {
    public class TickerCaptureDataInfo {
        public CaptureStreamType StreamType { get; set; }
        public CaptureMessageType MessageType { get; set; }
        public DateTime Time { get; set; }
        public string Message { get; set; }
    }

    public enum CaptureStreamType {
        OrderBook,
        TradeHistory,
        KLine
    }
    public enum CaptureMessageType {
        Incremental,
        Snapshot
    }

    [Serializable]
    public class TickerCaptureData : ISupportSerialization {
        public ExchangeType Exchange { get; set; }
        public string TickerName { get; set; }
        public List<TickerCaptureDataInfo> Items { get; set; } = new List<TickerCaptureDataInfo>();
        public string FileName { get; set; }

        public static TickerCaptureData FromFile(string fileName) {
            TickerCaptureData res = (TickerCaptureData)SerializationHelper.FromFile(fileName, typeof(TickerCaptureData));
            return res;
        }
        public bool Save(string path) {
            return SerializationHelper.Save(this, GetType(), path);
        }

        void ISupportSerialization.OnEndDeserialize() {
        }

        public bool Load(string simulationDataFile) {
            string destDirectory = Path.GetDirectoryName(simulationDataFile);
            string simulationXml = simulationDataFile.Replace(".zip", ".xml");
            try {
                if(File.Exists(simulationXml))
                    File.Delete(simulationXml);
                ZipFile.ExtractToDirectory(simulationDataFile, destDirectory);
            }
            catch(Exception e) {
                Telemetry.Default.TrackException(e);
                return false;
            }

            try {
                TickerCaptureData data = TickerCaptureData.FromFile(simulationXml);
                Exchange = data.Exchange;
                TickerName = data.TickerName;
                Items = data.Items;
                File.Delete(simulationXml);
            }
            catch(Exception e) {
                Telemetry.Default.TrackException(e);
                return false;
            }
            return true;
        }
    }
}
