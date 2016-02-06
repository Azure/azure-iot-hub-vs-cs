#include "stdafx.h"

#include <ppltasks.h>
#include <stdio.h>

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


void send_device_to_cloud_message()
{
    // NYI
}
