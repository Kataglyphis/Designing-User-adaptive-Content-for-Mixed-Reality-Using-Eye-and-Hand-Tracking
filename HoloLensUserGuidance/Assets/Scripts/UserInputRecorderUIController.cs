// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using UnityEngine;

namespace HoloLensUserGuidance.EyeTracking.Logging
{
    [AddComponentMenu("Scripts/HoloLensUserGuidance/UserInputRecorderUIController")]
    public class UserInputRecorderUIController : MonoBehaviour
    {
        [SerializeField]
        private GameObject btn_StartRecording = null;

        [SerializeField]
        private GameObject btn_StopRecording = null;

        [SerializeField]
        private GameObject btn_StartPlayback_Inactive = null;

        [SerializeField]
        private GameObject btn_StartPlayback = null;

        [SerializeField]
        private GameObject btn_PausePlayback = null;

        public void Start()
        {
            RecordingUI_Reset(true);
            ReplayUI_SetActive(false);
        }

        #region Data recording
        public void StartRecording()
        {
            RecordingUI_Reset(false);
        }

        public void StopRecording()
        {
            RecordingUI_Reset(true);
        }

        private void RecordingUI_Reset(bool reset)
        {
            if (btn_StopRecording != null)
            {
                btn_StopRecording.SetActive(!reset);
            }

            if (btn_StartRecording != null)
            {
                btn_StartRecording.SetActive(reset);
            }
        }
        #endregion

        #region Data replay
        public void LoadData()
        {
            ReplayUI_SetActive(true);
        }

        private void ReplayUI_SetActive(bool active)
        {
            ResetPlayback(active, false);

            if (btn_StartPlayback_Inactive != null)
            {
                btn_StartPlayback_Inactive.SetActive(!active);
            }
        }

        public void StartReplay()
        {
            Debug.Log("StartReplay");
            ResetPlayback(false, true);
        }

        public void PauseReplay()
        {
            Debug.Log("PauseReplay");
            ResetPlayback(true, false);
        }

        private void ResetPlayback(bool showPlayBtn, bool showPauseBtn)
        {
            if (btn_StartPlayback != null)
            {
                btn_StartPlayback.SetActive(showPlayBtn);
            }

            if (btn_PausePlayback != null)
            {
                btn_PausePlayback.SetActive(showPauseBtn);
            }
        }
        #endregion
    }
}