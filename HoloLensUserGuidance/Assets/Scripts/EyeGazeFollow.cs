using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;

namespace HoloLensUserGuidance.EyeTracking.Logging
{
    [AddComponentMenu("Scripts/HoloLensUserGuidance/EyeGazeFollow")]
    public class EyeGazeFollow : MonoBehaviour
    {
        [SerializeField]
        private float defaultDistanceInMeters = 0.5f;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        public void SetIntoView()
        {
            var eyeGazeProvider = CoreServices.InputSystem?.EyeGazeProvider;
            if (eyeGazeProvider != null)
            {
                //gameObject.transform.localEulerAngles = new Vector3(0, 180, 0);
                gameObject.transform.position = eyeGazeProvider.GazeOrigin + eyeGazeProvider.GazeDirection.normalized * defaultDistanceInMeters;

                var dir = CameraCache.Main.transform.right * 0.15f;
                gameObject.transform.Translate(dir, Space.World);

                gameObject.transform.LookAt(CameraCache.Main.transform);


                //EyeTrackingTarget lookedAtEyeTarget = EyeTrackingTarget.LookedAtEyeTarget;

                //// Update GameObject to the current eye gaze position at a given distance
                //if (lookedAtEyeTarget != null)
                //{
                //    // Show the object at the center of the currently looked at target.
                //    if (lookedAtEyeTarget.EyeCursorSnapToTargetCenter)
                //    {
                //        Ray rayToCenter = new Ray(CameraCache.Main.transform.position, lookedAtEyeTarget.transform.position - CameraCache.Main.transform.position);
                //        RaycastHit hitInfo;
                //        UnityEngine.Physics.Raycast(rayToCenter, out hitInfo);
                //        gameObject.transform.position = hitInfo.point;
                //    }
                //    else
                //    {
                //        // Show the object at the hit position of the user's eye gaze ray with the target.
                //        gameObject.transform.position = eyeGazeProvider.GazeOrigin + eyeGazeProvider.GazeDirection.normalized * defaultDistanceInMeters;
                //    }
                //}
                //else
                //{
                //    // If no target is hit, show the object at a default distance along the gaze ray.
                //    gameObject.transform.position = eyeGazeProvider.GazeOrigin + eyeGazeProvider.GazeDirection.normalized * defaultDistanceInMeters;
                //}
            }
        }
    
    }
}

