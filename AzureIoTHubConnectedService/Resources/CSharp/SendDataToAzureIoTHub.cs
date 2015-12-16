using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

static class AzureIoTHub
{
    public static async Task SendDeviceToCloudMessageAsync()
    {
        string iotHubUri = "$iotHubUri$";
        string deviceId = "$deviceId$";
        string deviceKey = "$deviceKey$";

        var deviceClient = DeviceClient.Create(iotHubUri,
                AuthenticationMethodFactory.
                    CreateAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey),
                TransportType.Http1);

        var str = "Hello, Cloud!";
        var message = new Message(Encoding.ASCII.GetBytes(str));

        await deviceClient.SendEventAsync(message);
    }
}