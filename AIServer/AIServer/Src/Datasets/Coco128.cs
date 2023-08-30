// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Windows.UI;

namespace AIServer.Datasets
{
    public class Coco128 : Dataset
    {

        protected override Color[] Colors
        {
            get
            {
                Color[] colors = new Color[Labels.Length];
                for (int m = 0; m < Labels.Length; m++)
                {
                    Random random = new Random();
                    byte randomRChannel = (byte)random.Next(20, 256);
                    byte randomGChannel = (byte)random.Next(20, 256);
                    byte randomBChannel = (byte)random.Next(20, 256);
                    colors[m] = Color.FromArgb(Byte.MaxValue, randomRChannel, randomGChannel, randomBChannel);
                }
                return colors;
            }
        }

        public override string[] Labels { get; } = {
            "person", "bicycle", "car", "motorcycle", "airplane", "bus", "train", "truck", "boat", "traffic light",
            "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow",
            "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee",
            "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard",
            "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple",
            "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "couch",
            "potted plant", "bed", "dining table", "toilet", "tv", "laptop", "mouse", "remote", "keyboard",
            "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors",
            "teddy bear", "hair drier", "toothbrush"
        };

    }
}
