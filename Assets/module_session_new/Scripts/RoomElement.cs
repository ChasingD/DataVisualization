using System;
using System.Collections.Generic;
using UnityEngine;
namespace Module.Session
{
    public class RoomElement
    {
        private Action<RoomElement> OnRoomDied;
        private int life;
        public int Life
        {
            get
            {
                return life;
            }
            set
            {
                life = value;
                if (life <= 0)
                {
                    OnRoomDied.Invoke(this);
                }
            }
        }
        public string ServerAddress { get; set; }
        public GameObject RoomElementClone { get; set; }
        public RoomElement(string serverAddress, Action<RoomElement> OnRoomDied)
        {
            Life = 2;
            ServerAddress = serverAddress;
            this.OnRoomDied = OnRoomDied;
        }
        public bool Equals(RoomElement y)
        {
            return ServerAddress.Equals(y.ServerAddress);
        }

    }
}