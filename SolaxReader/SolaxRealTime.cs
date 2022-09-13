using System.Net.Http.Headers;
using System.Text.Json;

namespace SolaxReader
{
    /// <summary>
    /// Class used to deserialize the information from the Solax API call
    /// </summary>
    public class SolaxResult
    {
        /// <summary>Did the API call succeed? </summary>
        public bool success { get; set; }
        /// <summary>Success or exception details </summary>
        public string? exception { get; set; }
        
        /// <summary>If the API call was successful, the details requested will be contained in result </summary>
        public SolaxRealTime? result { get; set; }
    }

    /// <summary>
    /// Class representing the current real-time information from the inverter requested
    /// </summary>
    public class SolaxRealTime
    {
        public static SolaxRealTime? srtDataFromApi;
        public static bool blIsGetActive { get; private set; }
         // Variable to indicate if we already have an API call active
        
        #region Private enums and variables

        /// <summary>Enum used to represent the type of units to display </summary>
        private enum Units
        {
            None = 0,
            Watts,
            KiloWattHours,
            Percentage
        }

        private static HttpClient client = new HttpClient();
        #endregion

        #region Solax Real-time Information

        // Every variable here is set to nullable. Some are unlikely to be null, but we can't guarantee that and
        // different installations may result in different results being returned

        public string? inverterSN { get; set; } // Unique identifier of inverter (Serial No. of inverter)
        public string? sn { get; set; } // Unique identifier of communication module (Registration No.)
        public double? acpower { get; set; } // Inverter.AC.power.total 1 W
        public double? yieldtoday { get; set; } // Inverter.AC.energy.out.daily 0.1 KWh
        public double? yieldtotal { get; set; } // Inverter.AC.energy.out.total 0.1 KWh
        public double? feedinpower { get; set; } // Grid.power.total 1 W
        public double? feedinenergy { get; set; } // Grid.energy.toGrid.total 0.01 KWh
        public double? consumeenergy { get; set; } // Grid.energy.fromGrid.total 0.01 KWh
        public double? feedinpowerM2 { get; set; } // Inverter.Meter2.AC.power.total 1 W
        public double? soc { get; set; } // BMS.energy.SOC 1 %
        public double? peps1 { get; set; } // Inverter.AC.EPS.power.R 1 W
        public double? peps2 { get; set; } // Inverter.AC.EPS.power.S 1 W
        public double? peps3 { get; set; } // Inverter.AC.EPS.power.T 1 W
        public string? inverterType { get; set; } // Inverter type, details refer to Table 4 in appendix
        public string? inverterStatus { get; set; } // Inverter status, details refer to Table 5 in appendix
        public string? uploadTime { get; set; } // Update time
        public double? batPower { get; set; } // Inverter.DC.Battery.power.total 1 W
        public double? powerdc1 { get; set; } // Inverter.DC.PV.power.MPPT1 1 W
        public double? powerdc2 { get; set; } // Inverter.DC.PV.power.MPPT2 1 W
        public double? powerdc3 { get; set; } // Inverter.DC.PV.power.MPPT3 1 W
        public double? powerdc4 { get; set; } // Inverter.DC.PV.power.MPPT4 1 W

        #endregion

        #region Constructors

        /// <summary>
        /// Standard Constructor
        /// </summary>
        public SolaxRealTime()
        {
        }

        #endregion

        #region Decoder methods

        private static string DecodeUnits(Units unitsSource)
        {
            switch (unitsSource)
            {
                case Units.None:
                    return ("");

                case Units.Watts:
                    return ("W");

                case Units.KiloWattHours:
                    return ("kWh");

                case Units.Percentage:
                    return ("%");

                default:
                    throw new ArgumentOutOfRangeException(nameof(unitsSource), unitsSource, null);
            }
        }

        private static string DecodeInverterType(string strInverterCode)
        {
            if (string.IsNullOrEmpty(strInverterCode)) return ("Unknown");

            switch (strInverterCode)
            {
                case "1":
                    return ("X1 LX");
                case "2":
                    return ("X Hybrid");
                case "3":
                    return ("X1 Hybrid Fit");
                case "4":
                    return ("X1 Boost Air Mini");
                case "5":
                    return ("X3 Hybrid Fit");
                case "6":
                    return ("X3 20K 30K");
                case "7":
                    return ("X3 MIC PRO");
                case "8":
                    return ("X1 Smart");
                case "9":
                    return ("X1 AC");
                case "10":
                    return ("A1 Hybrid");
                case "11":
                    return ("A1 Fit");
                case "12":
                    return ("A1 Grid");
                case "13":
                    return ("J1 ESS");
                case "14":
                    return ("X3 Hybrid G4");
                case "15":
                    return ("X1 Hybrid G4");
                case "16":
                    return ("X3 MIC PRO G2");
                case "17":
                    return ("X1 SPT");
                case "18":
                    return ("X1 Boost Mini G4");
                case "19":
                    return ("A1 HYB G2");
                case "20":
                    return ("A1 AC G2");
                case "21":
                    return ("A1 SMT G2");
                case "22":
                    return ("X3 FTH");
                case "23":
                    return ("X3 MGA G2");
                default:
                    return ("Unknown");
            }
        }

        private static string DecodeInverterStatus(string strInverterStatus)
        {
            if (string.IsNullOrEmpty(strInverterStatus)) return ("Unknown");

            switch (strInverterStatus)
            {
                case "100":
                    return ("WaitMode");
                case "101":
                    return ("CheckMode");
                case "102":
                    return ("NormalMode");
                case "103":
                    return ("FaultMode");
                case "104":
                    return ("PermanentFaultMode");
                case "105":
                    return ("UpdateMode");
                case "106":
                    return ("EPSCheckMode");
                case "107":
                    return ("EPSMode");
                case "108":
                    return ("SelfTestMode");
                case "109":
                    return ("IdleMode");
                case "110":
                    return ("StandbyMode");
                case "111":
                    return ("PvWakeUpBatMode");
                case "112":
                    return ("GenCheckMode");
                case "113":
                    return ("GenRunMode");

                default:
                    return ("Unknown");
            }
        }

        #endregion

        #region Display Methods

        private bool DisplayIfNotNull(string strTitle, string strSource, Units unitsDisplay)
        {
            if (string.IsNullOrEmpty(strSource)) return (false);

            Console.WriteLine($"\t{strTitle}: {strSource}{DecodeUnits(unitsDisplay)}");
            return (true);
        }

        private bool DisplayIfNotNull(string strTitle, double? dSource, Units unitsDisplay)
        {
            if (dSource == null) return (false);

            Console.WriteLine($"\t{strTitle}: {dSource}{DecodeUnits(unitsDisplay)}");
            return (true);
        }

        /// <summary>
        /// Method to display the contents and results from the API call
        /// </summary>
        public void Display()
        {
            Console.WriteLine($"Solax RealTime Data for: {inverterSN} uploaded at: {uploadTime}\n");
            Console.WriteLine("Inverter Details");
            DisplayIfNotNull("Inverter Type", DecodeInverterType(inverterType), Units.None);
            DisplayIfNotNull("Inverter Status", DecodeInverterStatus(inverterStatus), Units.None);
            //DisplayIfNotNull("Communications Module Id",sn, Units.None);

            Console.WriteLine("\nCurrent Load");
            DisplayIfNotNull("Load", acpower - feedinpower, Units.Watts);

            Console.WriteLine("\nCurrently Available");
            DisplayIfNotNull("AC Power", acpower, Units.Watts);
            DisplayIfNotNull("Battery", batPower, Units.Watts);
            DisplayIfNotNull("Battery", soc, Units.Percentage);

            Console.WriteLine("\nCurrent Feed-in");
            DisplayIfNotNull("Current Feed-in", feedinpower, Units.Watts);
            DisplayIfNotNull("Current Feed-in M2", feedinpowerM2, Units.Watts);

            Console.WriteLine("\nCurrent Generation");
            DisplayIfNotNull("DC Power 1", powerdc1, Units.Watts);
            DisplayIfNotNull("DC Power 2", powerdc2, Units.Watts);
            DisplayIfNotNull("DC Power 3", powerdc3, Units.Watts);
            DisplayIfNotNull("DC Power 4", powerdc4, Units.Watts);

            Console.WriteLine("\nYield Details");
            DisplayIfNotNull("Daily Yield", yieldtoday, Units.KiloWattHours);
            DisplayIfNotNull("Total Yield", yieldtotal, Units.KiloWattHours);

            Console.WriteLine("\nGrid Details");
            DisplayIfNotNull("Total Feed-in", feedinenergy, Units.KiloWattHours);
            DisplayIfNotNull("Total From Grid", consumeenergy, Units.KiloWattHours);

            Console.WriteLine("\nEmergency Power Supply Details");
            DisplayIfNotNull("EPS Power 1", peps1, Units.Watts);
            DisplayIfNotNull("EPS Power 2", peps2, Units.Watts);
            DisplayIfNotNull("EPS Power 3", peps3, Units.Watts);
        }

        #endregion

        /// <summary>
        /// Method to obtain the RealTimeData from the Solax API.
        ///
        /// NOTE: ONLY A SINGLE CALL CAN BE ACTIVE AT A TIME - THIS IS NOT THREAD SAFE
        /// </summary>
        /// <param name="strBaseApiAddress">The URL to use to trigger the API</param>
        /// <param name="strRegistrationNumber">The registration number for the inverter whose data you wish to obtain</param>
        /// <param name="strTokenId">The TokenId provided by SolaxCloud</param>
        /// <returns>The date returned by the API - this may be null</returns>
        /// <exception cref="Exception">Thrown if a call is already active</exception>
        /// <exception cref="ArgumentException">Thrown if any of the required parameters are empty or null</exception>
        public static SolaxRealTime? GetSolaxRealTimeData(string strBaseApiAddress, string strRegistrationNumber, string strTokenId)
        {
            if (blIsGetActive)
            {
                throw new Exception("Only a single call to GetSolaxRealTimeData can be made at a time");
            }

            if (string.IsNullOrEmpty(strBaseApiAddress))
            {
                throw new ArgumentException($"{nameof(strBaseApiAddress)} must have a value.");
            }

            if (string.IsNullOrEmpty(strRegistrationNumber))
            {
                throw new ArgumentException($"{nameof(strRegistrationNumber)} must have a value.");
            }
            
            if (string.IsNullOrEmpty(strTokenId))
            {
                throw new ArgumentException($"{nameof(strTokenId)} must have a value.");
            }
            
            blIsGetActive = true;
            RunAsync(strBaseApiAddress, strRegistrationNumber, strTokenId).GetAwaiter().GetResult();
            blIsGetActive = false;

            return (srtDataFromApi);
        }

        /// <summary>
        /// Method to get the Solax Real-Time data via an API call
        /// </summary>
        /// <param name="path">The URL to use to trigger the API</param>
        /// <returns>The information requested or null if an error occurs</returns>
        private static async Task<SolaxRealTime?> GetSolaxDetailsAsync(string path)
        {
            HttpResponseMessage response = await client.GetAsync(path);

            // If we got an http/tcp-ip error
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(
                    $"Error Returned: HTTP error {response.StatusCode}. Check you Solax credentials and internet connection before retrying.");
                return (null);
            }

            // Get the full response from the API
            string strResult = await response.Content.ReadAsStringAsync();

            // Uncomment the following line to see the JSON returned by the API
            //Console.WriteLine(strResult);

            // Deserialize the information returned from the API call
            SolaxResult? srResult = JsonSerializer.Deserialize<SolaxResult>(strResult);

            // If there was an error we report it and return
            if (!(bool)(srResult?.success))
            {
                string strErrorMessage = ((srResult != null) && (srResult.exception != null))
                    ? srResult.exception
                    : "Unknown Error";

                Console.WriteLine($"Error Returned:{strErrorMessage}");
            }

            // Return what we have, this may be null
            return (srResult?.result);
        }

        /// <summary>
        /// Method to manage the calling of the API
        /// </summary>
        private static async Task RunAsync(string strBaseApiAddress, string strRegistrationNumber, string strTokenId)
        {
            string strApiAddress = $"{strBaseApiAddress}?tokenId={strTokenId}&sn={strRegistrationNumber}";
            
            // Prepare the client for 
            client.BaseAddress = new Uri(strApiAddress);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            try
            {
                // Get the real-time data and then display it if what we get back isn't null
                srtDataFromApi = await GetSolaxDetailsAsync(client.BaseAddress.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}