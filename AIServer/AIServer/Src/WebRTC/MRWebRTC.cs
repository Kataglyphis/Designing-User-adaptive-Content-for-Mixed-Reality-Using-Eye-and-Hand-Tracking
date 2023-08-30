// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using AIServer.Datasets;
using AIServer.Src;
using AIServer.Src.UserGuidance;
using AIServer.Video;
using Microsoft.MixedReality.WebRTC;
using Newtonsoft.Json;
using SharedResultsBetweenServerAndHoloLens;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.Capture;
using Windows.Media.Core;
using Windows.Media.MediaProperties;
using Windows.UI;
using static TorchSharp.torch;
using System.Collections;
using Windows.Storage;

namespace AIServer.WebRTC
{
    public class MRWebRTC : YoloResultsCreator
    {

        private PeerConnection _peerConnection;

        private NodeDssSignaler _signaler;

        private object _remoteVideoLock = new object();
        private bool _remoteVideoPlaying = false;
        private MediaStreamSource _remoteVideoSource;
        private VideoBridge _remoteVideoBridge = new VideoBridge(5);
        private RemoteVideoTrack _remoteVideoTrack;

        private DataChannel _detectionResultsChannel;

        private DataChannel[] _instanceSegmentationResultsChannel = new DataChannel[4];

        private DataChannel[] _instanceSegmentationMaskChannel = new DataChannel[4] ;

        private DataChannel _userDataChannel;

        VideoFrame _outputVideoFrameObjectDetection;
        VideoFrame _outputVideoFrameInstanceSegmentation;

        private Yolov5Seg _yoloModelInstanceSegmentation;
        private YoloObjectDetection _yoloModelObjectDetection;
        private bool _enableObjectDetection = false;

        private UserGuidance _userGuidance;
        private UserData _userData;

        private Dataset _objectDetectionDataset;
        private Dataset _instanceSegmentationDataset;

        private List<InstanceSegmentationResult> _lastDetectionResultsInstanceSegmentation;

        static Windows.Storage.StorageFile sampleFile;

        /**
        <summary>
            avoid creation via constructor; we have 1 create method
        </summary>
         */
        private MRWebRTC()
        {
            _lastDetectionResultsInstanceSegmentation = new List<InstanceSegmentationResult>();

        }

        /**
        <summary>
            <para>
                Here we initialize all the WebRTC things
            </para>
            <param name="httpServerAddress">
                address where our applicatio will run
            </param>
            <param name="localPeerId">
                our local name for WebRTC connection
            </param>
            <param name="remotePeerId">
                name to our remote MR source
            </param>
        </summary>
         */
        public async static Task<MRWebRTC> Create(  string httpServerAddress,
                                                    string localPeerId,
                                                    string remotePeerId)
        {

            MRWebRTC newInstance = new MRWebRTC();

            Windows.Storage.StorageFolder storageFolder =
            Windows.Storage.ApplicationData.Current.LocalFolder;
            sampleFile = await storageFolder.CreateFileAsync("log.txt",
                         Windows.Storage.CreationCollisionOption.ReplaceExisting);

            newInstance.InitAI();

            // MixedReality Web RTC things 
            // Request access to microphone and camera
            MediaCaptureInitializationSettings settings =
                                    new MediaCaptureInitializationSettings();
            settings.StreamingCaptureMode = StreamingCaptureMode.Video;
            MediaCapture capture = new MediaCapture();
            await capture.InitializeAsync(settings);

            // Retrieve a list of available video capture devices (webcams).
            IReadOnlyList<VideoCaptureDevice> deviceList =
            await DeviceVideoTrackSource.GetCaptureDevicesAsync();

            // Get the device list and, for example, print them to the debugger console
            foreach (var device in deviceList)
            {
                // This message will show up in the Output window of Visual Studio
                Debugger.Log(0, "", $"Webcam {device.name} (id: {device.id})\n");
            }

            // init peer connection
            newInstance.InitPeerConnection();

            //_detectionResults = await _peerConnection.AddDataChannelAsync(dataChannelID,dataChannelLabel,true,true);

            // Initialize the signaler
            newInstance._signaler = new NodeDssSignaler()
            {
                HttpServerAddress = httpServerAddress,
                LocalPeerId = localPeerId,
                RemotePeerId = remotePeerId,
            };
            newInstance._signaler.OnMessage += async (NodeDssSignaler.Message msg) =>
            {
                switch (msg.MessageType)
                {
                    case NodeDssSignaler.Message.WireMessageType.Offer:
                        // Wait for the offer to be applied
                        await newInstance._peerConnection.SetRemoteDescriptionAsync(msg.ToSdpMessage());
                        // Once applied, create an answer
                        newInstance._peerConnection.CreateAnswer();
                        break;

                    case NodeDssSignaler.Message.WireMessageType.Answer:
                        // No need to await this call; we have nothing to do after it
                        newInstance._peerConnection.SetRemoteDescriptionAsync(msg.ToSdpMessage());
                        break;

                    case NodeDssSignaler.Message.WireMessageType.Ice:
                        newInstance._peerConnection.AddIceCandidate(msg.ToIceCandidate());
                        break;
                }
            };

            newInstance._signaler.StartPollingAsync();

            return newInstance;

        }

        private async void InitAI()
        {
            _objectDetectionDataset = new TruckDataSet("TruckDetectionDataset.yaml");
            _instanceSegmentationDataset = new TruckDataSetSeg("TruckSegmentationDataset.yaml");

            _userGuidance = new UserGuidance();
            await _userGuidance.LoadModelAsync();

            _userData = new UserData();

            //AI stuff init
            _yoloModelObjectDetection = await Yolov5.CreateAsync(
                                        modelAssetFile: "yolov5m_truck.onnx",
                                        inputWidth: 640,
                                        inputHeight: 640,
                                        inputDepth: 3,
                                        inputs: new[] { "images" },
                                        outputs: new[] { "output0" },
                                        dataset: _objectDetectionDataset,
                                        confidence: 0.4f);

            //instance segmentation results
            _yoloModelInstanceSegmentation = await Yolov5Seg.CreateAsync(
                                        modelAssetFile: "yolov5l_truckseg.onnx",
                                        inputWidth: 640,
                                        inputHeight: 640,
                                        inputDepth: 3,
                                        inputs: new[] { "images" },
                                        outputs: new[] { "output0", "output1" },
                                        dataset: _instanceSegmentationDataset,
                                        confidence: 0.3f,
                                        maskOutputSizeX: 160,
                                        maskOutputSizeY: 160);

            //yoloModelObjectDetection = new Yolov7(  inputWidth: 640,
            //                                        inputHeight: 640,
            //                                        inputDepth: 3,
            //                                        new[] { "images" },
            //                                        new[] { "output" },
            //                                        new TruckDataSet(),
            //                                        0.25f);

            //yoloModelObjectDetection = new Yolov4(  "Yolov4.onnx",
            //                                        inputWidth: 416,
            //                                        inputHeight: 416,
            //                                        inputDepth: 3,
            //                                        new[] { "input_1:0" },
            //                                        new[] { "Identity:0" },
            //                                        new TruckDataSet(),
            //                                        0.25f);

            //yoloModelInstanceSegmentation = new Yolov7Mask( "yolov7-mask.onnx",
            //                                                inputWidth: 640,
            //                                                inputHeight: 640,
            //                                                inputDepth: 3,
            //                                                new[] { "images" },
            //                                                new[] {   "output", "755", "783", "741",
            //                                                          "769", "797", "649", "713" },
            //                                                new TruckDataSetSeg()
            //                                                );
        }

        private async void InitPeerConnection()
        {
            _peerConnection = new PeerConnection();

            // config peer connection
            PeerConnectionConfiguration config = new PeerConnectionConfiguration
            {
                IceServers = new List<IceServer> {
            new IceServer{ Urls = { "stun:stun.l.google.com:19302" } }
        }
            };

            await _peerConnection.InitializeAsync(config);
            Debugger.Log(0, "", "Peer connection initialized successfully.\n");

            _peerConnection.Connected += () =>
            {
                Debugger.Log(0, "", "PeerConnection: connected.\n");
            };
            _peerConnection.IceStateChanged += (IceConnectionState newState) =>
            {
                Debugger.Log(0, "", $"ICE state: {newState}\n");
            };

            _peerConnection.VideoTrackAdded += (RemoteVideoTrack track) =>
            {
                _remoteVideoTrack = track;
                _remoteVideoTrack.I420AVideoFrameReady += RemoteVideo_I420AFrameReady;
            };

            _peerConnection.LocalSdpReadytoSend += Peer_LocalSdpReadytoSend;
            _peerConnection.IceCandidateReadytoSend += Peer_IceCandidateReadytoSend;
            _peerConnection.DataChannelAdded += OnDataChannelAdded;
        }

        private void OnDataChannelAdded(DataChannel channel)
        {
            if (channel.Label.Equals(ResultExchangeDataChannelConstants.DataChannelLabelDetection)) //
            {
                Debugger.Log(0, "", "DataChannel added.\n");
                _detectionResultsChannel = channel;
            }
            else if (channel.Label.Equals(ResultExchangeDataChannelConstants.DataChannelLabelsSegmentation[0]))//
            {
                Debugger.Log(0, "", "DataChannel added.\n");
                _instanceSegmentationResultsChannel[0] = channel;
            }
            else if (channel.Label.Equals(ResultExchangeDataChannelConstants.DataChannelLabelsSegmentation[1]))//
            {
                Debugger.Log(0, "", "DataChannel added.\n");
                _instanceSegmentationResultsChannel[1] = channel;
            }
            else if (channel.Label.Equals(ResultExchangeDataChannelConstants.DataChannelLabelsSegmentation[2]))//
            {
                Debugger.Log(0, "", "DataChannel added.\n");
                _instanceSegmentationResultsChannel[2] = channel;
            }
            else if (channel.Label.Equals(ResultExchangeDataChannelConstants.DataChannelLabelsSegmentation[3]))//
            {
                Debugger.Log(0, "", "DataChannel added.\n");
                _instanceSegmentationResultsChannel[3] = channel;
            }
            else if (channel.Label.Equals(ResultExchangeDataChannelConstants.DataChannelLabelsMask[0]))//
            {
                Debugger.Log(0, "", "DataChannel added.\n");
                _instanceSegmentationMaskChannel[0] = channel;
            }
            else if (channel.Label.Equals(ResultExchangeDataChannelConstants.DataChannelLabelsMask[1]))//
            {
                Debugger.Log(0, "", "DataChannel added.\n");
                _instanceSegmentationMaskChannel[1] = channel;
            }
            else if (channel.Label.Equals(ResultExchangeDataChannelConstants.DataChannelLabelsMask[2]))//
            {
                Debugger.Log(0, "", "DataChannel added.\n");
                _instanceSegmentationMaskChannel[2] = channel;
            }
            else if (channel.Label.Equals(ResultExchangeDataChannelConstants.DataChannelLabelsMask[3]))//
            {
                Debugger.Log(0, "", "DataChannel added.\n");
                _instanceSegmentationMaskChannel[3] = channel;
            }
            else if (channel.Label.Equals(ResultExchangeDataChannelConstants.DataChannelUserData))//
            {
                Debugger.Log(0, "", "DataChannel added.\n");
                _userDataChannel = channel;

                _userDataChannel.MessageReceived += (byte[] message) =>
                {
                    if ((message != null) && message.Length > 0)
                    {
                        // create a second float array and copy the bytes into it...
                        var floatArray = new float[message.Length / 4];
                        Buffer.BlockCopy(message, 0, floatArray, 0, message.Length);

                        UserSamplePoint user_data_sample_point = new UserSamplePoint();

                        user_data_sample_point.unroll_big_array(floatArray);
                        _userData.addSample(user_data_sample_point, _lastDetectionResultsInstanceSegmentation);

                    }
                };
            }
        }


        private void Peer_LocalSdpReadytoSend(SdpMessage message)
        {
            NodeDssSignaler.Message msg = NodeDssSignaler.Message.FromSdpMessage(message);
            _signaler.SendMessageAsync(msg);
        }

        private void Peer_IceCandidateReadytoSend(IceCandidate iceCandidate)
        {
            NodeDssSignaler.Message msg = NodeDssSignaler.Message.FromIceCandidate(iceCandidate);
            _signaler.SendMessageAsync(msg);
        }

        private MediaStreamSource CreateI420VideoStreamSource(
            uint width, uint height, int framerate)
        {
            if (width == 0)
            {
                throw new ArgumentException("Invalid zero width for video.", "width");
            }
            if (height == 0)
            {
                throw new ArgumentException("Invalid zero height for video.", "height");
            }
            // Note: IYUV and I420 have same memory layout (though different FOURCC)

            // https://docs.microsoft.com/en-us/windows/desktop/medfound/video-subtype-guids
            VideoEncodingProperties videoProperties = VideoEncodingProperties.CreateUncompressed(
                MediaEncodingSubtypes.Iyuv, width, height);
            VideoStreamDescriptor videoStreamDesc = new VideoStreamDescriptor(videoProperties);
            videoStreamDesc.EncodingProperties.FrameRate.Numerator = (uint)framerate;
            videoStreamDesc.EncodingProperties.FrameRate.Denominator = 1;
            // Bitrate in bits per second : framerate * frame pixel size * I420=12bpp
            videoStreamDesc.EncodingProperties.Bitrate = ((uint)framerate * width * height * 12);
            MediaStreamSource videoStreamSource = new MediaStreamSource(videoStreamDesc);
            videoStreamSource.BufferTime = TimeSpan.Zero;
            videoStreamSource.SampleRequested += OnMediaStreamSourceRequested;
            videoStreamSource.IsLive = true; // Enables optimizations for live sources
            videoStreamSource.CanSeek = false; // Cannot seek live WebRTC video stream
            return videoStreamSource;
        }

        private void RemoteVideo_I420AFrameReady(I420AVideoFrame frame)
        {
            lock (_remoteVideoLock)
            {
                if (!_remoteVideoPlaying)
                {
                    _remoteVideoPlaying = true;
                    uint width = frame.width;
                    uint height = frame.height;
                    // Bridge the remote video track with the remote media player UI
                    int framerate = 30; // assumed, for lack of an actual value
                    _remoteVideoSource = CreateI420VideoStreamSource(width, height,
                        framerate);
                    Notify(_remoteVideoSource);
                }
            }

            uint byteSizeRGBA8 = (frame.width * frame.height) * 8; // RGBA8 = 4 byte per pixel
            byte[] imageArrRGBA8 = new byte[byteSizeRGBA8];

            YuvUtils.CopyYuvToBufferAsRgb(frame.dataY, frame.dataU, frame.dataV,
                                            frame.strideY, frame.strideU, frame.strideV,
                                            frame.width, frame.height, imageArrRGBA8);

            SoftwareBitmap softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Rgba8, (int)frame.width,
                                                                (int)frame.height, BitmapAlphaMode.Premultiplied);
            softwareBitmap.CopyFromBuffer(imageArrRGBA8.AsBuffer());

            // we need the video frame in Bgra8 form for it is the standard layout for further api calls
            // we don't need alpha channel for our NN(Neural Net) inputs
            SoftwareBitmap softwareBitmapBgra = SoftwareBitmap.Convert(softwareBitmap,
                                                                        BitmapPixelFormat.Bgra8,
                                                                        BitmapAlphaMode.Ignore);

            uint inputWidth = 0;
            uint inputHeight = 0;

            // we need the VideoFrame in appropriate input size for our NN(neural networks)
            Task<SoftwareBitmap> outputBitmap;

            if (_enableObjectDetection)
            {
                // Resize frame to the Net input shape
                inputWidth = _yoloModelObjectDetection.InputWidth;
                inputHeight = _yoloModelObjectDetection.InputHeight;
                outputBitmap = BitmapUtils.ResizeBitmap(softwareBitmapBgra, inputWidth,
                                                inputHeight, BitmapAlphaMode.Ignore, BitmapInterpolationMode.Linear);
                outputBitmap.Wait();
                // Only use this line if you want to save every frame to a .jpg for debugging purposes!!!
                // BitmapUtils.SaveSoftwareBitmapToFile(outputBitmap.Result);

                _outputVideoFrameObjectDetection = VideoFrame.CreateWithSoftwareBitmap(outputBitmap.Result);
            }

            // Resize frame to the Net input shape
            // TODO: Add extra softwarebitmap for instance segmentation
            inputWidth = _yoloModelInstanceSegmentation.InputWidth;
            inputHeight = _yoloModelInstanceSegmentation.InputHeight;

            // we need the VideoFrame in appropriate input size for our NN(neural networks)
            outputBitmap = BitmapUtils.ResizeBitmap(softwareBitmapBgra, inputWidth,
                inputHeight, BitmapAlphaMode.Ignore, BitmapInterpolationMode.Linear);
            outputBitmap.Wait();

            //Only use this line if you want to save every frame to a.jpg for debugging purposes!!!
            //BitmapUtils.SaveSoftwareBitmapToFile(outputBitmap.Result);

            _outputVideoFrameInstanceSegmentation = VideoFrame.CreateWithSoftwareBitmap(outputBitmap.Result);

            _remoteVideoBridge.HandleIncomingVideoFrame(frame);

        }

        private void OnMediaStreamSourceRequested(MediaStreamSource sender,
                        MediaStreamSourceSampleRequestedEventArgs args)
        {
            VideoBridge videoBridge;
            if (sender == _remoteVideoSource)
                videoBridge = _remoteVideoBridge;
            else
                return;

            List<DetectionResult> detectionResults = new List<DetectionResult>();
            List<Color> detectionResultColors = new List<Color>();

            if (_enableObjectDetection)
            {
                detectionResults = _yoloModelObjectDetection.EvaluateFrame(_outputVideoFrameObjectDetection);

                if (detectionResults.Count > 0)
                {
                    detectionResultColors.Capacity = detectionResults.Count;
                
                    for (int i = 0; i < detectionResults.Count; i++)
                    {
                        int label_index = Array.IndexOf(_objectDetectionDataset.Labels, detectionResults[i].label);
                        string label = _objectDetectionDataset.Labels[label_index];
                        var class_color = _objectDetectionDataset.Class_colors[label];
                        detectionResultColors.Add(class_color);
                    }  

                }
            }

            // transfer results to hololens back for storing reasons
            if (_detectionResultsChannel != null)
            {
                string csvWritableResults = CSVFileHelper.ConvertDetectionResultsToString(detectionResults);
                byte[] detectionResultsBytes =
                                Encoding.UTF8.GetBytes(csvWritableResults);
                if (_detectionResultsChannel.State == DataChannel.ChannelState.Open
                    && detectionResultsBytes != null)
                    _detectionResultsChannel.SendMessage(detectionResultsBytes);
            }

            List<InstanceSegmentationResult> detectionResultsInstanceSegmentation =
                                    _yoloModelInstanceSegmentation.EvaluateFrame(_outputVideoFrameInstanceSegmentation);

            _lastDetectionResultsInstanceSegmentation = new List<InstanceSegmentationResult>(detectionResultsInstanceSegmentation);

            List<Color> detectionResultsInstanceSegmentationColors = new List<Color>(detectionResultsInstanceSegmentation.Count);

            if (detectionResultsInstanceSegmentation.Count > 0)
            {
                
                for (int i = 0; i < detectionResultsInstanceSegmentation.Count; i++)
                {
                    int label_index = Array.IndexOf(_instanceSegmentationDataset.Labels,
                                                    detectionResultsInstanceSegmentation[i].label);
                    string label = _instanceSegmentationDataset.Labels[label_index];
                    var class_color = _instanceSegmentationDataset.Class_colors[label];
                    detectionResultsInstanceSegmentationColors.Add(class_color);
                }

                for (int i = 0; i < detectionResultsInstanceSegmentation.Count; i++)
                {

                    if (_instanceSegmentationResultsChannel[i] != null)
                    {

                        string csvWritableResults = detectionResultsInstanceSegmentation[i].ConvertToCSVFilePresentation(false); //CSVFileHelper.ConvertDetectionResultsToString(detectionResultsInstanceSegmentation);

                        try
                        {

                            byte[] instanceSegmentationResultsBytes = Encoding.UTF8.GetBytes(csvWritableResults);

                            if ((_instanceSegmentationResultsChannel[i].State == DataChannel.ChannelState.Open)
                                && (instanceSegmentationResultsBytes != null)
                                && (instanceSegmentationResultsBytes.Length > 0))
                            {
                                _instanceSegmentationResultsChannel[i].SendMessage(instanceSegmentationResultsBytes);
                            }
                        } catch(Exception e)
                        {
                            Debugger.Log(0, "", $"Error in sending segmentation result. Error Message: {e.Message}.\n");
                        }
                    }

                    if (_instanceSegmentationMaskChannel[i] != null)
                    {

                        try
                        {

                            byte[] instanceSegmentationResultsBytes = detectionResultsInstanceSegmentation[i].mask_img;

                            if ((_instanceSegmentationMaskChannel[i].State == DataChannel.ChannelState.Open)
                                && (instanceSegmentationResultsBytes != null)
                                && (instanceSegmentationResultsBytes.Length > 0))
                            {
                                _instanceSegmentationMaskChannel[i].SendMessage(instanceSegmentationResultsBytes);
                            }
                        }
                        catch (Exception e)
                        {
                            Debugger.Log(0, "", $"Error in sending segmentation result. Error Message: {e.Message}.\n");
                        }

                    }
                    
                }

            }

            if (_userDataChannel != null)
            {

                try
                {
                    
                    string detection_label = _userGuidance.Evaluate(_userData);
                    byte[] instanceSegmentationResultsBytes = Encoding.UTF8.GetBytes(detection_label);

                    //await Windows.Storage.FileIO.WriteTextAsync(sampleFile, $"Current detected intention: {detection_label}.\n");
                    Debugger.Log(0, "", $"Current detected intention: {detection_label}.\n");

                    if ((_userDataChannel.State == DataChannel.ChannelState.Open)
                        && (instanceSegmentationResultsBytes != null)
                        && (instanceSegmentationResultsBytes.Length > 0))
                    {
                        _userDataChannel.SendMessage(instanceSegmentationResultsBytes);
                    }
                }
                catch (Exception e)
                {
                    Debugger.Log(0, "", $"Error in sending segmentation result. Error Message: {e.Message}.\n");
                }

            }

            // rendering agnostic things
            Notify(new YoloResults( detectionResults,
                                    detectionResultColors,
                                    detectionResultsInstanceSegmentation,
                                    detectionResultsInstanceSegmentationColors,
                                    _yoloModelInstanceSegmentation.InputWidth,
                                    _yoloModelInstanceSegmentation.InputHeight,
                                    _yoloModelObjectDetection.InputWidth,
                                    _yoloModelObjectDetection.InputHeight,
                                    _yoloModelInstanceSegmentation.MaskOutputSizeX,
                                    _yoloModelInstanceSegmentation.MaskOutputSizeY));

            videoBridge.TryServeVideoFrame(args);

        }

        private void CleanUp()
        {

            if (_peerConnection != null)
            {
                _peerConnection.Close();
                _peerConnection.Dispose();
                _peerConnection = null;
            }

            if (_signaler != null)
            {
                _signaler.StopPollingAsync();
                _signaler = null;
            }
        }

        ~MRWebRTC()
        {
            CleanUp();
        }

    }
}
