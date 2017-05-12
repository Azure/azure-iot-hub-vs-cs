$stdafx$

#include <string>
#include <future>
#include <stdio.h>

#include "azure_c_shared_utility/platform.h"
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

//
// To monitor messages sent to device "$deviceId$" use iothub-explorer as follows:
//    iothub-explorer HostName=$iotHubUri$;SharedAccessKeyName=service;SharedAccessKey=$servicePrimaryKey$ monitor-events "$deviceId$"
//

// Refer to http://aka.ms/azure-iot-hub-vs-cs-wiki for more information on Connected Service for Azure IoT Hub

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
    if (platform_init() != 0)
    {
        printf("Failed initializing platform.\r\n");
        return;
    }

    // Setup IoTHub client configuration
    IOTHUB_CLIENT_HANDLE iothub_client_handle = IoTHubClient_CreateFromConnectionString(connection_string, AMQP_Protocol);
    if (iothub_client_handle == nullptr)
    {
        printf("Failed on IoTHubClient_Create\r\n");
    }
    else
    {
		std::string message = "{\"deviceId\":\"$deviceId$\",\"messageId\":1,\"text\":\"Hello, Cloud from a C++ app!\"}";

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

    platform_deinit();
}

static IOTHUBMESSAGE_DISPOSITION_RESULT receive_callback(IOTHUB_MESSAGE_HANDLE message, void* context)
{
    auto completion = (std::promise<void>*)context;

    const unsigned char* buffer;
    size_t size;
    if (IoTHubMessage_GetByteArray(message, &buffer, &size) != IOTHUB_MESSAGE_OK)
    {
        printf("unable to IoTHubMessage_GetByteArray\r\n");
    }
    else
    {
        /*buffer is not zero terminated*/
        std::string str_msg;
        str_msg.resize(size + 1);

        memcpy((void*)str_msg.data(), buffer, size);
        str_msg[size] = '\0';

        printf("Received message '%s' from IoT Hub\n", str_msg.c_str());
    }

    completion->set_value();

    return IOTHUBMESSAGE_ACCEPTED;
}

void receive_cloud_to_device_message()
{
    if (platform_init() != 0)
    {
        printf("Failed initializing platform.\r\n");
        return;
    }

    IOTHUB_CLIENT_HANDLE iothub_client_handle = IoTHubClient_CreateFromConnectionString(connection_string, AMQP_Protocol);
    if (iothub_client_handle == nullptr)
    {
        printf("Failed on IoTHubClient_Create\r\n");
    }
    else
    {
        std::promise<void> completion;
        if (IoTHubClient_SetMessageCallback(iothub_client_handle, receive_callback, &completion) != IOTHUB_CLIENT_OK)
        {
            printf("unable to IoTHubClient_SetMessageCallback\r\n");
        }

        completion.get_future().wait();
        IoTHubClient_Destroy(iothub_client_handle);
    }

    platform_deinit();
}

