using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;

namespace HoloLensUserGuidance.EyeTracking.Logging
{
    [AddComponentMenu("Scripts/HoloLensUserGuidance/EyeHelperPoint")]
    public class EyeHelperPoint : MonoBehaviour
    {
        private IMixedRealityEyeGazeProvider EyeTrackingProvider =>
                            eyeTrackingProvider ?? (eyeTrackingProvider = CoreServices.InputSystem?.EyeGazeProvider);
        private IMixedRealityEyeGazeProvider eyeTrackingProvider = null;

        // Start is called before the first frame update
        void Start()
        {

        }

        public void Update_Pos(Vector3 world_position)
        {
            gameObject.transform.position = world_position;
        }

       

    }
}
