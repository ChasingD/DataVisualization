using Mirror;
using TMPro;
using UnityEngine;

namespace DataVisualization.Scripts
{
    public class ChannelItemAction : MonoBehaviour
    {
        public TMP_Dropdown[] dropdowns;

        public void DeleteThis()
        {
            Destroy(gameObject);
        }

        public void SyncAction()
        {
            GetComponentInParent<DxrSyncController>().CmdDeleteItem(NetworkClient.localPlayer, transform.GetSiblingIndex());
        }

        public void SyncDropdown1(int index)
        {
            GetComponentInParent<DxrSyncController>().CmdSyncItemDropdown(NetworkClient.localPlayer, transform.GetSiblingIndex(), 0, index);
        }

        public void SyncDropdown2(int index)
        {
            GetComponentInParent<DxrSyncController>().CmdSyncItemDropdown(NetworkClient.localPlayer, transform.GetSiblingIndex(), 1, index);
        }

        public void SyncDropdown3(int index)
        {
            GetComponentInParent<DxrSyncController>().CmdSyncItemDropdown(NetworkClient.localPlayer, transform.GetSiblingIndex(), 2, index);
        }
    }
}