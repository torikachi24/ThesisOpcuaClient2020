using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
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

        public MonitoredItemNotificationEventHandler ItemChangedNotification = null;
        public NotificationEventHandler ItemEventNotification = null;
        public KeepAliveEventHandler KeepAliveNotification = null;
        public CertificateValidationEventHandler CertificateValidationNotification = null;
        public SampleClient(LabelViewModel text)
        {
            connectionStatus = ConnectionStatus.None;
            session = null;
            info = text;
            haveAppCertificate = false;
            config = null;
        }

        #region EventHandling
        /// <summary>Eventhandler to validate the server certificate forwards this event</summary>
        private void Notificatio_CertificateValidation(CertificateValidator certificate, CertificateValidationEventArgs e)
        {
            CertificateValidationNotification(certificate, e);
        }

        /// <summary>Eventhandler for MonitoredItemNotifications forwards this event</summary>
        private void Notification_MonitoredItem(MonitoredItem monitoredItem, MonitoredItemNotificationEventArgs e)
        {
            ItemChangedNotification(monitoredItem, e);
        }

        /// <summary>Eventhandler for MonitoredItemNotifications for event items forwards this event</summary>
        private void Notification_MonitoredEventItem(Session session, NotificationEventArgs e)
        {
            NotificationMessage message = e.NotificationMessage;

            // Check for keep alive.
            if (message.NotificationData.Count == 0)
            {
                return;
            }

            ItemEventNotification(session, e);
        }

        /// <summary>Eventhandler for KeepAlive forwards this event</summary>
        private void Notification_KeepAlive(Session session, KeepAliveEventArgs e)
        {
            KeepAliveNotification(session, e);
        }
        #endregion

        #region Subscription
        /// <summary>Creats a Subscription object to a server</summary>
        /// <param name="publishingInterval">The publishing interval</param>
        /// <returns>Subscription</returns>
        /// <exception cref="Exception">Throws and forwards any exception with short error description.</exception>
        public Subscription Subscribe(int publishingInterval)
        {
            //Create a Subscription object
            Subscription subscription = new Subscription(session.DefaultSubscription);
            //Enable publishing
            subscription.PublishingEnabled = true;
            //Set the publishing interval
            subscription.PublishingInterval = publishingInterval;
            //Add the subscription to the session
            session.AddSubscription(subscription);
            try
            {
                //Create/Activate the subscription
                subscription.Create();
                return subscription;
            }
            catch (Exception e)
            {
                //handle Exception here
                throw e;
            }
        }

        /// <summary>Ads a monitored item to an existing subscription</summary>
        /// <param name="subscription">The subscription</param>
        /// <param name="nodeIdString">The node Id as string</param>
        /// <param name="itemName">The name of the item to add</param>
        /// <param name="samplingInterval">The sampling interval</param>
        /// <returns>The added item</returns>
        /// <exception cref="Exception">Throws and forwards any exception with short error description.</exception>
        public MonitoredItem AddMonitoredItem(Subscription subscription, string nodeIdString, string itemName, int samplingInterval)
        {
            //Create a monitored item
            MonitoredItem monitoredItem = new MonitoredItem();
            //Set the name of the item for assigning items and values later on; make sure item names differ
            monitoredItem.DisplayName = itemName;
            //Set the NodeId of the item
            monitoredItem.StartNodeId = nodeIdString;
            //Set the attribute Id (value here)
            monitoredItem.AttributeId = Attributes.Value;
            //Set reporting mode
            monitoredItem.MonitoringMode = MonitoringMode.Reporting;
            //Set the sampling interval (1 = fastest possible)
            monitoredItem.SamplingInterval = samplingInterval;
            //Set the queue size
            monitoredItem.QueueSize = 1;
            //Discard the oldest item after new one has been received
            monitoredItem.DiscardOldest = true;
            //Define event handler for this item and then add to monitoredItem
            monitoredItem.Notification += new MonitoredItemNotificationEventHandler(Notification_MonitoredItem);
            try
            {
                //Add the item to the subscription
                subscription.AddItem(monitoredItem);
                //Apply changes to the subscription
                subscription.ApplyChanges();
                return monitoredItem;
            }
            catch (Exception e)
            {
                //handle Exception here
                throw e;
            }
        }

        /// <summary>Ads a monitored event item to an existing subscription</summary>
        /// <param name="subscription">The subscription</param>
        /// <param name="nodeIdString">The node Id as string</param>
        /// <param name="itemName">The name of the item to add</param>
        /// <param name="samplingInterval">The sampling interval</param>
        /// <param name="filter">The event filter</param>
        /// <returns>The added item</returns>
        /// <exception cref="Exception">Throws and forwards any exception with short error description.</exception>
        public MonitoredItem AddEventMonitoredItem(Subscription subscription, string nodeIdString, string itemName, int samplingInterval, EventFilter filter)
        {
            //Create a monitored item
            MonitoredItem monitoredItem = new MonitoredItem(subscription.DefaultItem);
            //Set the name of the item for assigning items and values later on; make sure item names differ
            monitoredItem.DisplayName = itemName;
            //Set the NodeId of the item
            monitoredItem.StartNodeId = nodeIdString;
            //Set the attribute Id (value here)
            monitoredItem.AttributeId = Attributes.EventNotifier;
            //Set reporting mode
            monitoredItem.MonitoringMode = MonitoringMode.Reporting;
            //Set the sampling interval (1 = fastest possible)
            monitoredItem.SamplingInterval = samplingInterval;
            //Set the queue size
            monitoredItem.QueueSize = 1;
            //Discard the oldest item after new one has been received
            monitoredItem.DiscardOldest = true;
            //Set the filter for the event item
            monitoredItem.Filter = filter;

            //Define event handler for this item and then add to monitoredItem
            session.Notification += new NotificationEventHandler(Notification_MonitoredEventItem);

            try
            {
                //Add the item to the subscription
                subscription.AddItem(monitoredItem);
                //Apply changes to the subscription
                subscription.ApplyChanges();
                return monitoredItem;
            }
            catch (Exception e)
            {
                //handle Exception here
                throw e;
            }
        }

        /// <summary>Removs a monitored item from an existing subscription</summary>
        /// <param name="subscription">The subscription</param>
        /// <param name="monitoredItem">The item</param>
        /// <exception cref="Exception">Throws and forwards any exception with short error description.</exception>
        public MonitoredItem RemoveMonitoredItem(Subscription subscription, MonitoredItem monitoredItem)
        {
            try
            {
                //Add the item to the subscription
                subscription.RemoveItem(monitoredItem);
                //Apply changes to the subscription
                subscription.ApplyChanges();
                return null;
            }
            catch (Exception e)
            {
                //handle Exception here
                throw e;
            }
        }

        /// <summary>Removes an existing Subscription</summary>
        /// <param name="subscription">The subscription</param>
        /// <exception cref="Exception">Throws and forwards any exception with short error description.</exception>
        public void RemoveSubscription(Subscription subscription)
        {
            try
            {
                //Delete the subscription and all items submitted
                subscription.Delete(true);
            }
            catch (Exception e)
            {
                //handle Exception here
                throw e;
            }
        }
        #endregion

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
                            ImageUrl = (nodeReference.NodeClass.ToString() == "Variable") ? "tag.png" : "folder.png"
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

        public StatusCodeCollection VariableWrite(string nodeId, object value)
        {
            var nodeToWrite = new WriteValue()
            {
                NodeId = nodeId,
                AttributeId = Attributes.Value,
                Value = new DataValue()
                {
                    WrappedValue = new Variant(value)
                }
            };

            WriteValueCollection nodesToWrite = new WriteValueCollection() { nodeToWrite };

            ResponseHeader responseHeader = session.Write(
                null,
                nodesToWrite,
                out StatusCodeCollection results,
                out DiagnosticInfoCollection diagnosticInfos);

            ClientBase.ValidateResponse(results, nodesToWrite);
            ClientBase.ValidateDiagnosticInfos(diagnosticInfos, nodesToWrite);

            return results;
        }


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