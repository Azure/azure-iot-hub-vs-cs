using System.Runtime.Serialization;

namespace AzureIoTHubConnectedService
{
    [DataContract]
    internal class AzureErrorResponse
    {
        [DataMember(Name = "error")]
        public AzureError Error { get; set; }
    }
}
