﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DeviceHive.Core.Mapping;
using DeviceHive.Data;
using DeviceHive.WebSockets.API.Core;
using DeviceHive.WebSockets.Core.ActionsFramework;
using Newtonsoft.Json.Linq;
using Ninject;
using ControllerBase = DeviceHive.WebSockets.API.Controllers.ControllerBase;
using DeviceHive.WebSockets.API.Filters;

namespace DeviceHive.DocGenerator
{
    public class WsMetadataGenerator
    {
        private XmlCommentReader _dataXmlCommentReader;
        private XmlCommentReader _wsXmlCommentReader;
        private GeneratorHelper _helper;

        public WsMetadataGenerator()
        {
            var kernel = new StandardKernel();
            kernel.Bind<JsonMapperManager>().ToSelf().InSingletonScope().OnActivation(JsonMapperConfig.ConfigureMapping);
            kernel.Bind<DataContext>().ToSelf().InSingletonScope()
                .OnActivation<DataContext>(context => { context.SetRepositoryCreator(type => kernel.Get(type)); });

            _dataXmlCommentReader = new XmlCommentReader("DeviceHive.Data.xml");
            _wsXmlCommentReader = new XmlCommentReader("DeviceHive.WebSockets.API.xml");

            _helper = new GeneratorHelper(kernel.Get<JsonMapperManager>(), _dataXmlCommentReader);
        }

        public Metadata Generate()
        {
            var controllers = typeof(ControllerBase).Assembly.GetTypes()
                .Where(t => !t.IsAbstract && typeof(ControllerBase).IsAssignableFrom(t)).ToArray();

            var services = new List<MetadataService>();
            foreach (var controller in controllers)
            {
                var methods = new List<MetadataMethod>();

                // inbound messages
                foreach (var action in controller.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.IsDefined(typeof(ActionAttribute), true)))
                {
                    var actionAttribute = action.GetCustomAttributes(typeof(ActionAttribute), true).Cast<ActionAttribute>().First();
                    methods.Add(new MetadataMethod
                    {
                        Name = actionAttribute.ActionName,
                        Documentation = _wsXmlCommentReader.GetMethodElement(action).ElementContents("summary"),
                        Originator = IsDeviceMethod(action) ? "Device" : "Client",
                        Authorization = GetAuthorization(action),
                        RequestParameters = GetRequestParameters(action),
                        ResponseParameters = GetResponseParameters(action, actionAttribute.ActionName),
                    });
                }

                // outbound messages
                foreach (var action in controller.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    var methodElement = _wsXmlCommentReader.GetMethodElement(action);
                    var actionElement = methodElement == null ? null : methodElement.Element("action");
                    if (actionElement == null)
                        continue;

                    methods.Add(new MetadataMethod
                    {
                        Name = "srv: " + actionElement.Contents(),
                        Documentation = methodElement.ElementContents("summary"),
                        Originator = "Server",
                        Authorization = "n/a",
                        ResponseParameters = GetResponseParameters(action, actionElement.Contents()),
                    });
                }

                services.Add(new MetadataService
                {
                    Name = controller.Name.Replace("Controller", ""),
                    Uri = "/" + controller.Name.Replace("Controller", "").ToLower(),
                    Documentation = _wsXmlCommentReader.GetTypeElement(controller).ElementContents("summary"),
                    Methods = methods.OrderBy(m => m.Name).ToArray(),
                });
            }
            var metadata = new Metadata { Services = services.ToArray() };
            return metadata;
        }

        private string GetAuthorization(MethodInfo method)
        {
            var actionAttributes = method.GetCustomAttributes(typeof(ActionFilterAttribute), true).Cast<ActionFilterAttribute>().ToArray();

            var authorizeClient = actionAttributes.OfType<AuthorizeClientAttribute>().FirstOrDefault();
            if (authorizeClient != null)
                return "User" + (authorizeClient.AccessKeyAction == null ? null : " or Key (" + authorizeClient.AccessKeyAction + ")");

            var authorizeDevice = actionAttributes.OfType<AuthorizeDeviceAttribute>().FirstOrDefault();
            var authorizeDeviceRegistration = actionAttributes.OfType<AuthorizeDeviceRegistrationAttribute>().FirstOrDefault();
            if (authorizeDevice != null || authorizeDeviceRegistration != null)
                return "Device";

            return "None";
        }

        private MetadataParameter[] GetRequestParameters(MethodInfo method)
        {
            var parameters = new List<MetadataParameter>();

            // add common parameters
            var actionAttribute = method.GetCustomAttributes(typeof(ActionAttribute), true).Cast<ActionAttribute>().First();
            parameters.Add(new MetadataParameter("action", _helper.ToJsonType(typeof(string)), "Action name: " + actionAttribute.ActionName, true));
            parameters.Add(new MetadataParameter("requestId", _helper.ToJsonType(typeof(object)), "Request unique identifier, will be passed back in the response message.", false));
            
            // add device authentication parameters
            if (IsDeviceMethod(method))
            {
                if (GetAuthorization(method) == "Device")
                {
                    parameters.Add(new MetadataParameter("deviceId", _helper.ToJsonType(typeof(string)), "Device unique identifier (specify if not authenticated).", false));
                    parameters.Add(new MetadataParameter("deviceKey", _helper.ToJsonType(typeof(string)), "Device authentication key (specify if not authenticated).", false));
                }
            }

            // add action method parameters
            foreach (var p in method.GetParameters())
            {
                var methodParamElement = _wsXmlCommentReader.GetMethodParameterElement(method, p.Name);
                parameters.Add(new MetadataParameter
                    {
                        Name = p.Name,
                        Type = _helper.ToJsonType(p.ParameterType),
                        Documentation = methodParamElement == null ? null : methodParamElement.Contents(),
                        IsRequred = !p.IsOptional && !(p.ParameterType.IsGenericType &&
                            p.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>)),
                    });

                if (methodParamElement != null && typeof(JToken).IsAssignableFrom(p.ParameterType))
                {
                    var resourceType = _helper.GetCrefType(methodParamElement);
                    if (resourceType != null)
                    {
                        parameters.AddRange(_helper.GetTypeParameters(resourceType, JsonMapperEntryMode.FromJson, prefix: p.Name + "."));
                    }
                }
            }

            // adjust parameters according to the XML request element
            var methodElement = _wsXmlCommentReader.GetMethodElement(method);
            var requestElement = methodElement == null ? null : methodElement.Element("request");
            if (requestElement != null)
            {
                _helper.AdjustParameters(parameters, requestElement, JsonMapperEntryMode.FromJson);
            }

            // adjust documentation for device/save method
            if (IsDeviceMethod(method) && actionAttribute.ActionName == "device/save")
            {
                parameters.Insert(3, new MetadataParameter("deviceKey", _helper.ToJsonType(typeof(string)), "Device authentication key.", true));
            }


            return parameters.ToArray();
        }

        private MetadataParameter[] GetResponseParameters(MethodInfo method, string actionName)
        {
            var parameters = new List<MetadataParameter>();

            // add common parameters
            parameters.Add(new MetadataParameter("action", _helper.ToJsonType(typeof(string)), "Action name: " + actionName, true));
            if (method.IsDefined(typeof(ActionAttribute), true))
            {
                parameters.Add(new MetadataParameter("status", _helper.ToJsonType(typeof(string)), "Operation execution status (success or error).", true));
                parameters.Add(new MetadataParameter("requestId", _helper.ToJsonType(typeof(object)), "Request unique identifier as specified in the request message.", false));
            }

            // add parameters list from the XML response element
            var methodElement = _wsXmlCommentReader.GetMethodElement(method);
            var responseElement = methodElement == null ? null : methodElement.Element("response");
            if (responseElement != null)
            {
                _helper.AdjustParameters(parameters, responseElement, JsonMapperEntryMode.ToJson);
            }

            return parameters.ToArray();
        }

        private bool IsDeviceMethod(MethodInfo method)
        {
            return method.DeclaringType.Name.StartsWith("Device");
        }
    }
}
