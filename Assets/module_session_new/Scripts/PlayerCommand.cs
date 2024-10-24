using Mirror;
using UnityEngine;

namespace Module.Session
{
    public class PlayerCommand : NetworkBehaviour
    {
        private static PlayerCommand _Instance = null;

        /// <summary>
        /// Instance of the PlayerController that represents the local player.
        /// </summary>
        public static PlayerCommand Instance
        {
            get { return _Instance; }
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            _Instance = this;
        }

        private void Start()
        {
        }

        [Command]
        public void CmdApplyAuthority(NetworkIdentity networkIdentity)
        {
            //if (!networkIdentity.hasAuthority)
            //{
            //    print("给" + networkIdentity.gameObject.name + "添加权限" + connectionToClient.address);
            //    if (!networkIdentity.hasAuthority)
            //    {
            //        if (networkIdentity.connectionToClient != null && connectionToClient != networkIdentity.connectionToClient)
            //        {
            //            networkIdentity.RemoveClientAuthority();

            //        }
            //        networkIdentity.AssignClientAuthority(connectionToClient);

            //    }
            //}
            //else
            //{
            //    print(networkIdentity.gameObject.name + "已有权限");
            //}
        }

        public void CmdRemoveAuthority(NetworkIdentity identity)
        {
        }

        [Command]
        public void CmdSetObjectMoveAuthority(NetworkIdentity target, int value)
        {
            target.GetComponent<SyncController>().canMove = value;
        }

        [Command]
        public void CmdDestroyGameobject(NetworkIdentity networkIdentity)
        {
            NetworkServer.Destroy(networkIdentity.gameObject);
        }

        [Command]
        public void CmdSpawnModel(int index, NetworkIdentity owner)
        {
            if (!NetworkManager.singleton.spawnPrefabs[index])
            {
                Debug.LogError("模型没有添加到NetworkManager SpawnPrefabs中");
                return;
            }

            var model = Instantiate(NetworkManager.singleton.spawnPrefabs[index]);

            model.GetComponent<SyncController>().owner = owner;

            NetworkServer.Spawn(model, owner.gameObject);
        }
    }
}