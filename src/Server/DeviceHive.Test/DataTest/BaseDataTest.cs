﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeviceHive.Data;
using DeviceHive.Data.Model;
using DeviceHive.Data.Repositories;
using NUnit.Framework;

namespace DeviceHive.Test.DataTest
{
    public abstract class BaseDataTest
    {
        private Stack<Action> _tearDownActions = new Stack<Action>();

        [SetUp]
        protected virtual void SetUp()
        {
        }

        [TearDown]
        protected virtual void TearDown()
        {
            while (_tearDownActions.Count > 0)
            {
                try
                {
                    _tearDownActions.Pop()();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        protected DataContext DataContext { get; set; }

        protected void RegisterTearDown(Action action)
        {
            _tearDownActions.Push(action);
        }

        [Test]
        public void User()
        {
            var user = new User("Test", "TestPass", (int)UserRole.Administrator, (int)UserStatus.Active);
            DataContext.User.Save(user);
            RegisterTearDown(() => DataContext.User.Delete(user.ID));

            // test GetAll
            var users = DataContext.User.GetAll();
            Assert.Greater(users.Count, 0);

            // test Get(id)
            var user1 = DataContext.User.Get(user.ID);
            Assert.IsNotNull(user1);
            Assert.AreEqual("Test", user1.Login);
            Assert.IsTrue(user1.IsValidPassword("TestPass"));
            Assert.IsFalse(user1.IsValidPassword("TestPass2"));
            Assert.AreEqual((int)UserRole.Administrator, user1.Role);
            Assert.AreEqual((int)UserStatus.Active, user1.Status);

            // test Get(name)
            var user2 = DataContext.User.Get("Test");
            Assert.IsNotNull(user2);

            // test Save
            user.Login = "Test2";
            user.SetPassword("TestPass2");
            user.Role = (int)UserRole.Client;
            user.Status = (int)UserStatus.Disabled;
            user.LastLogin = DateTime.UtcNow;
            user.LoginAttempts = 1;
            DataContext.User.Save(user);
            var user3 = DataContext.User.Get(user.ID);
            Assert.AreEqual("Test2", user3.Login);
            Assert.IsTrue(user3.IsValidPassword("TestPass2"));
            Assert.AreEqual((int)UserRole.Client, user3.Role);
            Assert.AreEqual((int)UserStatus.Disabled, user3.Status);
            Assert.IsNotNull(user3.LastLogin);
            Assert.AreEqual(1, user3.LoginAttempts);

            // test Delete
            DataContext.User.Delete(user.ID);
            var user4 = DataContext.User.Get(user.ID);
            Assert.IsNull(user4);
        }

        [Test]
        public void UserNetwork()
        {
            var user = new User("Test", "TestPass", (int)UserRole.Administrator, (int)UserStatus.Active);
            DataContext.User.Save(user);
            RegisterTearDown(() => DataContext.User.Delete(user.ID));

            var network = new Network("Test");
            DataContext.Network.Save(network);
            RegisterTearDown(() => DataContext.Network.Delete(network.ID));

            var userNetwork = new UserNetwork(user, network);
            DataContext.UserNetwork.Save(userNetwork);
            RegisterTearDown(() => DataContext.UserNetwork.Delete(userNetwork.ID));

            // test GetByUser
            var userNetworks1 = DataContext.UserNetwork.GetByUser(user.ID);
            Assert.Greater(userNetworks1.Count, 0);

            // test GetByNetwork
            var userNetworks2 = DataContext.UserNetwork.GetByNetwork(network.ID);
            Assert.Greater(userNetworks2.Count, 0);

            // test Get(id)
            var userNetwork1 = DataContext.UserNetwork.Get(userNetwork.ID);
            Assert.IsNotNull(userNetwork1);
            Assert.AreEqual(user.ID, userNetwork1.UserID);
            Assert.AreEqual(network.ID, userNetwork1.NetworkID);
            Assert.IsNotNull(userNetwork1.User);
            Assert.IsNotNull(userNetwork1.Network);

            // test Get(userId, networkId)
            var userNetwork2 = DataContext.UserNetwork.Get(user.ID, network.ID);
            Assert.IsNotNull(userNetwork2);

            // test Delete
            DataContext.UserNetwork.Delete(userNetwork.ID);
            var userNetwork3 = DataContext.UserNetwork.Get(userNetwork.ID);
            Assert.IsNull(userNetwork3);
        }

        [Test]
        public void Network()
        {
            var network = new Network("Test") { Key = "Key" };
            DataContext.Network.Save(network);
            RegisterTearDown(() => DataContext.Network.Delete(network.ID));

            // test GetAll
            var networks = DataContext.Network.GetAll();
            Assert.Greater(networks.Count, 0);

            // test Get(id)
            var network1 = DataContext.Network.Get(network.ID);
            Assert.IsNotNull(network1);
            Assert.AreEqual("Test", network1.Name);

            // test Get(name)
            var network2 = DataContext.Network.Get("Test");
            Assert.IsNotNull(network2);

            // test Save
            network.Name = "Test2";
            network.Description = "Desc";
            DataContext.Network.Save(network);
            var network4 = DataContext.Network.Get(network.ID);
            Assert.AreEqual("Test2", network4.Name);
            Assert.AreEqual("Desc", network4.Description);

            // test Delete
            DataContext.Network.Delete(network.ID);
            var network5 = DataContext.Network.Get(network.ID);
            Assert.IsNull(network5);
        }

        [Test]
        public void DeviceClass()
        {
            var deviceClass = new DeviceClass("Test", "V1");
            DataContext.DeviceClass.Save(deviceClass);
            RegisterTearDown(() => DataContext.DeviceClass.Delete(deviceClass.ID));

            // test GetAll
            var deviceClasses = DataContext.DeviceClass.GetAll();
            Assert.Greater(deviceClasses.Count, 0);

            // test Get(id)
            var deviceClass1 = DataContext.DeviceClass.Get(deviceClass.ID);
            Assert.IsNotNull(deviceClass1);
            Assert.AreEqual("Test", deviceClass1.Name);

            // test Get(name, version)
            var deviceClass2 = DataContext.DeviceClass.Get("Test", "V1");
            Assert.IsNotNull(deviceClass2);

            // test Save
            deviceClass.Name = "Test2";
            deviceClass.Version = "V2";
            deviceClass.IsPermanent = true;
            deviceClass.OfflineTimeout = 10;
            deviceClass.Data = "{ }";
            DataContext.DeviceClass.Save(deviceClass);
            var deviceClass3 = DataContext.DeviceClass.Get(deviceClass.ID);
            Assert.AreEqual("Test2", deviceClass3.Name);
            Assert.AreEqual("V2", deviceClass3.Version);
            Assert.AreEqual(true, deviceClass3.IsPermanent);
            Assert.AreEqual(10, deviceClass3.OfflineTimeout);
            Assert.AreEqual("{ }", deviceClass3.Data);

            // test Delete
            DataContext.DeviceClass.Delete(deviceClass.ID);
            var deviceClass4 = DataContext.DeviceClass.Get(deviceClass.ID);
            Assert.IsNull(deviceClass4);
        }

        [Test]
        public void Equipment()
        {
            var deviceClass = new DeviceClass("D1", "V1");
            DataContext.DeviceClass.Save(deviceClass);
            RegisterTearDown(() => DataContext.DeviceClass.Delete(deviceClass.ID));

            var equipment = new Equipment("Test", "Code", "Type", deviceClass);
            DataContext.Equipment.Save(equipment);
            RegisterTearDown(() => DataContext.Equipment.Delete(equipment.ID));

            // test GetByDeviceClass
            var equipments = DataContext.Equipment.GetByDeviceClass(deviceClass.ID);
            Assert.AreEqual(1, equipments.Count);
            Assert.AreEqual(equipment.ID, equipments[0].ID);
            Assert.AreEqual(deviceClass.ID, equipments[0].DeviceClassID);
            Assert.IsNotNull(equipments[0].DeviceClass);

            // test Get(id)
            var equipment1 = DataContext.Equipment.Get(equipment.ID);
            Assert.IsNotNull(equipment1);
            Assert.AreEqual("Test", equipment1.Name);
            Assert.AreEqual("Code", equipment1.Code);
            Assert.AreEqual("Type", equipment1.Type);
            Assert.AreEqual(deviceClass.ID, equipment1.DeviceClassID);
            Assert.IsNotNull(equipment1.DeviceClass);

            // test Save
            equipment.Name = "Test2";
            equipment.Code = "Code2";
            equipment.Type = "Type2";
            equipment.Data = "{ }";
            DataContext.Equipment.Save(equipment);
            var equipment2 = DataContext.Equipment.Get(equipment.ID);
            Assert.AreEqual("Test2", equipment2.Name);
            Assert.AreEqual("Code2", equipment2.Code);
            Assert.AreEqual("Type2", equipment2.Type);
            Assert.AreEqual("{ }", equipment2.Data);

            // test update relationship
            var deviceClass2 = new DeviceClass("D2", "V2");
            DataContext.DeviceClass.Save(deviceClass2);
            RegisterTearDown(() => DataContext.DeviceClass.Delete(deviceClass2.ID));
            equipment.DeviceClass = deviceClass2;
            DataContext.Equipment.Save(equipment);
            var equipment3 = DataContext.Equipment.Get(equipment.ID);
            Assert.AreEqual(deviceClass2.ID, equipment3.DeviceClassID);
            Assert.IsNotNull(equipment3.DeviceClass);

            // test Delete
            DataContext.Equipment.Delete(equipment.ID);
            var equipment4 = DataContext.Equipment.Get(equipment.ID);
            Assert.IsNull(equipment4);
        }

        [Test]
        public void Device()
        {
            var network = new Network("N1");
            DataContext.Network.Save(network);
            RegisterTearDown(() => DataContext.Network.Delete(network.ID));

            var deviceClass = new DeviceClass("D1", "V1");
            DataContext.DeviceClass.Save(deviceClass);
            RegisterTearDown(() => DataContext.DeviceClass.Delete(deviceClass.ID));

            var device = new Device(Guid.NewGuid(), "key", "Test", network, deviceClass);
            DataContext.Device.Save(device);
            RegisterTearDown(() => DataContext.Device.Delete(device.ID));

            // test GetByNetwork
            var devices = DataContext.Device.GetByNetwork(network.ID);
            Assert.Greater(devices.Count, 0);

            // test Get(id)
            var device1 = DataContext.Device.Get(device.ID);
            Assert.IsNotNull(device1);
            Assert.AreEqual(device.GUID, device1.GUID);
            Assert.AreEqual("Test", device1.Name);
            Assert.AreEqual(network.ID, device1.NetworkID);
            Assert.AreEqual(deviceClass.ID, device1.DeviceClassID);
            Assert.IsNotNull(device1.Network);
            Assert.IsNotNull(device1.DeviceClass);

            // test Get(guid)
            var device2 = DataContext.Device.Get(device.GUID);
            Assert.IsNotNull(device2);
            Assert.AreEqual(device.GUID, device2.GUID);
            Assert.AreEqual("Test", device2.Name);
            Assert.AreEqual(network.ID, device2.NetworkID);
            Assert.AreEqual(deviceClass.ID, device2.DeviceClassID);
            Assert.IsNotNull(device2.Network);
            Assert.IsNotNull(device2.DeviceClass);

            // test Save
            device.Name = "Test2";
            device.Status = "Status";
            device.Data = "{ }";
            device.Network = null;
            device.NetworkID = null;
            DataContext.Device.Save(device);
            var device3 = DataContext.Device.Get(device.ID);
            Assert.AreEqual("Test2", device3.Name);
            Assert.AreEqual("Status", device3.Status);
            Assert.AreEqual("{ }", device3.Data);
            Assert.IsNull(device3.Network);
            Assert.IsNull(device3.NetworkID);

            // test update relationship
            var deviceClass2 = new DeviceClass("D2", "V2");
            DataContext.DeviceClass.Save(deviceClass2);
            RegisterTearDown(() => DataContext.DeviceClass.Delete(deviceClass2.ID));
            device.DeviceClass = deviceClass2;
            DataContext.Device.Save(device);
            var device4 = DataContext.Device.Get(device.ID);
            Assert.AreEqual(deviceClass2.ID, device4.DeviceClassID);
            Assert.IsNotNull(device4.DeviceClass);

            // test Delete
            DataContext.Device.Delete(device.ID);
            var device5 = DataContext.Device.Get(device.ID);
            Assert.IsNull(device5);
        }

        [Test]
        public void DeviceNotification()
        {
            var network = new Network("N1");
            DataContext.Network.Save(network);
            RegisterTearDown(() => DataContext.Network.Delete(network.ID));

            var deviceClass = new DeviceClass("D1", "V1");
            DataContext.DeviceClass.Save(deviceClass);
            RegisterTearDown(() => DataContext.DeviceClass.Delete(deviceClass.ID));

            var device = new Device(Guid.NewGuid(), "key", "Test", network, deviceClass);
            DataContext.Device.Save(device);
            RegisterTearDown(() => DataContext.Device.Delete(device.ID));

            var notification = new DeviceNotification("Test", device);
            DataContext.DeviceNotification.Save(notification);
            RegisterTearDown(() => DataContext.DeviceNotification.Delete(notification.ID));

            // test GetByDevice
            var notifications = DataContext.DeviceNotification.GetByDevice(device.ID);
            Assert.Greater(notifications.Count, 0);

            // test Get(id)
            var notification1 = DataContext.DeviceNotification.Get(notification.ID);
            Assert.IsNotNull(notification1);
            Assert.AreEqual("Test", notification1.Notification);
            Assert.AreEqual(device.ID, notification1.DeviceID);

            // test Save
            notification.Notification = "Test2";
            notification.Parameters = "{ }";
            DataContext.DeviceNotification.Save(notification);
            var notification2 = DataContext.DeviceNotification.Get(notification.ID);
            Assert.AreEqual("Test2", notification2.Notification);
            Assert.AreEqual("{ }", notification2.Parameters);

            // test Delete
            DataContext.DeviceNotification.Delete(notification.ID);
            var notification3 = DataContext.DeviceNotification.Get(notification.ID);
            Assert.IsNull(notification3);
        }

        [Test]
        public void DeviceCommand()
        {
            var network = new Network("N1");
            DataContext.Network.Save(network);
            RegisterTearDown(() => DataContext.Network.Delete(network.ID));

            var deviceClass = new DeviceClass("D1", "V1");
            DataContext.DeviceClass.Save(deviceClass);
            RegisterTearDown(() => DataContext.DeviceClass.Delete(deviceClass.ID));

            var device = new Device(Guid.NewGuid(), "key", "Test", network, deviceClass);
            DataContext.Device.Save(device);
            RegisterTearDown(() => DataContext.Device.Delete(device.ID));

            var command = new DeviceCommand("Test", device);
            DataContext.DeviceCommand.Save(command);
            RegisterTearDown(() => DataContext.DeviceCommand.Delete(command.ID));

            // test GetByDevice
            var commands = DataContext.DeviceCommand.GetByDevice(device.ID);
            Assert.Greater(commands.Count, 0);

            // test Get(id)
            var command1 = DataContext.DeviceCommand.Get(command.ID);
            Assert.IsNotNull(command1);
            Assert.AreEqual("Test", command1.Command);
            Assert.AreEqual(device.ID, command1.DeviceID);

            // test Save
            command.Command = "Test2";
            command.Parameters = "{ }";
            command.Status = "OK";
            command.Result = "\"Success\"";
            command.UserID = 1;
            DataContext.DeviceCommand.Save(command);
            var command2 = DataContext.DeviceCommand.Get(command.ID);
            Assert.AreEqual("Test2", command2.Command);
            Assert.AreEqual("{ }", command2.Parameters);
            Assert.AreEqual("OK", command2.Status);
            Assert.AreEqual("\"Success\"", command2.Result);
            Assert.AreEqual(1, command2.UserID);

            // test Delete
            DataContext.DeviceCommand.Delete(command.ID);
            var command3 = DataContext.DeviceCommand.Get(command.ID);
            Assert.IsNull(command3);
        }

        [Test]
        public void DeviceEquipment()
        {
            var network = new Network("N1");
            DataContext.Network.Save(network);
            RegisterTearDown(() => DataContext.Network.Delete(network.ID));

            var deviceClass = new DeviceClass("D1", "V1");
            DataContext.DeviceClass.Save(deviceClass);
            RegisterTearDown(() => DataContext.DeviceClass.Delete(deviceClass.ID));

            var device = new Device(Guid.NewGuid(), "key", "Test", network, deviceClass);
            DataContext.Device.Save(device);
            RegisterTearDown(() => DataContext.Device.Delete(device.ID));

            var equipment = new DeviceEquipment("Test", DateTime.UtcNow, device);
            DataContext.DeviceEquipment.Save(equipment);
            RegisterTearDown(() => DataContext.DeviceEquipment.Delete(equipment.ID));

            // test GetByDevice
            var equipments = DataContext.DeviceEquipment.GetByDevice(device.ID);
            Assert.Greater(equipments.Count, 0);

            // test GetByDeviceAndCode
            var equipment0 = DataContext.DeviceEquipment.GetByDeviceAndCode(device.ID, "Test");
            Assert.IsNotNull(equipment0);

            // test Get(id)
            var equipment1 = DataContext.DeviceEquipment.Get(equipment.ID);
            Assert.IsNotNull(equipment1);
            Assert.AreEqual("Test", equipment1.Code);
            Assert.AreEqual(device.ID, equipment1.DeviceID);

            // test Save
            equipment.Code = "Test2";
            equipment.Parameters = "{ }";
            DataContext.DeviceEquipment.Save(equipment);
            var equipment2 = DataContext.DeviceEquipment.Get(equipment.ID);
            Assert.AreEqual("Test2", equipment2.Code);
            Assert.AreEqual("{ }", equipment2.Parameters);

            // test Delete
            DataContext.DeviceEquipment.Delete(equipment.ID);
            var equipment3 = DataContext.DeviceEquipment.Get(equipment.ID);
            Assert.IsNull(equipment3);
        }

        [Test]
        public void Timestamp()
        {
            var timestamp = DataContext.Timestamp.GetCurrentTimestamp();
            Assert.Less(Math.Abs(DateTime.UtcNow.Subtract(timestamp).TotalMinutes), 10);
        }
    }
}