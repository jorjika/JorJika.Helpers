using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text.RegularExpressions;

namespace JorJika.Helpers
{
    public class WCFHelper<T, TInterface> where TInterface : class
    {
        #region Private Fields

        private T _service { get; set; }
        private string _wsUrl { get; set; }
        private int _sendTimeOut { get; set; }
        private int _receiveTimeOut { get; set; }
        private int _openTimeOut { get; set; }
        private int _closeTimeOut { get; set; } 

        #endregion

        #region Constructors

        public WCFHelper(string wsUrl, int sendTimeOut = 1, int receiveTimeOut = 1, int openTimeOut = 1, int closeTimeOut = 1)
        {
            _wsUrl = wsUrl;
            _sendTimeOut = sendTimeOut;
            _receiveTimeOut = receiveTimeOut;
            _openTimeOut = openTimeOut;
            _closeTimeOut = closeTimeOut;

            _service = ConfigureServiceDefault();
        }

        public WCFHelper(string wsUrl, BasicHttpBinding basicHttpBinding)
        {
            _wsUrl = wsUrl;
            _service = ConfigureServiceWithCustomBinding(basicHttpBinding);
        }

        public WCFHelper(string wsUrl, string username, string password, int sendTimeOut = 1, int receiveTimeOut = 1, int openTimeOut = 1, int closeTimeOut = 1)
        {
            _wsUrl = wsUrl;
            _sendTimeOut = sendTimeOut;
            _receiveTimeOut = receiveTimeOut;
            _openTimeOut = openTimeOut;
            _closeTimeOut = closeTimeOut;

            _service = ConfigureServiceWithBasic(username, password);
        }

        #endregion

        public T GetService() => _service;

        #region Configurations

        private T ConfigureServiceWithCustomBinding(BasicHttpBinding binding)
        {
            var wsUrl = _wsUrl;

            var service = (T)Activator.CreateInstance(typeof(T), binding, new EndpointAddress(wsUrl));
            var svc = service as ClientBase<TInterface>;

            foreach (DataContractSerializerOperationBehavior dscob in from op in svc.Endpoint.Contract.Operations from dscob1 in op.Behaviors.FindAll<DataContractSerializerOperationBehavior>() select dscob1)
                dscob.MaxItemsInObjectGraph = int.MaxValue;

            return service;
        }

        private T ConfigureServiceDefault()
        {
            var binding = new BasicHttpBinding();
            binding.Name = $"BasicHttpBinding_I{typeof(T).ToString()}";
            binding.SendTimeout = TimeSpan.FromMinutes(_sendTimeOut);
            binding.CloseTimeout = TimeSpan.FromMinutes(_closeTimeOut);
            binding.OpenTimeout = TimeSpan.FromMinutes(_openTimeOut);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(_receiveTimeOut);
            binding.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;
            binding.MaxReceivedMessageSize = int.MaxValue;

            binding.Security.Mode = BasicHttpSecurityMode.None;
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;

            var wsUrl = _wsUrl;

            var service = (T)Activator.CreateInstance(typeof(T), binding, new EndpointAddress(wsUrl));
            var svc = service as ClientBase<TInterface>;

            foreach (DataContractSerializerOperationBehavior dscob in from op in svc.Endpoint.Contract.Operations from dscob1 in op.Behaviors.FindAll<DataContractSerializerOperationBehavior>() select dscob1)
                dscob.MaxItemsInObjectGraph = int.MaxValue;

            return service;
        }

        private T ConfigureServiceWithBasic(string username, string password)
        {
            var binding = new BasicHttpBinding(BasicHttpSecurityMode.TransportCredentialOnly);
            binding.Name = $"BasicHttpBinding_I{typeof(T).ToString()}";
            binding.SendTimeout = TimeSpan.FromMinutes(_sendTimeOut);
            binding.CloseTimeout = TimeSpan.FromMinutes(_closeTimeOut);
            binding.OpenTimeout = TimeSpan.FromMinutes(_openTimeOut);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(_receiveTimeOut);
            binding.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;
            binding.MaxReceivedMessageSize = int.MaxValue;

            binding.Security.Mode = BasicHttpSecurityMode.None;
            binding.Security.Transport = new HttpTransportSecurity() { ClientCredentialType = HttpClientCredentialType.Basic };

            var wsUrl = _wsUrl;

            var service = (T)Activator.CreateInstance(typeof(T), binding, new EndpointAddress(wsUrl));
            var svc = service as ClientBase<TInterface>;

            svc.ClientCredentials.UserName.UserName = username;
            svc.ClientCredentials.UserName.Password = password;

            foreach (DataContractSerializerOperationBehavior dscob in from op in svc.Endpoint.Contract.Operations from dscob1 in op.Behaviors.FindAll<DataContractSerializerOperationBehavior>() select dscob1)
                dscob.MaxItemsInObjectGraph = int.MaxValue;

            return service;
        }

        #endregion

        public override string ToString()
        {
            return base.ToString();
        }

    }
}
