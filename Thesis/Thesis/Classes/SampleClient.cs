using System;
using System.IO;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using Xamarin.Forms;
namespace Thesis
{
    public class SampleClient
    {
        public enum ConnectionStatus
        {
            None,
            NotConnected,
            Connected,
            Error
        }

        public ConnectionStatus connectionStatus;
        public bool haveAppCertificate;
        public Session session;
        private SessionReconnectHandler reconnectHandler;
        private const int ReconnectPeriod = 10;

        private LabelViewModel info;
        private ApplicationConfiguration config;

        public SampleClient(LabelViewModel text)
        {
            connectionStatus = ConnectionStatus.None;
            session = null;
            info = text;
            haveAppCertificate = false;
            config = null;
        }

        public async void CreateCertificate()
        {
            ApplicationInstance application = new ApplicationInstance
            {
                ApplicationType = ApplicationType.Client,
                ConfigSectionName = "Opc.Ua.SampleClient"
            };

            if (Device.RuntimePlatform == "Android")
            {
                string currentFolder = DependencyService.Get<IPathService>().PublicExternalFolder.ToString();
                string filename = application.ConfigSectionName + ".Config.xml";
                string content = DependencyService.Get<IAssetService>().LoadFile(filename);

                File.WriteAllText(currentFolder + filename, content);
                // load the application configuration.
                config = await application.LoadApplicationConfiguration(currentFolder + filename, false);
            }
            else
            {
                // load the application configuration.
                config = await application.LoadApplicationConfiguration(false);
            }

            // check the application certificate.
            haveAppCertificate = await application.CheckApplicationInstanceCertificate(false, 0);

            switch (Device.RuntimePlatform)
            {
                case "Android":
                    config.ApplicationName = "OPC UA Xamarin Sample Client Android";
                    break;

                case "UWP":
                    config.ApplicationName = "OPC UA Xamarin Sample Client UWP";
                    break;

                case "iOS":
                    config.ApplicationName = "OPC UA Xamarin Sample Client IOS";
                    break;
            }

            if (haveAppCertificate)
            {
                config.ApplicationUri = Utils.GetApplicationUriFromCertificate(config.SecurityConfiguration.ApplicationCertificate.Certificate);

                config.CertificateValidator.CertificateValidation += new CertificateValidationEventHandler(CertificateValidator_CertificateValidation);
            }
        }

        public async Task<ConnectionStatus> OpcClient(string endpointURL)
        {
            try
            {
                Uri endpointURI = new Uri(endpointURL);
                var selectedEndpoint = CoreClientUtils.SelectEndpoint(endpointURL, haveAppCertificate, 15000);

                info.LabelText = "Selected endpoint uses: " + selectedEndpoint.SecurityPolicyUri.Substring(selectedEndpoint.SecurityPolicyUri.LastIndexOf('#') + 1);

                var endpointConfiguration = EndpointConfiguration.Create(config);
                var endpoint = new ConfiguredEndpoint(selectedEndpoint.Server, endpointConfiguration);
                endpoint.Update(selectedEndpoint);

                var platform = Device.RuntimePlatform;
                var sessionName = "";

                switch (Device.RuntimePlatform)
                {
                    case "Android":
                        sessionName = "OPC UA Xamarin Client Android";
                        break;

                    case "UWP":
                        sessionName = "OPC UA Xamarin Client UWP";
                        break;

                    case "iOS":
                        sessionName = "OPC UA Xamarin Client IOS";
                        break;
                }
                session = await Session.Create(config, endpoint, false, sessionName, 60000, new UserIdentity(new AnonymousIdentityToken()), null);

                if (session != null)
                {
                    connectionStatus = ConnectionStatus.Connected;
                }
                else
                {
                    connectionStatus = ConnectionStatus.NotConnected;
                }
                // register keep alive handler
                session.KeepAlive += Client_KeepAlive;
            }
            catch
            {
                connectionStatus = ConnectionStatus.Error;
            }
            return connectionStatus;
        }

        public void Disconnect(Session session)
        {
            if (session != null)
            {
                if (info != null)
                {
                    info.LabelText = "";
                }

                session.Close();
            }
        }

        private void Client_KeepAlive(Session sender, KeepAliveEventArgs e)
        {
            if (e.Status != null && ServiceResult.IsNotGood(e.Status))
            {
                info.LabelText = e.Status.ToString() + sender.OutstandingRequestCount.ToString() + "/" + sender.DefunctRequestCount.ToString();

                if (reconnectHandler == null)
                {
                    info.LabelText = "--- RECONNECTING ---";
                    reconnectHandler = new SessionReconnectHandler();
                    reconnectHandler.BeginReconnect(sender, ReconnectPeriod * 1000, Client_ReconnectComplete);
                }
            }
        }

        private void Client_ReconnectComplete(object sender, EventArgs e)
        {
            // ignore callbacks from discarded objects.
            if (!Object.ReferenceEquals(sender, reconnectHandler))
            {
                return;
            }

            session = reconnectHandler.Session;
            reconnectHandler.Dispose();
            reconnectHandler = null;

            info.LabelText = "--- RECONNECTING ---";
        }

        public Tree GetRootNode(LabelViewModel textInfo)
        {
            ReferenceDescriptionCollection references;
            Byte[] continuationPoint;
            Tree browserTree = new Tree();

            try
            {
                session.Browse(
                    null,
                    null,
                    ObjectIds.ObjectsFolder,
                    0u,
                    BrowseDirection.Forward,
                    ReferenceTypeIds.HierarchicalReferences,
                    true,
                    0,
                    out continuationPoint,
                    out references);

                browserTree.currentView.Add(new ListNode { id = ObjectIds.ObjectsFolder.ToString(), NodeName = "Root", children = (references?.Count != 0) });

                return browserTree;
            }
            catch
            {
                Disconnect(session);
                return null;
            }
        }

        public Tree GetChildren(string node)
        {
            ReferenceDescriptionCollection references;
            Byte[] continuationPoint;
            Tree browserTree = new Tree();

            try
            {
                session.Browse(
                    null,
                    null,
                    node,
                    0u,
                    BrowseDirection.Forward,
                    ReferenceTypeIds.HierarchicalReferences,
                    true,
                    0,
                    out continuationPoint,
                    out references);

                if (references != null)
                {
                    foreach (var nodeReference in references)
                    {
                        ReferenceDescriptionCollection childReferences = null;
                        Byte[] childContinuationPoint;

                        session.Browse(
                            null,
                            null,
                            ExpandedNodeId.ToNodeId(nodeReference.NodeId, session.NamespaceUris),
                            0u,
                            BrowseDirection.Forward,
                            ReferenceTypeIds.HierarchicalReferences,
                            true,
                            0,
                            out childContinuationPoint,
                            out childReferences);

                        INode currentNode = null;
                        try
                        {
                            currentNode = session.ReadNode(ExpandedNodeId.ToNodeId(nodeReference.NodeId, session.NamespaceUris));
                        }
                        catch (Exception)
                        {
                            // skip this node
                            continue;
                        }

                        byte currentNodeAccessLevel = 0;
                        byte currentNodeEventNotifier = 0;
                        bool currentNodeExecutable = false;

                        VariableNode variableNode = currentNode as VariableNode;
                        if (variableNode != null)
                        {
                            currentNodeAccessLevel = variableNode.UserAccessLevel;
                            currentNodeAccessLevel = (byte)((uint)currentNodeAccessLevel & ~0x2);
                        }

                        ObjectNode objectNode = currentNode as ObjectNode;
                        if (objectNode != null)
                        {
                            currentNodeEventNotifier = objectNode.EventNotifier;
                        }

                        ViewNode viewNode = currentNode as ViewNode;
                        if (viewNode != null)
                        {
                            currentNodeEventNotifier = viewNode.EventNotifier;
                        }

                        MethodNode methodNode = currentNode as MethodNode;
                        if (methodNode != null)
                        {
                            currentNodeExecutable = methodNode.UserExecutable;
                        }

                        browserTree.currentView.Add(new ListNode()
                        {
                            id = nodeReference.NodeId.ToString(),
                            NodeName = nodeReference.DisplayName.Text.ToString(),
                            nodeClass = nodeReference.NodeClass.ToString(),
                            accessLevel = currentNodeAccessLevel.ToString(),
                            eventNotifier = currentNodeEventNotifier.ToString(),
                            executable = currentNodeExecutable.ToString(),
                            children = (references?.Count != 0),
                            ImageUrl = (nodeReference.NodeClass.ToString() == "Variable") ? "folderOpen.jpg" : "folder.jpg"
                        });
                        if (browserTree.currentView[0].ImageUrl == null)
                        {
                            browserTree.currentView[0].ImageUrl = "";
                        }
                    }
                    if (browserTree.currentView.Count == 0)
                    {
                        INode currentNode = session.ReadNode(new NodeId(node));

                        byte currentNodeAccessLevel = 0;
                        byte currentNodeEventNotifier = 0;
                        bool currentNodeExecutable = false;

                        VariableNode variableNode = currentNode as VariableNode;

                        if (variableNode != null)
                        {
                            currentNodeAccessLevel = variableNode.UserAccessLevel;
                            currentNodeAccessLevel = (byte)((uint)currentNodeAccessLevel & ~0x2);
                        }

                        ObjectNode objectNode = currentNode as ObjectNode;

                        if (objectNode != null)
                        {
                            currentNodeEventNotifier = objectNode.EventNotifier;
                        }

                        ViewNode viewNode = currentNode as ViewNode;

                        if (viewNode != null)
                        {
                            currentNodeEventNotifier = viewNode.EventNotifier;
                        }

                        MethodNode methodNode = currentNode as MethodNode;

                        if (methodNode != null)
                        {
                            currentNodeExecutable = methodNode.UserExecutable;
                        }

                        browserTree.currentView.Add(new ListNode()
                        {
                            id = node,
                            NodeName = currentNode.DisplayName.Text.ToString(),
                            nodeClass = currentNode.NodeClass.ToString(),
                            accessLevel = currentNodeAccessLevel.ToString(),
                            eventNotifier = currentNodeEventNotifier.ToString(),
                            executable = currentNodeExecutable.ToString(),
                            children = false,
                            ImageUrl = null
                        });
                    }
                }
                return browserTree;
            }
            catch
            {
                Disconnect(session);
                return null;
            }
        }

        public string VariableRead(string node)
        {
            try
            {
                DataValueCollection values = null;
                DiagnosticInfoCollection diagnosticInfos = null;
                ReadValueIdCollection nodesToRead = new ReadValueIdCollection();
                ReadValueId valueId = new ReadValueId();
                valueId.NodeId = new NodeId(node);
                valueId.AttributeId = Attributes.Value;
                valueId.IndexRange = null;
                valueId.DataEncoding = null;
                nodesToRead.Add(valueId);
                ResponseHeader responseHeader = session.Read(null, 0, TimestampsToReturn.Both, nodesToRead, out values, out diagnosticInfos);
                string value = "";
                if (values[0].Value != null)
                {
                    var rawValue = values[0].WrappedValue.ToString();
                    value = rawValue.Replace("|", "\r\n").Replace("{", "").Replace("}", "");
                }
                return value;
            }
            catch
            {
                return null;
            }
        }

        //public void WriteValues(List<String> values, List<String> nodeIdStrings)
        //{
        //    //Create a collection of values to write
        //    WriteValueCollection valuesToWrite = new WriteValueCollection();
        //    //Create a collection for StatusCodes
        //    StatusCodeCollection result = new StatusCodeCollection();
        //    //Create a collection for DiagnosticInfos
        //    DiagnosticInfoCollection diagnostics = new DiagnosticInfoCollection();

        //    foreach (String str in nodeIdStrings)
        //    {
        //        //Create a nodeId
        //        NodeId nodeId = new NodeId(str);
        //        //Create a dataValue
        //        DataValue dataValue = new DataValue();
        //        //Read the dataValue
        //        try
        //        {
        //            dataValue = mSession.ReadValue(nodeId);
        //        }
        //        catch (Exception e)
        //        {
        //            //handle Exception here
        //            throw e;
        //        }

        //        string test = dataValue.Value.GetType().Name;
        //        //Get the data type of the read dataValue
        //        //Handle Arrays here: TBD
        //        Variant variant = 0;
        //        try
        //        {
        //            variant = new Variant(Convert.ChangeType(values[nodeIdStrings.IndexOf(str)], dataValue.Value.GetType()));
        //        }
        //        catch //no base data type
        //        {
        //            //Handle different arrays types here: TBD
        //            if (dataValue.Value.GetType().Name == "string[]")
        //            {
        //                string[] arrString = values[nodeIdStrings.IndexOf(str)].Split(';');
        //                variant = new Variant(arrString);
        //            }
        //            else if (dataValue.Value.GetType().Name == "Byte[]")
        //            {
        //                string[] arrString = values[nodeIdStrings.IndexOf(str)].Split(';');
        //                Byte[] arrInt = new Byte[arrString.Length];

        //                for (int i = 0; i < arrString.Length; i++)
        //                {
        //                    arrInt[i] = Convert.ToByte(arrString[i]);
        //                }
        //                variant = new Variant(arrInt);
        //            }
        //            else if (dataValue.Value.GetType().Name == "Int16[]")
        //            {
        //                string[] arrString = values[nodeIdStrings.IndexOf(str)].Split(';');
        //                Int16[] arrInt = new Int16[arrString.Length];

        //                for (int i = 0; i < arrString.Length; i++)
        //                {
        //                    arrInt[i] = Convert.ToInt16(arrString[i]);
        //                }
        //                variant = new Variant(arrInt);
        //            }
        //        }

        //        //Overwrite the dataValue with a new constructor using read dataType
        //        dataValue = new DataValue(variant);

        //        //Create a WriteValue using the NodeId, dataValue and attributeType
        //        WriteValue valueToWrite = new WriteValue();
        //        valueToWrite.Value = dataValue;
        //        valueToWrite.NodeId = nodeId;
        //        valueToWrite.AttributeId = Attributes.Value;

        //        //Add the dataValues to the collection
        //        valuesToWrite.Add(valueToWrite);
        //    }

        //    try
        //    {
        //        //Write the      to the server
        //        mSession.Write(null, valuesToWrite, out result, out diagnostics);
        //        foreach (StatusCode code in result)
        //        {
        //            if (code != 0)
        //            {
        //                Exception ex = new Exception(code.ToString());
        //                throw ex;
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        //handle Exception here
        //        throw e;
        //    }
        //}
        private void CertificateValidator_CertificateValidation(CertificateValidator validator, CertificateValidationEventArgs e)
        {
            if (e.Error.StatusCode == StatusCodes.BadCertificateUntrusted)
            {
                e.Accept = config.SecurityConfiguration.AutoAcceptUntrustedCertificates;
                if (config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                {
                    info.LabelText = "Accepted Certificate: " + e.Certificate.Subject.ToString();
                }
                else
                {
                    info.LabelText = "Rejected Certificate: " + e.Certificate.Subject.ToString();
                }
            }
        }
    }
}

