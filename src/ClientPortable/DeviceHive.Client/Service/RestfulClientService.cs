﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
//using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;

namespace DeviceHive.Client
{
    /// <summary>
    /// Provides default implementation of the <see cref="IClientService"/> interface to connect a client with the RESTful DeviceHive service.
    /// Using this class, clients can get information about networks and devices, receive notification and send commands.
    /// </summary>
    public class RestfulClientService : IClientService
    {
        #region Public Properties

        /// <summary>
        /// Gets URL of the DeviceHive service.
        /// </summary>
        public string ServiceUrl { get; private set; }
        
        /// <summary>
        /// Gets login used for service authentication.
        /// </summary>
        public string Login { get; private set; }
        
        /// <summary>
        /// Gets password used for service authentication.
        /// </summary>
        public string Password { get; private set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="serviceUrl">URL of the DeviceHive service.</param>
        /// <param name="login">Login used for service authentication.</param>
        /// <param name="password">Password used for service authentication.</param>
        public RestfulClientService(string serviceUrl, string login, string password)
        {
            ServiceUrl = serviceUrl;
            Login = login;
            Password = password;
        }
        #endregion

        #region IClientService Members

        /// <summary>
        /// Gets API info.
        /// </summary>
        /// <returns><see cref="ApiInfo"/> object.</returns>
        public async Task<ApiInfo> GetApiInfoAsync()
        {
            return await GetAsync<ApiInfo>("/info");
        }

        /// <summary>
        /// Gets a list of networks.
        /// </summary>
        /// <returns>A list of <see cref="Network"/> objects.</returns>
        public async Task<List<Network>> GetNetworksAsync()
        {
            return await GetAsync<List<Network>>("/network");
        }

        /// <summary>
        /// Gets a list of devices of the specific network.
        /// </summary>
        /// <param name="networkId">Network identifier.</param>
        /// <returns>A list of <see cref="Device"/> objects that belongs to the specified network.</returns>
        public async Task<List<Device>> GetDevicesAsync(int networkId)
        {
            var network = await GetAsync<Network>(string.Format("/network/{0}", networkId));
            return network == null ? null : network.Devices;
        }

        /// <summary>
        /// Gets a list of devices of all networks.
        /// </summary>
        /// <returns>A list of <see cref="Device"/> objects.</returns>
        public async Task<List<Device>> GetDevicesAsync()
        {
            return await GetAsync<List<Device>>("/device");
        }

        /// <summary>
        /// Gets information about device.
        /// </summary>
        /// <param name="deviceId">Device unique identifier.</param>
        /// <returns><see cref="Device"/> object.</returns>
        public async Task<Device> GetDeviceAsync(Guid deviceId)
        {
            return await GetAsync<Device>(string.Format("/device/{0}", deviceId));
        }

        /// <summary>
        /// Gets a list of equipment in a device class.
        /// </summary>
        /// <param name="deviceClassId">Device class identifier.</param>
        /// <returns>A list of <see cref="Equipment"/> objects for the specified device class.</returns>
        public async Task<List<Equipment>> GetEquipmentAsync(int deviceClassId)
        {
            var deviceClass = await GetAsync<DeviceClass>(string.Format("/device/class/{0}", deviceClassId));
            return deviceClass == null ? null : deviceClass.Equipment;
        }

        /// <summary>
        /// Gets a list of device equipment states.
        /// These objects provide information about the current state of device equipment.
        /// </summary>
        /// <param name="deviceId">Device unique identifier.</param>
        /// <returns>A list of <see cref="DeviceEquipmentState"/> objects.</returns>
        public async Task<List<DeviceEquipmentState>> GetEquipmentStateAsync(Guid deviceId)
        {
            return await GetAsync<List<DeviceEquipmentState>>(string.Format("/device/{0}/equipment", deviceId));
        }

        /// <summary>
        /// Gets a list of notifications generated by the device.
        /// </summary>
        /// <param name="deviceId">Device unique identifier.</param>
        /// <param name="start">Notifications start date (inclusive, optional).</param>
        /// <param name="end">Notifications end date (inclusive, optional).</param>
        /// <returns>A list of <see cref="Notification"/> objects.</returns>
        public async Task<List<Notification>> GetNotificationsAsync(Guid deviceId, DateTime? start = null, DateTime? end = null)
        {
            var url = string.Format("/device/{0}/notification", deviceId);
            var parameters = new[]
                {
                    start == null ? null : "start=" + start.Value.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"),
                    end == null ? null : "end=" + end.Value.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"),
                }.Where(p => p != null);

            return await GetAsync<List<Notification>>(url + (parameters.Any() ? "?" + string.Join("&", parameters) : null));
        }

        /// <summary>
        /// Polls device notification from the service.
        /// </summary>
        /// <param name="callback">Callback action used to process notifications.</param>
        /// <param name="deviceId">Device unique identifier.</param>
        /// <param name="timestamp">Last received notification timestamp.</param>
        /// <param name="token">Cancellation token used to cancel polling operation.</param>
        public async Task PollNotifications(Action<List<Notification>> callback, Guid deviceId, DateTime? timestamp, CancellationToken token)
        {
            if (callback == null)
            {
                throw new ArgumentException("Callback can't be null");
            }
            if (deviceId == null)
            {
                throw new ArgumentException("Device GUID can't be null");
            }
            while (true)
            {
                try
                {
                    var url = string.Format("/device/{0}/notification/poll", deviceId);
                    if (timestamp == null)
                    {
                        var apiInfo = await GetApiInfoAsync();
                        timestamp = apiInfo.ServerTimestamp;
                    }
                    url += "?timestamp=" + timestamp.Value.ToString("yyyy-MM-ddTHH:mm:ss.ffffff");

                    var notifications = await GetAsync<List<Notification>>(url, token);
                    if (notifications != null && notifications.Any())
                    {
                        timestamp = notifications.Max(n => n.Timestamp.Value);
                        callback(notifications);
                    }
                    continue; // Continue to next iteration w/o waiting on-exception delay
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception) { }

                await TaskEx.Delay(TimeSpan.FromSeconds(1)); // Wait on exception to avoid thread lock when request fails immediately
            }
        }

        /// <summary>
        /// Polls device notification from the service.
        /// </summary>
        /// <param name="callback">Callback action used to process notifications.</param>
        /// <param name="deviceIds">List of device unique identifiers.</param>
        /// <param name="timestamp">Last received notification timestamp.</param>
        /// <param name="token">Cancellation token used to cancel the polling operation.</param>
        public async Task PollNotifications(Action<List<DeviceNotification>> callback, Guid[] deviceIds, DateTime? timestamp, CancellationToken token)
        {
            if (callback == null)
            {
                throw new ArgumentException("Callback can't be null");
            }
            if (deviceIds == null || deviceIds.Length == 0)
            {
                throw new ArgumentException("Device GUID array can't be empty");
            }
            while (true)
            {
                try
                {
                    var url = "/device/notification/poll";
                    if (timestamp == null)
                    {
                        var apiInfo = await GetApiInfoAsync();
                        timestamp = apiInfo.ServerTimestamp;
                    }
                    var parameters = new[]
                    {
                        "timestamp=" + timestamp.Value.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"),
                        (deviceIds == null || deviceIds.Length == 0) ? null :
                            "deviceGuids=" + (string.Join(",", deviceIds))
                    };
                    if (parameters.Any())
                        url += "?" + string.Join("&", parameters);

                    var notifications = await GetAsync<List<DeviceNotification>>(url, token);
                    if (notifications != null && notifications.Any())
                    {
                        timestamp = notifications.Max(n => n.Notification.Timestamp.Value);
                        callback(notifications);
                    }
                    continue; // Continue to next iteration w/o waiting on-exception delay
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception) { }

                await TaskEx.Delay(TimeSpan.FromSeconds(1)); // Wait on exception to avoid thread lock when request fails immediately
            }
        }

        /// <summary>
        /// Gets information about a notification generated by the device.
        /// </summary>
        /// <param name="deviceId">Device unique identifier.</param>
        /// <param name="id">Notification identifier.</param>
        /// <returns>The <see cref="Notification"/> object.</returns>
        public async Task<Notification> GetNotificationAsync(Guid deviceId, int id)
        {
            return await GetAsync<Notification>(string.Format("/device/{0}/notification/{1}", deviceId, id));
        }

        /// <summary>
        /// Gets a list of commands sent to the device.
        /// </summary>
        /// <param name="deviceId">Device unique identifier.</param>
        /// <param name="start">Commands start date (inclusive, optional).</param>
        /// <param name="end">Commands end date (inclusive, optional).</param>
        /// <returns>A list of <see cref="Command"/> objects.</returns>
        public async Task<List<Command>> GetCommandsAsync(Guid deviceId, DateTime? start = null, DateTime? end = null)
        {
            var url = string.Format("/device/{0}/command", deviceId);
            var parameters = new[]
                {
                    start == null ? null : "start=" + start.Value.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"),
                    end == null ? null : "end=" + end.Value.ToString("yyyy-MM-ddTHH:mm:ss.ffffff"),
                }.Where(p => p != null);

            return await GetAsync<List<Command>>(url + (parameters.Any() ? "?" + string.Join("&", parameters) : null));
        }

        /// <summary>
        /// Polls a list of commands sent to the service.
        /// </summary>
        /// <param name="callback">Callback action used to process commands.</param>
        /// <param name="deviceId">Device unique identifier.</param>
        /// <param name="timestamp">Last received command timestamp.</param>
        /// <param name="token">Cancellation token used to cancel polling operation.</param>
        public async Task PollCommands(Action<List<Command>> callback, Guid deviceId, DateTime? timestamp, CancellationToken token)
        {
            if (callback == null)
            {
                throw new ArgumentException("Callback can't be null");
            }
            if (deviceId == null)
            {
                throw new ArgumentException("Device GUID can't be null");
            }
            while (true)
            {
                try
                {
                    var url = string.Format("/device/{0}/command/poll", deviceId);
                    if (timestamp == null)
                    {
                        var apiInfo = await GetApiInfoAsync();
                        timestamp = apiInfo.ServerTimestamp;
                    }
                    url += "?timestamp=" + timestamp.Value.ToString("yyyy-MM-ddTHH:mm:ss.ffffff");

                    var commands = await GetAsync<List<Command>>(url, token);
                    if (commands != null && commands.Any())
                    {
                        timestamp = commands.Max(n => n.Timestamp.Value);
                        callback(commands);
                    }
                    continue; // Continue to next iteration w/o waiting on-exception delay
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception) { }

                await TaskEx.Delay(TimeSpan.FromSeconds(1)); // Wait on exception to avoid thread lock when request fails immediately
            }
        }

        /// <summary>
        /// Gets information about a command sent to the device.
        /// </summary>
        /// <param name="deviceId">Device unique identifier.</param>
        /// <param name="id">Command identifier.</param>
        /// <returns>The <see cref="Command"/> object.</returns>
        public async Task<Command> GetCommandAsync(Guid deviceId, int id)
        {
            return await GetAsync<Command>(string.Format("/device/{0}/command/{1}", deviceId, id));
        }

        /// <summary>
        /// Sends new command to the device.
        /// </summary>
        /// <param name="deviceId">Device unique identifier.</param>
        /// <param name="command">A <see cref="Command"/> object to be sent.</param>
        /// <returns>The <see cref="Command"/> object with updated identifier and timestamp.</returns>
        public async Task<Command> SendCommandAsync(Guid deviceId, Command command)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            return await PostAsync<Command>(string.Format("/device/{0}/command", deviceId), command);
        }


        /// <summary>
        /// Waits for a command to be handled by the device.
        /// </summary>
        /// <param name="deviceId">Device unique identifier.</param>
        /// <param name="id">Command identifier.</param>
        /// <param name="token">Cancellation token used to cancel the polling operation.</param>
        /// <returns>The <see cref="Command"/> object with status and result fields.</returns>
        public async Task<Command> WaitCommandAsync(Guid deviceId, int id, CancellationToken token)
        {
            if (deviceId == null)
            {
                throw new ArgumentException("Device GUID can't be null");
            }
            while (true)
            {
                var url = string.Format("/device/{0}/command/{1}/poll", deviceId, id);
                try
                {
                    var command = await GetAsync<Command>(url, token);
                    if (command != null)
                    {
                        return command;
                    }
                    continue; // Continue to next iteration w/o waiting on-exception delay
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception) { }

                await TaskEx.Delay(TimeSpan.FromSeconds(1)); // Wait on exception to avoid thread lock when request fails immediately
            }
            return null;
        }
        #endregion

        #region Private Methods

        private async Task<T> GetAsync<T>(string url, CancellationToken? token = null)
        {
            var request = WebRequest.CreateHttp(ServiceUrl + url);
            request.Credentials = new NetworkCredential(Login, Password);
            try
            {
                var task = request.GetResponseAsync();
                if (token != null)
                {
                    task = task.WithCancellation((CancellationToken)token);
                }
                var response = await task;
                using (var stream = response.GetResponseStream())
                {
                    return Deserialize<T>(stream);
                }
            }
            catch (WebException ex)
            {
                throw new ClientServiceException("Network error while sending request to the server", ex);
            }
            catch (OperationCanceledException ex)
            {
                request.Abort();
                throw ex;
            }
        }

        private async Task<T> PostAsync<T>(string url, T obj)
        {
            var request = WebRequest.CreateHttp(ServiceUrl + url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Credentials = new NetworkCredential(Login, Password);
            using (var stream = await request.GetRequestStreamAsync())
            {
                Serialize(stream, obj);
            }

            try
            {
                var response = await request.GetResponseAsync();
                using (var stream = response.GetResponseStream())
                {
                    return Deserialize<T>(stream);
                }
            }
            catch (WebException ex)
            {
                throw new ClientServiceException("Network error while sending request to the server", ex);
            }
        }

        private void Serialize<T>(Stream stream, T obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            using (var writer = new StreamWriter(stream))
            {
                var serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Ignore;
                serializer.ContractResolver = new JsonContractResolver();
                serializer.Serialize(writer, obj);
            }
        }

        private T Deserialize<T>(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                var serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Ignore;
                serializer.ContractResolver = new JsonContractResolver();
                return (T)serializer.Deserialize(reader, typeof(T));
            }
        }
        #endregion

        #region JsonContractResolver class

        private class JsonContractResolver : CamelCasePropertyNamesContractResolver
        {
            #region DefaultContractResolver Members

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                if (property.DeclaringType == typeof(Notification) && property.PropertyName == "name")
                {
                    property.PropertyName = "notification";
                }
                if (property.DeclaringType == typeof(Command) && property.PropertyName == "name")
                {
                    property.PropertyName = "command";
                }
                return property;
            }
            #endregion
        }
        #endregion
    }
}