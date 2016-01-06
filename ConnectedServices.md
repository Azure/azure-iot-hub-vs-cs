Inspired by http://azure.microsoft.com/en-us/documentation/articles/vs-storage-aspnet-getting-started-blobs/

<properties
	pageTitle="Get started with Azure IoT Hub and Visual Studio connected services (C#) | Microsoft Azure"
	description="How to get started using Azure IoT Hub in a Visual Studio UWP C# project after you have created an IoT Hub using Visual Studio connected services"
	services="iot-hub"
	documentationCenter=""
	authors="MikHegn"
	manager="PaulYuk"
	editor=""/>

<tags
	ms.service="iot-hub"
	ms.workload="uwp"
	ms.tgt_pltfrm="vs-getting-started"
	ms.devlang="na"
	ms.topic="article"
	ms.date="5/1/2016"
	ms.author="mikhegn"/>

# Get started with Azure IoT Hub and Visual Studio connected services (C#)

##Overview

This article describes how to get started using Azure IoT Hub in Visual Studio after you have created or referenced an Azure IoT Hub in a Universal Windows Project by using the Visual Studio Add Connected Services dialog.
The article shows you how to send messages from a device to an IoT Hub and how to send messages from an IoT Hub to a device. The samples are written in C#.

> __Note__

> For information about how to add Azure IoT Hub as a Connected Service in Visual Studio, please see this aticle:
http://aka.ms/iothubconnectedservice 

Azure IoT Hub is a fully managed service that enables reliable and secure bi-directional communications between millions of IoT devices and a solution back end. One of the biggest challenges IoT projects face is how to reliably and securely connect devices to the solution back end. To address this challenge, IoT Hub:

- Offers reliable device-to-cloud and cloud-to-device hyper-scale messaging.
- Enables secure communications using per-device security credentials and access control.
- Includes device libraries for the most popular languages and platforms.

## Congratulations on setting up an IoT Hub and connecting your device using Visual Studio Connected Services

Now that you have connected your device to an IoT Hub, there are a few more steps to go through to start enjoying the flow of data and control of your device through Azure IoT Hub.

The following has been added to your project by the Connected Service:
1. The AzureIoTHub class - contains functions to send and receive messages to/from the IoT Hub
2. A configuration file containing the IoT Hub URI and device credentials 

## Using the AzureIoTHub class

The **AzureIoTHub** class contains two functions that you can start using right away from your own classes:
1. A class to send messages to the IoT Hub - SendDeviceToCloudMessageAsync(string message)
2. A class to start listening to messages from the Iot Hub - RecevieCloudToDeviceMessageAsync()

> [ToDo] Code snippets to show how to use the two functions

## Testing and Debugging

> [ToDo] How can I verify that I'm connected, taht messages are sent and that I can receive messages

## Deploy client code to my device

> [ToDo] How to I deploy my app to the device?

## Configurations

> [ToDo] How to I deploy my app to the device?

## Now what?

> [ToDo] What's next with the dat and control?
> I suggest we create deployable ARM Templates / guides for these scenarios (if possible):
> 1. Collect and Store (IoT Hub > Stream Analytics > Blob Storage)
> 2. Collect and Visualize (IoT Hub > Stream Analytics > Power BI)
> 3. Collect, analyze and Adjust Device (IoT Hub > Stream Analytics > IoT Hub (device message))

### Further reading

You can find out more about IoT Hub in the following articles:

* [IoT Hub overview][lnk-hub-overview]
* [IoT Hub developer guide][lnk-hub-dev-guide]
* [Design your IoT Hub solution][lnk-hub-guidance]
* [Supported device platforms and languages][lnk-supported-devices]
* [Azure IoT Developer Center][lnk-dev-center]

- [Send Cloud-to-Device messages with IoT Hub][lnk-c2d-tutorial] shows how to send messages to devices, and process the delivery feedback produced by IoT Hub.
- [Process Device-to-Cloud messages][lnk-process-d2c-tutorial] shows how to reliably process telemetry and interactive messages coming from devices.
- [Uploading files from devices][lnk-upload-tutorial] describes a pattern that makes use of cloud-to-device messages to facilitate file uploads from devices.

[ToDo]
[AZURE.INCLUDE [vs-storage-dotnet-blobs-next-steps](../../includes/vs-storage-dotnet-blobs-next-steps.md)]