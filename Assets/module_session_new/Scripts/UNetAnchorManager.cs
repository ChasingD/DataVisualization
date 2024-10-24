// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Mirror;
using UnityEngine;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.Events;
using System.IO;

#if UNITY_UWP || WINDOWS_UWP || UNITY_WSA
#if UNITY_2020_3_OR_NEWER
using Microsoft.MixedReality.OpenXR;
using UnityEngine.XR.ARFoundation;
#else
using UnityEngine.XR.WSA;
using UnityEngine.XR.WSA.Persistence;
using UnityEngine.XR.WSA.Sharing;
#endif
#endif

namespace Module.Session
{
    /// <summary>
    /// Creates, exports, and imports anchors as required.
    /// </summary>
    public class UNetAnchorManager : NetworkBehaviour
    {
        private const string SavedAnchorKey = "SavedAnchorName";

        /// <summary>
        ///  Since we aren't a MonoBehavior we can't just use the singleton class
        ///  so we'll reroll it as a one off here.
        /// </summary>
        private static UNetAnchorManager _Instance;

        public static UNetAnchorManager Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = FindObjectOfType<UNetAnchorManager>();
                }
                return _Instance;
            }
        }

        /// <summary>
        /// Sometimes we'll see a really small anchor blob get generated.
        /// These tend to not work, so we have a minimum trustable size.
        /// </summary>
        private const uint minTrustworthySerializedAnchorDataSize = 500000;

        /// <summary>
        /// Keeps track of the name of the world anchor to use.
        /// </summary>
        [SyncVar]
        public string AnchorName = "";

        [SyncVar(hook = nameof(CreateAnchorHook))]
        public string AnchorOwnerIP = "";

        public UnityAction<string> CreateAnchorEvent;

        private async void CreateAnchorHook(string oldValue, string value)
        {
            // AnchorOwnerIP = value;
            // if (value != string.Empty)
            // {
            //     Debug.Log("Setting server IP: " + value);
            //     GenericNetworkTransmitter.Instance.SetServerIP(value);
            //     Debug.Log("Creating Anchor");
            //     // await UniTask.WaitUntil(() => CreateAnchorEvent != null);
            //     CreateAnchorEvent?.Invoke(value);
            //     await UniTask.WaitUntil(() => NetworkClient.localPlayer);
            //     NetworkClient.localPlayer.GetComponent<PlayerController>().CreateAnchor(value);
            // }
        }

        public void FindAnchorOwner()
        {
            //            createdAnchor = false;

            //            //#if !UNITY_EDITOR
            //#if UNITY_UWP || WINDOWS_UWP
            //            AnchorEstablished = false;
            //#endif

            //            if (WorkShop.Networking.WSPlayerController.Instance != null)
            //            {
            //                WorkShop.Networking.WSPlayerController.Instance.CmdFindNewAnchorOwner();
            //            }
        }

        /// <summary>
        /// List of bytes that represent the anchor data to export.
        /// </summary>
        private List<byte> exportingAnchorBytes = new List<byte>();

        /// <summary>
        /// The UNet network manager in the scene.
        /// </summary>
        private NetworkManager networkManager;

        /// <summary>
        /// The UNetNetworkTransmitter in the scene which can send an anchor to another device.
        /// </summary>
        private GenericNetworkTransmitter networkTransmitter;

        /// <summary>
        /// Keeps track of if we created the anchor.
        /// </summary>
#pragma warning disable 0414
        private bool createdAnchor = false;
#pragma warning restore 0414

        /// <summary>
        /// The object to attach the anchor to when created or imported.
        /// </summary>
        private GameObject objectToAnchor;

        /// <summary>
        /// Previous anchor name.
        /// </summary>
#pragma warning disable 0414
        private string oldAnchorName = "";
#pragma warning restore 0414

        /// <summary>
        /// The anchorData to import.
        /// </summary>
        private byte[] anchorData = null;

        /// <summary>
        /// Tracks if we have updated data to import.
        /// </summary>
#pragma warning disable 0414
        private bool gotOne = false;
#pragma warning restore 0414

        /// <summary>
        /// Keeps track of the name of the anchor we are exporting.
        /// </summary>
        private string exportingAnchorName;

        /// <summary>
        /// Tracks if we have a shared anchor established
        /// </summary>
        public bool AnchorEstablished { get; set; }

        /// <summary>
        /// Tracks if an import is in flight.
        /// </summary>
        public bool ImportInProgress { get; private set; }

        /// <summary>
        /// Tracks if a download is in flight.
        /// </summary>
        public bool DownloadingAnchor { get; private set; }

        /// <summary>
        /// Ensures that the scene has what we need to continue.
        /// </summary>
        /// <returns>True if we can proceed, false otherwise.</returns>
        private bool CheckConfiguration()
        {
            networkTransmitter = GenericNetworkTransmitter.Instance;
            if (networkTransmitter == null)
            {
                Debug.Log("No UNetNetworkTransmitter found in scene");
                return false;
            }

            networkManager = NetworkManager.singleton;
            if (networkManager == null)
            {
                Debug.Log("No NetworkManager found in scene");
                return false;
            }

            if (SharedCollection.Instance == null)
            {
                Debug.Log("No SharedCollection found in scene");
                return false;
            }
            else
            {
                objectToAnchor = SharedCollection.Instance.gameObject;
            }

            return true;
        }

        private void Start()
        {
            if (!CheckConfiguration())
            {
                Debug.Log("Missing required component for UNetAnchorManager");
                // Destroy(this);
                return;
            }

#if UNITY_EDITOR || UNITY_STANDALONE || PICOVIEWER
            AnchorEstablished = true;
#elif UNITY_UWP || WINDOWS_UWP || UNITY_WSA
            networkTransmitter.dataReadyEvent += NetworkTransmitter_dataReadyEvent;
#endif
        }

        private async void Update()
        {
#if UNITY_UWP || WINDOWS_UWP || UNITY_WSA

            if (gotOne)
            {
                //PopWindowsController.Instance.ApplyTextContentNoButton("正在同步空间锚点");

                Debug.Log("importing");
                gotOne = false;
                ImportInProgress = true;
#if UNITY_2020_3_OR_NEWER
                Stream stream = new MemoryStream(anchorData);
                stream.Seek(0, SeekOrigin.Begin);
                var transferBatch = await XRAnchorTransferBatch.ImportAsync(stream);
                if (transferBatch.AnchorNames.Count > 0)
                {
                    Debug.Log("Import complete");

                    string first = transferBatch.AnchorNames[0];
                    Debug.Log("Anchor name: " + first);
                    //ARAnchor existingAnchor = objectToAnchor.GetComponent<ARAnchor>();
                    //if (existingAnchor != null)
                    //{
                    //	DestroyImmediate(existingAnchor);
                    //}
                    var id = transferBatch.LoadAnchor(first);
                    ARAnchor existingAnchor = objectToAnchor.GetComponent<ARAnchor>();
                    if (existingAnchor == null)
                    {
                        objectToAnchor.AddComponent<ARAnchor>();
                        await UniTask.Delay(2000);
                    }
                    var newId = transferBatch.LoadAndReplaceAnchor(first, existingAnchor.trackableId);
                    print("newId: " + newId);

                    ImportInProgress = false;
                }
                else
                {
                    // if we failed, we can simply try again.
                    gotOne = true;
                    //PopWindowsController.Instance.ApplyTextContentOneButton("同步空间锚点失败");

                    Debug.Log("Import fail");
                }
#else
				WorldAnchorTransferBatch.ImportAsync(anchorData, ImportComplete);
#endif
            }

            if (oldAnchorName != AnchorName &&
                !createdAnchor &&
                !String.IsNullOrEmpty(GenericNetworkTransmitter.Instance.serverIP))
            {
                Debug.LogFormat("New anchor name {0} => {1}", oldAnchorName, AnchorName);
                Debug.Log("Server IP: " + GenericNetworkTransmitter.Instance.serverIP);

                oldAnchorName = AnchorName;
                if (string.IsNullOrEmpty(AnchorName))
                {
                    Debug.Log("anchor is empty");
                    AnchorEstablished = false;
                }
                //else if (PlayerPrefs.HasKey(SavedAnchorKey) && AttachToCachedAnchor(PlayerPrefs.GetString(SavedAnchorKey)))
                //{
                //    Debug.Log("__found " + AnchorName + " again");
                //}
                else/* if (!AttachToCachedAnchor(AnchorName))*/
                {
                    AnchorEstablished = true;
                    Debug.Log("Need to import anchor.");
                    WaitForAnchor();
                }
            }
#else
            return;
#endif
        }

        /// <summary>
        /// If we are supposed to create the anchor for export, this is the function to call.
        /// </summary>
        public async void CreateAnchor()
        {
            Debug.Log("Debug 0");

#if UNITY_EDITOR || UNITY_STANDALONE || PICOVIEWER
            Debug.Log("Anchors cannot be created from the Unity editor.");
#elif UNITY_WSA
			Debug.Log("Debug 1");
			exportingAnchorBytes.Clear();
			GenericNetworkTransmitter.Instance.SetData(null);
			objectToAnchor = SharedCollection.Instance.gameObject;

#if UNITY_2019_4
			WorldAnchor worldAnchor = objectToAnchor.GetComponent<WorldAnchor>();
			if (worldAnchor == null)
			{
				worldAnchor = objectToAnchor.AddComponent<WorldAnchor>();
			}
			exportingAnchorName = Guid.NewGuid().ToString();

			WorldAnchorTransferBatch watb = new WorldAnchorTransferBatch();

			Debug.Log("exporting " + exportingAnchorName);
			if (watb.AddWorldAnchor(exportingAnchorName, worldAnchor))
			{
				WorldAnchorTransferBatch.ExportAsync(watb, WriteBuffer, ExportComplete);
			}
			else
			{
				print("!!!!");
			}

#else
Debug.Log("Debug 2");			
ARAnchor anchor = objectToAnchor.GetComponent<ARAnchor>();
			if (anchor == null)
			{
				anchor = objectToAnchor.AddComponent<ARAnchor>();
			}
			exportingAnchorName = anchor.trackableId.ToString();
Debug.Log("Debug 3");


			XRAnchorTransferBatch transferBatch = new XRAnchorTransferBatch();

			Debug.Log("exporting " + exportingAnchorName);

			if(transferBatch.AddAnchor(anchor.trackableId, exportingAnchorName))
			{
				Debug.Log("Debug 4");
				using (MemoryStream stream = (MemoryStream)await XRAnchorTransferBatch.ExportAsync(transferBatch))
				{
					byte[] result = stream.ToArray();
					WriteBuffer(result);
				}
				Debug.Log("Debug 5"+exportingAnchorBytes.Count +"---"+ minTrustworthySerializedAnchorDataSize);
				if (exportingAnchorBytes.Count > minTrustworthySerializedAnchorDataSize)
				{
					AnchorName = exportingAnchorName;
					anchorData = exportingAnchorBytes.ToArray();
					GenericNetworkTransmitter.Instance.SetData(anchorData);
					createdAnchor = true;
					Debug.Log("Anchor ready " + exportingAnchorBytes.Count);
					GenericNetworkTransmitter.Instance.ConfigureAsServer();

					AnchorEstablished = true;
					PlayerController.Instance.CmdSetAnchorName(exportingAnchorName);

					//PopWindowsController.Instance.ApplyTextContentOneButton("创建空间锚点成功");
				}
				else
				{
					//PopWindowsController.Instance.ApplyTextContentOneButton("创建空间锚点失败");

					Debug.Log("Create anchor failed " + exportingAnchorBytes.Count);
					exportingAnchorBytes.Clear();
					objectToAnchor = SharedCollection.Instance.gameObject;
					//DestroyImmediate(objectToAnchor.GetComponent<ARAnchor>());
					CreateAnchor();
				}
			}
#endif
#endif
        }

#if UNITY_UWP || WINDOWS_UWP || UNITY_WSA

        /// <summary>
        /// If we don't have the anchor already, call this to download the anchor.
        /// </summary>
        public void WaitForAnchor()
        {
            DownloadingAnchor = true;
            networkTransmitter.RequestAndGetData();
        }

        /// <summary>
        /// Attempts to attach to  an anchor by anchorName in the local store..
        /// </summary>
        /// <returns>True if it attached, false if it could not attach</returns>
        //private bool AttachToCachedAnchor(string CachedAnchorName)
        //{
        //	if (string.IsNullOrEmpty(CachedAnchorName))
        //	{
        //		Debug.Log("Ignoring empty name");
        //		return false;
        //	}

        //	Debug.Log("Looking for " + CachedAnchorName);
        //	if (WorldAnchorManager.Instance == null)
        //	{
        //		Debug.LogError("WorldAnchorManager is null.");
        //	}
        //	if (WorldAnchorManager.Instance.AnchorStore == null)
        //	{
        //		Debug.LogError("AnchorStore is null.");
        //	}

        //	WorldAnchorStore anchorStore = WorldAnchorManager.Instance.AnchorStore;
        //	string[] ids = anchorStore.GetAllIds();
        //	Debug.Log(ids.Length + " stored anchors.");
        //	for (int index = 0; index < ids.Length; index++)
        //	{
        //		if (ids[index] == CachedAnchorName)
        //		{
        //			Debug.Log("Using what we have");
        //			anchorStore.Load(ids[index], objectToAnchor);
        //			AnchorEstablished = true;
        //			return true;
        //		}
        //		else
        //		{
        //			Debug.Log(ids[index]);
        //		}
        //	}

        //	// Didn't find the anchor.
        //	return false;
        //}

        /// <summary>
        /// Called when anchor data is ready.
        /// </summary>
        /// <param name="data">The data blob to import.</param>
        private void NetworkTransmitter_dataReadyEvent(byte[] data)
        {
            Debug.Log("Anchor data arrived.");
            anchorData = data;
            //Debug.Log(data.Length);
            DownloadingAnchor = false;
            gotOne = true;
        }

        /// <summary>
        /// Called when a remote anchor has been deserialized
        /// </summary>
        /// <param name="status">Tracks if the import worked</param>
        /// <param name="wat">The WorldAnchorTransferBatch that has the anchor information.</param>
#if UNITY_2020_3_OR_NEWER
#else
		private void ImportComplete(SerializationCompletionReason status, WorldAnchorTransferBatch wat)
		{
			if (status == SerializationCompletionReason.Succeeded && wat.GetAllIds().Length > 0)
			{
				Debug.Log("Import complete");

				string first = wat.GetAllIds()[0];
				Debug.Log("Anchor name: " + first);
				WorldAnchor existingAnchor = objectToAnchor.GetComponent<WorldAnchor>();
				if (existingAnchor != null)
				{
					DestroyImmediate(existingAnchor);
				}

				WorldAnchor anchor = wat.LockObject(first, objectToAnchor);
				anchor.OnTrackingChanged += Anchor_OnTrackingChanged;
				Anchor_OnTrackingChanged(anchor, anchor.isLocated);

				ImportInProgress = false;
			}
			else
			{
				// if we failed, we can simply try again.
				gotOne = true;
				//PopWindowsController.Instance.ApplyTextContentOneButton("同步空间锚点失败");

				Debug.Log("Import fail");
			}
		}

		private void Anchor_OnTrackingChanged(WorldAnchor self, bool located)
		{
			if (located)
			{
				AnchorEstablished = true;
				WorldAnchorManager.Instance.AnchorStore.Save(AnchorName, self);
				self.OnTrackingChanged -= Anchor_OnTrackingChanged;
			}
		}
		/// <summary>
		/// Called when serializing an anchor is complete.
		/// </summary>
		/// <param name="status">If the serialization succeeded.</param>
		private void ExportComplete(SerializationCompletionReason status)
		{
			if (status == SerializationCompletionReason.Succeeded && exportingAnchorBytes.Count > minTrustworthySerializedAnchorDataSize)
			{
				AnchorName = exportingAnchorName;
				anchorData = exportingAnchorBytes.ToArray();
				GenericNetworkTransmitter.Instance.SetData(anchorData);
				createdAnchor = true;
				Debug.Log("Anchor ready " + exportingAnchorBytes.Count);
				GenericNetworkTransmitter.Instance.ConfigureAsServer();

				AnchorEstablished = true;
				PlayerController.Instance.CmdSetAnchorName(exportingAnchorName);

				//PopWindowsController.Instance.ApplyTextContentOneButton("创建空间锚点成功");
			}
			else
			{
				//PopWindowsController.Instance.ApplyTextContentOneButton("创建空间锚点失败");

				Debug.Log("Create anchor failed " + status + " " + exportingAnchorBytes.Count);
				exportingAnchorBytes.Clear();
				objectToAnchor = SharedCollection.Instance.gameObject;
				DestroyImmediate(objectToAnchor.GetComponent<WorldAnchor>());
				CreateAnchor();
			}
		}
#endif

        /// <summary>
        /// Called as anchor data becomes available to export
        /// </summary>
        /// <param name="data">The next chunk of data.</param>
        private void WriteBuffer(byte[] data)
        {
            //print(data.Length);
            exportingAnchorBytes.AddRange(data);
        }



#endif

        public void AnchorFoundRemotely()
        {
            Debug.Log("Setting saved anchor to " + AnchorName);
            //#if !UNITY_EDITOR
#if UNITY_UWP || WINDOWS_UWP || UNITY_WSA

            SaveAnchor(AnchorName);
#endif
        }

#if UNITY_UWP || WINDOWS_UWP || UNITY_WSA

        private void SaveAnchor(string anchorName)
        {
#if UNITY_2020_3_OR_NEWER
#else
			WorldAnchorManager.Instance.AnchorStore.Save(anchorName, objectToAnchor.GetComponent<WorldAnchor>());
#endif
            PlayerPrefs.SetString(SavedAnchorKey, anchorName);
            PlayerPrefs.Save();
        }

#endif
    }
}