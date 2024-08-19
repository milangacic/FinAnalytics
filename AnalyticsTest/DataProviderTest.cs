using Microsoft.VisualStudio.TestTools.UnitTesting;
using AnalyticsEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace AnalyticsEngineTest {
    [TestClass]    
    public class DataProviderTest {

        #region Constants

        const string PATH_GEMINI_1MIN = "Data/Testdata_gemini_2021_1min.csv";
        const string PATH_GEMINI_1HR = "Data/Testdata_gemini_2016-2021_1hr.csv";

        const string PATH_1HR_REG = "Data/Testdata_1h_reg.csv";
        const string PATH_1MIN_REG = "Data/Testdata_1min_reg.csv";
        const string PATH_3HR_REG = "Data/Testdata_3h_reg.csv";
        const string PATH_40MIN_REG = "Data/Testdata_40min_reg.csv";
        const string PATH_353HR_UNREG = "Data/Testdata_3-5-3h_unreg.csv";
        const string PATH_363HR_UNREG = "Data/Testdata_3-6-3h_unreg.csv";
        const string PATH_353MIN_UNREG = "Data/Testdata_3-5-3min_unreg.csv";

        const string URL_GEMINI_1MIN = @"https://www.cryptodatadownload.com/cdd/gemini_ETHUSD_2021_1min.csv";
        const string URL_GEMINI_1HR = @"https://www.cryptodatadownload.com/cdd/gemini_ETHUSD_1hr.csv";

        const byte PRECISION = 6;

        #endregion

        [TestMethod]
        [TestCategory("Real data")]
        public void CDDRealDataLocalTest() {
            // 5 minute steps from 1 minute data (Gemini 2021)
            using (var dataprovider = new CDDDataProvider(PATH_GEMINI_1MIN, TimeSpan.FromMinutes(5))) {
                // 01/03/2021 - 01/06/2021 @ 25/04/2021 0:45
                CheckData(dataprovider, new DateTime(2021, 03, 01), new DateTime(2021, 06, 01), new DateTime(2021, 04, 25, 0, 45, 0), 2218.23M, 2234.86M, 2218.23M, 2221.24M, 66.02921559M);
                // 01/05/2021 - 13/08/2021 @ 10/08/2021 18:15
                CheckData(dataprovider, new DateTime(2021, 05, 01), new DateTime(2021, 08, 13), new DateTime(2021, 08, 10, 18, 15, 0), 3101.91M, 3115.01M, 3096.84M, 3111.38M, 529.16304818M);
                // 20/02/2021 - 09/04/2021 @ 15/03/2021 16:00
                CheckData(dataprovider, new DateTime(2021, 02, 20), new DateTime(2021, 04, 09), new DateTime(2021, 03, 15, 16, 00, 0), 1765.92M, 1767.62M, 1751.94M, 1759.45M, 175.400592M);
                // 01/01/2021 - 31/12/2021 @ 25/04/2021 0:45
                CheckData(dataprovider, new DateTime(2021, 03, 01), new DateTime(2021, 06, 01), new DateTime(2021, 04, 25, 0, 45, 0), 2218.23M, 2234.86M, 2218.23M, 2221.24M, 66.02921559M);
            }

            // 15 minute steps from 1 minute data (Gemini 2021)
            using (var dataprovider = new CDDDataProvider(PATH_GEMINI_1MIN, TimeSpan.FromMinutes(15))) {
                // 01/03/2021 - 01/06/2021 @ 25/04/2021 0:45
                CheckData(dataprovider, new DateTime(2021, 03, 01), new DateTime(2021, 06, 01), new DateTime(2021, 04, 25, 0, 45, 0), 2218.23M, 2234.86M, 2206.1M, 2218.59M, 120.78880717M);
            }

            // 5 minute steps from 1 hour data (Gemini 2021)
            using (var dataprovider = new CDDDataProvider(PATH_GEMINI_1HR, TimeSpan.FromMinutes(5))) {
                // 01/03/2021 - 01/06/2021 @ 25/04/2021 0:45
                CheckData(dataprovider, new DateTime(2021, 03, 01), new DateTime(2021, 06, 01), new DateTime(2021, 04, 25, 1, 00, 0), 2218.59M, 2254.48M, 2216.91M, 2242.31M, 597.78694335M);
            }

            // 3 minute steps from 1 hour data (Gemini 2021)
            using (var dataprovider = new CDDDataProvider(PATH_GEMINI_1HR, TimeSpan.FromMinutes(3))) {
                // 01/03/2021 - 01/06/2021 @ 25/04/2021 0:45
                CheckData(dataprovider, new DateTime(2021, 03, 01), new DateTime(2021, 06, 01), new DateTime(2021, 04, 25, 1, 00, 0), 2218.59M, 2254.48M, 2216.91M, 2242.31M, 597.78694335M);
            }
        }

        [TestMethod]
        [TestCategory("Real data")]
        public void CDDRealDataWebTest() {
            // 5 minute steps from 1 minute data
            using (var dataprovider = new CDDDataProvider(new System.Uri(URL_GEMINI_1MIN), TimeSpan.FromMinutes(5))) {
                // 01/03/2021 - 01/06/2021 @ 25/04/2021 0:45
                CheckData(dataprovider, new DateTime(2021, 03, 01), new DateTime(2021, 06, 01), new DateTime(2021, 04, 25, 0, 45, 0), 2218.23M, 2234.86M, 2218.23M, 2221.24M, 66.02921559M);
                // 01/05/2021 - 13/08/2021 @ 10/08/2021 18:15
                CheckData(dataprovider, new DateTime(2021, 05, 01), new DateTime(2021, 08, 13), new DateTime(2021, 08, 10, 18, 15, 0), 3101.91M, 3115.01M, 3096.84M, 3111.38M, 529.16304818M);
            }
        }

        [TestMethod]
        [TestCategory("Testcases")]        
        public void CDDTestcase01Test() {
            // Testcase 1 - Last record with not matching steps
            using (var dataprovider = new CDDDataProvider(PATH_1HR_REG, TimeSpan.FromHours(2))) {
                // 20/08/2021 2:00 - 20/08/2021 8:00
                var data = dataprovider.GetMarketData(new DateTime(2021, 08, 20, 2, 0, 0), new DateTime(2021, 08, 20, 8, 0, 0));
                Assert.IsTrue(data.Count() == 4);
                CheckData(data, new DateTime(2021, 08, 20, 2, 0, 0), 3237.67M, 3241.86M, 3210.9M, 3217.82M, 1020.629568M);
                CheckData(data, new DateTime(2021, 08, 20, 4, 0, 0), 3217.82M, 3234.44M, 3203.64M, 3225M, 827.5461402M);
                CheckData(data, new DateTime(2021, 08, 20, 6, 0, 0), 3225M, 3235M, 3208.1M, 3213.25M, 705.3098844M);
                CheckData(data, new DateTime(2021, 08, 20, 8, 0, 0), 3213.25M, 3224.9M, 3187.16M, 3206.46M, 851.9418648M);
            }
        }

        [TestMethod]
        [TestCategory("Testcases")]
        public void CDDTestcase02Test() {
            // Testcase 2 - Same steps
            using (var dataprovider = new CDDDataProvider(PATH_1HR_REG, TimeSpan.FromHours(1))) {
                // 20/08/2021 2:00 - 20/08/2021 8:00
                var data = dataprovider.GetMarketData(new DateTime(2021, 08, 20, 2, 0, 0), new DateTime(2021, 08, 20, 8, 0, 0));
                Assert.IsTrue(data.Count() == 7);
                CheckData(data, new DateTime(2021, 08, 20, 2, 0, 0), 3237.67M, 3241.86M, 3223.99M, 3230.92M, 428.4332445M);
                CheckData(data, new DateTime(2021, 08, 20, 3, 0, 0), 3230.92M, 3234.36M, 3210.9M, 3217.82M, 592.1963239M);
                CheckData(data, new DateTime(2021, 08, 20, 4, 0, 0), 3217.82M, 3234.44M, 3208.84M, 3224.3M, 317.8495698M);
                CheckData(data, new DateTime(2021, 08, 20, 5, 0, 0), 3224.3M, 3226.26M, 3203.64M, 3225M, 509.6965704M);
                CheckData(data, new DateTime(2021, 08, 20, 6, 0, 0), 3225M, 3235M, 3209.25M, 3229.54M, 431.1432132M);
                CheckData(data, new DateTime(2021, 08, 20, 7, 0, 0), 3229.54M, 3231.38M, 3208.1M, 3213.25M, 274.1666712M);
                CheckData(data, new DateTime(2021, 08, 20, 8, 0, 0), 3213.25M, 3224.9M, 3187.16M, 3198.67M, 435.8235884M);
            }
        }

        [TestMethod]
        [TestCategory("Testcases")]
        public void CDDTestcase03Test() {
            // Testcase 3 - Large to small but matching steps
            using (var dataprovider = new CDDDataProvider(PATH_1HR_REG, TimeSpan.FromMinutes(1))) {
                // 20/08/2021  2:00:00 AM -20/08/2021  8:00:00 AM
                var data = dataprovider.GetMarketData(new DateTime(2021, 08, 20, 2, 0, 0), new DateTime(2021, 08, 20, 8, 0, 0));
                Assert.IsTrue(data.Count() == 7);
                CheckData(data, new DateTime(2021, 08, 20, 2, 0, 0), 3237.67M, 3241.86M, 3223.99M, 3230.92M, 428.4332445M);
                CheckData(data, new DateTime(2021, 08, 20, 3, 0, 0), 3230.92M, 3234.36M, 3210.9M, 3217.82M, 592.1963239M);
                CheckData(data, new DateTime(2021, 08, 20, 4, 0, 0), 3217.82M, 3234.44M, 3208.84M, 3224.3M, 317.8495698M);
                CheckData(data, new DateTime(2021, 08, 20, 5, 0, 0), 3224.3M, 3226.26M, 3203.64M, 3225M, 509.6965704M);
                CheckData(data, new DateTime(2021, 08, 20, 6, 0, 0), 3225M, 3235M, 3209.25M, 3229.54M, 431.1432132M);
                CheckData(data, new DateTime(2021, 08, 20, 7, 0, 0), 3229.54M, 3231.38M, 3208.1M, 3213.25M, 274.1666712M);
                CheckData(data, new DateTime(2021, 08, 20, 8, 0, 0), 3213.25M, 3224.9M, 3187.16M, 3198.67M, 435.8235884M);
            }
        }

        [TestMethod]
        [TestCategory("Testcases")]
        public void CDDTestcase04Test() {
            // Testcase 4 - Small to large but matching steps
            using (var dataprovider = new CDDDataProvider(PATH_1MIN_REG, TimeSpan.FromHours(1))) {
                // 13/08/2021  6:00:00 PM - 13/08/2021  10:00:00 PM
                var data = dataprovider.GetMarketData(new DateTime(2021, 08, 13, 18, 0, 0), new DateTime(2021, 08, 13, 22, 0, 0));
                Assert.IsTrue(data.Count() == 5);
                CheckData(data, new DateTime(2021, 08, 13, 18, 0, 0), 3233.56M, 3240.17M, 3220.91M, 3229M, 598.7874452M);
                CheckData(data, new DateTime(2021, 08, 13, 19, 0, 0), 3229M, 3234.77M, 3214.74M, 3226M, 626.8472862M);
                CheckData(data, new DateTime(2021, 08, 13, 20, 0, 0), 3226M, 3295M, 3219M, 3283.37M, 2091.819445M);
                CheckData(data, new DateTime(2021, 08, 13, 21, 0, 0), 3283.37M, 3298.11M, 3274.28M, 3281.3M, 534.1517439M);
                CheckData(data, new DateTime(2021, 08, 13, 22, 0, 0), 3281.3M, 3295M, 3277.03M, 3291.15M, 421.78812M);
            }
        }

        [TestMethod]
        [TestCategory("Testcases")]
        public void CDDTestcase05Test() {
            // Testcase 5 - Large to small with not matching steps
            using (var dataprovider = new CDDDataProvider(PATH_1HR_REG, TimeSpan.FromMinutes(1))) {
                // 20/08/2021  2:30:00 AM - 20/08/2021  8:30:00 AM
                var data = dataprovider.GetMarketData(new DateTime(2021, 08, 20, 2, 30, 0), new DateTime(2021, 08, 20, 8, 30, 0));
                Assert.IsTrue(data.Count() == 6);
                CheckData(data, new DateTime(2021, 08, 20, 03, 0, 0), 3230.92M, 3234.36M, 3210.9M, 3217.82M, 592.1963239M);
                CheckData(data, new DateTime(2021, 08, 20, 04, 0, 0), 3217.82M, 3234.44M, 3208.84M, 3224.3M, 317.8495698M);
                CheckData(data, new DateTime(2021, 08, 20, 05, 0, 0), 3224.3M, 3226.26M, 3203.64M, 3225M, 509.6965704M);
                CheckData(data, new DateTime(2021, 08, 20, 06, 0, 0), 3225M, 3235M, 3209.25M, 3229.54M, 431.1432132M);
                CheckData(data, new DateTime(2021, 08, 20, 07, 0, 0), 3229.54M, 3231.38M, 3208.1M, 3213.25M, 274.1666712M);
                CheckData(data, new DateTime(2021, 08, 20, 08, 0, 0), 3213.25M, 3224.9M, 3187.16M, 3198.67M, 435.8235884M);
            }
        }

        [TestMethod]
        [TestCategory("Testcases")]
        public void CDDTestcase06Test() {
            // Testcase 6 - Small to large with not matching steps
            using (var dataprovider = new CDDDataProvider(PATH_40MIN_REG, TimeSpan.FromHours(2))) {
                // 13/08/2021  1:00:00 AM - 13/08/2021  13:00:00 PM
                var data = dataprovider.GetMarketData(new DateTime(2021, 08, 13, 1, 0, 0), new DateTime(2021, 08, 13, 13, 0, 0));
                Assert.IsTrue(data.Count() == 6);
                CheckData(data, new DateTime(2021, 08, 13, 01, 20, 0), 3068.09M, 3111.11M, 3086.91M, 3109.38M, 13.55428744M);
                CheckData(data, new DateTime(2021, 08, 13, 03, 20, 0), 3109.38M, 3132M, 3118.23M, 3123.55M, 10.651477M);
                CheckData(data, new DateTime(2021, 08, 13, 05, 20, 0), 3123.55M, 3170.88M, 3121.33M, 3168.88M, 23.64267635M);
                CheckData(data, new DateTime(2021, 08, 13, 07, 20, 0), 3168.88M, 3241.19M, 3207.84M, 3238.84M, 53.37699562M);
                CheckData(data, new DateTime(2021, 08, 13, 09, 20, 0), 3238.84M, 3253.98M, 3229.1M, 3249.6M, 66.87678911M);
                CheckData(data, new DateTime(2021, 08, 13, 11, 20, 0), 3249.6M, 3246.6M, 3214.68M, 3244.2M, 62.446135M);
            }
        }

        [TestMethod]
        [TestCategory("Testcases")]
        public void CDDTestcase07Test() {
            // Testcase 7 - Large to small with uneven steps
            using (var dataprovider = new CDDDataProvider(PATH_353HR_UNREG, TimeSpan.FromHours(2))) {
                // 18/08/2021  11:00:00 AM - 20/08/2021  1:00:00 AM
                var data = dataprovider.GetMarketData(new DateTime(2021, 08, 18, 11, 0, 0), new DateTime(2021, 08, 20, 1, 0, 0));
                Assert.IsTrue(data.Count() == 10);
                CheckData(data, new DateTime(2021, 08, 18, 11, 0, 0), 3044.84M, 3058.11M, 2992.05M, 2999.92M, 635.2949943M);
                CheckData(data, new DateTime(2021, 08, 18, 16, 0, 0), 2999.92M, 3125.99M, 3096.01M, 3121.5M, 282.7552236M);
                CheckData(data, new DateTime(2021, 08, 18, 19, 0, 0), 3121.5M, 3069.22M, 3021.99M, 3031.1M, 1796.177158M);
                CheckData(data, new DateTime(2021, 08, 19, 0, 0, 0), 3031.1M, 3048.87M, 3000M, 3046.67M, 450.3903113M);
                CheckData(data, new DateTime(2021, 08, 19, 3, 0, 0), 3046.67M, 3009.69M, 2967.28M, 2967.28M, 563.4732909M);
                CheckData(data, new DateTime(2021, 08, 19, 8, 0, 0), 2967.28M, 3022.53M, 2980.75M, 3001.68M, 608.2668325M);
                CheckData(data, new DateTime(2021, 08, 19, 11, 0, 0), 3001.68M, 3002.54M, 2975M, 2975M, 732.1066278M);
                CheckData(data, new DateTime(2021, 08, 19, 16, 0, 0), 2975M, 3078.94M, 3048.35M, 3048.35M, 967.9657936M);
                CheckData(data, new DateTime(2021, 08, 19, 19, 0, 0), 3048.35M, 3164.56M, 3131M, 3147.41M, 1711.120613M);
                CheckData(data, new DateTime(2021, 08, 20, 0, 0, 0), 3147.41M, 3246.09M, 3179M, 3229.06M, 3315.247691M);
            }
        }

        [TestMethod]
        [TestCategory("Testcases")]
        public void CDDTestcase08Test() {
            // Testcase 8 - Small to large with uneven steps
            using (var dataprovider = new CDDDataProvider(PATH_353HR_UNREG, TimeSpan.FromHours(7))) {
                // 18/08/2021  1:00:00 - 19/08/2021  23:00:00
                var data = dataprovider.GetMarketData(new DateTime(2021, 08, 18, 1, 0, 0), new DateTime(2021, 08, 19, 23, 0, 0));
                Assert.IsTrue(data.Count() == 6);
                CheckData(data, new DateTime(2021, 08, 18, 3, 0, 0), 3021.03M, 3050.05M, 3009.51M, 3044.84M, 892.766533M);
                CheckData(data, new DateTime(2021, 08, 18, 11, 0, 0), 3044.84M, 3125.99M, 2992.05M, 3121.5M, 918.0502179M);
                CheckData(data, new DateTime(2021, 08, 18, 19, 0, 0), 3121.5M, 3069.22M, 3000M, 3046.67M, 2246.567469M);
                CheckData(data, new DateTime(2021, 08, 19, 3, 0, 0), 3046.67M, 3009.69M, 2967.28M, 2967.28M, 563.4732909M);
                CheckData(data, new DateTime(2021, 08, 19, 8, 0, 0), 2967.28M, 3022.53M, 2975M, 2975M, 1340.37346M);
                CheckData(data, new DateTime(2021, 08, 19, 16, 0, 0), 2975M, 3164.56M, 3048.35M, 3147.41M, 2679.086407M);
            }
        }

        [TestMethod]
        [TestCategory("Testcases")]
        public void CDDTestcase09Test() {
            // Testcase 9 - Same but uneven steps
            using (var dataprovider = new CDDDataProvider(PATH_353HR_UNREG, TimeSpan.FromHours(3))) {
                // 18/08/2021  1:00:00 - 18/08/2021  22:00:00
                var data = dataprovider.GetMarketData(new DateTime(2021, 08, 18, 1, 0, 0), new DateTime(2021, 08, 18, 22, 0, 0));
                Assert.IsTrue(data.Count() == 5);
                CheckData(data, new DateTime(2021, 08, 18, 3, 0, 0), 3021.03M, 3050.05M, 3009.51M, 3050.05M, 477.433949M);
                CheckData(data, new DateTime(2021, 08, 18, 8, 0, 0), 3050.05M, 3050.04M, 3017M, 3044.84M, 415.3325841M);
                CheckData(data, new DateTime(2021, 08, 18, 11, 0, 0), 3044.84M, 3058.11M, 2992.05M, 2999.92M, 635.2949943M);
                CheckData(data, new DateTime(2021, 08, 18, 16, 0, 0), 2999.92M, 3125.99M, 3096.01M, 3121.5M, 282.7552236M);
                CheckData(data, new DateTime(2021, 08, 18, 19, 0, 0), 3121.5M, 3069.22M, 3021.99M, 3031.1M, 1796.177158M);
            }
        }

        [TestMethod]
        [TestCategory("Testcases")]
        public void CDDTestcase10Test() {
            // Testcase 10 - Intermediate uneven steps
            using (var dataprovider = new CDDDataProvider(PATH_363HR_UNREG, TimeSpan.FromHours(5))) {
                // 18/08/2021  2:00:00 - 20/08/2021  2:00:00
                var data = dataprovider.GetMarketData(new DateTime(2021, 08, 18, 2, 0, 0), new DateTime(2021, 08, 20, 2, 0, 0));
                Assert.IsTrue(data.Count() == 10);
                CheckData(data, new DateTime(2021, 08, 18, 3, 0, 0), 3021.03M, 3050.05M, 3009.51M, 3050.05M, 477.433949M);
                CheckData(data, new DateTime(2021, 08, 18, 9, 0, 0), 3050.05M, 3058.11M, 2992.05M, 2999.92M, 1050.627578M);
                CheckData(data, new DateTime(2021, 08, 18, 18, 0, 0), 2999.92M, 3125.99M, 3096.01M, 3121.5M, 282.7552236M);
                CheckData(data, new DateTime(2021, 08, 18, 21, 0, 0), 3121.5M, 3069.22M, 3021.99M, 3031.1M, 1796.177158M);
                CheckData(data, new DateTime(2021, 08, 19, 3, 0, 0), 3031.1M, 3048.87M, 3000M, 3046.67M, 450.3903113M);
                CheckData(data, new DateTime(2021, 08, 19, 6, 0, 0), 3046.67M, 3009.69M, 2967.28M, 2967.28M, 563.4732909M);
                CheckData(data, new DateTime(2021, 08, 19, 12, 0, 0), 2967.28M, 3022.53M, 2980.75M, 3001.68M, 608.2668325M);
                CheckData(data, new DateTime(2021, 08, 19, 15, 0, 0), 3001.68M, 3002.54M, 2975M, 2975M, 732.1066278M);
                CheckData(data, new DateTime(2021, 08, 19, 21, 0, 0), 2975M, 3078.94M, 3048.35M, 3048.35M, 967.9657936M);
                CheckData(data, new DateTime(2021, 08, 20, 0, 0, 0), 3048.35M, 3164.56M, 3131M, 3147.41M, 1711.120613M);
            }
        }               

        [TestMethod]
        [TestCategory("Functionality")]
        public void CDDChangeIntervalTest() {
            // Interval change clears data
            using (var dataprovider = new CDDDataProvider(PATH_GEMINI_1MIN, TimeSpan.FromMinutes(5))) {
                CheckSize(dataprovider, new DateTime(2021, 03, 01), new DateTime(2021, 04, 01), 5);
                dataprovider.Interval = TimeSpan.FromHours(1);
                CheckSize(dataprovider, new DateTime(2021, 03, 01), new DateTime(2021, 04, 01), 60);
            }
        }

        [TestMethod]
        [TestCategory("Functionality")]
        public void CDDTriggerEventsTest() {
            // Data and interval change triggers events
            using (var dataprovider = new CDDDataProvider(PATH_GEMINI_1MIN, TimeSpan.FromMinutes(5))) {
                bool intervalchanged, datachanged;
                intervalchanged = datachanged = false;
                dataprovider.IntervalChanged += (object s, EventArgs a) => { intervalchanged = true; };
                dataprovider.DataChanged += (object s, EventArgs a) => { datachanged = true; };
                var data = dataprovider.GetMarketData(new DateTime(2021, 03, 01), new DateTime(2021, 04, 01));
                dataprovider.Interval = TimeSpan.FromHours(1);
                Assert.IsTrue(intervalchanged);
                Assert.IsTrue(datachanged);
            }
        }

        [TestMethod]
        [TestCategory("Functionality")]
        public void CDDOverlapTest() {
            // Overlapping time ranges should not affect already loaded data
            using (var dataprovider = new CDDDataProvider(PATH_GEMINI_1MIN, TimeSpan.FromMinutes(5))) {
                CheckData(dataprovider, new DateTime(2021, 03, 01), new DateTime(2021, 06, 01), new DateTime(2021, 04, 25, 0, 45, 0), 2218.23M, 2234.86M, 2218.23M, 2221.24M, 66.02921559M);
                CheckData(dataprovider, new DateTime(2021, 04, 01), new DateTime(2021, 08, 13), new DateTime(2021, 04, 25, 0, 45, 0), 2218.23M, 2234.86M, 2218.23M, 2221.24M, 66.02921559M);
                CheckData(dataprovider, new DateTime(2021, 01, 20), new DateTime(2021, 5, 13), new DateTime(2021, 04, 25, 0, 45, 0), 2218.23M, 2234.86M, 2218.23M, 2221.24M, 66.02921559M);
            }
        }

        #region Helper functions        

        private void CheckData(CDDDataProvider dataprovider, DateTime from, DateTime to, DateTime time, decimal open, decimal high, decimal low, decimal close, decimal volume) {
            var data = dataprovider.GetMarketData(from, to);
            CheckData(data, time, open, high, low, close, volume);
        }

        private void CheckData(IEnumerable<IMarketData> data, DateTime time, decimal open, decimal high, decimal low, decimal close, decimal volume) {
            var marketdata = data.FirstOrDefault(item => item.Time == time);
            Assert.IsTrue(marketdata != null);
            Assert.IsTrue(Math.Round(marketdata.Open, PRECISION) == Math.Round(open, PRECISION));
            Assert.IsTrue(Math.Round(marketdata.Close, PRECISION) == Math.Round(close, PRECISION));
            Assert.IsTrue(Math.Round(marketdata.Low, PRECISION) == Math.Round(low, PRECISION));
            Assert.IsTrue(Math.Round(marketdata.High, PRECISION) == Math.Round(high, PRECISION));
            Assert.IsTrue(Math.Round(marketdata.Volume, PRECISION) == Math.Round(volume, PRECISION));
        }

        private void CheckSize(CDDDataProvider dataprovider, DateTime from, DateTime to, int interval) {
            var data = dataprovider.GetMarketData(from, to);
            var count = 1 + (to - from).TotalMinutes / interval;
            Assert.IsTrue(data.Count() == count);            
        }

        #endregion
    }
}
