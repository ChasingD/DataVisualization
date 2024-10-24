using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Module.Session.Test
{
    public class Test : MonoBehaviour
    {
        [ContextMenu("SpawnCube")]
        public void SpawnCube()
		{
            PlayerCommand.Instance.CmdSpawnModel(0, NetworkClient.localPlayer);
		}
    }
}