# SolaxReader
This C# library wraps the Solax, SolaxCloud, SolaxPower API, allowing the most recent statistics and information about your inverter to be collected and processed programmatically.

In order to access the SolaxCloud API, you must:

- Sign in to [https://www.solaxcloud.com/#/api](https://www.solaxcloud.com/#/api) and request a TokenId, this is in my experience a 23 digit numeric 
- Set strTokenId to the value provide in your implementation of SolaxRealTime.cs
- Set strRegistrationNumber to the 10 character alphanumeric code that is available from your SolaxCloud account of can often be found on the inverter or in the inverter settings

The code extract below shows how to call the library, included in the solution there is a command line application that demonstrates the code, **to make the code active you will need to provide your values for strApiBaseAddress, strRegistrationNumber and strTokenId.

<pre><code>
SolaxRealTime? srtData = SolaxRealTime.GetSolaxRealTimeData(strApiBaseAddress, strRegistrationNumber, strTokenId);
</code></pre>


Please note that I am not affiliated with Solax Power, or Solax Cloud in anyway and have simple used [version 6.1 of the Solax API Documentation](https://www.solaxcloud.com/user_api/SolaxCloud_User_Monitoring_API_V6.1.pdf) to write the code contained in this library.
