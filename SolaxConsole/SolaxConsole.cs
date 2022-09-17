using SolaxReader;
using VisualCrossingWeather;

namespace ConsoleApp1;

/// <summary>
/// A simple console program that:
/// * Obtains the weather conditions from Visual Crossing Weather
/// * Obtains the latest details from Solax
/// * Outputs the details to a CSV file 
/// </summary>
class Program
{
    private static Weather? wWeather;
    
    static void Main()
    {
        GetWeather();
        ProcessHouse(wWeather);
    }

    static void GetWeather()
    {
        HttpClient client = new HttpClient();
        string strApiBaseAddress = @"https://weather.visualcrossing.com/VisualCrossingWebServices/rest/services/timeline/{1}/{2}?unitGroup=uk&elements=datetime%2CdatetimeEpoch%2Cname%2Caddress%2Ctempmax%2Ctempmin%2Ctemp%2Cfeelslikemax%2Cfeelslikemin%2Cfeelslike%2Cdew%2Chumidity%2Cprecip%2Cprecipprob%2Cprecipcover%2Cpreciptype%2Csnow%2Csnowdepth%2Cwindgust%2Cwindspeed%2Cwinddir%2Cpressure%2Ccloudcover%2Cvisibility%2Csolarradiation%2Csolarenergy%2Cuvindex%2Csevererisk%2Csunrise%2Csunset%2Cconditions%2Cdescription%2Csunelevation&include=current%2Cremote&key={0}&options=nonulls&contentType=json";
        string strTokenId = "** YOUR DETAILS HERE **"; // Alpha-numeric token id obtained from Visual Crossing Weather
        string strCity = "london";
        string strPeriod = "today";

        wWeather = VisualCrossingWeather.Weather.GetWeatherData(client, strApiBaseAddress, strTokenId, strCity, strPeriod);
    }

    static void ProcessHouse(Weather? wWeather)
    {
        HttpClient client = new HttpClient();
        string strApiBaseAddress = @"https://www.solaxcloud.com/proxyApp/proxy/api/getRealtimeInfo.do"; // API address obtained from https://www.solaxcloud.com/#/api
        string strTokenId = "** YOUR DETAIL HERE **"; // Numeric token id obtained from https://www.solaxcloud.com/#/api
        string strRegistrationNumber = "** YOUR DETAILS HERE **"; // 10 character alpha-numeric registration number this can be found by the QR code on the inverter's communication module

        SolaxRealTime? srtData = SolaxRealTime.GetSolaxRealTimeData(client, strApiBaseAddress, strRegistrationNumber, strTokenId);

        srtData?.Display();
        srtData?.WriteToCSV("./Solax.csv", SolaxRealTime.HeaderOptions.AutoHeader, wWeather);
    }
}