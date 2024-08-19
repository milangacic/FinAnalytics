using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsEngine {

    public class CDDDataProvider : CachedDataProvider {

        #region Types

        private struct Record {
            public string Symbol;
            public DateTime Date;
            public decimal Open, High, Low, Close, Volume;

            public Record(string symbol, DateTime date, decimal open, decimal high, decimal low, decimal close, decimal volume) {
                Symbol = symbol;
                Date = date;
                Open = open;
                High = high;
                Low = low;
                Close = close;
                Volume = volume;
            }

            public void SetData(string symbol, DateTime date, decimal open, decimal high, decimal low, decimal close, decimal volume) {
                Symbol = symbol;
                Date = date;
                Open = open;
                High = high;
                Low = low;
                Close = close;
                Volume = volume;
            }
        }

        #endregion

        #region Constants

        private const string FIELD_DATE = "Date";
        private const string FIELD_SYMBOL = "Symbol";
        private const string FIELD_OPEN = "Open";
        private const string FIELD_HIGH = "High";
        private const string FIELD_LOW = "Low";
        private const string FIELD_CLOSE = "Close";
        private const string FIELD_VOLUME = "Volume";

        #endregion

        #region Fields

        Uri vCDDUri;
        string vCSVPath;
        DateTimeKind vTimeKind;

        #endregion

        #region Properties

        public override DateTimeKind TimeKind { get { return vTimeKind; } }

        #endregion

        #region Public methods

        public CDDDataProvider(Uri cdduri, TimeSpan interval, DateTimeKind timekind = DateTimeKind.Utc) : base(interval) {
            if (cdduri.IsFile) {
                vCSVPath = cdduri.AbsolutePath;
                vTimeKind = timekind;
                if (!File.Exists(vCSVPath)) throw new IOException($"File not found {vCSVPath}");
            } else {
                vCDDUri = cdduri;
                vCSVPath = DownloadFile(vCDDUri);
                vTimeKind = timekind;
                if (vCSVPath == null) throw new ApplicationException($"Download failed {vCDDUri}");
            }
        }

        public CDDDataProvider(string csvpath, TimeSpan interval, DateTimeKind timekind = DateTimeKind.Utc) : base(interval) {
            vCSVPath = csvpath;
            vTimeKind = timekind;
            if (!File.Exists(vCSVPath)) throw new IOException($"File not found {vCSVPath}");
        }

        public new void Dispose() {
            if (vCDDUri != null && File.Exists(vCSVPath)) {
                File.Delete(vCSVPath);
            }
            base.Dispose();
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Read records from file and add to cache
        /// Data is sorted by date in descending order
        /// Dates and intervals in the cache should be as close as possible to the requested dates and the requested interval
        /// Start by finding the first target date (to-date) or a date as close as possible to the target date                    
        /// Then set next target date so that the interval is as close as possible to the requested interval
        /// Data between two dates is accumulated and added to cache
        /// </summary>
        protected override bool ReadData(DateTime from, DateTime to, ref bool moredatapre, ref bool moredatapost) {            
            if (!File.Exists(vCSVPath)) return false;
            bool datachanged = false;
            StreamReader reader;
            // Read data from file                
            using (reader = new StreamReader(vCSVPath)) {
                // Skip timestamp record
                reader.ReadLine();
                // Read column positions
                int ixdate, ixsymbol, ixopen, ixhigh, ixlow, ixclose, ixvolume;
                if (!ReadColumns(reader.ReadLine(), out ixdate, out ixsymbol, out ixopen, out ixhigh, out ixlow, out ixclose, out ixvolume)) return false;

                // Initialise variables                    
                Record record, newrecord, prevrecord, firstrecord;
                record = newrecord = firstrecord = prevrecord = new Record(string.Empty, DateTime.MaxValue, 0, decimal.MinValue, decimal.MaxValue, 0, 0);
                newrecord.Date = to; // To-date is first target date to find
                DateTime lastdate = DateTime.MaxValue;
                bool beginofstream = true;
                bool isfirstrecord, isfirstdate;
                isfirstrecord = isfirstdate = true;
                
                while (!reader.EndOfStream) {
                    // Read current record and parse into data structure
                    if (!ReadRecord(reader.ReadLine(), ref record, ixdate, ixsymbol, ixopen, ixhigh, ixlow, ixclose, ixvolume)) continue;
                    // Update more-data flag if first record from file is cached
                    if (beginofstream && record.Date <= to) moredatapost = false;
                    beginofstream = false;
                    if (record.Date >= to + Interval) continue; // Include only records until to-date plus interval
                    if (record.Date < from && (prevrecord.Date - newrecord.Date) > (newrecord.Date - record.Date)) break; // Include only records from from-date, except the previous date is further away from the target date
                    if (isfirstrecord) {
                        // Data of first record might not need to be accumulated if it's not in the interval, so save record for later
                        firstrecord.SetData(record.Symbol, record.Date, record.Open, record.High, record.Low, record.Close, record.Volume);
                    } else {
                        // Accumulate OHLC data into new record
                        newrecord.SetData(record.Symbol, newrecord.Date, newrecord.Open, Math.Max(newrecord.High, record.High), Math.Min(newrecord.Low, record.Low), newrecord.Close != 0 ? newrecord.Close : record.Close, newrecord.Volume + record.Volume);
                    }
                    // Save new record when target date is found or crossed or when end of file is reached and date is close enough to target date
                    if (record.Date <= newrecord.Date || reader.EndOfStream && (newrecord.Date - record.Date) < Interval / 2) {
                        // If date from previous record is closer to target date, then save new record with data from previous record
                        if (!isfirstrecord && prevrecord.Date != lastdate && (prevrecord.Date - newrecord.Date) <= (newrecord.Date - record.Date)) {
                            if (SaveNewRecord(ref prevrecord, ref prevrecord, ref firstrecord, ref lastdate, ref isfirstdate)) {
                                datachanged = true;
                            }
                            // Initialise next new record and target date using previous record and accumulate with data from current record
                            newrecord.SetData(prevrecord.Symbol, GetNextDate(prevrecord.Date, newrecord.Date), 0, record.High, record.Low, record.Close, record.Volume);
                            // Continue if current date hasn't crossed new target data
                            if (record.Date > newrecord.Date) {
                                prevrecord.SetData(record.Symbol, record.Date, record.Open, newrecord.High, newrecord.Low, newrecord.Close, newrecord.Volume);
                                continue;
                            }
                        }
                        // Save new record with current data
                        if (SaveNewRecord(ref newrecord, ref record, ref firstrecord, ref lastdate, ref isfirstdate)) {
                            datachanged = true;
                        }
                        // Initialise next new record and target date using current record
                        newrecord.SetData(record.Symbol, GetNextDate(record.Date, newrecord.Date), 0, decimal.MinValue, decimal.MaxValue, 0, 0);
                    }
                    isfirstrecord = false;
                    prevrecord.SetData(record.Symbol, record.Date, record.Open, newrecord.High, newrecord.Low, newrecord.Close, newrecord.Volume);
                }
                // Update more-data flag if last record from file is cached
                if (reader.EndOfStream && record.Date >= from) moredatapre = false;
            }
            return datachanged;
        }

        protected override IMarketData ReadFirst() {
            if (!File.Exists(vCSVPath)) return null;
            var lines = File.ReadLines(vCSVPath);
            if (lines.Count() < 3) return null;
            var columns = lines.Skip(1).First();
            var line = lines.Last();
            int ixdate, ixsymbol, ixopen, ixhigh, ixlow, ixclose, ixvolume;
            Record record = new Record(string.Empty, DateTime.MaxValue, 0, decimal.MinValue, decimal.MaxValue, 0, 0);
            if (!ReadColumns(columns, out ixdate, out ixsymbol, out ixopen, out ixhigh, out ixlow, out ixclose, out ixvolume)) return null;
            if (!ReadRecord(line, ref record, ixdate, ixsymbol, ixopen, ixhigh, ixlow, ixclose, ixvolume)) return null;
            return new PriceData(record.Symbol, record.Date, record.Open, record.Close, record.High, record.Low, record.Volume);
        }

        protected override IMarketData ReadLast() {
            if (!File.Exists(vCSVPath)) return null;
            StreamReader reader;
            string columns;
            string line;
            using (reader = new StreamReader(vCSVPath)) {
                reader.ReadLine();
                if (reader.EndOfStream) return null;
                columns = reader.ReadLine();
                if (reader.EndOfStream) return null;
                line = reader.ReadLine();
            }
            int ixdate, ixsymbol, ixopen, ixhigh, ixlow, ixclose, ixvolume;
            Record record = new Record(string.Empty, DateTime.MaxValue, 0, decimal.MinValue, decimal.MaxValue, 0, 0);
            if (!ReadColumns(columns, out ixdate, out ixsymbol, out ixopen, out ixhigh, out ixlow, out ixclose, out ixvolume)) return null;
            if (!ReadRecord(line, ref record, ixdate, ixsymbol, ixopen, ixhigh, ixlow, ixclose, ixvolume)) return null;
            return new PriceData(record.Symbol, record.Date, record.Open, record.Close, record.High, record.Low, record.Volume);
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Create new price record and add to cache
        /// </summary>
        private bool SaveNewRecord(ref Record newrecord, ref Record record, ref Record firstrecord, ref DateTime lastdate, ref bool isfirstdate) {
            // Check if first record should be included in the interval of the first date and accumulate data if this is the case
            if (isfirstdate && record.Date + Interval > firstrecord.Date) {
                newrecord.Low = Math.Min(newrecord.Low, firstrecord.Low);
                newrecord.High = Math.Max(newrecord.High, firstrecord.High);
                newrecord.Volume += firstrecord.Volume;
                newrecord.Close = firstrecord.Close;
            }
            isfirstdate = false;
            lastdate = record.Date;
            return AddToCache(new PriceData(record.Symbol, record.Date, record.Open, newrecord.Close != 0 ? newrecord.Close : record.Close, newrecord.High, newrecord.Low, newrecord.Volume));
        }

        /// <summary>
        /// Calculate next target date using the interval
        /// If the last target date didn't match the actual date then the interval has to be adjusted so the distance to the next date is not too short 
        /// </summary>        
        private DateTime GetNextDate(DateTime date, DateTime targetdate) {
            return date < targetdate ? targetdate - Interval * (1 + Math.Round((targetdate - date) / Interval)) : targetdate - Interval;
        }

        private bool ReadColumns(string header, out int ixdate, out int ixsymbol, out int ixopen, out int ixhigh, out int ixlow, out int ixclose, out int ixvolume) {
            var columns = header.Split(',').ToArray();
            ixdate = Array.IndexOf(columns, FIELD_DATE);
            ixsymbol = Array.IndexOf(columns, FIELD_SYMBOL);
            ixopen = Array.IndexOf(columns, FIELD_OPEN);
            ixhigh = Array.IndexOf(columns, FIELD_HIGH);
            ixlow = Array.IndexOf(columns, FIELD_LOW);
            ixclose = Array.IndexOf(columns, FIELD_CLOSE);
            ixvolume = Array.IndexOf(columns, FIELD_VOLUME);
            return ixdate >= 0 && ixsymbol >= 0 && ixopen >= 0 && ixhigh >= 0 && ixlow >= 0 && ixclose >= 0 && ixvolume >= 0;
        }

        private bool ReadRecord(string line, ref Record record, int ixdate, int ixsymbol, int ixopen, int ixhigh, int ixlow, int ixclose, int ixvolume) {
            DateTime date;
            double closef, highf, lowf, volumef, openf;
            string symbol;
            var data = line.Split(',');
            if (data.Length < 8) return false;
            if (!DateTime.TryParse(data[ixdate], out date)) return false;
            symbol = data[ixsymbol];
            if (!double.TryParse(data[ixopen], out openf)) return false;
            if (!double.TryParse(data[ixclose], out closef)) return false;
            if (!double.TryParse(data[ixhigh], out highf)) return false;
            if (!double.TryParse(data[ixlow], out lowf)) return false;
            if (!double.TryParse(data[ixvolume], out volumef)) return false;
            record.SetData(symbol, vTimeKind == DateTimeKind.Local ? date.ToLocalTime() : new DateTime(date.Ticks, DateTimeKind.Utc), (decimal)openf, (decimal)highf, (decimal)lowf, (decimal)closef, (decimal)volumef);
            return true;
        }

        private string DownloadFile(Uri uri) {
            if (uri == null) return null;
            using (WebClient client = new WebClient()) {
                try {
                    var path = Path.GetTempFileName();
                    client.DownloadFile(uri, path);
                    return path;
                } catch (Exception) {
                    return null;
                }
            }
        }

        #endregion

    }
}
