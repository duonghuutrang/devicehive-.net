﻿using System;
using System.Collections.Generic;
using System.Linq;
using DeviceHive.Core.Mapping;
using DeviceHive.Core.Messaging;
using DeviceHive.Data;
using DeviceHive.Data.Model;
using DeviceHive.WebSockets.ActionsFramework;
using DeviceHive.WebSockets.Network;
using DeviceHive.WebSockets.Subscriptions;
using Newtonsoft.Json.Linq;
using Ninject;

namespace DeviceHive.WebSockets.Controllers
{
    public class ClientController : ControllerBase
    {        
        #region Private fields

        private const int _maxLoginAttempts = 10;

        private readonly DeviceSubscriptionManager _subscriptionManager;
        private readonly CommandSubscriptionManager _commandSubscriptionManager;
        private readonly MessageBus _messageBus;

        #endregion

        #region Constructor

        public ClientController(ActionInvoker actionInvoker, WebSocketServerBase server,
            DataContext dataContext, JsonMapperManager jsonMapperManager,
            [Named("DeviceNotification")] DeviceSubscriptionManager subscriptionManager,
            CommandSubscriptionManager commandSubscriptionManager,
            MessageBus messageBus) :
            base(actionInvoker, server, dataContext, jsonMapperManager)
        {
            _subscriptionManager = subscriptionManager;
            _commandSubscriptionManager = commandSubscriptionManager;
            _messageBus = messageBus;
        }

        #endregion

        #region Properties

        private User CurrentUser
        {
            get { return (User) Connection.Session["user"]; }
            set { Connection.Session["user"] = value; }
        }

        #endregion

        #region Overrides

        public override bool IsAuthenticated
        {
            get { return CurrentUser != null; }
        }

        public override void CleanupConnection(WebSocketConnectionBase connection)
        {
            base.CleanupConnection(connection);
            CleanupNotifications(connection);
        }

        #endregion

        #region Methods

        #region Actions

        [Action("authenticate")]
        public void Authenticate(string login, string password)
        {
            if (login == null || password == null)
                throw new WebSocketRequestException("Please specify 'login' and 'password'");

            var user = DataContext.User.Get(login);
            if (user == null || user.Status != (int)UserStatus.Active)
                throw new WebSocketRequestException("Invalid login or password");

            if (user.IsValidPassword(password))
            {
                UpdateUserLastLogin(user);
            }
            else
            {
                IncrementUserLoginAttempts(user);
                throw new WebSocketRequestException("Invalid login or password");
            }

            CurrentUser = user;
            SendSuccessResponse();
        }

        [Action("command/insert", NeedAuthentication = true)]
        public void InsertDeviceCommand(Guid deviceGuid, JObject command)
        {
            if (deviceGuid == Guid.Empty)
                throw new WebSocketRequestException("Please specify valid deviceGuid");

            if (command == null)
                throw new WebSocketRequestException("Please specify command");

            var device = DataContext.Device.Get(deviceGuid);
            if (device == null || !IsNetworkAccessible(device.NetworkID))
                throw new WebSocketRequestException("Device not found");

            var commandEntity = CommandMapper.Map(command);
            commandEntity.Device = device;
            commandEntity.UserID = CurrentUser.ID;
            Validate(commandEntity);

            DataContext.DeviceCommand.Save(commandEntity);
            _commandSubscriptionManager.Subscribe(Connection, commandEntity.ID);
            _messageBus.Notify(new DeviceCommandAddedMessage(device.ID, commandEntity.ID));
            
            command = CommandMapper.Map(commandEntity);
            SendResponse(new JProperty("command", command));
        }

        [Action("notification/subscribe", NeedAuthentication = true)]
        public void SubsrcibeToDeviceNotifications(DateTime? timestamp)
        {
            var devices = GetSubscriptionDevices().ToArray();

            if (timestamp != null)
                SendInitialNotifications(devices, timestamp);

            var deviceIds = GetSubscriptionDeviceIds(devices);
            foreach (var deviceId in deviceIds)
                _subscriptionManager.Subscribe(Connection, deviceId);

            SendSuccessResponse();
        }

        [Action("notification/unsubscribe", NeedAuthentication = true)]
        public void UnsubsrcibeFromDeviceNotifications()
        {
            var deviceIds = GetSubscriptionDeviceIds().ToArray();
            foreach (var deviceId in deviceIds)
                _subscriptionManager.Unsubscribe(Connection, deviceId);

            SendSuccessResponse();
        }

        #endregion

        #region Notification handling

        public void HandleDeviceNotification(int deviceId, int notificationId)
        {
            var notification = DataContext.DeviceNotification.Get(notificationId);
            var device = DataContext.Device.Get(deviceId);
            var connections = _subscriptionManager.GetConnections(deviceId);

            foreach (var connection in connections)
                Notify(connection, notification, device);
        }

        private void CleanupNotifications(WebSocketConnectionBase connection)
        {
            _subscriptionManager.Cleanup(connection);
        }

        private void SendInitialNotifications(Device[] devices, DateTime? timestamp)
        {
            var initialNotificationList = GetInitialNotificationList(Connection);

            if (devices.Length == 0 && devices[0] == null)
                devices = DataContext.Device.GetByUser(CurrentUser.ID).ToArray();

            lock (initialNotificationList)
            {
                var initialNotifications = DataContext.DeviceNotification.GetByDevices(
                    devices.Select(d => d.ID).ToArray(), timestamp.Value.AddTicks(10), null);

                foreach (var notification in initialNotifications)
                {
                    initialNotificationList.Add(notification.ID);

                    foreach (var device in devices)
                        Notify(Connection, notification, device, validateAccess: false);
                }
            }
        }

        private void Notify(WebSocketConnectionBase connection, DeviceNotification notification, Device device,
            bool validateAccess = true)
        {
            if (validateAccess)
            {
                var user = (User) connection.Session["user"];
                if (user == null || !IsNetworkAccessible(device.NetworkID, user))
                    return;
            }

            connection.SendResponse("notification/insert",
                new JProperty("deviceGuid", device.GUID),
                new JProperty("notification", NotificationMapper.Map(notification)));
        }

        #endregion

        #region Command update handle

        public void HandleCommandUpdate(int commandId)
        {
            var command = DataContext.DeviceCommand.Get(commandId);
            var connections = _commandSubscriptionManager.GetConnections(commandId);

            foreach (var connection in connections)
            {
                connection.SendResponse("command/update",
                    new JProperty("command", CommandMapper.Map(command)));
            }
        }

        #endregion

        #region Helper methods

        private bool IsNetworkAccessible(int? networkId, User user = null)
        {
            if (user == null)
                user = CurrentUser;

            if (user.Role == (int) UserRole.Administrator)
                return true;

            if (networkId == null)
                return false;

            var userNetworks = DataContext.UserNetwork.GetByUser(user.ID);
            return userNetworks.Any(un => un.NetworkID == networkId);
        }

        private void IncrementUserLoginAttempts(User user)
        {
            user.LoginAttempts++;
            if (user.LoginAttempts >= _maxLoginAttempts)
                user.Status = (int)UserStatus.LockedOut;
            DataContext.User.Save(user);
        }

        private void UpdateUserLastLogin(User user)
        {
            user.LoginAttempts = 0;
            user.LastLogin = DateTime.UtcNow;
            DataContext.User.Save(user);
        }

        private IEnumerable<int?> GetSubscriptionDeviceIds(IEnumerable<Device> devices = null)
        {
            if (devices == null)
                devices = GetSubscriptionDevices();

            foreach (var device in devices)
            {
                if (device == null)
                {
                    yield return null;
                }
                else
                {
                    yield return device.ID;
                }
            }
        }

        private IEnumerable<Device> GetSubscriptionDevices()
        {
            var deviceGuids = ParseDeviceGuids();
            if (deviceGuids == null)
            {
                yield return null;
                yield break;
            }

            foreach (var deviceGuid in deviceGuids)
            {
                var device = DataContext.Device.Get(deviceGuid);
                if (device == null || !IsNetworkAccessible(device.NetworkID))
                    throw new WebSocketRequestException("Invalid deviceGuid: " + deviceGuid);

                yield return device;
            }
        }

        private IEnumerable<Guid> ParseDeviceGuids()
        {
            if (ActionArgs == null)
                return null;

            var deviceGuids = ActionArgs["deviceGuids"];
            if (deviceGuids == null)
                return null;

            var deviceGuidsArray = deviceGuids as JArray;
            if (deviceGuidsArray != null)
                return deviceGuidsArray.Select(t => (Guid) t).ToArray();

            return new[] {(Guid) deviceGuids};
        }

        private ISet<int> GetInitialNotificationList(WebSocketConnectionBase connection)
        {
            return (ISet<int>)connection.Session.GetOrAdd("InitialNotifications", () => new HashSet<int>());
        }

        #endregion

        #endregion
    }
}