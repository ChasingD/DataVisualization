using Mirror;
using Mirror.Discovery;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Events;
#if WINDOWS_UWP || UNITY_UWP
using Windows.Networking.Connectivity;
using Windows.Networking;
#endif
namespace Module.Session
{
    [Serializable]
    public class ServerFoundUnityEvent : UnityEvent<ServerResponse> { };

    public class NetworkDiscoveryWithAnchors : NetworkDiscoveryBase<ServerRequest, ServerResponse>
    {
        public static NetworkDiscoveryWithAnchors Instance;
        public string LocalIp;

        private void Awake()
        {
            Instance = this;

#if WINDOWS_UWP || UNITY_UWP

            foreach (Windows.Networking.HostName hostName in Windows.Networking.Connectivity.NetworkInformation.GetHostNames())
            {
                if (hostName.DisplayName.Split(".".ToCharArray()).Length == 4)
                {
                    Debug.Log("Local IP " + hostName.DisplayName);
                    LocalIp = hostName.DisplayName;
                    break;
                }
            }
#else
            LocalIp = "editor";
#endif
        }
        private string GetLocalComputerName()
        {
            return Environment.UserName;
        }
        #region Server
        public string RoomId { get; set; }
        public long _ServerId { get; private set; }
        public string broadcastData;
   //     [Tooltip("Transport to be advertised during discovery")]
       // public Transport transport;

      //  [Tooltip("Invoked when a server is found")]
      //  public ServerFoundUnityEvent OnServerFound;

        public override void Start()
        {
            _ServerId = RandomLong();
            broadcastData = /*GetLocalComputerName().Length.ToString() + "_" + */GetLocalComputerName();
            print(broadcastData);
            // active transport gets initialized in awake
            // so make sure we set it here in Start()  (after awakes)
            // Or just let the user assign it in the inspector
            if (transport == null)
                transport = Transport.active;

            base.Start();
        }

        /// <summary>
        /// Process the request from a client
        /// </summary>
        /// <remarks>
        /// Override if you wish to provide more information to the clients
        /// such as the name of the host player
        /// </remarks>
        /// <param name="request">Request comming from client</param>
        /// <param name="endpoint">Address of the client that sent the request</param>
        /// <returns>The message to be sent back to the client or null</returns>
        protected override ServerResponse ProcessRequest(ServerRequest request, IPEndPoint endpoint)
        {
            // In this case we don't do anything with the request
            // but other discovery implementations might want to use the data
            // in there,  This way the client can ask for
            // specific game mode or something
           // var viewerNetworkManager = NetworkManager.singleton as ViewerNetworkManager;
            try
            {
                ServerResponse response = new ServerResponse
                {
                    serverId = _ServerId,
                    uri = transport.ServerUri()//,
                    //roomId = RoomId,
                    //roomName = SessionManager.Instance.RoomName,
                    //roomHostName = ViewerSettings.Instance.PlayerName,
                    //isPublic = SessionManager.Instance.RoomPublic,
                    //requirePassword = bool.Parse(SessionManager.Instance.RequirePassword),
                    //maxUserCount = viewerNetworkManager.maxConnections,
                    //currentUserCount = ClassInfoController.Instance.peopleElementList.Count
                };
                //print(response.isPublic);
                return response;
                
            }
            catch (NotImplementedException)
            {
                Debug.LogError($"Transport {transport} does not support network discovery");
                throw;
            }
        }

        #endregion

        #region Client

        /// <summary>
        /// Create a message that will be broadcasted on the network to discover servers
        /// </summary>
        /// <remarks>
        /// Override if you wish to include additional data in the discovery message
        /// such as desired game mode, language, difficulty, etc... </remarks>
        /// <returns>An instance of ServerRequest with data to be broadcasted</returns>
        protected override ServerRequest GetRequest() => new ServerRequest();

        /// <summary>
        /// Process the answer from a server
        /// </summary>
        /// <remarks>
        /// A client receives a reply from a server, this method processes the
        /// reply and raises an event
        /// </remarks>
        /// <param name="response">Response that came from the server</param>
        /// <param name="endpoint">Address of the server that replied</param>
        protected override void ProcessResponse(ServerResponse response, IPEndPoint endpoint)
        {
            // we received a message from the remote endpoint
            response.EndPoint = endpoint;

            // although we got a supposedly valid url, we may not be able to resolve
            // the provided host
            // However we know the real ip address of the server because we just
            // received a packet from it,  so use that as host.
            UriBuilder realUri = new UriBuilder(response.uri)
            {
                Host = response.EndPoint.Address.ToString()
            };
            response.uri = realUri.Uri;
            
            OnServerFound.Invoke(response);
        }

        #endregion
        //readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();
        //public void FindServers()
        //{
        //    discoveredServers.Clear();
        //    StartDiscovery();
        //}
        //public void StartHost()
        //{
        //    discoveredServers.Clear();
        //    NetworkManager.singleton.StartHost();
        //    AdvertiseServer();
        //}
        //public void Connect(ServerResponse info)
        //{
        //    NetworkManager.singleton.StartClient(info.uri);
        //}

        //public void OnDiscoveredServer(ServerResponse info)
        //{
        //    // Note that you can check the versioning to decide if you can connect to the server or not using this method
        //    discoveredServers[info.serverId] = info;
        //}
    }
}
