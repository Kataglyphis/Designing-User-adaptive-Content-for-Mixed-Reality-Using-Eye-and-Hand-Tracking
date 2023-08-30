using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;

namespace HoloLensUserGuidance.EyeTracking.Logging
{
    [AddComponentMenu("Scripts/HoloLensUserGuidance/RightIndexTipFollow")]
    public class RightIndexTipFollow : MonoBehaviour
    {

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Right, out MixedRealityPose rightIndexTipPose))
            {
                gameObject.transform.position = new Vector3(rightIndexTipPose.Position.x,
                                                            rightIndexTipPose.Position.y,
                                                            rightIndexTipPose.Position.z);
            }
            
        }

    }
}

