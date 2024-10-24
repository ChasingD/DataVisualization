using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace Module.Session
{
    public class RoomController : MonoBehaviour
    {
        private GridObjectCollection gridObjectCollection;
        private float timeCount = 0;

        public RoomList<RoomElement> roomElementList = new RoomList<RoomElement>();

        //public TMP_InputField roomIdInputField;
        //public PanelGroup panelGroup;
        //public GameObject joinRoomPanel;
        //public RoomJoinController joinController;
        public GameObject roomPrefab;
        public Transform roomRoot;
        public float duration = 3;
        //public JoinRoomByRoomId joinRoomByRoomId;

        private void Awake()
        {
            roomElementList.onAddItemDelegate += (item) =>
            {
                InstatiateRoomElement(item);
            };
            roomElementList.onRemoveItemDelegate += (item) =>
            {
                Destroy(item.RoomElementClone);
            };
        }
        protected virtual void Start()
        {
            gridObjectCollection = GetComponent<GridObjectCollection>();
        }
        //private void OnEnable()
        //{
        //    roomIdInputField.text = "";
        //}
        protected virtual void Update()
        {
            if (roomElementList.Count == 0)
            {
                return;
            }
            timeCount += Time.deltaTime;
            if (timeCount > duration)
            {
                timeCount = 0;

                for (int i = 0; i < roomElementList.Count; i++)
                {
                    roomElementList[i].Life--;
                }
            }
        }
        private void InstatiateRoomElement(RoomElement roomElement)
        {
            print($"发现房间名称: {roomElement.ServerAddress}");
            //if (roomElement.IsPublic)
            {
                var roomElementClone = Instantiate(roomPrefab, roomRoot);
                roomElementClone.SetActive(true);
                RoomComponent roomComponent = roomElementClone.GetComponent<RoomComponent>();
                roomComponent.SetRoomAddress(roomElement.ServerAddress);
                roomElement.RoomElementClone = roomElementClone;
                if (!gridObjectCollection)
                {
                    gridObjectCollection = gameObject.AddComponent<GridObjectCollection>();
                }
                gridObjectCollection.UpdateCollection();
            }
        }
        protected void GenerateRoomElement(RoomElement roomElementToAdd)
        {
 //           SessionManager.Instance.CreatButton();

            RoomElement roomElement = roomElementList.Find(re => re.ServerAddress.Equals(roomElementToAdd.ServerAddress));
            if (roomElement != null)
            {
                if (roomElement.Equals(roomElementToAdd))
                {
                    print("已经生成过并且一样，不再生成");
                    //投币续命
                    roomElement.Life++;
                    return;
                }
                else
                {
                    print("已经生成过但是参数不一样，需要移除重新生成");
                    roomElementList.Remove(roomElement);
                }
            }
            roomElementList.Add(roomElementToAdd);
        }
    }
}