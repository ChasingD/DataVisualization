using System;
using DxR;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace DataVisualization.Scripts
{
    public class DxrSyncController : NetworkBehaviour
    {
        public DxR.GUI gui;

        private void Start()
        {
            transform.SetParent(RootCollection.Instance.transform, false);
        }

        #region data

        [SyncVar(hook = nameof(OnDataDropdownIndexChanged))]
        public int dataDropdownIndex = -1;

        private void OnDataDropdownIndexChanged(int oldvalue, int newvalue)
        {
            gui.dataDropdown.value = newvalue;
        }

        [Command(requiresAuthority = false)]
        public void CmdSetDataDropdownIndex(int index)
        {
            dataDropdownIndex = index;
        }

        public void SetDataDropdownIndex(int index)
        {
            if (NetworkClient.active)
            {
                CmdSetDataDropdownIndex(index);
            }
        }

        #endregion

        #region mark

        [SyncVar(hook = nameof(OnMarkDropdownIndexChanged))]
        public int markDropdownIndex = 1;

        private void OnMarkDropdownIndexChanged(int oldvalue, int newvalue)
        {
            gui.markDropdown.value = newvalue;
        }

        [Command(requiresAuthority = false)]
        public void CmdSetMarkDropdownIndex(int index)
        {
            if (NetworkClient.active)
            {
                markDropdownIndex = index;
            }
        }
        
        public void SetMarkDropdownIndex(int index)
        {
            if (NetworkClient.active)
            {
                CmdSetMarkDropdownIndex(index);
            }
        }

        #endregion

        #region 添加新Channel

        public void AddItem()
        {
            CmdAddItem(NetworkClient.localPlayer);
        }

        [Command(requiresAuthority = false)]
        public void CmdAddItem(NetworkIdentity invoker)
        {
            RpcAddItem(invoker);
        }

        [ClientRpc]
        public void RpcAddItem(NetworkIdentity invoker)
        {
            if (invoker == NetworkClient.localPlayer)
            {
                return;
            }

            gui.AddEmptyChannelGUICallback();
        }

        #endregion

        #region 删除已有的一个Channel

        [Command(requiresAuthority = false)]
        public void CmdDeleteItem(NetworkIdentity invoker, int index)
        {
            RpcDeleteItem(invoker, index);
        }

        [ClientRpc]
        public void RpcDeleteItem(NetworkIdentity invoker, int index)
        {
            if (invoker == NetworkClient.localPlayer)
            {
                return;
            }

            Transform list = gui.gameObject.transform.Find("ChannelList/Viewport/ChannelListContent");
            if (list.childCount > index)
            {
                var target = list.GetChild(index);
                target.GetComponent<ChannelItemAction>().DeleteThis();
            }
        }

        #endregion

        #region 同步Item上的dropdowns

        [Command(requiresAuthority = false)]
        public void CmdSyncItemDropdown(NetworkIdentity invoker, int itemIndex, int dropdownIndex, int index)
        {
            RpcSyncItemDropdown(invoker, itemIndex, dropdownIndex, index);
        }

        [ClientRpc]
        public void RpcSyncItemDropdown(NetworkIdentity invoker, int itemIndex, int dropdownIndex, int index)
        {
            if (invoker == NetworkClient.localPlayer)
            {
                return;
            }

            Transform list = gui.gameObject.transform.Find("ChannelList/Viewport/ChannelListContent");
            if (list.childCount > itemIndex)
            {
                var target = list.GetChild(itemIndex);
                target.GetComponent<ChannelItemAction>().dropdowns[dropdownIndex].value = index;
            }
        }

        #endregion

        #region 同步UpdateVis

        [Command(requiresAuthority = false)]
        public void CmdUpdateVis()
        {
            RpcUpdateVis();
        }

        [ClientRpc]
        private void RpcUpdateVis()
        {
            gui.CallUpdateVisSpecsFromGUISpecs();
        }

        #endregion
    }
}