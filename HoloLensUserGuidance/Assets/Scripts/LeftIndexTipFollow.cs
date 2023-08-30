using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;

namespace HoloLensUserGuidance.EyeTracking.Logging
{
    [AddComponentMenu("Scripts/HoloLensUserGuidance/LeftIndexTipFollow")]
    public class LeftIndexTipFollow : MonoBehaviour
    {

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Left, out MixedRealityPose leftIndexTipPose))
            {
                gameObject.transform.position = new Vector3(leftIndexTipPose.Position.x,
                                                            leftIndexTipPose.Position.y,
                                                            leftIndexTipPose.Position.z);
            }

        }

    }
}

