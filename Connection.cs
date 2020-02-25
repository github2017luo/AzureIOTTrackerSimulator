using System;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;

namespace AzureIOTTrackerSimulator
{
    public class Connection
    {
        public DeviceClient deviceClient = null;
        public ConnectionStatus connectionStatus = ConnectionStatus.Disconnected;

        private void ConnectionStatusChangeHandler(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            connectionStatus = status;
            Console.WriteLine();
            Console.WriteLine("Connection Status Changed to {0}", status);
            Console.WriteLine("Connection Status Changed Reason is {0}", reason);
            Console.WriteLine();
        }

        public async Task Connect(string scopeId, string deviceId, string deviceKey)
        {
            if (deviceClient != null)
            {
                Console.WriteLine("Connect was already called?");
                return;
            }

            using (var security = new SecurityProviderSymmetricKey(deviceId, deviceKey, deviceKey /*use secondary key?*/))
            using (var transport = new ProvisioningTransportHandlerAmqp(TransportFallbackType.TcpOnly))
            {
                // connect to Azure IoT DPS (device provisioning service)
                var provClient = ProvisioningDeviceClient.Create("global.azure-devices-provisioning.net", scopeId, security, transport);
                Console.Write("ProvisioningClient RegisterAsync . . . ");
                DeviceRegistrationResult result = await provClient.RegisterAsync().ConfigureAwait(false);

                Console.WriteLine($"{result.Status}");
                Console.WriteLine($"ProvisioningClient AssignedHub: {result.AssignedHub}; DeviceID: {result.DeviceId}");

                if (result.Status != ProvisioningRegistrationStatusType.Assigned)
                {
                    Console.WriteLine("Error: Authentication has failed");
                    return;
                }

                IAuthenticationMethod auth;
                Console.WriteLine("Creating Symmetric Key DeviceClient authenication");
                auth = new DeviceAuthenticationWithRegistrySymmetricKey(result.DeviceId,
                  (security as SecurityProviderSymmetricKey).GetPrimaryKey());
                deviceClient = DeviceClient.Create(result.AssignedHub, auth, TransportType.Amqp);
                deviceClient.SetConnectionStatusChangesHandler(ConnectionStatusChangeHandler);

                Console.WriteLine("DeviceClient OpenAsync.");
                await deviceClient.OpenAsync().ConfigureAwait(false);
            }
        }
    }
}
