// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Assets.Scripts.Data.Samples;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Utilities;

namespace Assets.Scripts.Data
{
    public class HandData : UserDataCluster
    {
        public override List<UserTrackingSamplePoint> UserTrackingSamplePoints
        {
            get
            {
                return new List<UserTrackingSamplePoint>() {

                                        new UserTrackingSamplePoint3D(  "RightWristPosition",
                                                                        "world space position of the right wrist"),
                                        new UserTrackingSamplePoint3D(  "RightWristUp",
                                                                        "Up vector of the right wrist in world space"),
                                        new UserTrackingSamplePoint3D(  "RightWristRight",
                                                                        "Right vector of the right wrist in world space"),
                                        new UserTrackingSamplePoint4D(  "RightWristRotation",
                                                                        "Quaternion rotation of the right wrist"),
                                        new UserTrackingSamplePoint3D(  "LeftWristPosition",
                                                                        "Position vector of the left wrist in world space"),
                                        new UserTrackingSamplePoint3D(  "LeftWristUp",
                                                                        "Up vector of the left wrist in world space"),
                                        new UserTrackingSamplePoint3D(  "LeftWristRight",
                                                                        "Right vector of the left wrist in world space"),
                                        new UserTrackingSamplePoint4D(  "LeftWristRotation",
                                                                        "Quaternion rotation of the left wrist"),
                                        new UserTrackingSamplePoint3D(  "RightIndexTipPosition",
                                                                        "Position vector of the right index tip in world space"),
                                        new UserTrackingSamplePoint3D(  "RightIndexTipUp",
                                                                        "Up vector of the right index tip in world space"),
                                        new UserTrackingSamplePoint3D(  "RightIndexTipRight",
                                                                        "Right vector of the right index tip in world space"),
                                        new UserTrackingSamplePoint4D(  "RightIndexTipRotation",
                                                                        "Quaternion rotation of the right index tip"),
                                        new UserTrackingSamplePoint3D(  "LeftIndexTipPosition",
                                                                        "Position vector of the left index tip in world space"),
                                        new UserTrackingSamplePoint3D(  "LeftIndexTipUp",
                                                                        "Up vector of the left index tip in world space"),
                                        new UserTrackingSamplePoint3D(  "LeftIndexTipRight",
                                                                        "Right vector of the left index tip in world space"),
                                        new UserTrackingSamplePoint4D(  "LeftIndexTipRotation",
                                                                        "Quaternion rotation of the left index tip"),};
            }
        }

        protected override List<object> RawData
        {
            get
            {
                if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Right, out MixedRealityPose rightIndexTipPose))
                {

                }

                if (HandJointUtils.TryGetJointPose(TrackedHandJoint.IndexTip, Handedness.Left, out MixedRealityPose leftIndexTipPose))
                {

                }

                if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, Handedness.Right, out MixedRealityPose rightWristPose))
                {

                }

                if (HandJointUtils.TryGetJointPose(TrackedHandJoint.Wrist, Handedness.Left, out MixedRealityPose leftWristPose))
                {

                }

                return new List<object>()
                    {

                        rightIndexTipPose.Position.x,
                        rightIndexTipPose.Position.y,
                        rightIndexTipPose.Position.z,
                        rightIndexTipPose.Up.x,
                        rightIndexTipPose.Up.y,
                        rightIndexTipPose.Up.z,
                        rightIndexTipPose.Right.x,
                        rightIndexTipPose.Right.y,
                        rightIndexTipPose.Right.z,
                        rightIndexTipPose.Rotation.x,
                        rightIndexTipPose.Rotation.y,
                        rightIndexTipPose.Rotation.z,
                        rightIndexTipPose.Rotation.w,

                        leftIndexTipPose.Position.x,
                        leftIndexTipPose.Position.y,
                        leftIndexTipPose.Position.z,
                        leftIndexTipPose.Up.x,
                        leftIndexTipPose.Up.y,
                        leftIndexTipPose.Up.z,
                        leftIndexTipPose.Right.x,
                        leftIndexTipPose.Right.y,
                        leftIndexTipPose.Right.z,
                        leftIndexTipPose.Rotation.x,
                        leftIndexTipPose.Rotation.y,
                        leftIndexTipPose.Rotation.z,
                        leftIndexTipPose.Rotation.w,

                        rightWristPose.Position.x,
                        rightWristPose.Position.y,
                        rightWristPose.Position.z,
                        rightWristPose.Up.x,
                        rightWristPose.Up.y,
                        rightWristPose.Up.z,
                        rightWristPose.Right.x,
                        rightWristPose.Right.y,
                        rightWristPose.Right.z,
                        rightWristPose.Rotation.x,
                        rightWristPose.Rotation.y,
                        rightWristPose.Rotation.z,
                        rightWristPose.Rotation.w,

                        leftWristPose.Position.x,
                        leftWristPose.Position.y,
                        leftWristPose.Position.z,
                        leftWristPose.Up.x,
                        leftWristPose.Up.y,
                        leftWristPose.Up.z,
                        leftWristPose.Right.x,
                        leftWristPose.Right.y,
                        leftWristPose.Right.z,
                        leftWristPose.Rotation.x,
                        leftWristPose.Rotation.y,
                        leftWristPose.Rotation.z,
                        leftWristPose.Rotation.w,

                    };
            }

        }
    }
}
