using Mirror;
using Mirror.Discovery;
using TMPro;
using UnityEngine;

namespace Module.Session
{
    public class RoomComponent : MonoBehaviour
    {
        public TMP_Text TmpTextRoomAddress;
        public void SetRoomAddress(string address)
        {
            TmpTextRoomAddress.text = address;
        }
        public void SelectCurrentRoom()
        {
            NetworkManager.singleton.networkAddress = TmpTextRoomAddress.text;
        }
        public void DeselectCurrentRoom()
        {
            NetworkManager.singleton.networkAddress = string.Empty;

        }
        
         public void ToggleCurrentRoom(bool on)
        {
            if (on)
            {
                SelectCurrentRoom();

            }
            else
            {
                DeselectCurrentRoom();

            }
        }
    }
}