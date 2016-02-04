#include "stdafx.h"

#include <string>
#include <future>
#include <stdio.h>

#include "iothub_client.h"
#include "iothubtransportamqp.h"

//
// String containing Hostname, Device Id & Device Key in the format:
// "HostName=<host_name>;DeviceId=<device_id>;SharedAccessKey=<device_key>"
//
// Note: this connection string is specific to the device "$deviceId$". To configure other devices,
// see information on iothub-explorer at http://aka.ms/iothubgetstartedVSCS
//
static const char* connection_string = "HostName=$iotHubUri$;DeviceId=$deviceId$;SharedAccessKey=$deviceKey$";

struct callback_parameter
{
    std::string &message;
    std::promise<void> completion;
};

void send_callback(IOTHUB_CLIENT_CONFIRMATION_RESULT result, void* context)
{
    auto callback_param = (callback_parameter*)context;

    printf("Message '%s' Received. Result is: %d\n", callback_param->message.c_str(), result);

    callback_param->completion.set_value();
}

void send_device_to_cloud_message()
{
    // Setup IoTHub client configuration
    IOTHUB_CLIENT_HANDLE iothub_client_handle = IoTHubClient_CreateFromConnectionString(connection_string, AMQP_Protocol);
    if (iothub_client_handle == nullptr)
    {
        printf("Failed on IoTHubClient_Create\r\n");
    }
    else
    {
        std::string message = "Hello, Cloud!";

        IOTHUB_MESSAGE_HANDLE message_handle = IoTHubMessage_CreateFromByteArray((const unsigned char*)message.data(), message.size());
        if (message_handle == nullptr)
        {
            printf("unable to create a new IoTHubMessage\n");
        }
        else
        {
            callback_parameter callback_param = { message };
            if (IoTHubClient_SendEventAsync(iothub_client_handle, message_handle, send_callback, &callback_param) != IOTHUB_CLIENT_OK)
            {
                printf("failed to hand over the message to IoTHubClient");
            }
            else
            {
                printf("IoTHubClient accepted the message for delivery\n");
            }

            IoTHubMessage_Destroy(message_handle);
            callback_param.completion.get_future().wait();
        }

        printf("Done!\n");
    }
    IoTHubClient_Destroy(iothub_client_handle);
}

