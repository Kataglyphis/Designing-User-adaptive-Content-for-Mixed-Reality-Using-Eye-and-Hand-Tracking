// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using AIServer.WebRTC;
using SharedResultsBetweenServerAndHoloLens;
using TorchSharp;
using Windows.ApplicationModel;
using Windows.Graphics.Imaging;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using static TorchSharp.torch;
using static TorchSharp.torch.nn.functional;

namespace AIServer
{

    /** 
    <summary>
        Our main application UI with the video stream from our HoloLens on it
    </summary>
    */
    public sealed partial class MainPage : Page, IYoloResultObserver
    {

        private uint _applicationWindowWidth = 1500;
        private uint _applicationWindowHeigth = 1000;
        private readonly double _bbox_line_thickness = 4.0;

        /**
        should be between 0.0 and 1.0  
        */
        private readonly float _maskOpacity = 0.5f;

        MRWebRTC _webRTCConnectionToHololens = null;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
            Application.Current.Suspending += App_Suspending;

        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {

            _webRTCConnectionToHololens = await MRWebRTC.Create(httpServerAddress: "http://localhost:3000/",
                                                                localPeerId: "AIServer",
                                                                remotePeerId: "HoloLens");

            _webRTCConnectionToHololens.Subscribe(this);

        }

        /** 
        <summary>
            All UI calls must be on the UI thread
        </summary>
        */
        private void RunOnMainThread(Windows.UI.Core.DispatchedHandler handler)
        {

            if (Dispatcher.HasThreadAccess)
            {
                handler.Invoke();
            }
            else
            {
                // Note: use a discard "_" to silence CS4014 warning
                _ = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, handler);
            }
        }

        void IYoloResultObserver.RenderYoloResults(YoloResults yoloResults)
        {

            List<SoftwareBitmap> masksSoftwareBitmaps =
                                    ProcessMasksForRendering(yoloResults);

            try
            {
                // every call on some UI widget has to run on main thread!
                RunOnMainThread(() =>
                {
                    this.OverlayCanvas.Children.Clear();

                    // update application window size variables
                    // for we need them outside the main thread
                    this._applicationWindowWidth = (uint)this.Container.ActualWidth;
                    this._applicationWindowHeigth = (uint)this.Container.ActualHeight;

                    // Debug.Assert(detectionResultsInstanceSegmentation.Count == masksSoftwareBitmaps.Count);

                    for (int i = 0; i < yoloResults.detectionResultsInstanceSegmentation.Count; i++)
                    {
                        DrawBoxes(yoloResults.detectionResultsInstanceSegmentation[i],
                                    yoloResults.detectionResultsInstanceSegmentationColors[i],
                                    i,
                                    yoloResults.instanceSegmentationInputWidth,
                                    yoloResults.instanceSegmentationInputHeight);

                        DrawMasks(masksSoftwareBitmaps[i], i).Wait();

                    }

                    //for (int i = 0; i < yoloResults.detectionResults.Count; i++)
                    //{

                    //    DrawBoxes(yoloResults.detectionResults[i], i,
                    //                yoloResults.objectDetectionInputWidth,
                    //                yoloResults.objectDetectionInputHeight);

                    //}

                });
            }
            catch (AggregateException err)
            {
                foreach (var errInner in err.InnerExceptions)
                {
                    //this will call ToString() on the inner execption and get you message,
                    //stacktrace and you could perhaps drill down further into the inner
                    //exception of it if necessary 
                    Debug.WriteLine(errInner);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        /**
        <summary>
        
        </summary>
            <param name="detection">
                The detections we want to draw upon the canvas.
            </param>
            <param name="inputWidth">
                Width of the image we feed into the NN
            </param>
            <param name="inputHeight">
                Height of the image we feed into the NN
            </param>
            <param name="tag">
                for identifying our components on the UI
            </param>
        */
        private void DrawBoxes(DetectionResult detection,
                                Color detectionColor,
                                int tag,
                                uint inputWidth,
                                uint inputHeight)
        {
            // Be aware of the Canvas layout:
            // *****************************************************************
            // In NDC:
            // (-0.5,-0.5) ------------ (0.5,-0.5)
            // -                            -
            // -                            -
            // -                            -
            // -                            -
            // -                            -
            // -                            -
            // -                            -
            // (-0.5, 0.5) ------------ (0.5, 0.5)
            // // *****************************************************************
            // Whereas the incoming BB has coordinates like: 
            // In NDC:
            // (0.0, 0.0) ------------ (1.0, 0.0)
            // -                            -
            // -                            -
            // -                            -
            // -                            -
            // -                            -
            // -                            -
            // -                            -
            // (0.0, 1.0) ------------ (1.0, 1.0)
            // Make sure to transform adequately.

            int top_yoloOutput = (int)(detection.bbox[0]);
            int left_yoloOutput = (int)(detection.bbox[1]);
            int bottom_yoloOutput = (int)(detection.bbox[2]);
            int right_yoloOutput = (int)(detection.bbox[3]);

            float factor_x = (float)this.Container.ActualWidth;
            float factor_y = (float)this.Container.ActualHeight;

            factor_x /= (float)inputWidth;
            factor_y /= (float)inputHeight;

            int top_canvas = (int)((float)top_yoloOutput * factor_y);
            int left_canvas = (int)((float)left_yoloOutput * factor_x);

            // here comes the tricky transformation part as described in the very beginning of this method
            int top_canvas_for_rendering = top_canvas - (int)(this.Container.ActualHeight / 2f);
            int left_canvas_for_rendering = left_canvas - (int)(this.Container.ActualWidth / 2f);

            var r = new Windows.UI.Xaml.Shapes.Rectangle();
            r.Tag = tag;
            // also rescale size accordingly
            r.Width = (right_yoloOutput - left_yoloOutput) * factor_x;
            r.Height = (bottom_yoloOutput - top_yoloOutput) * factor_y;

            SolidColorBrush fill_brush = new SolidColorBrush(Colors.Transparent);
            SolidColorBrush line_brush = new SolidColorBrush(detectionColor);

            r.Fill = fill_brush;
            r.Stroke = line_brush;
            r.StrokeThickness = this._bbox_line_thickness;
            r.Margin = new Thickness(left_canvas_for_rendering, top_canvas_for_rendering, 0, 0);

            this.OverlayCanvas.Children.Add(r);
            // Default configuration for border
            // Render text label

            Border border = new Border();
            SolidColorBrush backgroundColorBrush = new SolidColorBrush(Colors.Black);
            SolidColorBrush foregroundColorBrush = new SolidColorBrush(detectionColor);
            TextBlock textBlock = new TextBlock();
            textBlock.Foreground = foregroundColorBrush;
            textBlock.Text = detection.label + " prob:" + Math.Round(detection.prob, 4);
            textBlock.FontSize = 20;
            // Hide
            textBlock.Visibility = Visibility.Collapsed;
            border.Background = backgroundColorBrush;
            border.Child = textBlock;
            textBlock.Visibility = Visibility.Visible;
            // Add to canvas
            this.OverlayCanvas.Children.Add(border);
            Canvas.SetLeft(border, left_canvas_for_rendering);
            Canvas.SetTop(border, top_canvas_for_rendering);

        }

        private async Task DrawMasks(SoftwareBitmap mask, int tag)
        {
            // Be aware of the Canvas layout:
            // *****************************************************************
            // In NDC:
            // (-0.5,-0.5) ------------ (0.5,-0.5)
            // -                            -
            // -                            -
            // -                            -
            // -                            -
            // -                            -
            // -                            -
            // -                            -
            // (-0.5, 0.5) ------------ (0.5, 0.5)
            // // ***************************************************************

            SoftwareBitmapSource source = new SoftwareBitmapSource();

            // do NOT wait on this call!!!
            source.SetBitmapAsync(mask);

            var maskImage = new Windows.UI.Xaml.Controls.Image();
            maskImage.Tag = tag;
            // also rescale size accordingly
            maskImage.Width = this.Container.ActualWidth;
            maskImage.Height = this.Container.ActualHeight;
            maskImage.Source = source;
            maskImage.Margin = new Thickness(-(float)this.Container.ActualWidth / 2f,
                                             -(float)this.Container.ActualHeight / 2f,
                                             0, 0);

            this.OverlayCanvas.Children.Add(maskImage);

        }

        private List<SoftwareBitmap> ProcessMasksForRendering(YoloResults yoloResults)
        {

            List<SoftwareBitmap> instanceSegmentationResults = new List<SoftwareBitmap>();

            for (int i = 0; i < yoloResults.detectionResultsInstanceSegmentation.Count; i++)
            {
                InstanceSegmentationResult detection = yoloResults.detectionResultsInstanceSegmentation[i];
                if (detection.mask_img == null) return instanceSegmentationResults;

                float factor_x = (float)_applicationWindowWidth / (float)yoloResults.instanceSegmentationInputWidth;
                float factor_y = (float)_applicationWindowHeigth / (float)yoloResults.instanceSegmentationInputHeight;

                // rescale bbox coordinates from model input to the current window size
                int bbTop = (int)(detection.bbox[0] * factor_y);
                int bbLeft = (int)(detection.bbox[1] * factor_x);
                int bbBottom = (int)(detection.bbox[2] * factor_y);
                int bbRight = (int)(detection.bbox[3] * factor_x);

                SoftwareBitmap softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8,
                                                                    (int)_applicationWindowWidth,
                                                                    (int)_applicationWindowHeigth,
                                                                    BitmapAlphaMode.Premultiplied);


                uint numberOfMaskChannels = 1;
                uint numberOfRenderingChannels = 4;
                uint miniBatchSize = 1;
                float maskThreshold = Byte.MaxValue / 2.0f;
                uint numberOfTotalPixels = _applicationWindowWidth * _applicationWindowHeigth *
                        numberOfRenderingChannels;
                byte[] resizedMask = new byte[numberOfTotalPixels];

                using (var d = torch.NewDisposeScope())
                {

                    // from https://pytorch.org/docs/stable/generated/torch.nn.functional.interpolate.html:
                    // The input dimensions are interpreted in the form:
                    // mini - batch x channels x[optional depth] x[optional height] x width.
                    var mask = torch.tensor(detection.mask_img, device: torch.CUDA, dtype: ScalarType.Byte)
                                    .reshape(miniBatchSize,
                                                numberOfMaskChannels,
                                                (int)yoloResults.maskOutputSizeY,
                                                (int)yoloResults.maskOutputSizeX
                                    ).to(ScalarType.Float32);

                    long[] newshape = { (int)_applicationWindowHeigth, (int)_applicationWindowWidth };

                    var interpolatedMask = interpolate(mask, newshape, mode: InterpolationMode.Bilinear,
                                                        align_corners: false)[0];

                    // follow the memory image layout; row major
                    // the reason why ones and not zeros are filled in has reasons in regard to rendering:
                    // with only zeros filled in the hole area outside the mask would be black and not transparent
                    // therefore fill it the most minimal value
                    var segmentationImage =
                        torch.ones(size: new long[] {  (int)_applicationWindowHeigth,
                                                        (int)_applicationWindowWidth,
                                                        numberOfRenderingChannels },
                                    device: torch.CUDA,
                                    dtype: torch.float32
                        );

                    TensorIndex[] bbCoordsBGRChannel = {    TensorIndex.Slice(bbTop, bbBottom),
                                                            TensorIndex.Slice(bbLeft, bbRight),
                                                            TensorIndex.Slice(0, numberOfRenderingChannels - 1) };

                    var bboxMask = interpolatedMask[0,
                                                    TensorIndex.Slice(bbTop, bbBottom),
                                                    TensorIndex.Slice(bbLeft, bbRight)]
                                    .ge(maskThreshold);

                    segmentationImage[bbCoordsBGRChannel][bboxMask] = torch.tensor(new byte[]{
                                                        yoloResults.detectionResultsInstanceSegmentationColors[i].B,
                                                        yoloResults.detectionResultsInstanceSegmentationColors[i].G,
                                                        yoloResults.detectionResultsInstanceSegmentationColors[i].R },
                                                        device: torch.CUDA
                                                        ) * _maskOpacity;

                    // this auxiliary tensor is only here for avoiding very expensive copying
                    // the underlying .NET array will be directly written with our tensor :)
                    var auxTensor = torch.as_tensor(resizedMask, device: torch.CPU, dtype: ScalarType.Byte);
                    auxTensor[TensorIndex.Slice()] = segmentationImage.to(ScalarType.Byte).cpu().flatten();

                    softwareBitmap.CopyFromBuffer(resizedMask.AsBuffer());
                    instanceSegmentationResults.Add(softwareBitmap);

                }

                // This is the old unoptimized approach without tensor calculations and nasty nested for loops :(
                // For illustration I will let this approach as an comment
                // Especially for people new to tensor calculations this will be useful
                // Different steps for preprocess our results
                //
                //      1.) Put the mask into a Softwarebitmap so that we can 
                //      2.) use the bilinear interpolation in the resizing bitmap method
                //          from (yoloModel.maskOutputSizeX,yoloModel.maskOutputSizeY)
                //                              --> (applicationWindowWidth, applicationWindowHeigth)
                //      3.) apply threshold of 0.5 to each value
                //      4.) and crop mask to detection box sizes

                // 1.) step
                //SoftwareBitmap softwareBitmap = new SoftwareBitmap(BitmapPixelFormat.Bgra8,
                //                                                  (int)yoloModelInstanceSegmentation.maskOutputSizeX,
                //                                                  (int)yoloModelInstanceSegmentation.maskOutputSizeY,
                //                                                  BitmapAlphaMode.Premultiplied);
                //
                //softwareBitmap.CopyFromBuffer(resizingBuffer.AsBuffer()); // detection.mask_img.AsBuffer()
                //
                //// 2.) step
                //var outputBitmap = BitmapUtils.ResizeBitmap(  softwareBitmap, applicationWindowWidth,
                //                                              applicationWindowHeigth, BitmapAlphaMode.Premultiplied,
                //                                              BitmapInterpolationMode.Linear);
                //outputBitmap.Wait();

                //byte[] resizedMask = new byte[applicationWindowWidth * applicationWindowHeigth * numberOfChannels];
                //outputBitmap.Result.CopyToBuffer(resizedMask.AsBuffer());
                //byte[] resizedMaskRendering =
                //                  new byte[applicationWindowWidth * applicationWindowHeigth * numRenderingChannels];
                //// 3.) step
                //int numRenderingChannels = 4;
                //for (int i = 0; i < applicationWindowHeigth; i++)
                //{
                //    for (int j = 0; j < applicationWindowWidth; j++)
                //    {
                //        // be aware bbox coordinates are in range [yoloModel.inputWidth,yoloModel.inputHeight]
                //        // therefore resize them for rendering purposes once again :)
                //        //int bbTop = (int)(detection.bbox[0] * factor_y);
                //        //int bbLeft = (int)(detection.bbox[1] * factor_x);
                //        //int bbBottom = (int)(detection.bbox[2] * factor_y);
                //        //int bbRight = (int)(detection.bbox[3] * factor_x);

                //        bool insideBoundingBox = i > bbTop && i < bbBottom && j > bbLeft && j < bbRight;

                //        int renderingIndex = (i * (int)applicationWindowWidth + j) * numRenderingChannels;
                //        int maskIndex = (i * (int)applicationWindowWidth + j);
                //        if (!insideBoundingBox || resizedMask[maskIndex + 2] < (255.0f / 2.0f))
                //        {
                //            // kinda hacky; with this minimal values there is no visual added
                //            // color in regions under threshold
                //            resizedMaskRendering[renderingIndex + 0] = (byte)1;
                //            resizedMaskRendering[renderingIndex + 1] = (byte)1;
                //            resizedMaskRendering[renderingIndex + 2] = (byte)1;
                //            resizedMaskRendering[renderingIndex + 3] = (byte)1;
                //        }
                //        else if (resizedMask[maskIndex + 2] >= (255.0f / 2.0f))
                //        {
                //            // set alpha and red channcel to max value
                //            resizedMaskRendering[renderingIndex + 2] = (byte)255 / 10;
                //            resizedMaskRendering[renderingIndex + 3] = (byte)((float)255 / (float)10);
                //        }

                //    }
                //}

                //softwareBitmap.CopyFromBuffer(resizedMaskRendering.AsBuffer());
                //instanceSegmentationResults.Add(softwareBitmap);
                //outputBitmap.Result.CopyFromBuffer(resizedMaskRendering.AsBuffer());
                //instanceSegmentationResults.Add(outputBitmap.Result);

                // this only for debugging purposes; this results in a heavy storage operation
                //BitmapUtils.SaveSoftwareBitmapToFile(outputBitmap.Result);
            }

            return instanceSegmentationResults;
        }

        public void UpdateRemoteVideoSource(MediaStreamSource remoteVideoSource)
        {
            RunOnMainThread(() =>
            {
                MediaPlayer remoteVideoPlayer = new MediaPlayer();
                remoteVideoPlayer.Source = MediaSource.CreateFromMediaStreamSource(
                    remoteVideoSource);
                remoteVideoPlayerElement.SetMediaPlayer(remoteVideoPlayer);
                remoteVideoPlayer.Play();
            });
        }

        private void App_Suspending(object sender, SuspendingEventArgs e)
        {

            remoteVideoPlayerElement.SetMediaPlayer(null);

        }
    }


}
