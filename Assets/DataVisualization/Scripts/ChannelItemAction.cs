using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace DataVisualization.Scripts
{
    public class ChannelItemAction : MonoBehaviour
    {
        public Dropdown[] dropdowns;

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