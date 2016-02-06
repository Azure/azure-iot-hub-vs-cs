# Get started with Azure IoT Hub and Visual Studio connected services (C#)

## Congratulations on setting up an IoT Hub and connecting your device using Visual Studio Connected Services

Now that you have connected your device to an IoT Hub, there are a few more steps to go through to start enjoying the flow of data and control of your device through Azure IoT Hub.

The following has been added to your project by the Connected Service:
1. The AzureIoTHub class - contains functions to send and receive messages to/from the IoT Hub
2. A configuration file containing the IoT Hub URI and device credentials 

> __Note__

> This article assumes that you have chosen to add Azure IoT-Hub as a connected service to your Visual Studio project.
> To learn more about Connected Services, please see this: https://www.visualstudio.com/en-us/features/connected-services-vs.aspx
> To learn more about IoT-Hub, please see this: https://azure.microsoft.com/en-us/services/iot-hub/ 
 
## Using the AzureIoTHub class

The **AzureIoTHub** class contains two methods that you can start using right away from your own classes:

1. A method to send messages to the IoT Hub - SendDeviceToCloudMessageAsync()
2. A method to start listening to messages from the Iot Hub - ReceiveCloudToDeviceMessageAsync()

Simply just call the methods to send a message and receive commands any way you like.

## Verifying Connecivity and Messages

In order to verify your devices connectivity, you can use IoT-Hub Explorer. This is a cross-platform CLI tool based on node, so if node is not installed on your system you should install it now https://nodejs.org/en/download/.

Get the tool via npm:
> npm install -g iothub-explorer@latest

Now in your command prompt start the IoT-Hub Explorer to see data sent from your device into Azure IoT-Hub. The tool will need the connection string to connect to your instance of the Azure IoT Hub. You can find the connection string in the Azure Portal under the settings tab of your IoT Hub instance: navigate to Settings | Shared access policies | Connection string – primary key.

Use the following command to start monitoring events from the device
> iothub-explorer `<connection-string`> monitor-events `<device-name>`

You will see a message that says events from your device are being monitored. Now run your application. You should see an event received in the console.


Use the following commands to send a command and then wait for the device to respond with an acknowledgement
> iothub-explorer send `<device-name>` MyMessage --ack=full
> iothub-explorer receive `<device-name>`

For further details about the IoT-Hub Explorer, please refere to this article: https://github.com/Azure/azure-iot-sdks/blob/master/tools/iothub-explorer/readme.md

## Configurations

If you have more devices that needs the same code, you need to create a new device and put in corresponding access tokens for each devie to uniquely identify it against the IoT-Hub.

## Further reading

You can find out more about IoT Hub in the following articles:

* Azure IoT developer center: https://azure.microsoft.com/en-us/develop/iot/
* Design your IoT Hub solution: https://azure.microsoft.com/en-us/documentation/articles/iot-hub-guidance/