using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace VisualCrossingWeather
{
    public class Day
    {
        public string datetime { get; set; }
        public long datetimeEpoch { get; set; }
        public double? tempmax { get; set; }
        public double? tempmin { get; set; }
        public double? temp { get; set; }
        public double? feelslike { get; set; }
        public double? feelslikemax { get; set; }
        public double? feelslikemin { get; set; }
        public double? precipcover { get; set; }
        public double? humidity { get; set; }
        public double? dew { get; set; }
        public double? snowdepth { get; set; }
        public double? windgust { get; set; }
        public double? windspeed { get; set; }
        public double? winddir { get; set; }
        public double? pressure { get; set; }
        public double? visibility { get; set; }
        public double? cloudcover { get; set; }
        public string? conditions { get; set; }
        public string? description { get; set; }
        public string? sunrise { get; set; }
        public string? sunset { get; set; }
    }
    
    
    public class Weather
    {
        public static Weather? wWeather;
        
        public int queryCost { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
        public string resolvedAddress { get; set; }
        public string address { get; set; }
        public string timezone { get; set; }
        public double tzoffset { get; set; }
        public List<Day> days { get; set; }
        public Day currentConditions;

        #region API Call methods

        /// <summary>
        /// Method to obtain the RealTimeData from the Solax API.
        /// 
        /// </summary>
        /// <param name="client">The HTTP Client to use for api calls</param>
        /// <param name="strBaseApiAddress">The URL to use to trigger the API</param>
        /// <param name="strTokenId">The TokenId provided by the weather service provider</param>
        /// <param name="strCity">The city that we want the weather for</param>
        /// <param name="strPeriod">The time period that we want the weather for</param>
        /// <returns>The date returned by the API - this may be null</returns>
        /// <exception cref="Exception">Thrown if a call is already active</exception>
        /// <exception cref="ArgumentException">Thrown if any of the required parameters are empty or null</exception>
        public static Weather? GetWeatherData(HttpClient client,
            string strBaseApiAddress,
            string strTokenId,
            string strCity,
            string strPeriod)
        {
            if (string.IsNullOrEmpty(strBaseApiAddress))
            {
                throw new ArgumentException($"{nameof(strBaseApiAddress)} must have a value.");
            }

            if (string.IsNullOrEmpty(strCity))
            {
                throw new ArgumentException($"{nameof(strCity)} must have a value.");
            }

            if (string.IsNullOrEmpty(strTokenId))
            {
                throw new ArgumentException($"{nameof(strTokenId)} must have a value.");
            }
            
            RunAsync(client, strBaseApiAddress, strTokenId, strCity, strPeriod).GetAwaiter().GetResult();

            return (wWeather);
        }

        /// <summary>
        /// Method to get the Solax Real-Time data via an API call
        /// </summary>
        /// <param name="client">The HTTP client to use for the api call</param>
        /// <returns>The information requested or null if an error occurs</returns>
        private static async Task<Weather?> GetWeatherAsync(HttpClient client)
        {
            HttpResponseMessage response = await client.GetAsync(client.BaseAddress);

            // If we got an http/tcp-ip error
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(
                    $"Error Returned: HTTP error {response.StatusCode}. Check credentials and internet connection before retrying.");
                return (null);
            }

            // Get the full response from the API
            string strResult = await response.Content.ReadAsStringAsync();

            // Uncomment the following line to see the JSON returned by the API
            Console.WriteLine(strResult);

            // Deserialize the information returned from the API call
            Weather? wResult = JsonConvert.DeserializeObject<Weather>(strResult);

            // Return what we have, this may be null
            return (wResult);
        }

        /// <summary>
        /// Method to manage the calling of the API
        /// </summary>
        private static async Task RunAsync(HttpClient client, string strBaseApiAddress, string strToken, string strCity, string strPeriod)
        {
            // https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/{1}/{2}?unitGroup=metric&elements=datetime%2CdatetimeEpoch%2Cname%2Ctempmax%2Ctempmin%2Ctemp%2Cfeelslikemax%2Cfeelslikemin%2Cfeelslike%2Cdew%2Chumidity%2Cprecip%2Cprecipprob%2Cprecipcover%2Cpreciptype%2Csnow%2Csnowdepth%2Cwindgust%2Cwindspeed%2Cwinddir%2Cpressure%2Ccloudcover%2Cvisibility%2Csolarradiation%2Csolarenergy%2Cuvindex%2Csevererisk%2Csunrise%2Csunset%2Cdescription%2Cwindspeed50%2Cwinddir50%2Csunelevation&include=remote%2Cdays%2Ccurrent&key={0}}&options=nonulls&contentType=json
            string strApiAddress = strBaseApiAddress.Replace("{0}", strToken);

            strApiAddress = strApiAddress.Replace("{1}", strCity);
            strApiAddress = strApiAddress.Replace("{2}", strPeriod);
            
            // Prepare the client for API call
            if (client.BaseAddress == null)
            {
                client.BaseAddress = new Uri(strApiAddress);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            }

            try
            {
                wWeather = await GetWeatherAsync(client);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        #endregion
    }
}