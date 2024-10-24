using System.Linq;
using Mirror;
using UnityEngine;

namespace DataVisualization.Scripts
{
    public class HandMenuAction : NetworkBehaviour
    {
        private GameObject dxrvisDupliated;
        [Command(requiresAuthority = false)]
        public void CmdDuplicate()
        {
            if (dxrvisDupliated)
            {
                print("已经生成过副本，不再生成");
                return;
            }
            var prefab = NetworkManager.singleton.spawnPrefabs.Single(o => o.name == "DxRVis");
            dxrvisDupliated = Instantiate(prefab);
            NetworkServer.Spawn(dxrvisDupliated);
        }
        [Command(requiresAuthority = false)]
        public void CmdDelete()
        {
            Destroy(dxrvisDupliated);
        }
    }
}