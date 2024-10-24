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
            // FindObjectOfType<NetworkDiscovery>().networkAddress = TmpTextRoomAddress.text;
        }
        public void DeselectCurrentRoom()
        {
            // FindObjectOfType<NetworkDiscovery>().networkAddress = string.Empty;

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