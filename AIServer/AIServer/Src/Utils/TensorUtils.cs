// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices.WindowsRuntime;
using Windows.AI.MachineLearning;
using Windows.Media;

namespace AIServer
{
    public class TensorUtils
    {
        /// <summary>
        /// Extracts pixels into tensor for net input.
        /// We also have to normalize input for our nets
        /// </summary>
        public static TensorFloat ExtractPixelsAndNormalize(VideoFrame vf)
        {

            int inputHeight = vf.SoftwareBitmap.PixelHeight;
            int inputWidth = vf.SoftwareBitmap.PixelWidth;

            // good resource: https://www.1stvision.com/cameras/IDS/IDS-manuals/en/basics-color-pixel-formats.html
            // RGBA8 = 4 byte per pixel
            uint bytesPerPixelByteArray = 4;
            uint byteSizeRGBA8ByteArray = (uint)(inputHeight * inputWidth) * bytesPerPixelByteArray;
            byte[] _imageArrRGBA8 = new byte[byteSizeRGBA8ByteArray];

            vf.SoftwareBitmap.CopyToBuffer(_imageArrRGBA8.AsBuffer());

            uint bytesPerPixelFloatArray = 3; // we discard alpha channel here!!
            uint byteSizeRGBA8FloatArray = (uint)(inputHeight * inputWidth) * bytesPerPixelFloatArray;
            float[] _imageArrRGBA8Float = new float[byteSizeRGBA8FloatArray];

            int green_offset = inputHeight * inputWidth;
            int red_offset = 2 * green_offset;

            for (int y = 0; y < (int)inputHeight; y++)
            {
                for (int x = 0; x < (int)inputWidth; x++)
                {
                    int index = y * (int)inputWidth + x;

                    // some memory reordering happens here;
                    // keep in mind that tensors demand first all blue values in consecutive order,
                    // than green and red values!
                    _imageArrRGBA8Float[index] =
                        _imageArrRGBA8[index * bytesPerPixelByteArray + 2] / 255.0F; // b-channel
                    _imageArrRGBA8Float[index + green_offset] =
                        _imageArrRGBA8[index * bytesPerPixelByteArray + 1] / 255.0F; // g-channel
                    _imageArrRGBA8Float[index + red_offset] =
                        _imageArrRGBA8[index * bytesPerPixelByteArray + 0] / 255.0F; // r-channel

                }
            }

            // Tensor layout allways follows: NxCxHxW
            // N=Batch size, C=#Channels, H=Height, W=Width
            var tensor = TensorFloat.CreateFromArray(new[] { (long)1, (long)3,
                                                             (long)inputHeight, (long)inputWidth },
                                                            _imageArrRGBA8Float);

            return tensor;
        }
    }
}
