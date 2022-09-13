using SolaxReader;

namespace ConsoleApp1;

/// <summary>
/// A simple console program that calls the SolaxRealTime library
/// </summary>
class Program
{
    static void Main()
    {
        string strApiBaseAddress = @"https://www.solaxcloud.com/proxyApp/proxy/api/getRealtimeInfo.do"; // API address obtained from https://www.solaxcloud.com/#/api
        string strTokenId = "***ENTER YOUR TOKEN ID HERE***"; // Numeric token id obtained from https://www.solaxcloud.com/#/api
        string strRegistrationNumber = "***ENTER YOUR REGISTRATION NUMBER HERE***"; // 10 character alpha-numeric registration number this can be found by the QR code on the inverter's communication module

        SolaxRealTime? srtData = SolaxRealTime.GetSolaxRealTimeData(strApiBaseAddress, strRegistrationNumber, strTokenId);

        srtData?.Display();
    }
}