using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

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
            Percentage,
            Centimeters,
            MPH,
            Miles,
            Centigrade,
            Millibar,
        }
        #endregion
        
        #region Public Enums

        public enum HeaderOptions
        {
            NoHeader=0,
            IncludeHeader,
            AutoHeader
        }
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
            blIsGetActive = false;
        }

        ~SolaxRealTime()
        {
            blIsGetActive = false;
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
                
                case Units.Centimeters:
                    return ("CM");
                
                case Units.MPH:
                    return ("MPH");
                
                case Units.Miles:
                    return ("miles");
                
                case Units.Centigrade:
                    return ("°C");
                
                case Units.Millibar:
                    return ("mbar");

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

        private static string DecodeDirection(double? dDirection)
        {
            if (dDirection == null)
                return ("Unknown");
            if (IsBetween(dDirection, 348.74, 360.00)) 
                return("N");
            if (IsBetween(dDirection, 0.00, 11.24)) 
                return("N");
            if (IsBetween(dDirection, 11.25, 33.75)) 
                return("NNE");
            if (IsBetween(dDirection, 33.74, 56.25)) 
                return("NE");
            if (IsBetween(dDirection, 56.24, 78.75)) 
                return("ENE");
            if (IsBetween(dDirection, 78.74, 101.25)) 
                return("E");
            if (IsBetween(dDirection, 101.24, 123.75)) 
                return("ESE");
            if (IsBetween(dDirection, 123.74, 146.25)) 
                return("SE");
            if (IsBetween(dDirection, 146.24, 168.75)) 
                return("SSE");
            if (IsBetween(dDirection, 168.74, 191.25)) 
                return("S");
            if (IsBetween(dDirection, 191.24, 213.75)) 
                return("SSW");
            if (IsBetween(dDirection, 213.74, 236.25)) 
                return("SW");
            if (IsBetween(dDirection, 236.24, 258.75)) 
                return("WSW");
            if (IsBetween(dDirection, 258.74, 281.25)) 
                return("W");
            if (IsBetween(dDirection, 281.24, 303.75)) 
                return("WNW");
            if (IsBetween(dDirection, 303.74, 326.25)) 
                return("NW");
            if (IsBetween(dDirection, 326.24, 348.75)) 
                return("NNW");
            
            return ("Unknown");
        }

        private static bool IsBetween(double? dValue, double dMin, double dMax)
        {
            if (dValue == null) return (false);
            return((dValue>=dMin && dValue<=dMax));
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

        #region Save to CSV
        private static string SafeValue(string strSource, bool blIncludeComma=true, string strDefaultValue="")
        {
            return((string.IsNullOrWhiteSpace(strSource) ? strDefaultValue : strSource) + (blIncludeComma ? "," : ""));
        }

        private static string SafeValue(double? dSource, bool blIncludeComma=true, string strDefaultValue = "0.0")
        {
            return((dSource==null ? strDefaultValue : $"{dSource:0.0}") + (blIncludeComma ? "," : ""));
        }
        
        /// <summary>
        /// Method to write the instance to a CSV file
        /// </summary>
        /// <param name="strTargetFile">The file to create or append to</param>
        /// <param name="hoHeader">Header options</param>
        /// <param name="wWeather">The weather data to include, or null if no weather is not to be included</param>
        public void WriteToCSV(string strTargetFile, HeaderOptions hoHeader = HeaderOptions.AutoHeader, VisualCrossingWeather.Weather? wWeather=null)
        {
            HeaderOptions hoRealHeader = hoHeader;
            StringBuilder strContent = new StringBuilder();

            if (hoRealHeader == HeaderOptions.AutoHeader)
            {
                hoRealHeader = File.Exists(strTargetFile) ? HeaderOptions.NoHeader : HeaderOptions.IncludeHeader;
            }

            if (hoRealHeader == HeaderOptions.IncludeHeader)
            {
                strContent.Append($"Time,Serial Number,Registration Number,Inverter Type,Inverter Status,% Required Generated,Load ({DecodeUnits(Units.Watts)}),AC Power ({DecodeUnits(Units.Watts)}),Battery ({DecodeUnits(Units.Watts)}),Battery %,Current Feed-in ({DecodeUnits(Units.Watts)}),Current Feed-in M2 ({DecodeUnits(Units.Watts)}),DC Power 1 ({DecodeUnits(Units.Watts)}),DC Power 2 ({DecodeUnits(Units.Watts)}),DC Power 3 ({DecodeUnits(Units.Watts)}),DC Power 4 ({DecodeUnits(Units.Watts)}),Daily Yield ({DecodeUnits(Units.KiloWattHours)}),Total Yield ({DecodeUnits(Units.KiloWattHours)}),Total Feed-in ({DecodeUnits(Units.KiloWattHours)}),Total From Grid ({DecodeUnits(Units.KiloWattHours)}),EPS Power 1 ({DecodeUnits(Units.Watts)}),EPS Power 2 ({DecodeUnits(Units.Watts)}),EPS Power 3 ({DecodeUnits(Units.Watts)})");
                if (wWeather != null)
                {
                    strContent.Append($",Cloud Cover %,Conditions,Temp ({DecodeUnits(Units.Centigrade)}),Humidity,Dew Point ({DecodeUnits(Units.Centigrade)}),Wind Speed ({DecodeUnits(Units.MPH)}),Wind Gust ({DecodeUnits(Units.MPH)}),Direction,Pressure,Visibility ({DecodeUnits(Units.Miles)}),Sunrise,Sunset");
                }
            }

            strContent.Append("\n");
            strContent.Append(SafeValue(uploadTime));
            strContent.Append(SafeValue(inverterSN));
            strContent.Append(SafeValue(sn));
            strContent.Append(SafeValue(DecodeInverterType(inverterType)));
            strContent.Append(SafeValue(DecodeInverterStatus(inverterStatus)));
            strContent.Append(SafeValue((acpower > 0.0) ? (acpower / (acpower - feedinpower)*100) : 0));
            strContent.Append(SafeValue(acpower - feedinpower));
            strContent.Append(SafeValue(acpower));
            strContent.Append(SafeValue(batPower));
            strContent.Append(SafeValue(soc));
            strContent.Append(SafeValue(feedinpower));
            strContent.Append(SafeValue(feedinpowerM2));
            strContent.Append(SafeValue(powerdc1));
            strContent.Append(SafeValue(powerdc2));
            strContent.Append(SafeValue(powerdc3));
            strContent.Append(SafeValue(powerdc4));
            strContent.Append(SafeValue(yieldtoday));
            strContent.Append(SafeValue(yieldtotal));
            strContent.Append(SafeValue(feedinenergy));
            strContent.Append(SafeValue(consumeenergy));
            strContent.Append(SafeValue(peps1));
            strContent.Append(SafeValue(peps2));
            strContent.Append(SafeValue(peps3, wWeather != null));

            if (wWeather != null)
            {
                strContent.Append(SafeValue(wWeather.currentConditions.cloudcover));
                strContent.Append(SafeValue(wWeather.currentConditions.conditions));
                strContent.Append(SafeValue(wWeather.currentConditions.temp));
                strContent.Append(SafeValue(wWeather.currentConditions.humidity));
                strContent.Append(SafeValue(wWeather.currentConditions.dew));
                strContent.Append(SafeValue(wWeather.currentConditions.windspeed));
                strContent.Append(SafeValue(wWeather.currentConditions.windgust));
                strContent.Append(DecodeDirection(wWeather.currentConditions.winddir)+",");
                strContent.Append(SafeValue(wWeather.currentConditions.pressure));
                strContent.Append(SafeValue(wWeather.currentConditions.visibility));
                strContent.Append(SafeValue(wWeather.currentConditions.sunrise));
                strContent.Append(SafeValue(wWeather.currentConditions.sunset, false));
            }
            
            File.AppendAllText(strTargetFile, strContent.ToString());
        }
        #endregion

        private void GetWeather()
        {
            
        }
        
        #region API Call methods

        /// <summary>
        /// Method to obtain the RealTimeData from the Solax API.
        /// 
        /// </summary>
        /// <param name="client">The HTTP Client to use for api calls</param>
        /// <param name="strBaseApiAddress">The URL to use to trigger the API</param>
        /// <param name="strRegistrationNumber">The registration number for the inverter whose data you wish to obtain</param>
        /// <param name="strTokenId">The TokenId provided by SolaxCloud</param>
        /// <returns>The date returned by the API - this may be null</returns>
        /// <exception cref="Exception">Thrown if a call is already active</exception>
        /// <exception cref="ArgumentException">Thrown if any of the required parameters are empty or null</exception>
        public static SolaxRealTime? GetSolaxRealTimeData(HttpClient client, string strBaseApiAddress, string strRegistrationNumber, string strTokenId)
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
            RunAsync(client, strBaseApiAddress, strRegistrationNumber, strTokenId).GetAwaiter().GetResult();
            blIsGetActive = false;

            return (srtDataFromApi);
        }

        /// <summary>
        /// Method to get the Solax Real-Time data via an API call
        /// </summary>
        /// <param name="client">The HTTP client to use for the api call</param>
        /// <returns>The information requested or null if an error occurs</returns>
        private static async Task<SolaxRealTime?> GetSolaxDetailsAsync(HttpClient client)
        {
            
            HttpResponseMessage response = await client.GetAsync(client.BaseAddress);

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
            SolaxResult? srResult = JsonConvert.DeserializeObject<SolaxResult>(strResult);

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
        private static async Task RunAsync(HttpClient client, string strBaseApiAddress, string strRegistrationNumber, string strTokenId)
        {
            string strApiAddress = $"{strBaseApiAddress}?tokenId={strTokenId}&sn={strRegistrationNumber}";
            
            // Prepare the client for API call
            if (client.BaseAddress == null)
            {
                client.BaseAddress = new Uri(strApiAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            try
            {
                // Get the real-time data and then display it if what we get back isn't null
                srtDataFromApi = await GetSolaxDetailsAsync(client);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #endregion
    }
}