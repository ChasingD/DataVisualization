using Cysharp.Threading.Tasks;
using Mirror;
using Mirror.Discovery;
using UnityEngine;
using UnityEngine.Events;

namespace Module.Session
{
	public class OnFindServerEvent : UnityEvent<ServerResponse>
	{ }

	public class SessionManager : Singleton<SessionManager>
	{
		public GameObject sessionMenu;
		public OnFindServerEvent onFindServer = new OnFindServerEvent();


		//public UnityEvent OnHostQuitRoomOnWan = new UnityEvent();
		//public UnityEvent OnClientQuitRoomOnWan = new UnityEvent();
		public bool autoCreateRoom;

		// Start is called before the first frame update
		private void Start()
		{
			//Observable.EveryUpdate().Subscribe(_ =>
			//{
			//	if (NetworkManager.singleton.isNetworkActive)
			//	{
			//		sessionMenu.SetActive(false);
			//	}
			//	else
			//	{
			//		sessionMenu.SetActive(true);
			//	}
			//}).AddTo(this);
			//if (!NetworkManager.singleton)
			//{
			//	Debug.LogError("场景中没有NetworkManager");
			//}

#if UNITY_EDITOR || UNITY_STANDALONE

			if (autoCreateRoom)
			{
				CreateRoom();
			}
#endif
		}

		/// <summary>
		/// 检查当前是否已经在一个房间中了
		/// </summary>
		/// <returns></returns>
		public bool CheckInsideRoom()
		{
			//如果starthost或者startclient，isnetworkactive true
			if (NetworkManager.singleton.isNetworkActive)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public async void CreateRoom(bool isHeadless = false)
		{
			if (isHeadless)
			{
				CreateHeadlessServer();
			}
			else
			{
				CreateRoomOnLAN();
			}

			// await UniTask.Delay(1000);
			//
			// var all = SharedCollection.Instance.GetComponentsInChildren<PlayerController>(true);

            // Debug.LogError("目前玩家数量+" + all.Length);

		}

		public async void JoinRoom()
		{
			//if (MyNetworkManager.Instance.networkAddress.Equals(string.Empty))
			//{
			//	//PopWindowsController.Instance.ApplyTextContentOneButton("当前没有选择房间");
			//	return;
			//}

			//var url = new Uri("kcp://" + MyNetworkManager.Instance.networkAddress + ":" + MyNetworkManager.Instance.transform.GetComponent<KcpTransport>().port);

		//	Debug.LogError("加入房间传入的url" + url);

            NetworkManager.singleton.StartClient();
            //
            // await UniTask.Delay(1000);
            //
            // var all = SharedCollection.Instance.GetComponentsInChildren<PlayerController>(true);
            //
            // Debug.LogError("目前玩家数量+" + all.Length);
        }

		public void QuitRoom()
		{
			//if (NetworkManager.singleton.isNetworkActive)
			//{
			//	PopWindowsController.Instance.ApplyTextContentTwoButtons("是否退出当前房间？", "是", "否");
			//	PopWindowsController.Instance.ApplyButton1OnClick(() =>
			//	{
			//		switch (NetworkManager.singleton.mode)
			//		{
			//			case NetworkManagerMode.Offline:
			//				break;

			//			case NetworkManagerMode.ServerOnly:
			//				break;

			//			case NetworkManagerMode.ClientOnly:
			//				NetworkManager.singleton.StopClient();
			//				break;

			//			case NetworkManagerMode.Host:
			//				NetworkManager.singleton.StopHost();
			//				break;

			//			default:
			//				break;
			//		}
			//	});
			//}
			//else
			//{
			//	PopWindowsController.Instance.ApplyTextContentOneButton("当前并没有加入房间？");
			//}
		}

		private void CreateHeadlessServer()
		{
			NetworkManager.singleton.StartServer();
			NetworkDiscoveryWithAnchors.Instance.AdvertiseServer();
		}

		private void CreateRoomOnLAN()
		{
            NetworkManager.singleton.StartHost();
            FindObjectOfType<NetworkDiscoveryWithAnchors>().AdvertiseServer();
            //NetworkManager.singleton.StartHost();
            //NetworkDiscoveryWithAnchors.Instance.AdvertiseServer();
        }

		public void SearchRoomOnLAN()
		{
			//NetworkDiscoveryWithAnchors.Instance.StartDiscovery();
			var networkDiscovery = FindObjectOfType<NetworkDiscoveryWithAnchors>();


			networkDiscovery.StartDiscovery();
            //UnityEventTools.AddPersistentListener(networkDiscovery.OnServerFound, OnDiscoveredServer);
        }

		public void StopSearchRoomOnLAN()
		{
			//networkDiscovery.OnServerFound.RemoveListener(OnDiscoveredServer);
			NetworkDiscoveryWithAnchors.Instance.StopDiscovery();
		}

		public void RegistOnDiscoveredServerHandler()
		{

			FindObjectOfType<NetworkDiscoveryWithAnchors>().OnServerFound.AddListener(OnDiscoveredServer);

		}

      

        public void OnCloseButtonClicked()
		{
			NetworkDiscoveryWithAnchors.Instance.AdvertiseServer();
		}

		private void OnDiscoveredServer(ServerResponse info)
		{
			onFindServer?.Invoke(info);
		}

		
	}
}