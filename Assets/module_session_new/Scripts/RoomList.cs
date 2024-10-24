using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Module.Session
{
    public class RoomList<T> : List<T>
    {
        public delegate void OnAddItemDelegate(T item);
        public delegate void OnRemoveItemDelegate(T item);
        public OnAddItemDelegate onAddItemDelegate;
        public OnRemoveItemDelegate onRemoveItemDelegate;
        new public void Add(T item)
        {
            base.Add(item);
            onAddItemDelegate(item);
        }
        new public void Remove(T item)
        {
            base.Remove(item);
            onRemoveItemDelegate(item);
        }
    }
}