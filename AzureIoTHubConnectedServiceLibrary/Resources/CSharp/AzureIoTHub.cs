using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;

static class AzureIoTHub
{
    //
    // Note: this connection string is specific to the device "$deviceId$". To configure other devices,
    // see information on iothub-explorer at http://aka.ms/iothubgetstartedVSCS
    //
    const string deviceConnectionString = "HostName=$iotHubUri$;DeviceId=$deviceId$;SharedAccessKey=$deviceKey$";

    //
    // To monitor messages sent to device "$deviceId$" use iothub-explorer as follows:
    //    iothub-explorer monitor-events --login HostName=$iotHubUri$;SharedAccessKeyName=service;SharedAccessKey=$servicePrimaryKey$ "$deviceId$"
    //

    // Refer to http://aka.ms/azure-iot-hub-vs-cs-wiki for more information on Connected Service for Azure IoT Hub

    public static async Task SendDeviceToCloudMessageAsync()
    {
        var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Mqtt);

#if WINDOWS_UWP
        var str = "{\"deviceId\":\"$deviceId$\",\"messageId\":1,\"text\":\"Hello, Cloud from a UWP C# app!\"}";
#else
        var str = "{\"deviceId\":\"$deviceId$\",\"messageId\":1,\"text\":\"Hello, Cloud from a C# app!\"}";
#endif
        var message = new Message(Encoding.ASCII.GetBytes(str));

        await deviceClient.SendEventAsync(message);
    }

    public static async Task<string> ReceiveCloudToDeviceMessageAsync()
    {
        var deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Mqtt);

        while (true)
        {
            var receivedMessage = await deviceClient.ReceiveAsync();

            if (receivedMessage != null)
            {
                var messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                await deviceClient.CompleteAsync(receivedMessage);
                return messageData;
            }

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}
