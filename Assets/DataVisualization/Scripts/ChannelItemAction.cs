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

        // public void SyncDropdown1()
        // {
        //     
        // }
        // public void SyncDropdown2()
        // {
        //     
        // }
        // public void SyncDropdown3()
        // {
        //     
        // }
    }
}