using Cysharp.Threading.Tasks;
using Mirror.Discovery;

namespace Module.Session
{
    public class LanRoomController : RoomController
    {
        //[SerializeField]
        //private string lanHostPort;

        protected async override void Start()
        {
            base.Start();
        
            await UniTask.Delay(1000);

            SessionManager.Instance.SearchRoomOnLAN();
            SessionManager.Instance.RegistOnDiscoveredServerHandler();
            SessionManager.Instance.onFindServer.AddListener(OnRecievedServersOnLan);
        }
        private void OnRecievedServersOnLan(ServerResponse serverResponse)
        {
            RoomElement roomElementToAdd = new RoomElement(
                serverResponse.EndPoint.Address.ToString(),
                o => roomElementList.Remove(o));

            GenerateRoomElement(roomElementToAdd);
        }
        
    }
}