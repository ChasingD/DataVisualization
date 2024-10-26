//#define USE_INTERACTION_GUI

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using UnityEngine.UI;
using System;
using DataVisualization.Scripts;
using Microsoft.MixedReality.Toolkit.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace DxR
{
    /// <summary>
    /// Whenever a GUI action is performed, button clicked, TMP_Dropdown clicked, etc., the guiVisSpecs is automatically updated so it 
    /// should be in sync all the time. The visSpecs of the targetVis is only updated when calling UpdateVisSpecsFromGUISpecs, and 
    /// for the other way around, the guiVisSpecs is updated from the targetVis specs when calling UpdateGUISpecsFromVisSpecs.
    /// </summary>
    public class GUI : MonoBehaviour
    {
        Vis targetVis = null;
        JSONNode guiVisSpecs = null;
        public TMP_Dropdown dataDropdown = null;
        public TMP_Dropdown markDropdown = null;
        
        public Transform addChannelButtonTransform = null;
        GameObject channelGUIPrefab = null;

        Transform addInteractionButtonTransform = null;
        GameObject interactionGUIPrefab = null;

        List<string> dataFieldTypeDropdownOptions;

        public Interactable updateButton;
        public Interactable resetBtn;
        public Interactable zoomInBtn;
        public Interactable zoomOutBtn;
        public Interactable rotateXBtn;
        public Interactable rotateYBtn;
        public Interactable rotateZBtn;
        // Use this for initialization
        void Start()
        {

        }

        public Vis GetTargetVis()
        {
            return targetVis;
        }

        public void Init(Vis targetVisInstance)
        {
            if (targetVis == null)
            {
                targetVis = targetVisInstance;
            }
            
            dataFieldTypeDropdownOptions = new List<string> { "quantitative", "nominal", "ordinal", "temporal" };

            Transform dataDropdownTransform = gameObject.transform.Find("DataDropdown");
            // dataDropdown = dataDropdownTransform.gameObject.GetComponent<TMP_Dropdown>();
            dataDropdown.onValueChanged.AddListener(delegate {
                OnDataDropdownValueChanged(dataDropdown);
            });

            Transform marksDropdownTransform = gameObject.transform.Find("MarkDropdown");
            // markDropdown = marksDropdownTransform.gameObject.GetComponent<TMP_Dropdown>();
            markDropdown.onValueChanged.AddListener(delegate {
                OnMarkDropdownValueChanged(markDropdown);
            });

            // Button btn = gameObject.transform.Find("UpdateButton").GetComponent<Button>();
            updateButton.OnClick.AddListener(CallUpdateVisSpecsFromGUISpecs);

            channelGUIPrefab = Resources.Load("GUI/ChannelGUI") as GameObject;

            // addChannelButtonTransform = gameObject.transform.Find("ChannelList/Viewport/ChannelListContent/AddChannelButton");
            Interactable addChannelBtn = addChannelButtonTransform.GetComponent<Interactable>();
            addChannelBtn.OnClick.AddListener(AddEmptyChannelGUICallback);

#if USE_INTERACTION_GUI
            
            interactionGUIPrefab = Resources.Load("GUI/InteractionGUI") as GameObject;

            addInteractionButtonTransform = gameObject.transform.Find("InteractionList/Viewport/InteractionListContent/AddInteractionButton");
            Button addInteractionBtn = addInteractionButtonTransform.GetComponent<Button>();
            addInteractionBtn.onClick.AddListener(AddEmptyInteractionGUICallback);
#endif
            InitInteractiveButtons();
            UpdateGUISpecsFromVisSpecs();
        }

        private void InitInteractiveButtons()
        {
            // resetBtn = gameObject.transform.Find("ResetButton").GetComponent<Button>();
            resetBtn.OnClick.AddListener(ResetCallback);

            // zoomInBtn = gameObject.transform.Find("ZoomInButton").GetComponent<Button>();
            zoomInBtn.OnClick.AddListener(ZoomInCallback);

            // zoomOutBtn = gameObject.transform.Find("ZoomOutButton").GetComponent<Button>();
            zoomOutBtn.OnClick.AddListener(ZoomOutCallback);

            // rotateXBtn = gameObject.transform.Find("RotateXButton").GetComponent<Button>();
            rotateXBtn.OnClick.AddListener(RotateXCallback);

            // rotateYBtn = gameObject.transform.Find("RotateYButton").GetComponent<Button>();
            rotateYBtn.OnClick.AddListener(RotateYCallback);

            // rotateZBtn = gameObject.transform.Find("RotateZButton").GetComponent<Button>();
            rotateZBtn.OnClick.AddListener(RotateZCallback);
        }

        public void RotateXCallback()
        {
            if (targetVis != null)
            {
                targetVis.RotateAroundCenter(Vector3.right, -15);
            }
        }

        public void RotateYCallback()
        {
            if (targetVis != null)
            {
                targetVis.RotateAroundCenter(Vector3.up, -15);
            }
        }

        public void RotateZCallback()
        {
            if (targetVis != null)
            {
                targetVis.RotateAroundCenter(Vector3.forward, -15);
            }
        }

        public void ResetCallback()
        {
            if (targetVis != null)
            {
                targetVis.ResetView();
            }
        }

        public void ZoomInCallback()
        {
            if(targetVis != null)
            {
                targetVis.Rescale(1.10f);
            }
        }

        public void ZoomOutCallback()
        {
            if (targetVis != null)
            {
                targetVis.Rescale(0.9f);
            }
        }

        // Call this to update the GUI and its specs when the vis specs of 
        // the target vis is updated.
        public void UpdateGUISpecsFromVisSpecs()
        {
            // Update the JSONNOde specs:
            guiVisSpecs = JSON.Parse(targetVis.GetVisSpecs().ToString());


            List<string> marksList = targetVis.GetMarksList();

            // Update the TMP_Dropdown options:
            UpdateGUIDataDropdownList(targetVis.GetDataList());
            UpdateGUIMarksDropdownList(marksList);

            if(!marksList.Contains(guiVisSpecs["mark"].Value.ToString()))
            {
                throw new Exception("Cannot find mark name in DxR/Resources/Marks/marks.json");
            }

            // Update the TMP_Dropdown values:
            UpdateDataDropdownValue(guiVisSpecs["data"]["url"].Value);
UpdateMarkDropdownValue(guiVisSpecs["mark"].Value);

            // Update GUI for channels:
            UpdateGUIChannelsList(guiVisSpecs);

#if USE_INTERACTION_GUI
            // Update GUI for interactions:
            //UpdateGUIInteractionsList(guiVisSpecs);
#endif 
        }

        // Adds or removes channel GUIs according to specs and updates the dropdowns.
        private void UpdateGUIChannelsList(JSONNode guiVisSpecs)
        {
            // Remove all channels;
            RemoveAllChannelGUIs();

            // Go through each channel encoding in the specs and add GUI for each:
            JSONObject channelEncodings = guiVisSpecs["encoding"].AsObject;
            if(channelEncodings != null)
            {
                foreach (KeyValuePair<string, JSONNode> kvp in channelEncodings.AsObject)
                {
                    string channelName = kvp.Key;
                    if(guiVisSpecs["encoding"][channelName]["value"] == null && 
                        IsChannelInMarksChannelList(guiVisSpecs["mark"].Value, channelName))
                    {
                        AddChannelGUI(channelName, kvp.Value.AsObject);
                    }
                }
            }
        }

        // Adds or removes interaction GUIs according to specs and updates the dropdowns.
        private void UpdateGUIInteractionsList(JSONNode guiVisSpecs)
        {
            // Remove all interactions;
            RemoveAllInteractionGUIs();

            // Go through each interaction in the specs and add GUI for each:
            JSONArray interactionSpecsArray = guiVisSpecs["interaction"].AsArray;
            if (interactionSpecsArray != null)
            {
                foreach (JSONObject interactionSpecs in interactionSpecsArray)
                {
                    AddInteractionGUI(interactionSpecs);
                }
            }
        }

        private void AddInteractionGUI(JSONObject interactionSpecs)
        {
            GameObject interactionGUI = AddEmptyInteractionGUI();

            UpdateInteractionGUIInteractionTypeDropdownValue(interactionSpecs["type"].Value, ref interactionGUI);
            UpdateInteractionGUIDataFieldDropdownValue(interactionSpecs["field"].Value, ref interactionGUI);
        }

        private void UpdateInteractionGUIInteractionTypeDropdownValue(string value, ref GameObject interactionGUI)
        {
            TMP_Dropdown TMP_Dropdown = interactionGUI.transform.Find("InteractionTypeDropdown").GetComponent<TMP_Dropdown>();
            int valueIndex = GetOptionIndex(TMP_Dropdown, value);
            if (valueIndex > 0)
            {
                TMP_Dropdown.value = valueIndex;
            }
        }

        private void UpdateInteractionGUIDataFieldDropdownValue(string value, ref GameObject interactionGUI)
        {
            TMP_Dropdown TMP_Dropdown = interactionGUI.transform.Find("DataFieldDropdown").GetComponent<TMP_Dropdown>();
            //string prevValue = TMP_Dropdown.options[TMP_Dropdown.value].text;
            int valueIndex = GetOptionIndex(TMP_Dropdown, value);
            if (valueIndex > 0)
            {
                TMP_Dropdown.value = valueIndex;
            }
        }

        private bool IsChannelInMarksChannelList(string markName, string channelName)
        {
            return targetVis.GetChannelsList(markName).Contains(channelName);
        }

        private void AddChannelGUI(string channelName, JSONObject channelEncodingSpecs)
        {
            GameObject channelGUI = AddEmptyChannelGUI();

            Debug.Log("Encoding:" + channelName + "," + channelEncodingSpecs.ToString(2));

            UpdateChannelGUIChannelDropdownValue(channelName, ref channelGUI);
            UpdateChannelGUIDataFieldDropdownValue(channelEncodingSpecs["field"].Value, ref channelGUI);
            UpdateChannelGUIDataFieldTypeDropdownValue(channelEncodingSpecs["type"].Value, ref channelGUI);

        }

        private void UpdateChannelGUIDataFieldTypeDropdownValue(string value, ref GameObject channelGUI)
        {
            TMP_Dropdown TMP_Dropdown = channelGUI.GetComponent<ChannelItemAction>().dropdowns[2].GetComponent<TMP_Dropdown>();
            int valueIndex = GetOptionIndex(TMP_Dropdown, value);
            if (valueIndex > 0)
            {
                TMP_Dropdown.value = valueIndex;
            }
        }

        private void UpdateChannelGUIDataFieldDropdownValue(string value, ref GameObject channelGUI)
        {
            TMP_Dropdown TMP_Dropdown = channelGUI.GetComponent<ChannelItemAction>().dropdowns[1].GetComponent<TMP_Dropdown>();
            int valueIndex = GetOptionIndex(TMP_Dropdown, value);
            if (valueIndex > 0)
            {
                TMP_Dropdown.value = valueIndex;
            }
        }

        private void UpdateChannelGUIChannelDropdownValue(string value, ref GameObject channelGUI)
        {
            TMP_Dropdown TMP_Dropdown = channelGUI.GetComponent<ChannelItemAction>().dropdowns[0].GetComponent<TMP_Dropdown>();
            int valueIndex = GetOptionIndex(TMP_Dropdown, value);
            if (valueIndex > 0)
            {
                TMP_Dropdown.value = valueIndex;
            }
        }

        private void RemoveAllChannelGUIs()
        {
            Transform channelListContent = gameObject.transform.Find("ChannelList/Viewport/ChannelListContent");
            for(int i = 0; i < channelListContent.childCount - 1; i++)
            {
                GameObject.Destroy(channelListContent.GetChild(i).gameObject);
            }
        }

        private void RemoveAllInteractionGUIs()
        {
            Transform interactionListContent = gameObject.transform.Find("InteractionList/Viewport/InteractionListContent");
            for (int i = 0; i < interactionListContent.childCount - 1; i++)
            {
                GameObject.Destroy(interactionListContent.GetChild(i).gameObject);
            }
        }

        // Call this to update the vis specs with the current GUI specs.
        public void UpdateVisSpecsFromGUISpecs()
        {
            UpdateGUISpecsFromGUIValues();

            targetVis.UpdateVisSpecsFromGUISpecs();
        }

        private void UpdateGUISpecsFromGUIValues()
        {
            guiVisSpecs["data"]["url"] = dataDropdown.options[dataDropdown.value].text;
            guiVisSpecs["mark"] = markDropdown.options[markDropdown.value].text;

            guiVisSpecs["encoding"] = null;

            JSONObject encodingObject = new JSONObject();
            Transform channelListContent = gameObject.transform.Find("ChannelList/Viewport/ChannelListContent");
            for (int i = 0; i < channelListContent.childCount - 1; i++)
            {
                GameObject channelGUI = channelListContent.GetChild(i).gameObject;
                JSONObject channelSpecs = new JSONObject();
                
                TMP_Dropdown TMP_Dropdown = channelGUI.transform.Find("DataFieldDropdown").GetComponent<TMP_Dropdown>();
                string dataField = TMP_Dropdown.options[TMP_Dropdown.value].text;
                channelSpecs.Add("field", new JSONString(dataField));

                TMP_Dropdown = channelGUI.transform.Find("DataFieldTypeDropdown").GetComponent<TMP_Dropdown>();
                string dataFieldType = TMP_Dropdown.options[TMP_Dropdown.value].text;
                channelSpecs.Add("type", new JSONString(dataFieldType));

                TMP_Dropdown = channelGUI.transform.Find("ChannelDropdown").GetComponent<TMP_Dropdown>();
                string channel = TMP_Dropdown.options[TMP_Dropdown.value].text;
                encodingObject.Add(channel, channelSpecs);
            }

            guiVisSpecs["encoding"] = encodingObject;
            Debug.Log("GUI CHANNEL SPECS: " + guiVisSpecs["encoding"].ToString());

#if USE_INTERACTION_GUI
            // Update interaction specs:
            guiVisSpecs["interaction"] = null;
            JSONArray interactionArrayObject = new JSONArray();
            Transform interactionListContent = gameObject.transform.Find("InteractionList/Viewport/InteractionListContent");
            for (int i = 0; i < interactionListContent.childCount - 1; i++)
            {
                GameObject interactionGUI = interactionListContent.GetChild(i).gameObject;
                JSONObject interactionSpecs = new JSONObject();

                TMP_Dropdown TMP_Dropdown = interactionGUI.transform.Find("DataFieldDropdown").GetComponent<TMP_Dropdown>();
                string dataField = TMP_Dropdown.options[TMP_Dropdown.value].text;
                interactionSpecs.Add("field", new JSONString(dataField));

                TMP_Dropdown = interactionGUI.transform.Find("InteractionTypeDropdown").GetComponent<TMP_Dropdown>();
                string interactionType = TMP_Dropdown.options[TMP_Dropdown.value].text;
                interactionSpecs.Add("type", new JSONString(interactionType));

                interactionArrayObject.Add(interactionSpecs);
            }

            guiVisSpecs["interaction"] = interactionArrayObject;
            Debug.Log("GUI INTERACTION SPECS: " + guiVisSpecs["interaction"].ToString());
#endif
        }

        public JSONNode GetGUIVisSpecs()
        {
            return guiVisSpecs;
        }

        public void CallUpdateVisSpecsFromGUISpecs()
        {
            UpdateVisSpecsFromGUISpecs();
        }

        // TODO:
        public void OnChannelGUIChannelDropdownValueChanged(TMP_Dropdown changed)
        {
            Debug.Log("New data " + changed.options[changed.value].text);
            string prevValue = ""; // guiVisSpecs["data"]["url"].Value;
            string curValue = changed.options[changed.value].text;
            if (prevValue != curValue)
            {
                Debug.Log("Updated specs " + curValue);

//                UpdateGUIChannelsList(guiVisSpecs);
            }
        }

        // TODO:
        public void OnChannelGUIDataFieldDropdownValueChanged(TMP_Dropdown changed)
        {
            Debug.Log("New data " + changed.options[changed.value].text);
            string prevValue = ""; // guiVisSpecs["data"]["url"].Value;
            string curValue = changed.options[changed.value].text;
            if (prevValue != curValue)
            {
                Debug.Log("Updated specs " + curValue);

                //                UpdateGUIChannelsList(guiVisSpecs);
            }
        }

        public void OnInteractionGUIDataFieldDropdownValueChanged(TMP_Dropdown changed, GameObject interactionGUI)
        {
            Debug.Log("New data " + changed.options[changed.value].text);
            string prevValue = ""; // guiVisSpecs["data"]["url"].Value;
            string curValue = changed.options[changed.value].text;
            if (prevValue != curValue)
            {
                Debug.Log("Updated specs " + curValue);

                //                UpdateGUIChannelsList(guiVisSpecs);
            }

            Debug.Log("Object name " + interactionGUI.name);

            UpdateInteraction(interactionGUI);
        }

        private void UpdateInteraction(GameObject interactionGUI)
        {
            
        }

        public void OnInteractionGUIInteractionTypeDropdownValueChanged(TMP_Dropdown changed, GameObject interactionGUI)
        {
            Debug.Log("New data " + changed.options[changed.value].text);
            string prevValue = ""; // guiVisSpecs["data"]["url"].Value;
            string curValue = changed.options[changed.value].text;
            if (prevValue != curValue)
            {
                Debug.Log("Updated specs " + curValue);

                //                UpdateGUIChannelsList(guiVisSpecs);
            }

            Debug.Log("Object name " + interactionGUI.name);
        }

        // TODO:
        public void OnChannelGUIDataFieldTypeDropdownValueChanged(TMP_Dropdown changed)
        {
            Debug.Log("New data " + changed.options[changed.value].text);
            string prevValue = ""; // guiVisSpecs["data"]["url"].Value;
            string curValue = changed.options[changed.value].text;
            if (prevValue != curValue)
            {
                Debug.Log("Updated specs " + curValue);

                //                UpdateGUIChannelsList(guiVisSpecs);
            }
        }

        public void OnDataDropdownValueChanged(TMP_Dropdown changed)
        {
            Debug.Log("New data " + changed.options[changed.value].text);
            string prevValue = guiVisSpecs["data"]["url"].Value;
            string curValue = changed.options[changed.value].text;
            if (prevValue != curValue)
            {
                guiVisSpecs["data"]["url"] = curValue;

                // Keep channel field names if they exist in the data
                // and set to undefined if not, so user can use specs as template for new data.
                
                List<string> newDataFields = GetDataFieldsList();
                Transform channelListContent = gameObject.transform.Find("ChannelList/Viewport/ChannelListContent");
                for (int i = 0; i < channelListContent.childCount - 1; i++)
                {
                    GameObject channelGUI = channelListContent.GetChild(i).gameObject;
                    
                    TMP_Dropdown TMP_Dropdown = channelGUI.GetComponent<ChannelItemAction>().dropdowns[0].GetComponent<TMP_Dropdown>();
                    string channel = TMP_Dropdown.options[TMP_Dropdown.value].text;

                    if(!newDataFields.Contains(channel))
                    {
                        TMP_Dropdown = channelGUI.GetComponent<ChannelItemAction>().dropdowns[1].GetComponent<TMP_Dropdown>();
                        UpdateChannelGUIDataFieldDropdownValue("undefined", ref channelGUI);
                    }
                }

                Debug.Log("Updated specs " + guiVisSpecs["encoding"].ToString());

                UpdateGUIChannelsList(guiVisSpecs);
            }
        }

        public void OnMarkDropdownValueChanged(TMP_Dropdown changed)
        {
            Debug.Log("New mark " + changed.options[changed.value].text);
            string prevValue = guiVisSpecs["mark"].Value;
            string curValue = changed.options[changed.value].text;
            if (prevValue != curValue)
            {
                guiVisSpecs["mark"] = curValue;
/*
                // Reset channels!
                // TODO: Only reset parts of the spec.
                guiVisSpecs["encoding"] = null;

                Debug.Log("Updated specs " + guiVisSpecs["encoding"].ToString());
                */
                UpdateGUIChannelsList(guiVisSpecs);
            }
        }

        public void AddEmptyChannelGUICallback()
        {
            AddEmptyChannelGUI();
        }

        public void AddEmptyInteractionGUICallback()
        {
            AddEmptyInteractionGUI();
        }

        private GameObject AddEmptyChannelGUI()
        {
            Transform channelListContent = gameObject.transform.Find("ChannelList/Viewport/ChannelListContent");
            GameObject channelGUI = Instantiate(channelGUIPrefab, channelListContent);

            UpdateChannelsListOptions(ref channelGUI);
            UpdateDataFieldListOptions(ref channelGUI);
            UpdateDataFieldTypeOptions(ref channelGUI);

            AddChannelGUIChannelCallback(ref channelGUI);
            AddChannelGUIDataFieldCallback(ref channelGUI);
            AddChannelGUIDataFieldTypeCallback(ref channelGUI);
            AddChannelGUIDeleteCallback(ref channelGUI);

            addChannelButtonTransform.SetAsLastSibling();

            return channelGUI;
        }

        private GameObject AddEmptyInteractionGUI()
        {
            Transform interactionListContent = gameObject.transform.Find("InteractionList/Viewport/InteractionListContent");
            GameObject interactionGUI = Instantiate(interactionGUIPrefab, interactionListContent);

            UpdateDataFieldListOptions(ref interactionGUI);

            AddInteractionGUIDataFieldCallback(interactionGUI);
            AddInteractionGUIInteractionTypeCallback(interactionGUI);
            AddInteractionGUIDeleteCallback(ref interactionGUI);

            addInteractionButtonTransform.SetAsLastSibling();

            return interactionGUI;
        }

        private void AddChannelGUIDeleteCallback(ref GameObject channelGUI)
        {
            Transform deleteChannelObject = channelGUI.transform.Find("DeleteChannelButton");
            Interactable btn = deleteChannelObject.gameObject.GetComponent<Interactable>();
            btn.OnClick.AddListener(DeleteParentOfClickedObjectCallback);
        }

        private void AddInteractionGUIDeleteCallback(ref GameObject interactionGUI)
        {
            Transform deleteInteractionObject = interactionGUI.transform.Find("DeleteInteractionButton");
            Interactable btn = deleteInteractionObject.gameObject.GetComponent<Interactable>();
            btn.OnClick.AddListener(DeleteParentOfClickedObjectCallback);
        }

        private void AddChannelGUIChannelCallback(ref GameObject channelGUI)
        {
            Transform dropdownObject = channelGUI.GetComponent<ChannelItemAction>().dropdowns[0].transform;
            TMP_Dropdown TMP_Dropdown = dropdownObject.gameObject.GetComponent<TMP_Dropdown>();
            TMP_Dropdown.onValueChanged.AddListener(delegate {
                OnChannelGUIChannelDropdownValueChanged(TMP_Dropdown);
            });
        }

        private void AddChannelGUIDataFieldCallback(ref GameObject channelGUI)
        {
            Transform dropdownObject = channelGUI.GetComponent<ChannelItemAction>().dropdowns[1].transform;
            TMP_Dropdown TMP_Dropdown = dropdownObject.gameObject.GetComponent<TMP_Dropdown>();
            TMP_Dropdown.onValueChanged.AddListener(delegate {
                OnChannelGUIDataFieldDropdownValueChanged(TMP_Dropdown);
            });
        }

        private void AddInteractionGUIDataFieldCallback(GameObject interactionGUI)
        {
            Transform dropdownObject = interactionGUI.transform.Find("DataFieldDropdown");
            TMP_Dropdown TMP_Dropdown = dropdownObject.gameObject.GetComponent<TMP_Dropdown>();
            TMP_Dropdown.onValueChanged.AddListener(delegate {
                OnInteractionGUIDataFieldDropdownValueChanged(TMP_Dropdown, interactionGUI);
            });
        }

        private void AddInteractionGUIInteractionTypeCallback(GameObject interactionGUI)
        {
            Transform dropdownObject = interactionGUI.transform.Find("InteractionTypeDropdown");
            TMP_Dropdown TMP_Dropdown = dropdownObject.gameObject.GetComponent<TMP_Dropdown>();
            TMP_Dropdown.onValueChanged.AddListener(delegate {
                OnInteractionGUIInteractionTypeDropdownValueChanged(TMP_Dropdown, interactionGUI);
            });
        }

        private void AddChannelGUIDataFieldTypeCallback(ref GameObject channelGUI)
        {
            Transform dropdownObject = channelGUI.GetComponent<ChannelItemAction>().dropdowns[1].transform;
            TMP_Dropdown TMP_Dropdown = dropdownObject.gameObject.GetComponent<TMP_Dropdown>();
            TMP_Dropdown.onValueChanged.AddListener(delegate {
                OnChannelGUIDataFieldTypeDropdownValueChanged(TMP_Dropdown);
            });
        }

        private void UpdateChannelsListOptions(ref GameObject channelGUI)
        {
            TMP_Dropdown TMP_Dropdown = channelGUI.GetComponent<ChannelItemAction>().dropdowns[0].GetComponent<TMP_Dropdown>();
            TMP_Dropdown.ClearOptions();
            TMP_Dropdown.AddOptions(GetChannelDropdownOptions());
        }

        private void DeleteParentOfClickedObjectCallback()
        {
            // Debug.Log("Clicked " + EventSystem.current.currentSelectedGameObject.transform.parent.name);
            // //
            // GameObject.Destroy(EventSystem.current.currentSelectedGameObject.transform.parent.gameObject);
        }
        
        public List<string> GetChannelDropdownOptions()
        {
            return targetVis.GetChannelsList(markDropdown.options[markDropdown.value].text);
        }

        private void UpdateDataFieldListOptions(ref GameObject channelGUI)
        {
            TMP_Dropdown TMP_Dropdown = channelGUI.GetComponent<ChannelItemAction>().dropdowns[1].GetComponent<TMP_Dropdown>();
            TMP_Dropdown.ClearOptions();
            TMP_Dropdown.AddOptions(GetDataFieldDropdownOptions());
        }

        public List<string> GetDataFieldDropdownOptions()
        {
            List<string> fieldsListOptions = new List<string> { DxR.Vis.UNDEFINED };
            if(guiVisSpecs["data"]["url"].Value == "inline")
            {
                fieldsListOptions.AddRange(targetVis.GetDataFieldsListFromValues(guiVisSpecs["data"]["values"]));
            } else
            {
                fieldsListOptions.AddRange(targetVis.GetDataFieldsListFromURL(guiVisSpecs["data"]["url"].Value));
            }

            return fieldsListOptions;
        }

        public List<string> GetDataFieldsList()
        {
            if (guiVisSpecs["data"]["url"].Value == "inline")
            {
                return targetVis.GetDataFieldsListFromValues(guiVisSpecs["data"]["values"]);
            }
            else
            {
                return targetVis.GetDataFieldsListFromURL(guiVisSpecs["data"]["url"].Value);
            }
        }

        private void UpdateDataFieldTypeOptions(ref GameObject channelGUI)
        {
            TMP_Dropdown TMP_Dropdown = channelGUI.GetComponent<ChannelItemAction>().dropdowns[2].GetComponent<TMP_Dropdown>();
            TMP_Dropdown.ClearOptions();
            TMP_Dropdown.AddOptions(dataFieldTypeDropdownOptions);
        }

        public void UpdateGUIMarksDropdownList(List<string> marksList)
        {
            markDropdown.ClearOptions();
            markDropdown.AddOptions(marksList);
        }

        public void UpdateGUIDataDropdownList(List<string> dataList)
        {
            dataDropdown.ClearOptions();
            dataDropdown.AddOptions(dataList);
        }

        private int GetOptionIndex(TMP_Dropdown TMP_Dropdown, string value)
        {
            for (int i = 0; i < TMP_Dropdown.options.Count; i++)
            {
                if (TMP_Dropdown.options[i].text == value)
                {
                    return i;
                }
            }

            return -1;
        }

        public void UpdateDataDropdownValue(string value)
        {
            string prevValue = dataDropdown.options[dataDropdown.value].text;
            int valueIndex = GetOptionIndex(dataDropdown, value);
            if (valueIndex > 0)
            {
                dataDropdown.value = valueIndex;
            }

            Debug.Log("Updated GUI data value to " + value);
        }

        public void UpdateMarkDropdownValue(string value)
        {
            int valueIndex = GetOptionIndex(markDropdown, value);
            if (valueIndex > 0)
            {
                markDropdown.value = valueIndex;
            }
        }
    }
}
