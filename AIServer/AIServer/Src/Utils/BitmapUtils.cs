// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using SharedResultsBetweenServerAndHoloLens;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace AIServer
{
    [ComImport]
    [Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    unsafe interface IMemoryBufferByteAccess
    {
        void GetBuffer(out byte* buffer, out uint capacity);
    }

    class BitmapUtils
    {
        public static async Task<SoftwareBitmap> ResizeBitmap(SoftwareBitmap softwareBitmap, uint width, uint height,
                                                                BitmapAlphaMode alphaMode,
                                                                BitmapInterpolationMode interpolationMode)
        {
            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, stream);

                encoder.SetSoftwareBitmap(softwareBitmap);

                encoder.BitmapTransform.ScaledWidth = width;
                encoder.BitmapTransform.ScaledHeight = height;
                encoder.BitmapTransform.InterpolationMode = interpolationMode;

                await encoder.FlushAsync();

                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                return await decoder.GetSoftwareBitmapAsync(softwareBitmap.BitmapPixelFormat, alphaMode);
            }
        }

        // https://learn.microsoft.com/de-de/windows/uwp/audio-video-camera/imaging
        public static async void SaveSoftwareBitmapToFile(SoftwareBitmap softwareBitmap)
        {


            try
            {
                var storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                //storageFolder.Wait();
                var outputFile = storageFolder.CreateFileAsync("pic" +
                                            (DateTime.Now).ToString("yyyyMMddHHmmssffff") + ".jpg").AsTask();
                outputFile.Wait();


                using (IRandomAccessStream stream = await outputFile.Result.OpenAsync(FileAccessMode.ReadWrite))
                {
                    // Create an encoder with the desired format
                    BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);

                    // Set the software bitmap
                    encoder.SetSoftwareBitmap(softwareBitmap);

                    // Set additional encoding parameters, if needed
                    encoder.BitmapTransform.ScaledWidth = (uint)softwareBitmap.PixelWidth;
                    encoder.BitmapTransform.ScaledHeight = (uint)softwareBitmap.PixelHeight;
                    encoder.BitmapTransform.Rotation = Windows.Graphics.Imaging.BitmapRotation.None;
                    encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Fant;
                    encoder.IsThumbnailGenerated = true;

                    try
                    {
                        await encoder.FlushAsync();
                    }
                    catch (Exception err)
                    {
                        const int WINCODEC_ERR_UNSUPPORTEDOPERATION = unchecked((int)0x88982F81);
                        switch (err.HResult)
                        {
                            case WINCODEC_ERR_UNSUPPORTEDOPERATION:
                                // If the encoder does not support writing a thumbnail, then try again
                                // but disable thumbnail generation.
                                encoder.IsThumbnailGenerated = false;
                                break;
                            default:
                                throw;
                        }
                    }

                    if (encoder.IsThumbnailGenerated == false)
                    {
                        await encoder.FlushAsync();
                    }


                }

            }
            catch (AggregateException err)
            {
                foreach (var errInner in err.InnerExceptions)
                {
                    // this will call ToString() on the inner execption and get you message,
                    // stacktrace and you could perhaps drill down further into the inner exception of it if necessary
                    Debug.WriteLine(errInner);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }


        }

        public static async Task<SoftwareBitmap> LoadPictureToSoftwareBitmap(uint yoloModelInputWidth,
                                                                                uint yoloModelInputHeight)
        {

            var storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            //storageFolder.Wait();
            var inputFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/cat.jpg"));
            //inputFile.Wait();

            using (IRandomAccessStream stream = await inputFile.OpenAsync(FileAccessMode.Read))
            {
                // Create the decoder from the stream
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                // Get the SoftwareBitmap representation of the file
                SoftwareBitmap cat = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8,
                                                                            BitmapAlphaMode.Ignore);

                var cat_resized = ResizeBitmap(cat, yoloModelInputWidth, yoloModelInputHeight,
                                                BitmapAlphaMode.Ignore, BitmapInterpolationMode.NearestNeighbor);
                cat_resized.Wait();
                var cats_resized_NO_ALPHA = SoftwareBitmap.Convert(cat_resized.Result,
                                                                    BitmapPixelFormat.Bgra8,
                                                                    BitmapAlphaMode.Premultiplied);

                return cats_resized_NO_ALPHA;
            }
        }

        public static void DrawBBOverSoftwareBitmap(SoftwareBitmap softwareBitmap, List<DetectionResult> detections)
        {
            unsafe
            {
                using (BitmapBuffer buffer = softwareBitmap.LockBuffer(BitmapBufferAccessMode.Write))
                {
                    using (var reference = buffer.CreateReference())
                    {
                        byte* dataInBytes;
                        uint capacity;
                        ((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacity);

                        // Fill-in the BGRA plane
                        BitmapPlaneDescription bufferLayout = buffer.GetPlaneDescription(0);

                        for (int detection_index = 0; detection_index < detections.Count; detection_index++)
                        {
                            int left = (int)(detections[detection_index].bbox[0]
                                                    - detections[detection_index].bbox[2] / 2f);
                            int top = (int)(detections[detection_index].bbox[1]
                                                    - detections[detection_index].bbox[3] / 2f);
                            int bottom = (int)(detections[detection_index].bbox[1]
                                                    + detections[detection_index].bbox[3] / 2f);
                            int right = (int)(detections[detection_index].bbox[0]
                                                    + detections[detection_index].bbox[2] / 2f);

                            // draw horizontal lines of bb
                            for (int i = left; i < right; i++)
                            {
                                for (int j = top; j < top + 1; j++)
                                {
                                    if (i < 0 || j < 0 || i >= 640 || j >= 640) continue;
                                    dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * j + 4 * i + 0] = 51;
                                    dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * j + 4 * i + 1] = 255;
                                    dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * j + 4 * i + 2] = 51;
                                }

                                for (int j = bottom; j < bottom + 1; j++)
                                {
                                    if (i < 0 || j < 0 || i >= 640 || j >= 640) continue;
                                    dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * j + 4 * i + 0] = 51;
                                    dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * j + 4 * i + 1] = 255;
                                    dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * j + 4 * i + 2] = 51;
                                }
                            }

                            // draw vertical lines
                            for (int i = top; i < bottom; i++)
                            {
                                // left vertical line of bb
                                for (int j = left; j < left + 1; j++)
                                {
                                    if (i < 0 || j < 0 || i >= 640 || j >= 640) continue;
                                    dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 0] = 51;
                                    dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 1] = 255;
                                    dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 2] = 51;
                                }

                                // right vertical line of bb
                                for (int j = right; j < right + 1; j++)
                                {
                                    if (i < 0 || j < 0 || i >= 640 || j >= 640) continue;
                                    dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 0] = 51;
                                    dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 1] = 255;
                                    dataInBytes[bufferLayout.StartIndex + bufferLayout.Stride * i + 4 * j + 2] = 51;
                                }
                            }

                        }

                    }
                }
            }
        }

        public static async Task<WriteableBitmap> ResizeWritableBitmap(WriteableBitmap baseWriteBitmap,
                                                                        uint width, uint height)
        {
            // Get the pixel buffer of the writable bitmap in bytes
            Stream stream = baseWriteBitmap.PixelBuffer.AsStream();
            byte[] pixels = new byte[(uint)stream.Length];
            await stream.ReadAsync(pixels, 0, pixels.Length);
            //Encoding the data of the PixelBuffer we have from the writable bitmap
            var inMemoryRandomStream = new InMemoryRandomAccessStream();
            var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, inMemoryRandomStream);
            encoder.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Ignore, width, height, 96, 96, pixels);
            await encoder.FlushAsync();
            // At this point we have an encoded image in inMemoryRandomStream
            // We apply the transform and decode
            var transform = new BitmapTransform
            {
                ScaledWidth = width,
                ScaledHeight = height
            };
            inMemoryRandomStream.Seek(0);
            var decoder = await BitmapDecoder.CreateAsync(inMemoryRandomStream);
            var pixelData = await decoder.GetPixelDataAsync(
                            BitmapPixelFormat.Rgba8,
                            BitmapAlphaMode.Straight,
                            transform,
                            ExifOrientationMode.IgnoreExifOrientation,
                            ColorManagementMode.DoNotColorManage);
            //An array containing the decoded image data
            var sourceDecodedPixels = pixelData.DetachPixelData();
            // Approach 1 : Encoding the image buffer again:
            //Encoding data
            var inMemoryRandomStream2 = new InMemoryRandomAccessStream();
            var encoder2 = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, inMemoryRandomStream2);
            encoder2.SetPixelData(BitmapPixelFormat.Rgba8, BitmapAlphaMode.Ignore, width, height,
                                    96, 96, sourceDecodedPixels);
            await encoder2.FlushAsync();
            inMemoryRandomStream2.Seek(0);
            // finally the resized writablebitmap
            var bitmap = new WriteableBitmap((int)width, (int)height);
            bitmap.SetSourceAsync(inMemoryRandomStream2);
            return bitmap;
        }

    }



}
