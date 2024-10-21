using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DxR
{
    /// <summary>
    /// This is the class for point mark which enables setting of channel
    /// values which may involve calling custom scripts. The idea is that 
    /// in order to add a custom channel, the developer simply has to implement
    /// a function that takes in the "channel" name and value in string format
    /// and performs the necessary changes under the SetChannelValue function.
    /// </summary>

    public class MarkText : Mark
    {
        // Reference to the main camera for billboard effect
        // private Camera mainCamera;

        // public MarkText() : base()
        // {
        //     // Find and cache the main camera
        //     mainCamera = Camera.main;
        // }

        public override void SetChannelValue(string channel, string value)
        {
            switch (channel)
            {
                case "text":
                    SetText(value);
                    break;
                case "size":
                    SetFontSize(value);
                    break;
                case "color":
                    SetFontColor(value);
                    break;
                case "anchor":
                    SetAnchor(value);
                    break;
                case "following":
                    SetFollowing(value);
                    break;
                default:
                    base.SetChannelValue(channel, value);
                    break;
            }
        }

        // Replaces the HoloToolkit's Billboard with a custom following logic
        private void SetFollowing(string value)
        {
            bool shouldFollow = (value == "True");

            // Enable or disable a custom billboard effect based on input
            if (shouldFollow)
            {
                // This will make the object face the camera in the Update method
                StartFollowing();
            }
            else
            {
                StopFollowing();
            }
        }

        private void StartFollowing()
        {
            // Start following the camera
            if (!enabled)
                enabled = true;
        }

        private void StopFollowing()
        {
            // Stop following the camera
            if (enabled)
                enabled = false;
        }

        private void Update()
        {
            // If following is enabled, rotate the object to face the camera
            if (enabled && Camera.main != null)
            {
                Vector3 directionToCamera = Camera.main.transform.position - transform.position;
                directionToCamera.y = 0; // Optionally ignore the Y axis for 2D-like behavior
                transform.rotation = Quaternion.LookRotation(-directionToCamera);
            }
        }

        private void SetText(string value)
        {
            gameObject.GetComponent<TextMesh>().text = value;
        }

        private void SetFontSize(string value)
        {
            gameObject.GetComponent<TextMesh>().fontSize = int.Parse(value);
        }

        private void SetFontColor(string value)
        {
            Color color;
            bool colorParsed = ColorUtility.TryParseHtmlString(value, out color);
            gameObject.GetComponent<TextMesh>().color = color;
        }

        private void SetAnchor(string value)
        {
            TextAnchor anchor = TextAnchor.MiddleCenter;
            switch (value)
            {
                case "upperleft":
                    anchor = TextAnchor.UpperLeft;
                    break;
                case "uppercenter":
                    anchor = TextAnchor.UpperCenter;
                    break;
                case "upperright":
                    anchor = TextAnchor.UpperRight;
                    break;
                case "middleleft":
                    anchor = TextAnchor.MiddleLeft;
                    break;
                case "middlecenter":
                    anchor = TextAnchor.MiddleCenter;
                    break;
                case "middleright":
                    anchor = TextAnchor.MiddleRight;
                    break;
                case "lowerleft":
                    anchor = TextAnchor.LowerLeft;
                    break;
                case "lowercenter":
                    anchor = TextAnchor.LowerCenter;
                    break;
                case "lowerright":
                    anchor = TextAnchor.LowerRight;
                    break;
                default:
                    anchor = TextAnchor.MiddleCenter;
                    break;
            }

            gameObject.GetComponent<TextMesh>().anchor = anchor;
        }
    }
}
