using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using Mirror;
using Mirror.Discovery;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Module.Session
{
    /// <summary>
    /// Controls player behavior (local and remote).
    /// </summary>
    public class PlayerController : NetworkBehaviour
    {
        private static PlayerController _Instance = null;

        /// <summary>
        /// Instance of the PlayerController that represents the local player.
        /// </summary>
        public static PlayerController Instance
        {
            get { return _Instance; }
        }

        public bool CanEstablishAnchor()
        {
#if UNITY_EDITOR || UNITY_STANDALONE || PICOVIEWER
            return false;
#endif

            //TODO: differentiate between immersive devices and HoloLens devices.
            return !PlayerName.ToLower().StartsWith("editor");
        }

        /// <summary>
        /// The transform of the shared world anchor.
        /// </summary>
        private Transform sharedWorldAnchorTransform;

        public TMP_Text playerNameText;
        public Image playerPlateImage;
        public Button authorityButton;
        public GameObject adminTag;

        [SyncVar] public bool hasAuthority = true;

        // private UNetAnchorManager anchorManager;

        /// <summary>
        /// The position relative to the shared world anchor.
        /// </summary>
        [SyncVar] private Vector3 localPosition;

        [SyncVar] int spectatorViewPing = 0;

        /// <summary>
        /// The rotation relative to the shared world anchor.
        /// </summary>
        [SyncVar] private Quaternion localRotation;

        // Nanoseconds
        [SyncVar] private long colorDuration = 33333333;


        [Command]
        private void CmdSetColorDuration(long value)
        {
            ServerSetColorDuration(value);
        }

        private void ServerSetColorDuration(long value)
        {
            colorDuration = value;
        }

        public void SetColorDuration(long value)
        {
            if (colorDuration != value)
            {
                ServerSetColorDuration(value);
                CmdSetColorDuration(value);
            }
        }

        /// <summary>
        /// Sets the localPosition and localRotation on clients.
        /// </summary>
        /// <param name="postion">the localPosition to set</param>
        /// <param name="rotation">the localRotation to set</param>
        //[Command(channel = 1)]
        public void CmdTransform(Vector3 postion, Quaternion rotation /*, int ping*/)
        {
            localPosition = postion;
            localRotation = rotation;

            //if (IsSV())
            //{
            //    spectatorViewPing = ping;
            //}
        }

        [SyncVar(hook = nameof(AnchorEstablishedChanged))]
        public bool AnchorEstablished;

        [Command]
        private void CmdSendAnchorEstablished(bool Established)
        {
            AnchorEstablished = Established;
            if (Established && SharesSpatialAnchors && !isLocalPlayer)
            {
                Debug.Log("remote device likes the anchor");
                // anchorManager.AnchorFoundRemotely();
            }
        }

        void AnchorEstablishedChanged(bool oldUpdate, bool update)
        {
            Debug.LogFormat("AnchorEstablished for {0} was {1} is now {2}", PlayerName, AnchorEstablished, update);
            AnchorEstablished = update;

            // Renderer renderer = GetComponent<MeshRenderer>();
            // if (renderer != null)
            // {
            //     renderer.enabled = update;
            // }
            // renderer = GetComponentInChildren<MeshRenderer>();
            // if (renderer != null)
            // {
            //     renderer.enabled = update;
            // }

            if (!isLocalPlayer)
            {
                InitializeRemoteAvatar();
                // InitializeSpectatorView();
            }
        }

        [SyncVar(hook = nameof(PlayerNameChanged))]
        public string PlayerName = string.Empty;

        [Command]
        private void CmdSetPlayerName(string playerName)
        {
            PlayerName = playerName;
        }

        void PlayerNameChanged(string oldUpdate, string update)
        {
            Debug.LogFormat("Player name changing from {0} to {1}", PlayerName, update);
            PlayerName = update;
            playerNameText.text = PlayerName;
#if UNITY_EDITOR || UNITY_STANDALONE || PICOVIEWER
            // if (PlayerName.Trim().ToLower().StartsWith("editor"))
            // {
            //     Renderer renderer = gameObject.GetComponent<Renderer>();
            //     if (renderer != null)
            //     {
            //         gameObject.SetActive(false);
            //     }
            // }
#endif
            if (!isLocalPlayer)
            {
                InitializeRemoteAvatar();
                // InitializeSpectatorView();
            }
        }

        [SyncVar(hook = nameof(PlayerIpChanged))]
        public string PlayerIp = string.Empty;

        [Command]
        private void CmdSetPlayerIp(string playerIp)
        {
            PlayerIp = playerIp;
        }

        void PlayerIpChanged(string joldUpdate, string update)
        {
            PlayerIp = update;
        }

        [SyncVar] string HostIP;

        [Command]
        public void CmdSetAnchorName(string anchorName)
        {
            UNetAnchorManager.Instance.AnchorName = anchorName;
        }

        [Command]
        public void CmdSetAnchorOwnerIP(string anchorOwnerIP)
        {
            if (UNetAnchorManager.Instance.AnchorOwnerIP == string.Empty)
            {
                Debug.Log("Setting anchor owner to: " + anchorOwnerIP);
                UNetAnchorManager.Instance.AnchorOwnerIP = anchorOwnerIP;
            }
            else
            {
                Debug.LogWarning("Attempted to set anchor owner to " + anchorOwnerIP + ", but anchor was already established.");
            }
        }

        [Command]
        public void CmdFindNewAnchorOwner()
        {
            FindNewAnchorOwner();
        }

        public void FindNewAnchorOwner()
        {
            UNetAnchorManager.Instance.AnchorOwnerIP = string.Empty;
            RpcIdentifyPotentialAnchorOwner();
        }

        [ClientRpc]
        public void RpcIdentifyPotentialAnchorOwner()
        {
            if (UNetAnchorManager.Instance.AnchorOwnerIP == string.Empty &&
                CanEstablishAnchor() &&
                !string.IsNullOrEmpty(PlayerIp))
            {
                CmdSetAnchorOwnerIP(PlayerIp);
            }
        }

        private bool _createAnchor = false;

        public void CreateAnchor(string value)
        {
            // if (isLocalPlayer &&
            //     (value == networkDiscovery.LocalIp ||
            //      value.Trim().ToLower() == PlayerName.ToString().ToLower()) &&
            //     CanEstablishAnchor())
            // {
            //     Debug.Log("Taking ownership of creating anchor.");
            //     _createAnchor = true;
            // }
        }

        [SyncVar(hook = nameof(SharesAnchorsChanged))]
        public bool SharesSpatialAnchors;

        [Command]
        private void CmdSetCanShareAnchors(bool canShareAnchors)
        {
            Debug.Log("CMDSetCanShare " + canShareAnchors);
            SharesSpatialAnchors = canShareAnchors;
        }

        void SharesAnchorsChanged(bool oldUpdate, bool update)
        {
            SharesSpatialAnchors = update;
            Debug.LogFormat("{0} {1} share", PlayerName, SharesSpatialAnchors ? "does" : "does not");
        }

        NetworkDiscovery networkDiscovery;
        private GameObject playerCommClone;

        private void InitializeRemoteAvatar()
        {
            if (!string.IsNullOrEmpty(PlayerName) && AnchorEstablished)
            {
                OnPlayerAddedEvent?.Invoke();

                //TODO: do any application initialization logic for your remote player here.
                // if (PlayerName.Trim().ToLower().StartsWith("editor"))
                // {
                //     Renderer renderer = gameObject.GetComponent<Renderer>();
                //     if (renderer != false)
                //     {
                //         renderer.enabled = false;
                //     }
                // }
            }
        }

        void Awake()
        {
            networkDiscovery = FindObjectOfType<NetworkDiscovery>();
            // anchorManager = UNetAnchorManager.Instance;
        }

        private async void Start()
        {
            await UniTask.WaitUntil(() => UNetAnchorManager.Instance);
            if (UNetAnchorManager.Instance == null)
            {
                Debug.LogError("This script requires a UNetAnchorManager in the scene");
                Destroy(this);
                return;
            }

            if (isLocalPlayer)
            {
                Debug.Log("Init from start");
                InitializeLocalPlayer();
            }
            else
            {
                Debug.Log("remote player, analyzing start state " + PlayerName);
                AnchorEstablishedChanged(false, AnchorEstablished);
                SharesAnchorsChanged(false, SharesSpatialAnchors);
            }

            if (isServer)
            {
                // HostIP = networkDiscovery.LocalIp;
            }

            sharedWorldAnchorTransform = SharedCollection.Instance.root;
            transform.SetParent(sharedWorldAnchorTransform);
            // AuthorityController.Instance?.playerObjects.Add(this.gameObject);
            // AuthorityController.Instance?.AuthorityInit();

            ////权限分配
            //if (!AdminController.Instance.freeAuthority)
            //{
            //    authorityButton.onClick.AddListener(() =>
            //    {
            //        if (AdminController.Instance.whoHasAuth != netIdentity)
            //        {
            //            AdminController.Instance.CmdSetWhoHasAuth(netIdentity);
            //        }
            //        else
            //        {
            //            AdminController.Instance.CmdRemoveWhoHasAuth(AdminController.Instance.admin);
            //        }
            //    });
            //}
        }

        public UnityEvent OnPlayerAddedEvent;

        private void InitializeLocalPlayer()
        {
            if (isLocalPlayer)
            {
                Debug.Log("Setting instance for local player ");
                _Instance = this;

                string name = Environment.MachineName;
                //string name = Cookie.Username;

                // Debug.LogFormat("Set local player name {0} ip {1}", name, networkDiscovery.LocalIp);
                PlayerName = name;
                // PlayerIp = networkDiscovery.LocalIp;

                CmdSetPlayerName(name);
                // CmdSetPlayerIp(networkDiscovery.LocalIp);
                bool canShareAnchors = CanEstablishAnchor();
                Debug.LogFormat("local player {0} share anchors ", (canShareAnchors ? "does" : "does not"));
                CmdSetCanShareAnchors(canShareAnchors);

                if (UNetAnchorManager.Instance.AnchorOwnerIP == string.Empty &&
                    CanEstablishAnchor())
                {
                    CmdSetAnchorOwnerIP(PlayerIp);
                }

                if (UNetAnchorManager.Instance.AnchorOwnerIP != string.Empty)
                {
                    GenericNetworkTransmitter.Instance.SetServerIP(UNetAnchorManager.Instance.AnchorOwnerIP);
                }

                OnPlayerAddedEvent?.Invoke();
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
        }

        private void OnDestroy()
        {
            if (playerCommClone)
            {
                Destroy(playerCommClone);
            }

            // Anchor owner is disconnecting, find a new anchor.
            if (UNetAnchorManager.Instance && UNetAnchorManager.Instance.AnchorOwnerIP == PlayerIp)
            {
                Debug.Log("Find new anchor owner.");
                UNetAnchorManager.Instance.FindAnchorOwner();
            }

            if (HostIP == PlayerIp || HostIP == PlayerName)
            {
                //TODO: auto-rejoin
                Debug.Log("Host has disconnected.");
                //networkDiscovery.Restart();
                //ScrollingSessionListUIController.Instance.Show();
                ////没有用的语句，在SpectatorViewManager中，destroy里已经对networkDiscovery == null,下面的函数执行同样的操作
                ////6.29更新，如果没有这句话，networkdiscovery不为空，就无法进入SVM里面的判断，无法自动加入网络，需要手动
                ////选择服务器进入。所以必须加上下面这句话
                //SpectatorView.SpectatorViewManager.Instance.ResetAutoconnect();
            }
        }

        private void RetryFindingAnchorOwner()
        {
            if (UNetAnchorManager.Instance.AnchorOwnerIP == string.Empty)
            {
                FindNewAnchorOwner();
            }
        }

        private void Update()
        {
            if (_createAnchor
#if UNITY_UWP || WINDOWS_UWP
#if UNITY_2019_4
                &&
                WorldAnchorManager.Instance != null &&
                WorldAnchorManager.Instance.AnchorStore != null
#endif
#endif
               )
            {
                Debug.Log("Creating Anchor.");
                _createAnchor = false;
                Debug.Log(UNetAnchorManager.Instance.name + "CreateAnchor*****");
                UNetAnchorManager.Instance.CreateAnchor();
            }

            // if (isServer && isLocalPlayer)
            // {
            //     if (UNetAnchorManager.Instance.AnchorOwnerIP == string.Empty)
            //     {
            //         Invoke("RetryFindingAnchorOwner", 5.0f);
            //     }
            // }

            // If we aren't the local player, we just need to make sure that the position of this object is set properly
            // so that we properly render their avatar in our world.
            //if (!isLocalPlayer && string.IsNullOrEmpty(PlayerName) == false)
            //{
            //    transform.localPosition = Vector3.Lerp(transform.localPosition, localPosition, 0.3f);
            //    transform.localRotation = localRotation;

            //    return;
            //}


            // if (AnchorEstablished != anchorManager.AnchorEstablished)
            // {
            //     if (isLocalPlayer)
            //     {
            //         CmdSendAnchorEstablished(anchorManager.AnchorEstablished);
            //     }
            // }

            if (!isLocalPlayer)
            {
                return;
            }


           // print(transform.position + " ****************************** " + Camera.main.transform.position);
            transform.position = Camera.main.transform.position;
            transform.rotation = Camera.main.transform.rotation;

            //print("AnchorEstablished: " + AnchorEstablished);

            if (AnchorEstablished == false)
            {
                return;
            }

#if UNITY_EDITOR || UNITY_STANDALONE
            if (TheThirdCamera)
            {
                transform.position = TheThirdCamera.transform.position;
                transform.rotation = TheThirdCamera.transform.rotation;
            }
            else
            {
                transform.position = Camera.main.transform.position;
                transform.rotation = Camera.main.transform.rotation;
            }
#else
            // print(transform.position+" ****************************** "+Camera.main.transform.position);
            transform.position = Camera.main.transform.position;
            transform.rotation = Camera.main.transform.rotation;
#endif

            //transform.eulerAngles = new Vector3(0, Camera.main.transform.rotation.eulerAngles.y, 0);
            //CmdTransform(transform.localPosition, transform.localRotation);
        }

        private Camera theThirdCamera;

        private Camera TheThirdCamera
        {
            get
            {
                if (theThirdCamera == null)
                {
                    theThirdCamera = Camera.allCameras.FirstOrDefault(o => o.gameObject.tag == "TheThirdCamera");
                }

                return theThirdCamera;
            }
        }
        //public SyncList<string> studioScenesLoaded = new SyncList<string>();
    }
}