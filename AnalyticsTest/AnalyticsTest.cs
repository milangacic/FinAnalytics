using Microsoft.VisualStudio.TestTools.UnitTesting;
using AnalyticsEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticsEngineTest {    
    [TestClass]
    public class AnalyticsTest {

        #region Constants

        const string PATH_GEMINI_1MIN = "Data/Testdata_gemini_2021_1min.csv";
        const string PATH_1HR_REG = "Data/Testdata_1h_reg_small.csv";
        const string PATH_353HR_UNREG = "Data/Testdata_3-5-3h_unreg.csv";

        const byte PRECISION = 6;

        #endregion

        [TestMethod]
        [TestCategory("Real data")]
        public void AnalyticsRealDataTest() {
            // 1 minute steps from 1 minute data (Gemini 2021)
            using (var dataprovider = new CDDDataProvider(PATH_GEMINI_1MIN, TimeSpan.FromMinutes(1))) {
                // EMA 20 && EMA Daily 20
                using (var ema20 = new EMA(dataprovider, 20)) {
                    using (var ema5d = new EMADaily(dataprovider, 5)) {
                        // 01/04/2021 - 03/04/2021 @ 01/04/2021 11:04
                        CheckData(ema20, new DateTime(2021, 04, 01), new DateTime(2021, 04, 03), new DateTime(2021, 04, 01, 11, 04, 00), 1928.644293M);
                        // 01/04/2021 - 01/06/2021 @ 01/04/2021 11:04
                        CheckData(ema5d, new DateTime(2021, 04, 01), new DateTime(2021, 06, 01), new DateTime(2021, 04, 01, 11, 04, 00), 1794.255525M);
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("Functionality")]
        public void AnalyticsLengthTest() {
            // Change of length should clear cache and lead to different key ratios
            using (var dataprovider = new CDDDataProvider(PATH_GEMINI_1MIN, TimeSpan.FromMinutes(1))) {
                // EMA 20 vs EMA 10
                using (var ema20 = new EMA(dataprovider, 20)) {
                    using (var ema10 = new EMA(dataprovider, 10)) {
                        var data10 = ema10.GetAnalyticsData(new DateTime(2021, 04, 01), new DateTime(2021, 06, 01));
                        var data20 = ema20.GetAnalyticsData(new DateTime(2021, 04, 01), new DateTime(2021, 06, 01));
                        Assert.IsTrue(!data10.SequenceEqual(data20));
                        ema20.Length = 10;
                        data20 = ema20.GetAnalyticsData(new DateTime(2021, 04, 01), new DateTime(2021, 06, 01));
                        Assert.IsTrue(data10.SequenceEqual(data20));
                    }
                }
                // SMA 20 vs SMA 10
                using (var sma20 = new SMA(dataprovider, 20)) {
                    using (var sma10 = new SMA(dataprovider, 10)) {
                        var data10 = sma10.GetAnalyticsData(new DateTime(2021, 04, 01), new DateTime(2021, 06, 01));
                        var data20 = sma20.GetAnalyticsData(new DateTime(2021, 04, 01), new DateTime(2021, 06, 01));
                        Assert.IsTrue(!data10.SequenceEqual(data20));
                        sma20.Length = 10;
                        data20 = sma20.GetAnalyticsData(new DateTime(2021, 04, 01), new DateTime(2021, 06, 01));
                        Assert.IsTrue(data10.SequenceEqual(data20));
                    }
                }
                // RSI 20 vs RSI 10
                using (var rsi20 = new RSI(dataprovider, 20)) {
                    using (var rsi10 = new RSI(dataprovider, 10)) {
                        var data10 = rsi10.GetAnalyticsData(new DateTime(2021, 04, 01), new DateTime(2021, 06, 01));
                        var data20 = rsi20.GetAnalyticsData(new DateTime(2021, 04, 01), new DateTime(2021, 06, 01));
                        Assert.IsTrue(!data10.SequenceEqual(data20));
                        rsi20.Length = 10;
                        data20 = rsi20.GetAnalyticsData(new DateTime(2021, 04, 01), new DateTime(2021, 06, 01));
                        Assert.IsTrue(data10.SequenceEqual(data20));
                    }
                }
            }        
        }

        [TestMethod]
        [TestCategory("Functionality")]
        public void AnalyticsIntervalTest() {
            // Change of interval should clear cache
            using (var dataprovider = new CDDDataProvider(PATH_GEMINI_1MIN, TimeSpan.FromMinutes(1))) {
                // EMA 5 vs EMA Daily 5
                // EMA 5 and EMA Daily 5 should be the same with daily interval
                using (var ema5d = new EMADaily(dataprovider, 5)) {
                    using (var ema5 = new EMA(dataprovider, 5)) {
                        var data5d = ema5d.GetAnalyticsData(new DateTime(2021, 04, 01), new DateTime(2021, 06, 01));
                        var data5 = ema5.GetAnalyticsData(new DateTime(2021, 04, 01), new DateTime(2021, 06, 01));
                        Assert.IsTrue(!data5.SequenceEqual(data5d));
                        dataprovider.Interval = TimeSpan.FromDays(1);
                        data5d = ema5d.GetAnalyticsData(new DateTime(2021, 04, 01), new DateTime(2021, 06, 01));
                        data5 = ema5.GetAnalyticsData(new DateTime(2021, 04, 01), new DateTime(2021, 06, 01));
                        Assert.IsTrue(data5.SequenceEqual(data5d));
                    }
                }
                // SMA 5 vs SMA 10
                using (var sma5 = new SMA(dataprovider, 5)) {
                    using (var sma10 = new SMA(dataprovider, 10)) {
                        dataprovider.Interval = TimeSpan.FromMinutes(1);
                        var data5 = sma5.GetAnalyticsData(new DateTime(2021, 04, 01), new DateTime(2021, 06, 01));
                        var data10 = sma10.GetAnalyticsData(new DateTime(2021, 04, 01), new DateTime(2021, 06, 01));
                        Assert.IsTrue(data5.Count() == data10.Count());
                        dataprovider.Interval = TimeSpan.FromMinutes(2);
                        Assert.IsTrue(data5.Count() == 0);
                        Assert.IsTrue(data10.Count() == 0);
                        data5 = sma5.GetAnalyticsData(new DateTime(2021, 04, 01), new DateTime(2021, 06, 01));
                        data10 = sma10.GetAnalyticsData(new DateTime(2021, 04, 01), new DateTime(2021, 06, 01));
                        Assert.IsTrue(data5.Count() == data10.Count());
                    }
                }
                // RSI 5 vs RSI 10
                using (var rsi5 = new RSI(dataprovider, 5)) {
                    using (var rsi10 = new RSI(dataprovider, 10)) {
                        dataprovider.Interval = TimeSpan.FromMinutes(1);
                        var data5 = rsi5.GetAnalyticsData(new DateTime(2021, 04, 01), new DateTime(2021, 06, 01));
                        var data10 = rsi10.GetAnalyticsData(new DateTime(2021, 04, 01), new DateTime(2021, 06, 01));
                        Assert.IsTrue(data5.Count() == data10.Count());
                        dataprovider.Interval = TimeSpan.FromMinutes(2);
                        Assert.IsTrue(data5.Count() == 0);
                        Assert.IsTrue(data10.Count() == 0);
                        data5 = rsi5.GetAnalyticsData(new DateTime(2021, 04, 01), new DateTime(2021, 06, 01));
                        data10 = rsi10.GetAnalyticsData(new DateTime(2021, 04, 01), new DateTime(2021, 06, 01));
                        Assert.IsTrue(data5.Count() == data10.Count());
                    }
                }
                // EMA 5 Diff interval change
                using (var ema5 = new EMA(dataprovider, 5)) {
                    using (var ema5diff = new Derivative(ema5, 1, TimeSpan.FromMinutes(1))) {
                        dataprovider.Interval = TimeSpan.FromMinutes(1);
                        var data1min = ema5diff.GetAnalyticsData(new DateTime(2021, 04, 01), new DateTime(2021, 06, 01)).ToList();
                        dataprovider.Interval = TimeSpan.FromMinutes(2);
                        var data2min = ema5diff.GetAnalyticsData(new DateTime(2021, 04, 01), new DateTime(2021, 06, 01)).ToList();
                        Assert.IsFalse(data1min.SequenceEqual(data2min));
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("Testcases")]
        public void SMATestcase01Test() {
            // Testcase 1 - Period fully covered -  SMA 5, 10 & 15
            using (var dataprovider = new CDDDataProvider(PATH_1HR_REG, TimeSpan.FromHours(1))) {
                using (var sma5 = new SMA(dataprovider, 5)) {
                    var data = sma5.GetAnalyticsData(new DateTime(2021, 8, 20, 1, 0, 0), new DateTime(2021, 8, 20, 10, 0, 0));
                    Assert.IsTrue(data.Count() == 10);
                    CheckData(data, new DateTime(2021, 8, 20, 1, 0, 0), 3197.446M);
                    CheckData(data, new DateTime(2021, 8, 20, 2, 0, 0), 3210.65M);
                    CheckData(data, new DateTime(2021, 8, 20, 3, 0, 0), 3220.094M);
                    CheckData(data, new DateTime(2021, 8, 20, 4, 0, 0), 3227.954M);
                    CheckData(data, new DateTime(2021, 8, 20, 5, 0, 0), 3227.142M);
                    CheckData(data, new DateTime(2021, 8, 20, 6, 0, 0), 3225.516M);
                    CheckData(data, new DateTime(2021, 8, 20, 7, 0, 0), 3221.982M);
                    CheckData(data, new DateTime(2021, 8, 20, 8, 0, 0), 3218.152M);
                    CheckData(data, new DateTime(2021, 8, 20, 9, 0, 0), 3214.584M);
                    CheckData(data, new DateTime(2021, 8, 20, 10, 0, 0), 3212.77M);
                }

                using (var sma10 = new SMA(dataprovider, 10)) {
                    var data = sma10.GetAnalyticsData(new DateTime(2021, 8, 20, 1, 0, 0), new DateTime(2021, 8, 20, 10, 0, 0));
                    Assert.IsTrue(data.Count() == 10);
                    CheckData(data, new DateTime(2021, 8, 20, 1, 0, 0), 3152.518M);
                    CheckData(data, new DateTime(2021, 8, 20, 2, 0, 0), 3170.775M);
                    CheckData(data, new DateTime(2021, 8, 20, 3, 0, 0), 3187.59M);
                    CheckData(data, new DateTime(2021, 8, 20, 4, 0, 0), 3196.583M);
                    CheckData(data, new DateTime(2021, 8, 20, 5, 0, 0), 3204.342M);
                    CheckData(data, new DateTime(2021, 8, 20, 6, 0, 0), 3211.481M);
                    CheckData(data, new DateTime(2021, 8, 20, 7, 0, 0), 3216.316M);
                    CheckData(data, new DateTime(2021, 8, 20, 8, 0, 0), 3219.123M);
                    CheckData(data, new DateTime(2021, 8, 20, 9, 0, 0), 3221.269M);
                    CheckData(data, new DateTime(2021, 8, 20, 10, 0, 0), 3219.956M);
                }

                using (var sma15 = new SMA(dataprovider, 15)) {
                    var data = sma15.GetAnalyticsData(new DateTime(2021, 8, 20, 1, 0, 0), new DateTime(2021, 8, 20, 10, 0, 0));
                    Assert.IsTrue(data.Count() == 10);
                    CheckData(data, new DateTime(2021, 8, 20, 1, 0, 0), 3108.797333M);
                    CheckData(data, new DateTime(2021, 8, 20, 2, 0, 0), 3125.858667M);
                    CheckData(data, new DateTime(2021, 8, 20, 3, 0, 0), 3140.530667M);
                    CheckData(data, new DateTime(2021, 8, 20, 4, 0, 0), 3154.050667M);
                    CheckData(data, new DateTime(2021, 8, 20, 5, 0, 0), 3166.181333M);
                    CheckData(data, new DateTime(2021, 8, 20, 6, 0, 0), 3176.850667M);
                    CheckData(data, new DateTime(2021, 8, 20, 7, 0, 0), 3187.844M);
                    CheckData(data, new DateTime(2021, 8, 20, 8, 0, 0), 3197.777333M);
                    CheckData(data, new DateTime(2021, 8, 20, 9, 0, 0), 3202.583333M);
                    CheckData(data, new DateTime(2021, 8, 20, 10, 0, 0), 3207.151333M);
                }
            }
        }

        [TestMethod]
        [TestCategory("Testcases")]
        public void EMATestcase01Test() {
            // Testcase 1 - Period fully covered -  EMA 5, 10 & 15
            using (var dataprovider = new CDDDataProvider(PATH_1HR_REG, TimeSpan.FromHours(1))) {
                using (var ema5 = new EMA(dataprovider, 5)) {
                    var data = ema5.GetAnalyticsData(new DateTime(2021, 8, 20, 1, 0, 0), new DateTime(2021, 8, 20, 10, 0, 0));
                    Assert.IsTrue(data.Count() == 10);
                    CheckData(data, new DateTime(2021, 8, 20, 1, 0, 0), 3200.251333M);
                    CheckData(data, new DateTime(2021, 8, 20, 2, 0, 0), 3210.474222M);
                    CheckData(data, new DateTime(2021, 8, 20, 3, 0, 0), 3212.922815M);
                    CheckData(data, new DateTime(2021, 8, 20, 4, 0, 0), 3216.71521M);
                    CheckData(data, new DateTime(2021, 8, 20, 5, 0, 0), 3219.476807M);
                    CheckData(data, new DateTime(2021, 8, 20, 6, 0, 0), 3222.831204M);
                    CheckData(data, new DateTime(2021, 8, 20, 7, 0, 0), 3219.63747M);
                    CheckData(data, new DateTime(2021, 8, 20, 8, 0, 0), 3212.648313M);
                    CheckData(data, new DateTime(2021, 8, 20, 9, 0, 0), 3210.585542M);
                    CheckData(data, new DateTime(2021, 8, 20, 10, 0, 0), 3212.367028M);
                }

                using (var ema10 = new EMA(dataprovider, 10)) {
                    var data = ema10.GetAnalyticsData(new DateTime(2021, 8, 20, 1, 0, 0), new DateTime(2021, 8, 20, 10, 0, 0));
                    Assert.IsTrue(data.Count() == 10);
                    CheckData(data, new DateTime(2021, 8, 20, 1, 0, 0), 3154.240818M);
                    CheckData(data, new DateTime(2021, 8, 20, 2, 0, 0), 3168.182488M);
                    CheckData(data, new DateTime(2021, 8, 20, 3, 0, 0), 3177.20749M);
                    CheckData(data, new DateTime(2021, 8, 20, 4, 0, 0), 3185.769764M);
                    CheckData(data, new DateTime(2021, 8, 20, 5, 0, 0), 3192.902535M);
                    CheckData(data, new DateTime(2021, 8, 20, 6, 0, 0), 3199.563892M);
                    CheckData(data, new DateTime(2021, 8, 20, 7, 0, 0), 3202.052275M);
                    CheckData(data, new DateTime(2021, 8, 20, 8, 0, 0), 3201.437316M);
                    CheckData(data, new DateTime(2021, 8, 20, 9, 0, 0), 3202.350531M);
                    CheckData(data, new DateTime(2021, 8, 20, 10, 0, 0), 3204.819526M);
                }

                using (var ema15 = new EMA(dataprovider, 15)) {
                    var data = ema15.GetAnalyticsData(new DateTime(2021, 8, 20, 1, 0, 0), new DateTime(2021, 8, 20, 10, 0, 0));
                    Assert.IsTrue(data.Count() == 10);
                    CheckData(data, new DateTime(2021, 8, 20, 1, 0, 0), 3110.162083M);
                    CheckData(data, new DateTime(2021, 8, 20, 2, 0, 0), 3125.256823M);
                    CheckData(data, new DateTime(2021, 8, 20, 3, 0, 0), 3136.82722M);
                    CheckData(data, new DateTime(2021, 8, 20, 4, 0, 0), 3147.761318M);
                    CheckData(data, new DateTime(2021, 8, 20, 5, 0, 0), 3157.416153M);
                    CheckData(data, new DateTime(2021, 8, 20, 6, 0, 0), 3166.431634M);
                    CheckData(data, new DateTime(2021, 8, 20, 7, 0, 0), 3172.28393M);
                    CheckData(data, new DateTime(2021, 8, 20, 8, 0, 0), 3175.582188M);
                    CheckData(data, new DateTime(2021, 8, 20, 9, 0, 0), 3179.441915M);
                    CheckData(data, new DateTime(2021, 8, 20, 10, 0, 0), 3184.002925M);
                }
            }
        }

        [TestMethod]
        [TestCategory("Testcases")]
        public void RSITestcase01Test() {
            // Testcase 1 - Period fully covered -  RSI 5, 10 & 15
            using (var dataprovider = new CDDDataProvider(PATH_1HR_REG, TimeSpan.FromHours(1))) {
                using (var rsi5 = new RSI(dataprovider, 5)) {
                    var data = rsi5.GetAnalyticsData(new DateTime(2021, 8, 20, 1, 0, 0), new DateTime(2021, 8, 20, 10, 0, 0));
                    Assert.IsTrue(data.Count() == 10);
                    CheckData(data, new DateTime(2021, 8, 20, 1, 0, 0), 100M);
                    CheckData(data, new DateTime(2021, 8, 20, 2, 0, 0), 89.75627523M);
                    CheckData(data, new DateTime(2021, 8, 20, 3, 0, 0), 71.89099175M);
                    CheckData(data, new DateTime(2021, 8, 20, 4, 0, 0), 74.97131848M);
                    CheckData(data, new DateTime(2021, 8, 20, 5, 0, 0), 75.33627654M);
                    CheckData(data, new DateTime(2021, 8, 20, 6, 0, 0), 77.94366723M);
                    CheckData(data, new DateTime(2021, 8, 20, 7, 0, 0), 52.87336129M);
                    CheckData(data, new DateTime(2021, 8, 20, 8, 0, 0), 38.88167126M);
                    CheckData(data, new DateTime(2021, 8, 20, 9, 0, 0), 48.06110529M);
                    CheckData(data, new DateTime(2021, 8, 20, 10, 0, 0), 57.71229921M);
                }

                using (var rsi10 = new RSI(dataprovider, 10)) {
                    var data = rsi10.GetAnalyticsData(new DateTime(2021, 8, 20, 1, 0, 0), new DateTime(2021, 8, 20, 10, 0, 0));
                    Assert.IsTrue(data.Count() == 10);
                    CheckData(data, new DateTime(2021, 8, 20, 1, 0, 0), 91.10921167M);
                    CheckData(data, new DateTime(2021, 8, 20, 2, 0, 0), 88.02561395M);
                    CheckData(data, new DateTime(2021, 8, 20, 3, 0, 0), 82.03823497M);
                    CheckData(data, new DateTime(2021, 8, 20, 4, 0, 0), 82.68552399M);
                    CheckData(data, new DateTime(2021, 8, 20, 5, 0, 0), 82.76009407M);
                    CheckData(data, new DateTime(2021, 8, 20, 6, 0, 0), 83.27905124M);
                    CheckData(data, new DateTime(2021, 8, 20, 7, 0, 0), 74.35559689M);
                    CheckData(data, new DateTime(2021, 8, 20, 8, 0, 0), 67.19531438M);
                    CheckData(data, new DateTime(2021, 8, 20, 9, 0, 0), 68.96928221M);
                    CheckData(data, new DateTime(2021, 8, 20, 10, 0, 0), 71.08157677M);
                }

                using (var rsi15 = new RSI(dataprovider, 15)) {
                    var data = rsi15.GetAnalyticsData(new DateTime(2021, 8, 20, 1, 0, 0), new DateTime(2021, 8, 20, 10, 0, 0));
                    Assert.IsTrue(data.Count() == 10);
                    CheckData(data, new DateTime(2021, 8, 20, 1, 0, 0), 84.66461282M);
                    CheckData(data, new DateTime(2021, 8, 20, 2, 0, 0), 82.75586784M);
                    CheckData(data, new DateTime(2021, 8, 20, 3, 0, 0), 79.05009231M);
                    CheckData(data, new DateTime(2021, 8, 20, 4, 0, 0), 79.53576399M);
                    CheckData(data, new DateTime(2021, 8, 20, 5, 0, 0), 79.59052592M);
                    CheckData(data, new DateTime(2021, 8, 20, 6, 0, 0), 79.96311864M);
                    CheckData(data, new DateTime(2021, 8, 20, 7, 0, 0), 74.71911792M);
                    CheckData(data, new DateTime(2021, 8, 20, 8, 0, 0), 70.29815752M);
                    CheckData(data, new DateTime(2021, 8, 20, 9, 0, 0), 71.27122976M);
                    CheckData(data, new DateTime(2021, 8, 20, 10, 0, 0), 72.4469581M);
                }
            }
        }

        [TestMethod]
        [TestCategory("Testcases")]
        public void DiffTestcase01Test() {
            // Testcase 1 - Period fully covered -  EMA (15) Diff, EMA (15) Diff2 with dt = 1h
            using (var dataprovider = new CDDDataProvider(PATH_1HR_REG, TimeSpan.FromHours(1))) {
                using (var ema15 = new EMA(dataprovider, 15)) {
                    using (var ema15diff = new Derivative(ema15, 1, TimeSpan.FromHours(1))) {
                        var data = ema15diff.GetAnalyticsData(new DateTime(2021, 8, 20, 1, 0, 0), new DateTime(2021, 8, 20, 10, 0, 0));
                        Assert.IsTrue(data.Count() == 10);
                        CheckData(data, new DateTime(2021, 8, 20, 1, 0, 0), 0);
                        CheckData(data, new DateTime(2021, 8, 20, 2, 0, 0), 15.09473958M);
                        CheckData(data, new DateTime(2021, 8, 20, 3, 0, 0), 11.57039714M);
                        CheckData(data, new DateTime(2021, 8, 20, 4, 0, 0), 10.93409749M);
                        CheckData(data, new DateTime(2021, 8, 20, 5, 0, 0), 9.654835307M);
                        CheckData(data, new DateTime(2021, 8, 20, 6, 0, 0), 9.015480893M);
                        CheckData(data, new DateTime(2021, 8, 20, 7, 0, 0), 5.852295782M);
                        CheckData(data, new DateTime(2021, 8, 20, 8, 0, 0), 3.298258809M);
                        CheckData(data, new DateTime(2021, 8, 20, 9, 0, 0), 3.859726458M);
                        CheckData(data, new DateTime(2021, 8, 20, 10, 0, 0), 4.561010651M);

                        using (var ema15diff2 = new Derivative(ema15diff, 1, TimeSpan.FromHours(1))) {
                            var data2 = ema15diff2.GetAnalyticsData(new DateTime(2021, 8, 20, 1, 0, 0), new DateTime(2021, 8, 20, 10, 0, 0));
                            Assert.IsTrue(data.Count() == 10);
                            CheckData(data2, new DateTime(2021, 8, 20, 1, 0, 0), 0);
                            CheckData(data2, new DateTime(2021, 8, 20, 2, 0, 0), 15.09473958M);
                            CheckData(data2, new DateTime(2021, 8, 20, 3, 0, 0), -3.524342448M);
                            CheckData(data2, new DateTime(2021, 8, 20, 4, 0, 0), -0.636299642M);
                            CheckData(data2, new DateTime(2021, 8, 20, 5, 0, 0), -1.279262187M);
                            CheckData(data2, new DateTime(2021, 8, 20, 6, 0, 0), -0.639354413M);
                            CheckData(data2, new DateTime(2021, 8, 20, 7, 0, 0), -3.163185112M);
                            CheckData(data2, new DateTime(2021, 8, 20, 8, 0, 0), -2.554036973M);
                            CheckData(data2, new DateTime(2021, 8, 20, 9, 0, 0), 0.561467649M);
                            CheckData(data2, new DateTime(2021, 8, 20, 10, 0, 0), 0.701284193M);
                        }
                    }

                    using (var ema15difflg3 = new Derivative(ema15, 1, TimeSpan.FromHours(1), 3)) {
                        var data = ema15difflg3.GetAnalyticsData(new DateTime(2021, 8, 20, 1, 0, 0), new DateTime(2021, 8, 20, 10, 0, 0));
                        Assert.IsTrue(data.Count() == 10);
                        CheckData(data, new DateTime(2021, 8, 20, 1, 0, 0), 0);
                        CheckData(data, new DateTime(2021, 8, 20, 2, 0, 0), 15.09473958M);
                        CheckData(data, new DateTime(2021, 8, 20, 3, 0, 0), 13.33256836M);
                        CheckData(data, new DateTime(2021, 8, 20, 4, 0, 0), 11.25224731M);
                        CheckData(data, new DateTime(2021, 8, 20, 5, 0, 0), 10.2944664M);
                        CheckData(data, new DateTime(2021, 8, 20, 6, 0, 0), 9.3351581M);
                        CheckData(data, new DateTime(2021, 8, 20, 7, 0, 0), 7.433888338M);
                        CheckData(data, new DateTime(2021, 8, 20, 8, 0, 0), 4.575277295M);
                        CheckData(data, new DateTime(2021, 8, 20, 9, 0, 0), 3.578992633M);
                        CheckData(data, new DateTime(2021, 8, 20, 10, 0, 0), 4.210368554M);

                        using (var ema15diff2lg3 = new Derivative(ema15difflg3, 1, TimeSpan.FromHours(1), 3)) {
                            var data2 = ema15diff2lg3.GetAnalyticsData(new DateTime(2021, 8, 20, 1, 0, 0), new DateTime(2021, 8, 20, 10, 0, 0));
                            Assert.IsTrue(data.Count() == 10);                            
                            CheckData(data2, new DateTime(2021, 8, 20, 1, 0, 0), 0);
                            CheckData(data2, new DateTime(2021, 8, 20, 2, 0, 0), 15.09473958M);
                            CheckData(data2, new DateTime(2021, 8, 20, 3, 0, 0), 6.66628418M);
                            CheckData(data2, new DateTime(2021, 8, 20, 4, 0, 0), -1.921246134M);
                            CheckData(data2, new DateTime(2021, 8, 20, 5, 0, 0), -1.51905098M);
                            CheckData(data2, new DateTime(2021, 8, 20, 6, 0, 0), -0.958544607M);
                            CheckData(data2, new DateTime(2021, 8, 20, 7, 0, 0), -1.430289031M);
                            CheckData(data2, new DateTime(2021, 8, 20, 8, 0, 0), -2.379940402M);
                            CheckData(data2, new DateTime(2021, 8, 20, 9, 0, 0), -1.927447852M);
                            CheckData(data2, new DateTime(2021, 8, 20, 10, 0, 0), -0.182454371M);
                        }
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("Testcases")]
        public void SMATestcase02Test() {
            // Testcase 2 - Period not fully covered -  SMA 5, 10 & 15
            using (var dataprovider = new CDDDataProvider(PATH_1HR_REG, TimeSpan.FromHours(1))) {
                using (var sma5 = new SMA(dataprovider, 5)) {
                    var data = sma5.GetAnalyticsData(new DateTime(2021, 8, 19, 1, 0, 0), new DateTime(2021, 8, 19, 10, 0, 0));
                    Assert.IsNull(data.FirstOrDefault(item => item.Time >= new DateTime(2021, 8, 19, 1, 0, 0) && item.Time <= new DateTime(2021, 8, 19, 3, 0, 0)));
                    Assert.IsTrue(data.Count() == 7);
                    CheckData(data, new DateTime(2021, 8, 19, 4, 0, 0), 3006.656M);
                    CheckData(data, new DateTime(2021, 8, 19, 5, 0, 0), 2998.724M);
                    CheckData(data, new DateTime(2021, 8, 19, 6, 0, 0), 2992.602M);
                    CheckData(data, new DateTime(2021, 8, 19, 7, 0, 0), 2989.606M);
                    CheckData(data, new DateTime(2021, 8, 19, 8, 0, 0), 2996.486M);
                    CheckData(data, new DateTime(2021, 8, 19, 9, 0, 0), 3001.16M);
                    CheckData(data, new DateTime(2021, 8, 19, 10, 0, 0), 2996.74M);
                }

                using (var sma10 = new SMA(dataprovider, 10)) {
                    var data = sma10.GetAnalyticsData(new DateTime(2021, 8, 19, 1, 0, 0), new DateTime(2021, 8, 19, 10, 0, 0));
                    Assert.IsNull(data.FirstOrDefault(item => item.Time >= new DateTime(2021, 8, 19, 1, 0, 0) && item.Time <= new DateTime(2021, 8, 19, 8, 0, 0)));
                    Assert.IsTrue(data.Count() == 2);
                    CheckData(data, new DateTime(2021, 8, 19, 9, 0, 0), 3003.908M);
                    CheckData(data, new DateTime(2021, 8, 19, 10, 0, 0), 2997.732M);
                }

                using (var sma15 = new SMA(dataprovider, 15)) {
                    var data = sma15.GetAnalyticsData(new DateTime(2021, 8, 19, 1, 0, 0), new DateTime(2021, 8, 19, 10, 0, 0));
                    Assert.IsTrue(data.Count() == 0);
                }
            }
        }

        [TestMethod]
        [TestCategory("Testcases")]
        public void EMATestcase02Test() {
            using (var dataprovider = new CDDDataProvider(PATH_1HR_REG, TimeSpan.FromHours(1))) {
                // Testcase 2 - Period not fully covered -  EMA 5, 10 & 15
                using (var ema5 = new EMA(dataprovider, 5)) {
                    var data = ema5.GetAnalyticsData(new DateTime(2021, 8, 19, 1, 0, 0), new DateTime(2021, 8, 19, 10, 0, 0));
                    Assert.IsNull(data.FirstOrDefault(item => item.Time >= new DateTime(2021, 8, 19, 1, 0, 0) && item.Time <= new DateTime(2021, 8, 19, 4, 0, 0)));
                    Assert.IsTrue(data.Count() == 6);
                    CheckData(data, new DateTime(2021, 8, 19, 5, 0, 0), 3006.774M);
                    CheckData(data, new DateTime(2021, 8, 19, 6, 0, 0), 3003.526M);
                    CheckData(data, new DateTime(2021, 8, 19, 7, 0, 0), 3000.587333M);
                    CheckData(data, new DateTime(2021, 8, 19, 8, 0, 0), 3000.951556M);
                    CheckData(data, new DateTime(2021, 8, 19, 9, 0, 0), 3002.42437M);
                    CheckData(data, new DateTime(2021, 8, 19, 10, 0, 0), 2996.586247M);
                }

                using (var ema10 = new EMA(dataprovider, 10)) {
                    var data = ema10.GetAnalyticsData(new DateTime(2021, 8, 19, 1, 0, 0), new DateTime(2021, 8, 19, 10, 0, 0));
                    Assert.IsTrue(data.Count() == 1);
                    Assert.IsNull(data.FirstOrDefault(item => item.Time >= new DateTime(2021, 8, 19, 1, 0, 0) && item.Time <= new DateTime(2021, 8, 19, 9, 0, 0)));
                    CheckData(data, new DateTime(2021, 8, 19, 10, 0, 0), 3000.453818M);
                }

                using (var ema15 = new EMA(dataprovider, 15)) {
                    var data = ema15.GetAnalyticsData(new DateTime(2021, 8, 19, 1, 0, 0), new DateTime(2021, 8, 19, 10, 0, 0));
                    Assert.IsTrue(data.Count() == 0);
                }
            }                
        }        

        [TestMethod]
        [TestCategory("Testcases")]
        public void RSITestcase02Test() {
            // Testcase 2 - Period not fully covered -  RSI 5, 10 & 15
            using (var dataprovider = new CDDDataProvider(PATH_1HR_REG, TimeSpan.FromHours(1))) {
                using (var rsi5 = new RSI(dataprovider, 5)) {
                    var data = rsi5.GetAnalyticsData(new DateTime(2021, 8, 19, 1, 0, 0), new DateTime(2021, 8, 19, 10, 0, 0));
                    Assert.IsTrue(data.Count() == 6);
                    Assert.IsNull(data.FirstOrDefault(item => item.Time >= new DateTime(2021, 8, 19, 1, 0, 0) && item.Time <= new DateTime(2021, 8, 19, 4, 0, 0)));
                    CheckData(data, new DateTime(2021, 8, 19, 5, 0, 0), 50.19838469M);
                    CheckData(data, new DateTime(2021, 8, 19, 6, 0, 0), 45.7255098M);
                    CheckData(data, new DateTime(2021, 8, 19, 7, 0, 0), 44.57146836M);
                    CheckData(data, new DateTime(2021, 8, 19, 8, 0, 0), 49.37017381M);
                    CheckData(data, new DateTime(2021, 8, 19, 9, 0, 0), 52.11367904M);
                    CheckData(data, new DateTime(2021, 8, 19, 10, 0, 0), 37.88520624M);
                }

                using (var rsi10 = new RSI(dataprovider, 10)) {
                    var data = rsi10.GetAnalyticsData(new DateTime(2021, 8, 19, 1, 0, 0), new DateTime(2021, 8, 19, 10, 0, 0));
                    Assert.IsTrue(data.Count() == 1);
                    Assert.IsNull(data.FirstOrDefault(item => item.Time >= new DateTime(2021, 8, 19, 1, 0, 0) && item.Time <= new DateTime(2021, 8, 19, 9, 0, 0)));
                    CheckData(data, new DateTime(2021, 8, 19, 10, 0, 0), 42.45842832M);
                }

                using (var rsi15 = new RSI(dataprovider, 15)) {
                    var data = rsi15.GetAnalyticsData(new DateTime(2021, 8, 19, 1, 0, 0), new DateTime(2021, 8, 19, 10, 0, 0));
                    Assert.IsTrue(data.Count() == 0);
                }
            }
        }

        [TestMethod]
        [TestCategory("Testcases")]
        public void DiffTestcase02Test() {
            using (var dataprovider = new CDDDataProvider(PATH_1HR_REG, TimeSpan.FromHours(1))) {
                // Testcase 2 - Period not fully covered -  EMA (5) Diff, EMA (5) Diff2 with dt = 1h
                using (var ema5 = new EMA(dataprovider, 5)) {
                    using (var ema5diff = new Derivative(ema5, 1, TimeSpan.FromHours(1))) {
                        var data = ema5diff.GetAnalyticsData(new DateTime(2021, 8, 19, 1, 0, 0), new DateTime(2021, 8, 19, 10, 0, 0));
                        Assert.IsTrue(data.Count() == 6);
                        CheckData(data, new DateTime(2021, 8, 19, 5, 0, 0), 0);
                        CheckData(data, new DateTime(2021, 8, 19, 6, 0, 0), -3.248M);
                        CheckData(data, new DateTime(2021, 8, 19, 7, 0, 0), -2.938666667M);
                        CheckData(data, new DateTime(2021, 8, 19, 8, 0, 0), 0.364222222M);
                        CheckData(data, new DateTime(2021, 8, 19, 9, 0, 0), 1.472814815M);
                        CheckData(data, new DateTime(2021, 8, 19, 10, 0, 0), -5.838123457M);

                        using (var ema5diff2 = new Derivative(ema5diff, 1, TimeSpan.FromHours(1))) {
                            var data2 = ema5diff2.GetAnalyticsData(new DateTime(2021, 8, 19, 1, 0, 0), new DateTime(2021, 8, 19, 10, 0, 0));
                            Assert.IsTrue(data.Count() == 6);
                            CheckData(data2, new DateTime(2021, 8, 19, 5, 0, 0), 0);
                            CheckData(data2, new DateTime(2021, 8, 19, 6, 0, 0), -3.248M);
                            CheckData(data2, new DateTime(2021, 8, 19, 7, 0, 0), 0.309333333M);
                            CheckData(data2, new DateTime(2021, 8, 19, 8, 0, 0), 3.302888889M);
                            CheckData(data2, new DateTime(2021, 8, 19, 9, 0, 0), 1.108592593M);
                            CheckData(data2, new DateTime(2021, 8, 19, 10, 0, 0), -7.310938272M);
                        }
                    }

                    using (var ema5difflg3 = new Derivative(ema5, 1, TimeSpan.FromHours(1), 3)) {
                        var data = ema5difflg3.GetAnalyticsData(new DateTime(2021, 8, 19, 1, 0, 0), new DateTime(2021, 8, 19, 10, 0, 0));
                        Assert.IsTrue(data.Count() == 6);
                        CheckData(data, new DateTime(2021, 8, 19, 5, 0, 0), 0);
                        CheckData(data, new DateTime(2021, 8, 19, 6, 0, 0), -3.248M);
                        CheckData(data, new DateTime(2021, 8, 19, 7, 0, 0), -3.093333333M);
                        CheckData(data, new DateTime(2021, 8, 19, 8, 0, 0), -1.287222222M);
                        CheckData(data, new DateTime(2021, 8, 19, 9, 0, 0), 0.918518519M);
                        CheckData(data, new DateTime(2021, 8, 19, 10, 0, 0), -2.182654321M);

                        using (var ema5diff2lg3 = new Derivative(ema5difflg3, 1, TimeSpan.FromHours(1), 3)) {
                            var data2 = ema5diff2lg3.GetAnalyticsData(new DateTime(2021, 8, 19, 1, 0, 0), new DateTime(2021, 8, 19, 10, 0, 0));
                            Assert.IsTrue(data.Count() == 6);
                            CheckData(data2, new DateTime(2021, 8, 19, 5, 0, 0), 0);
                            CheckData(data2, new DateTime(2021, 8, 19, 6, 0, 0), -3.248M);
                            CheckData(data2, new DateTime(2021, 8, 19, 7, 0, 0), -1.546666667M);
                            CheckData(data2, new DateTime(2021, 8, 19, 8, 0, 0), 0.980388889M);
                            CheckData(data2, new DateTime(2021, 8, 19, 9, 0, 0), 2.005925926M);
                            CheckData(data2, new DateTime(2021, 8, 19, 10, 0, 0), -0.447716049M);
                        }
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("Testcases")]
        public void SMATestcase03Test() {
            // Testcase 3 - Unregular data & not fully covered -  SMA 5, 10 & 15
            using (var dataprovider = new CDDDataProvider(PATH_353HR_UNREG, TimeSpan.FromHours(3))) {
                using (var sma5 = new SMA(dataprovider, 5)) {
                    var data = sma5.GetAnalyticsData(new DateTime(2021, 8, 20, 0, 0, 0), new DateTime(2021, 8, 21, 0, 0, 0));
                    Assert.IsTrue(data.Count() == 7);
                    CheckData(data, new DateTime(2021, 8, 20, 0, 0, 0), 3080.3M);
                    CheckData(data, new DateTime(2021, 8, 20, 3, 0, 0), 3123.528M);
                    CheckData(data, new DateTime(2021, 8, 20, 8, 0, 0), 3168.262M);
                    CheckData(data, new DateTime(2021, 8, 20, 11, 0, 0), 3199.902M);
                    CheckData(data, new DateTime(2021, 8, 20, 16, 0, 0), 3223.65M);
                    CheckData(data, new DateTime(2021, 8, 20, 19, 0, 0), 3228.368M);
                    CheckData(data, new DateTime(2021, 8, 21, 0, 0, 0), 3240.774M);
                }
                
                using (var sma10 = new SMA(dataprovider, 10)) {
                    var data = sma10.GetAnalyticsData(new DateTime(2021, 8, 20, 0, 0, 0), new DateTime(2021, 8, 21, 0, 0, 0));
                    Assert.IsTrue(data.Count() == 7);
                    CheckData(data, new DateTime(2021, 8, 20, 0, 0, 0), 3056.797M);
                    CheckData(data, new DateTime(2021, 8, 20, 3, 0, 0), 3078.587M);
                    CheckData(data, new DateTime(2021, 8, 20, 8, 0, 0), 3086.304M);
                    CheckData(data, new DateTime(2021, 8, 20, 11, 0, 0), 3103.849M);
                    CheckData(data, new DateTime(2021, 8, 20, 16, 0, 0), 3125.797M);
                    CheckData(data, new DateTime(2021, 8, 20, 19, 0, 0), 3154.334M);
                    CheckData(data, new DateTime(2021, 8, 21, 0, 0, 0), 3182.151M);
                }
                
                using (var sma15 = new SMA(dataprovider, 15)) {
                    var data = sma15.GetAnalyticsData(new DateTime(2021, 8, 20, 0, 0, 0), new DateTime(2021, 8, 21, 0, 0, 0));
                    Assert.IsTrue(data.Count() == 5);
                    Assert.IsNull(data.FirstOrDefault(item => item.Time >= new DateTime(2021, 8, 20, 0, 0, 0) && item.Time < new DateTime(2021, 8, 20, 8, 0, 0)));
                    CheckData(data, new DateTime(2021, 8, 20, 8, 0, 0), 3073.358667M);
                    CheckData(data, new DateTime(2021, 8, 20, 11, 0, 0), 3085.726667M);
                    CheckData(data, new DateTime(2021, 8, 20, 16, 0, 0), 3100.133333M);
                    CheckData(data, new DateTime(2021, 8, 20, 19, 0, 0), 3113.987333M);
                    CheckData(data, new DateTime(2021, 8, 21, 0, 0, 0), 3132.649333M);
                }
            }
        }

        [TestMethod]
        [TestCategory("Testcases")]
        public void EMATestcase03Test() {
            // Testcase 3 - Unregular data & not fully covered -  EMA 5, 10 & 15
            using (var dataprovider = new CDDDataProvider(PATH_353HR_UNREG, TimeSpan.FromHours(3))) {
                using (var ema5 = new EMA(dataprovider, 5)) {
                    var data = ema5.GetAnalyticsData(new DateTime(2021, 8, 20, 0, 0, 0), new DateTime(2021, 8, 21, 0, 0, 0));
                    Assert.IsTrue(data.Count() == 7);
                    CheckData(data, new DateTime(2021, 8, 20, 0, 0, 0), 3094.982667M);
                    CheckData(data, new DateTime(2021, 8, 20, 3, 0, 0), 3135.928444M);
                    CheckData(data, new DateTime(2021, 8, 20, 8, 0, 0), 3156.842296M);
                    CheckData(data, new DateTime(2021, 8, 20, 11, 0, 0), 3173.411531M);
                    CheckData(data, new DateTime(2021, 8, 20, 16, 0, 0), 3204.324354M);
                    CheckData(data, new DateTime(2021, 8, 20, 19, 0, 0), 3220.432903M);
                    CheckData(data, new DateTime(2021, 8, 21, 0, 0, 0), 3240.238602M);
                }
                
                using (var ema10 = new EMA(dataprovider, 10)) {
                    var data = ema10.GetAnalyticsData(new DateTime(2021, 8, 20, 0, 0, 0), new DateTime(2021, 8, 21, 0, 0, 0));
                    Assert.IsTrue(data.Count() == 7);
                    CheckData(data, new DateTime(2021, 8, 20, 0, 0, 0), 3073.045M);
                    CheckData(data, new DateTime(2021, 8, 20, 3, 0, 0), 3099.367727M);
                    CheckData(data, new DateTime(2021, 8, 20, 8, 0, 0), 3117.422686M);
                    CheckData(data, new DateTime(2021, 8, 20, 11, 0, 0), 3133.627652M);
                    CheckData(data, new DateTime(2021, 8, 20, 16, 0, 0), 3157.722624M);
                    CheckData(data, new DateTime(2021, 8, 20, 19, 0, 0), 3174.982147M);
                    CheckData(data, new DateTime(2021, 8, 21, 0, 0, 0), 3194.04903M);
                }

                using (var ema15 = new EMA(dataprovider, 15)) {
                    var data = ema15.GetAnalyticsData(new DateTime(2021, 8, 20, 0, 0, 0), new DateTime(2021, 8, 21, 0, 0, 0));
                    Assert.IsTrue(data.Count() == 4);
                    Assert.IsNull(data.FirstOrDefault(item => item.Time >= new DateTime(2021, 8, 20, 0, 0, 0) && item.Time < new DateTime(2021, 8, 20, 11, 0, 0)));
                    CheckData(data, new DateTime(2021, 8, 20, 11, 0, 0), 3090.007583M);
                    CheckData(data, new DateTime(2021, 8, 20, 16, 0, 0), 3112.025385M);
                    CheckData(data, new DateTime(2021, 8, 20, 19, 0, 0), 3129.603462M);
                    CheckData(data, new DateTime(2021, 8, 21, 0, 0, 0), 3148.384279M);
                }
            }
        }

        [TestMethod]
        [TestCategory("Testcases")]
        public void RSITestcase03Test() {
            // Testcase 3 - Unregular data & not fully covered -  RSI 5, 10 & 15
            using (var dataprovider = new CDDDataProvider(PATH_353HR_UNREG, TimeSpan.FromHours(3))) {
                using (var rsi5 = new RSI(dataprovider, 5)) {
                    var data = rsi5.GetAnalyticsData(new DateTime(2021, 8, 20, 0, 0, 0), new DateTime(2021, 8, 21, 0, 0, 0));
                    Assert.IsTrue(data.Count() == 7);
                    CheckData(data, new DateTime(2021, 8, 20, 0, 0, 0), 74.43742205M);
                    CheckData(data, new DateTime(2021, 8, 20, 3, 0, 0), 71.41478133M);
                    CheckData(data, new DateTime(2021, 8, 20, 8, 0, 0), 65.73051137M);
                    CheckData(data, new DateTime(2021, 8, 20, 11, 0, 0), 67.07834323M);
                    CheckData(data, new DateTime(2021, 8, 20, 16, 0, 0), 76.00185536M);
                    CheckData(data, new DateTime(2021, 8, 20, 19, 0, 0), 70.58480496M);
                    CheckData(data, new DateTime(2021, 8, 21, 0, 0, 0), 75.0614707M);
                }

                using (var rsi10 = new RSI(dataprovider, 10)) {
                    var data = rsi10.GetAnalyticsData(new DateTime(2021, 8, 20, 0, 0, 0), new DateTime(2021, 8, 21, 0, 0, 0));
                    Assert.IsTrue(data.Count() == 7);
                    CheckData(data, new DateTime(2021, 8, 20, 0, 0, 0), 63.80354691M);
                    CheckData(data, new DateTime(2021, 8, 20, 3, 0, 0), 62.52992033M);
                    CheckData(data, new DateTime(2021, 8, 20, 8, 0, 0), 60.25306019M);
                    CheckData(data, new DateTime(2021, 8, 20, 11, 0, 0), 60.90393351M);
                    CheckData(data, new DateTime(2021, 8, 20, 16, 0, 0), 65.63334952M);
                    CheckData(data, new DateTime(2021, 8, 20, 19, 0, 0), 63.69416515M);
                    CheckData(data, new DateTime(2021, 8, 21, 0, 0, 0), 65.94657762M);
                }

                using (var rsi15 = new RSI(dataprovider, 15)) {
                    var data = rsi15.GetAnalyticsData(new DateTime(2021, 8, 20, 0, 0, 0), new DateTime(2021, 8, 21, 0, 0, 0));
                    Assert.IsTrue(data.Count() == 4);
                    Assert.IsNull(data.FirstOrDefault(item => item.Time >= new DateTime(2021, 8, 20, 0, 0, 0) && item.Time < new DateTime(2021, 8, 20, 11, 0, 0)));
                    CheckData(data, new DateTime(2021, 8, 20, 11, 0, 0), 62.95899361M);
                    CheckData(data, new DateTime(2021, 8, 20, 16, 0, 0), 66.06393153M);
                    CheckData(data, new DateTime(2021, 8, 20, 19, 0, 0), 64.74677016M);
                    CheckData(data, new DateTime(2021, 8, 21, 0, 0, 0), 66.20146149M);
                }
            }
        }

        [TestMethod]
        [TestCategory("Testcases")]
        public void DiffTestcase03Test() {
            // Testcase 3 - Unregular data & not fully covered -  EMA (10) Diff, EMA (10) Diff2 with dt = 1h
            using (var dataprovider = new CDDDataProvider(PATH_353HR_UNREG, TimeSpan.FromHours(3))) {
                using (var ema10 = new EMA(dataprovider, 10)) {
                    using (var ema10diff = new Derivative(ema10, 1, TimeSpan.FromHours(1))) {
                        var data = ema10diff.GetAnalyticsData(new DateTime(2021, 8, 20, 0, 0, 0), new DateTime(2021, 8, 21, 0, 0, 0));
                        Assert.IsTrue(data.Count() == 7);
                        CheckData(data, new DateTime(2021, 8, 20, 0, 0, 0), 0);
                        CheckData(data, new DateTime(2021, 8, 20, 3, 0, 0), 8.774242424M);
                        CheckData(data, new DateTime(2021, 8, 20, 8, 0, 0), 3.610991736M);
                        CheckData(data, new DateTime(2021, 8, 20, 11, 0, 0), 5.401655397M);
                        CheckData(data, new DateTime(2021, 8, 20, 16, 0, 0), 4.818994468M);
                        CheckData(data, new DateTime(2021, 8, 20, 19, 0, 0), 5.753174274M);
                        CheckData(data, new DateTime(2021, 8, 21, 0, 0, 0), 3.813376462M);

                        using (var ema10diff2 = new Derivative(ema10diff, 1, TimeSpan.FromHours(1))) {
                            var data2 = ema10diff2.GetAnalyticsData(new DateTime(2021, 8, 20, 0, 0, 0), new DateTime(2021, 8, 21, 0, 0, 0));
                            Assert.IsTrue(data2.Count() == 7);
                            CheckData(data2, new DateTime(2021, 8, 20, 0, 0, 0), 0);
                            CheckData(data2, new DateTime(2021, 8, 20, 3, 0, 0), 2.924747475M);
                            CheckData(data2, new DateTime(2021, 8, 20, 8, 0, 0), -1.032650138M);
                            CheckData(data2, new DateTime(2021, 8, 20, 11, 0, 0), 0.596887887M);
                            CheckData(data2, new DateTime(2021, 8, 20, 16, 0, 0), -0.116532186M);
                            CheckData(data2, new DateTime(2021, 8, 20, 19, 0, 0), 0.311393269M);
                            CheckData(data2, new DateTime(2021, 8, 21, 0, 0, 0), -0.38795956M);
                        }
                    }

                    using (var ema10difflg3 = new Derivative(ema10, 1, TimeSpan.FromHours(1), 3)) {
                        var data = ema10difflg3.GetAnalyticsData(new DateTime(2021, 8, 20, 0, 0, 0), new DateTime(2021, 8, 21, 0, 0, 0));
                        Assert.IsTrue(data.Count() == 7);
                        CheckData(data, new DateTime(2021, 8, 20, 0, 0, 0), 0);
                        CheckData(data, new DateTime(2021, 8, 20, 3, 0, 0), 8.774242424M);
                        CheckData(data, new DateTime(2021, 8, 20, 8, 0, 0), 5.349637376M);
                        CheckData(data, new DateTime(2021, 8, 20, 11, 0, 0), 4.213970315M);
                        CheckData(data, new DateTime(2021, 8, 20, 16, 0, 0), 5.015196617M);
                        CheckData(data, new DateTime(2021, 8, 20, 19, 0, 0), 5.133565219M);
                        CheckData(data, new DateTime(2021, 8, 21, 0, 0, 0), 4.466573684M);

                        using (var ema10diff2lg3 = new Derivative(ema10difflg3, 1, TimeSpan.FromHours(1), 3)) {
                            var data2 = ema10diff2lg3.GetAnalyticsData(new DateTime(2021, 8, 20, 0, 0, 0), new DateTime(2021, 8, 21, 0, 0, 0));
                            Assert.IsTrue(data2.Count() == 7);
                            CheckData(data2, new DateTime(2021, 8, 20, 0, 0, 0), 0);
                            CheckData(data2, new DateTime(2021, 8, 20, 3, 0, 0), 2.924747475M);
                            CheckData(data2, new DateTime(2021, 8, 20, 8, 0, 0), 0.530579602M);
                            CheckData(data2, new DateTime(2021, 8, 20, 11, 0, 0), -0.581757176M);
                            CheckData(data2, new DateTime(2021, 8, 20, 16, 0, 0), -0.021187712M);
                            CheckData(data2, new DateTime(2021, 8, 20, 19, 0, 0), 0.119571393M);
                            CheckData(data2, new DateTime(2021, 8, 21, 0, 0, 0), -0.075192197M);
                        }
                    }
                }
            }
        }

        [TestMethod]
        [TestCategory("Functionality")]
        public void AnalyticsOverlapTest() {
            // Overlapping time ranges should not affect already loaded data in the cache
            using (var dataprovider = new CDDDataProvider(PATH_GEMINI_1MIN, TimeSpan.FromMinutes(1))) {
                using (var sma20 = new SMA(dataprovider, 20)) {
                    CheckData(sma20, new DateTime(2021, 04, 01), new DateTime(2021, 06, 01), new DateTime(2021, 05, 01, 11, 04, 00), 2846.776M);
                    CheckData(sma20, new DateTime(2021, 04, 15), new DateTime(2021, 06, 15), new DateTime(2021, 05, 01, 11, 04, 00), 2846.776M);
                    CheckData(sma20, new DateTime(2021, 07, 01), new DateTime(2021, 09, 01), new DateTime(2021, 08, 01, 11, 04, 00), 2589.2335M);
                    CheckData(sma20, new DateTime(2021, 04, 01), new DateTime(2021, 06, 01), new DateTime(2021, 05, 01, 11, 04, 00), 2846.776M);
                }
                using (var ema20 = new EMA(dataprovider, 20)) {
                    CheckData(ema20, new DateTime(2021, 04, 01), new DateTime(2021, 06, 01), new DateTime(2021, 05, 01, 11, 04, 00), 2849.234596M);
                    CheckData(ema20, new DateTime(2021, 04, 15), new DateTime(2021, 06, 15), new DateTime(2021, 05, 01, 11, 04, 00), 2849.234596M);
                    CheckData(ema20, new DateTime(2021, 07, 01), new DateTime(2021, 09, 01), new DateTime(2021, 08, 01, 11, 04, 00), 2589.051959M);
                    CheckData(ema20, new DateTime(2021, 04, 01), new DateTime(2021, 06, 01), new DateTime(2021, 05, 01, 11, 04, 00), 2849.234596M);
                }
                using (var rsi20 = new RSI(dataprovider, 20)) {
                    CheckData(rsi20, new DateTime(2021, 04, 01), new DateTime(2021, 06, 01), new DateTime(2021, 05, 01, 11, 04, 00), 65.458278M);
                    CheckData(rsi20, new DateTime(2021, 04, 15), new DateTime(2021, 06, 15), new DateTime(2021, 05, 01, 11, 04, 00), 65.458278M);
                    CheckData(rsi20, new DateTime(2021, 07, 01), new DateTime(2021, 09, 01), new DateTime(2021, 08, 01, 11, 04, 00), 41.509670M);
                    CheckData(rsi20, new DateTime(2021, 04, 01), new DateTime(2021, 06, 01), new DateTime(2021, 05, 01, 11, 04, 00), 65.458278M);
                }
            }
        }

        #region Helper functions        

        private void CheckData(IAnalytics analytics, DateTime from, DateTime to, DateTime time, decimal value) {
            var data = analytics.GetAnalyticsData(from, to);
            var analyticsdata = data.FirstOrDefault(item => item.Time == time);
            Assert.IsTrue(analyticsdata != null);
            Assert.IsTrue(Math.Round(analyticsdata.Value, PRECISION) == Math.Round(value, PRECISION));
        }

        private void CheckData(IEnumerable<IAnalyticsData> data, DateTime time, decimal value) {
            var analyticsdata = data.FirstOrDefault(item => item.Time == time);
            Assert.IsTrue(analyticsdata != null);
            Assert.IsTrue(Math.Round(analyticsdata.Value, PRECISION) == Math.Round(value, PRECISION));
        }

        #endregion
    }
}
