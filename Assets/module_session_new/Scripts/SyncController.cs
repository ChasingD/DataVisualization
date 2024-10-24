using Cysharp.Threading.Tasks;
using Microsoft.MixedReality.Toolkit.UI;
using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Module.Session
{
	public class SyncController : NetworkBehaviour
	{
		public GameObject appbarGO;
		public BoundingBox boundingBox;
		public ObjectManipulator manipulator;
		[SyncVar(hook = nameof(OnOwnerChanged))]
		public NetworkIdentity owner;
		[SyncVar(hook = nameof(OnCanMoveValueChanged))]
		[SerializeField]
		public int canMove;
		private Action<int> onCanMoveAction;
		private AppBar appBar;
		private void OnCanMoveValueChanged(int _, int newValue)
		{
			onCanMoveAction?.Invoke(newValue);

		}
		protected virtual void OnOwnerChanged(NetworkIdentity _, NetworkIdentity newOwner)
		{

		}
		void Awake()
		{
			appBar = Instantiate(appbarGO).GetComponent<AppBar>();

		}
		// Start is called before the first frame update
		void Start()
		{
			appBar.Target = boundingBox;
			//await UniTask.Delay(5000);
			var appBarButtons = appBar.GetComponentsInChildren<AppBarButton>(true);
			foreach (var button in appBarButtons)
			{
				if (button.ButtonType == AppBar.ButtonTypeEnum.Adjust)
				{
					button.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() =>
					{
						PlayerCommand.Instance.CmdApplyAuthority(netIdentity);
						PlayerCommand.Instance.CmdSetObjectMoveAuthority(netIdentity, 1);
						manipulator.enabled = true;
					});
					onCanMoveAction = (value) => button.GetComponent<Interactable>().enabled = (value == 1 ? false : true);

				}
				else if (button.ButtonType == AppBar.ButtonTypeEnum.Done)
				{
					button.GetComponent<ButtonConfigHelper>().OnClick.AddListener(() =>
					{
						PlayerCommand.Instance.CmdRemoveAuthority(netIdentity);
						PlayerCommand.Instance.CmdSetObjectMoveAuthority(netIdentity, 0);
						manipulator.enabled = false;

					});
				}

			}
		}

		// Update is called once per frame
		void Update()
		{

		}
	}
}