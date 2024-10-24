using Mirror;
using UnityEngine;

namespace DataVisualization.Scripts
{
    public class ChannelItemAction : MonoBehaviour
    {
        public void DeleteThis()
        {
            Destroy(gameObject);
        }

        public void SyncAction()
        {
            GetComponentInParent<DxrSyncController>().CmdDeleteItem(NetworkClient.localPlayer, transform.GetSiblingIndex());
        }
    }
}