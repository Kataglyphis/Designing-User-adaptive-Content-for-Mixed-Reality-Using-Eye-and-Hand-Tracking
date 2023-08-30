// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Data;
using SharedResultsBetweenServerAndHoloLens;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.WebRTC;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Assets.Scripts;
using System.Text;
using Microsoft.MixedReality.Toolkit.Utilities;
using Microsoft.MixedReality.Toolkit;
using UnityEngine.XR;
using System.Collections;

namespace HoloLensUserGuidance.EyeTracking.Logging
{
    [AddComponentMenu("Scripts/HoloLensUserGuidance/LogStructureEyeGaze")]
    public class LogStructureEyeGaze : LogStructure
    {

        [SerializeField]
        private Microsoft.MixedReality.WebRTC.Unity.PeerConnection peerConnection = null;

        public GameObject FirstIntention;
        public GameObject SecondIntention;
        public GameObject ThirdIntention;

        public GameObject FirstIntentionImage;
        public GameObject SecondIntentionImage;
        public GameObject ThirdIntentionImage;

        public GameObject EyeHitPointVis;
        public GameObject LeftIndexPointVis;
        public GameObject RightIndexPointVis;

        private DataChannel _detectionResultsChannel = null;
        private DataChannel _userDataChannel = null;

        private DataChannel[] _segmentationResultsChannel = new DataChannel[4];
        private DataChannel[] _masksChannel = new DataChannel[4];

        private BasicData _basicData = new BasicData();

        // List of all data we want to track from the user
        private List<ReadOnlyDataCluster> _userData = new List<ReadOnlyDataCluster>() { new TransformData(),
                                                                                        new EyeData(),
                                                                                        new HandData(),
                                                                                        new HeadData(),};
        // List of all data from our NN nets
        private List<NNDataCluster> _nnData = new List<NNDataCluster>() {
                                        new DetectionResultCSVDescription(CSVFileHelper.MaxResults),
                                        new InstanceSegmentationResultCSVDescription(CSVFileHelper.MaxResults)};

        private string[] lastSegmentationResults = {string.Empty,
                                                    string.Empty,
                                                    string.Empty,
                                                    string.Empty};

        private string[] lastMaskResults = {string.Empty,
                                            string.Empty,
                                            string.Empty,
                                            string.Empty};

        private string lastDetectionResults = ";;;;;;;;;;;";

        public string _userIntention = "NoIntention";
        public string[] intentions = new string[] { "NoIntention", "FirstIntention", "SecondIntention", "ThirdIntention" };
        public float updateTimer = 10.0f;

        private bool[] segmentationResultsRetrieved = new bool[] { false, false, false, false };
        private bool[] maskResultsRetrieved = new bool[] { false, false, false, false };

        List<string> intentionsInTimeFrame;

        private void Start()
        {

            intentionsInTimeFrame = new List<string>() { "NoIntention" };

            FirstIntention = GameObject.Find("FirstIntention");
            SecondIntention = GameObject.Find("SecondIntention");
            ThirdIntention = GameObject.Find("ThirdIntention");

            FirstIntentionImage = GameObject.Find("FirstIntentionImage");
            SecondIntentionImage = GameObject.Find("SecondIntentionImage");
            ThirdIntentionImage = GameObject.Find("ThirdIntentionImage");

            EyeHitPointVis = GameObject.Find("EyeFollowingPoint");
            LeftIndexPointVis = GameObject.Find("LeftIndexTipFollowingPoint");
            RightIndexPointVis = GameObject.Find("RightIndexTipFollowingPoint");

            // detection results
            peerConnection.OnInitialized.AddListener(OnInitialized);

            // save data format to .json for using it in our python program
            string JsonDirectory = Directory.GetCurrentDirectory();
            string BasicDataJson = JsonConvert.SerializeObject(_basicData, Formatting.Indented);
            string UserDataJson = JsonConvert.SerializeObject(_userData, Formatting.Indented);
            string NNDataJson = JsonConvert.SerializeObject(_nnData, Formatting.Indented);
            FileHelper.WriteToFile(JsonDirectory, "UserData.json", UserDataJson);
            FileHelper.WriteToFile(JsonDirectory, "NNData.json", NNDataJson);
            FileHelper.WriteToFile(JsonDirectory, "BasicData.json", BasicDataJson);

            FirstIntention.SetActive(false);
            SecondIntention.SetActive(false);
            ThirdIntention.SetActive(false);

            FirstIntentionImage.SetActive(false);
            SecondIntentionImage.SetActive(false);
            ThirdIntentionImage.SetActive(false);

            //LeftIndexPointVis.SetActive(false);
            //RightIndexPointVis.SetActive(false);
        }

        private async void OnInitialized()
        {

            //LeftIndexPointVis.SetActive(true);
            //RightIndexPointVis.SetActive(true);

            Debug.Log("Reached OnInitialized()");

            _detectionResultsChannel = 
                await peerConnection.Peer.AddDataChannelAsync(ResultExchangeDataChannelConstants.DataChannelLabelDetection, true, true);
            if (_detectionResultsChannel != null) { Debug.Log("DataChannel creation successfull"); }
            _detectionResultsChannel.MessageReceived += (byte[] message) =>
            {
                if ((message != null) && message.Length > 0) { 
                    try
                    {
                        _nnData[0].Data = new List<object>() { Encoding.UTF8.GetString(message) };
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"Error in deserializing results: {e.Message}");
                    }
                }
            };

            _userDataChannel =
                            await peerConnection.Peer.AddDataChannelAsync(ResultExchangeDataChannelConstants.DataChannelUserData, true, true);
            if (_userDataChannel != null) { Debug.Log("DataChannel creation successfull"); }
            _userDataChannel.MessageReceived += (byte[] message) =>
            {
                if ((message != null) && message.Length > 0)
                {
                    try
                    {
                        _userIntention = Encoding.UTF8.GetString(message);
                        intentionsInTimeFrame.Add(_userIntention);
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"Error in deserializing results: {e.Message}");
                    }
                }
            };

            _segmentationResultsChannel[0] =
                await peerConnection.Peer.AddDataChannelAsync(ResultExchangeDataChannelConstants.DataChannelLabelsSegmentation[0], true, true);
            if (_segmentationResultsChannel[0] != null) { Debug.Log("DataChannel creation successfull"); }
            _segmentationResultsChannel[0].MessageReceived += (byte[] message) =>
            {
                if ((message != null) && message.Length > 0)
                {
                    try
                    {

                        if (!segmentationResultsRetrieved[0])
                        {
                            segmentationResultsRetrieved[0] = true;
                            Debug.Log($"Byte message: {message}");
                            lastSegmentationResults[0] = Encoding.UTF8.GetString(message); //System.Convert.ToBase64String((message));//
                                                                                            //Debug.Log($"String message: {lastSegmentationResults}");
                        }
                        //_nnData[1].Data = new List<object>() { Encoding.UTF8.GetString(message) };
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"Error in deserializing results: {e.Message}");
                    }
                }
            };

            _segmentationResultsChannel[1] =
                await peerConnection.Peer.AddDataChannelAsync(ResultExchangeDataChannelConstants.DataChannelLabelsSegmentation[1], true, true);
            if (_segmentationResultsChannel[1] != null) { Debug.Log("DataChannel creation successfull"); }
            _segmentationResultsChannel[1].MessageReceived += (byte[] message) =>
            {
                if ((message != null) && message.Length > 0)
                {
                    try
                    {
                        
                        if (!segmentationResultsRetrieved[1])
                        {
                            segmentationResultsRetrieved[1] = true;
                            Debug.Log($"Byte message: {message}");
                            lastSegmentationResults[1] = Encoding.UTF8.GetString(message); //System.Convert.ToBase64String((message));//
                                                                                           //Debug.Log($"String message: {lastSegmentationResults}");
                        }
                        //_nnData[1].Data = new List<object>() { Encoding.UTF8.GetString(message) };
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"Error in deserializing results: {e.Message}");
                    }
                }
            };

            _segmentationResultsChannel[2] =
                await peerConnection.Peer.AddDataChannelAsync(ResultExchangeDataChannelConstants.DataChannelLabelsSegmentation[2], true, true);
            if (_segmentationResultsChannel[2] != null) { Debug.Log("DataChannel creation successfull"); }
            _segmentationResultsChannel[2].MessageReceived += (byte[] message) =>
            {
                if ((message != null) && message.Length > 0)
                {
                    try
                    {

                        if (!segmentationResultsRetrieved[2])
                        {
                            segmentationResultsRetrieved[2] = true;
                            Debug.Log($"Byte message: {message}");
                            lastSegmentationResults[2] = Encoding.UTF8.GetString(message); //System.Convert.ToBase64String((message));//
                                                                                           //Debug.Log($"String message: {lastSegmentationResults}");
                        }
                        //_nnData[1].Data = new List<object>() { Encoding.UTF8.GetString(message) };
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"Error in deserializing results: {e.Message}");
                    }
                }
            };

            _segmentationResultsChannel[3] =
                await peerConnection.Peer.AddDataChannelAsync(ResultExchangeDataChannelConstants.DataChannelLabelsSegmentation[3], true, true);
            if (_segmentationResultsChannel[3] != null) { Debug.Log("DataChannel creation successfull"); }
            _segmentationResultsChannel[3].MessageReceived += (byte[] message) =>
            {
                if ((message != null) && message.Length > 0)
                {
                    try
                    {
                        
                        if (!segmentationResultsRetrieved[3])
                        {
                            segmentationResultsRetrieved[3] = true;
                            Debug.Log($"Byte message: {message}");
                            lastSegmentationResults[3] = Encoding.UTF8.GetString(message); //System.Convert.ToBase64String((message));//
                                                                                           //Debug.Log($"String message: {lastSegmentationResults}");
                        }
                        //_nnData[1].Data = new List<object>() { Encoding.UTF8.GetString(message) };
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"Error in deserializing results: {e.Message}");
                    }
                }
            };

            _masksChannel[0] =
                await peerConnection.Peer.AddDataChannelAsync(ResultExchangeDataChannelConstants.DataChannelLabelsMask[0], true, true);
            if (_masksChannel[0] != null) { Debug.Log("DataChannel creation successfull"); }
            _masksChannel[0].MessageReceived += (byte[] message) =>
            {
                if ((message != null) && message.Length > 0)
                {
                    try
                    {

                        if (!maskResultsRetrieved[0])
                        {
                            maskResultsRetrieved[0] = true;
                            Debug.Log($"Byte message: {message}");
                            lastMaskResults[0] = System.Convert.ToBase64String((message));//
                                                                                            //Debug.Log($"String message: {lastSegmentationResults}");
                        }
                        //_nnData[1].Data = new List<object>() { Encoding.UTF8.GetString(message) };
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"Error in deserializing results: {e.Message}");
                    }
                }
            };

            _masksChannel[1] =
                await peerConnection.Peer.AddDataChannelAsync(ResultExchangeDataChannelConstants.DataChannelLabelsMask[1], true, true);
            if (_masksChannel[1] != null) { Debug.Log("DataChannel creation successfull"); }
            _masksChannel[1].MessageReceived += (byte[] message) =>
            {
                if ((message != null) && message.Length > 0)
                {
                    try
                    {
                        
                        if (!maskResultsRetrieved[1])
                        {
                            maskResultsRetrieved[1] = true;
                            Debug.Log($"Byte message: {message}");
                            lastMaskResults[1] = System.Convert.ToBase64String((message));//
                                                                                          //Debug.Log($"String message: {lastSegmentationResults}");
                        }
                        //_nnData[1].Data = new List<object>() { Encoding.UTF8.GetString(message) };
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"Error in deserializing results: {e.Message}");
                    }
                }
            };

            _masksChannel[2] =
                await peerConnection.Peer.AddDataChannelAsync(ResultExchangeDataChannelConstants.DataChannelLabelsMask[2], true, true);
            if (_masksChannel[2] != null) { Debug.Log("DataChannel creation successfull"); }
            _masksChannel[2].MessageReceived += (byte[] message) =>
            {
                if ((message != null) && message.Length > 0)
                {
                    try
                    {
                        
                        if (!maskResultsRetrieved[2])
                        {
                            maskResultsRetrieved[2] = true;
                            Debug.Log($"Byte message: {message}");
                            lastMaskResults[2] = System.Convert.ToBase64String((message));//
                                                                                          //Debug.Log($"String message: {lastSegmentationResults}");
                        }
                        //_nnData[1].Data = new List<object>() { Encoding.UTF8.GetString(message) };
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"Error in deserializing results: {e.Message}");
                    }
                }
            };

            _masksChannel[3] =
                await peerConnection.Peer.AddDataChannelAsync(ResultExchangeDataChannelConstants.DataChannelLabelsMask[3], true, true);
            if (_masksChannel[3] != null) { Debug.Log("DataChannel creation successfull"); }
            _masksChannel[3].MessageReceived += (byte[] message) =>
            {
                if ((message != null) && message.Length > 0)
                {
                    try
                    {
                        
                        if (!maskResultsRetrieved[3])
                        {
                            maskResultsRetrieved[3] = true;
                            Debug.Log($"Byte message: {message}");
                            lastMaskResults[3] = System.Convert.ToBase64String((message));//
                                                                                          //Debug.Log($"String message: {lastSegmentationResults}");
                        }
                        //_nnData[1].Data = new List<object>() { Encoding.UTF8.GetString(message) };
                    }
                    catch (Exception e)
                    {
                        Debug.Log($"Error in deserializing results: {e.Message}");
                    }
                }
            };

        }

        private void Update()
        {

            UserSamplePoint _userSamplePoint = new UserSamplePoint();

            var MVP = CameraCache.Main.projectionMatrix * CameraCache.Main.worldToCameraMatrix;

            var eyeGazeData = _userData[1].Data;
            var handData = _userData[2].Data;
            var headData = _userData[3].Data;


            var eyeHitPosWorld = new Vector4((float)eyeGazeData[7],
                                             (float)eyeGazeData[8],
                                             (float)eyeGazeData[9],
                                             1.0f);

            if (!float.IsNaN((float)eyeGazeData[7]) &&
               !float.IsNaN((float)eyeGazeData[8]) &&
               !float.IsNaN((float)eyeGazeData[9]))
            {
                EyeHelperPoint EyeHelperPointScript = (EyeHelperPoint)EyeHitPointVis.GetComponent<EyeHelperPoint>();
                EyeHelperPointScript.Update_Pos(new Vector3((float)eyeGazeData[7],
                                                            (float)eyeGazeData[8],
                                                            (float)eyeGazeData[9]));
            }

            var rightIndexTipPosWorld = new Vector4((float)handData[0],
                                                    (float)handData[1],
                                                    (float)handData[2],
                                                    1.0f);

            var leftIndexTipPosWorld = new Vector4((float)handData[13],
                                                   (float)handData[14],
                                                   (float)handData[15],
                                                   1.0f);

            var eyeHitPosScreenSpace = MVP * eyeHitPosWorld;
            var rightIndexTipPosScreenSpace = MVP * rightIndexTipPosWorld;
            var leftIndexTipPosScreenSpace = MVP * leftIndexTipPosWorld;

            _userSamplePoint.rightIndexTipPosScreenSpace = new List<float> { Mathf.Clamp(rightIndexTipPosScreenSpace[0], -1.0f, 1.0f),
                                                                             Mathf.Clamp(rightIndexTipPosScreenSpace[1], -1.0f, 1.0f)};

            _userSamplePoint.rightIndexTipPosScreenSpace = new List<float> { Mathf.Clamp(leftIndexTipPosScreenSpace[0], -1.0f, 1.0f),
                                                                             Mathf.Clamp(leftIndexTipPosScreenSpace[1], -1.0f, 1.0f)};

            _userSamplePoint.eyeHitposScreenSpace = new List<float> { Mathf.Clamp(eyeHitPosScreenSpace[0], -1.0f, 1.0f),
                                                                      Mathf.Clamp(eyeHitPosScreenSpace[1], -1.0f, 1.0f)};

            _userSamplePoint.headMovement = new List<float> { (float)headData[9],
                                                              (float)headData[10],
                                                              (float)headData[11]};

            _userSamplePoint.headVelocity = new List<float> { (float)headData[12],
                                                              (float)headData[13],
                                                              (float)headData[14]};


            // https://stackoverflow.com/questions/4635769/how-do-i-convert-an-array-of-floats-to-a-byte-and-back
            float[] userData = _userSamplePoint.getUserDataAsArrayPresentation();
            // create a byte array and copy the floats into it...
            var userDataByteArray = new byte[userData.Length * 4];
            Buffer.BlockCopy(userData, 0, userDataByteArray, 0, userDataByteArray.Length);

            if (_userDataChannel != null)
            {
                if (_userDataChannel.State == DataChannel.ChannelState.Open
                    && userDataByteArray != null)
                    _userDataChannel.SendMessage(userDataByteArray);
            }

            updateTimer -= Time.deltaTime;

            if (updateTimer <= 0.0f)
            {
                var groups =
                    from s in intentionsInTimeFrame
                    group s by s into g
                    select new
                    {
                        intention = g.Key,
                        Count = g.Count()
                    };

                var sortedDict = from entry in groups orderby entry.Count ascending select entry;
                string mostProbableIntention = sortedDict.First().intention;

                if (mostProbableIntention == intentions[0])
                {
                    FirstIntention.SetActive(false);
                    SecondIntention.SetActive(false);
                    ThirdIntention.SetActive(false);

                    FirstIntentionImage.SetActive(false);
                    SecondIntentionImage.SetActive(false);
                    ThirdIntentionImage.SetActive(false);

                }
                else if (mostProbableIntention == intentions[1])
                {
                    FirstIntention.SetActive(true);
                    EyeGazeFollow FirstIntentionScript = (EyeGazeFollow) FirstIntention.GetComponent<EyeGazeFollow>();
                    FirstIntentionScript.SetIntoView();
                    SecondIntention.SetActive(false);
                    ThirdIntention.SetActive(false);

                    FirstIntentionImage.SetActive(true);
                    EyeGazeFollow FirstIntentionImageScript = (EyeGazeFollow)FirstIntentionImage.GetComponent<EyeGazeFollow>();
                    FirstIntentionImageScript.SetIntoView();
                    SecondIntentionImage.SetActive(false);
                    ThirdIntentionImage.SetActive(false);

                }
                else if (mostProbableIntention == intentions[2])
                {

                    FirstIntention.SetActive(false);
                    SecondIntention.SetActive(true);
                    EyeGazeFollow SecondIntentionScript = (EyeGazeFollow)SecondIntention.GetComponent<EyeGazeFollow>();
                    SecondIntentionScript.SetIntoView();
                    ThirdIntention.SetActive(false);

                    FirstIntentionImage.SetActive(false);
                    SecondIntentionImage.SetActive(true);
                    EyeGazeFollow SecondIntentionImageScript = (EyeGazeFollow)SecondIntentionImage.GetComponent<EyeGazeFollow>();
                    SecondIntentionImageScript.SetIntoView();
                    ThirdIntentionImage.SetActive(false);

                }
                else if (mostProbableIntention == intentions[3])
                {

                    FirstIntention.SetActive(false);
                    SecondIntention.SetActive(false);
                    ThirdIntention.SetActive(true);
                    EyeGazeFollow ThirdIntentionScript = (EyeGazeFollow)ThirdIntention.GetComponent<EyeGazeFollow>();
                    ThirdIntentionScript.SetIntoView();

                    FirstIntentionImage.SetActive(false);
                    SecondIntentionImage.SetActive(false);
                    ThirdIntentionImage.SetActive(true);
                    EyeGazeFollow ThirdIntentionImageScript = (EyeGazeFollow)ThirdIntentionImage.GetComponent<EyeGazeFollow>();
                    ThirdIntentionImageScript.SetIntoView();

                }

                intentionsInTimeFrame = new List<string>();
                updateTimer = 10.0f;

            }




            }

        public override string[] GetHeaderColumns()
        {

            //string[] basics = new string[] {    
            //                                    // UserId
            //                                    "UserId",
            //                                    // SessionType
            //                                    "SessionType",
            //                                    // Timestamp
            //                                    "dt in ms", };

            string[] basics = _basicData.Labels.ToArray();

            List<string> result = new List<string>();
            result.AddRange(basics);
            for (int i = 0; i < _userData.Count; i++)
            {
                result.AddRange(_userData[i].Labels);
            }

            for (int i = 0; i < _nnData.Count; i++)
            {
                result.AddRange(_nnData[i].Labels);
            }

            return result.ToArray();

            //Debug.Assert(basics.All(basic => !basic.Contains(CSVFileHelper.CsvSeparator)));

            //return basics.Concat(_userData.SelectMany(x => x.Labels))
            //             .Concat(_nnData.SelectMany(x => x.Labels))
            //             .ToArray();

        }
        public override string[] GetDescriptionColumns()
        {
            //string[] basics = new string[] {    
            //                                    // UserId
            //                                    "Which user has HoloLens",
            //                                    // SessionType
            //                                    "f.e. which intention is tracked",
            //                                    // Timestamp
            //                                    "duration of sampling", };

            string[] basics = _basicData.Description.ToArray();
            Debug.Assert(basics.All(basic => !basic.Contains(CSVFileHelper.CsvSeparator)));

            List<string> result = new List<string>();
            result.AddRange(basics);
            for (int i = 0; i < _userData.Count; i++)
            {
                result.AddRange(_userData[i].Description);
            }

            for (int i = 0; i < _nnData.Count; i++)
            {
                result.AddRange(_nnData[i].Description);
            }

            return result.ToArray();

            //return basics.Concat(_userData.SelectMany(x => x.Description))
            //             .Concat(_nnData.SelectMany(x => x.Description))
            //             .ToArray();
        }

        public override object[] GetData(string inputType, string inputStatus, EyeTrackingTarget intTarget)
        {

            List<object> result = new List<object>();
            for(int i = 0; i < _userData.Count; i++)
            {
                result.AddRange(_userData[i].Data);
            }
            result.Add(lastDetectionResults);

            for(int i = 0; i < lastSegmentationResults.Length; i++)
            {
                if((lastSegmentationResults[i] != string.Empty)
                    && lastMaskResults[i] != string.Empty)
                {
                    result.Add(lastSegmentationResults[i]);
                    result.Add(lastMaskResults[i].Substring(0, CSVFileHelper.MaxCharacterPerCSVCell));
                    //result.Add(CSVFileHelper.CsvSeparator);
                    result.Add(lastMaskResults[i].Substring(CSVFileHelper.MaxCharacterPerCSVCell));
                    //if (i == 3) result.Add(CSVFileHelper.CsvSeparator);
                    lastSegmentationResults[i] = string.Empty;
                    lastMaskResults[i] = string.Empty;
                }

                for (int m = 0; m < maskResultsRetrieved.Length; m++)
                {
                    maskResultsRetrieved[i] = false;
                    segmentationResultsRetrieved[i] = false;
                }


            }

            return result.ToArray();
            //return _userData.SelectMany(x => x.Data)
            //                .Concat(_nnData.SelectMany(x => x.Data))
            //                .ToArray();

        }

    }
}
