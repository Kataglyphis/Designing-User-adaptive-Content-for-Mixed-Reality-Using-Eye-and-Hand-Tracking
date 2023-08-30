// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using AIServer.Datasets;
using SharedResultsBetweenServerAndHoloLens;
using Windows.AI.MachineLearning;
using Windows.Storage;

namespace AIServer
{
    /**
    <summary>
        Yolo detection
    </summary>
     */
    public abstract class Yolo
    {
        protected LearningModel model;
        protected LearningModelSession session;
        protected LearningModelBinding binding;

        /**
        <value>
            Width of our image we feed into the NN
        </value>
        */
        protected string _modelAssetFile;
        public string ModelAssetFile { get { return _modelAssetFile; } }

        /**
        <value>
            Width of our image we feed into the NN
        </value>
        */
        protected uint _inputWidth;
        public uint InputWidth { get { return _inputWidth; } }

        /**
        <value>
            Height of our image we feed into the NN
        </value>
        */
        protected uint _inputHeight;
        public uint InputHeight { get { return _inputHeight; } }

        /**
        <value>
            # of channels of our image we feed into the NN
        </value>
        */
        protected uint _inputDepth;

        /**
        <value>
            All names of our NN inputs
        </value>
        */
        protected string[] _inputs;

        /**
        <value>
            All names of our NN outputs
        </value>
        */
        protected string[] _outputs;

        /**
        <value>
            Returns the equivalent sizes for all our outputs
        </value>
        */
        protected abstract List<uint> OutputEntrySizes { get; }


        /**
        <value>
            Very important hyperparameter of our model
            The relevant confidence threshold for our model; after training read at the F1 score for best confidence
        </value>
        */
        private float _confidence;
        protected float Confidence { get { return _confidence; } }

        /**
        <value>
            Dataset for which our model was compiled for
        </value>
        */
        private Dataset _dataset;
        protected Dataset Dataset { get { return _dataset; } }

        /**
        <summary>
            Dataset for which our model was compiled for
        </summary>
        */
        protected Yolo(string modelAssetFile, uint inputWidth, uint inputHeight, uint inputDepth,
                        string[] inputs, string[] outputs, Dataset dataset,
                        float confidence)
        {
            this._inputWidth = inputWidth;
            this._inputHeight = inputHeight;
            this._inputDepth = inputDepth;
            this._inputs = inputs;
            this._outputs = outputs;
            this._dataset = dataset;
            this._confidence = confidence;
            this._modelAssetFile = modelAssetFile;
        }

        /**
        <summary>
        Every Yolo model needs a startup method for loading the .onnx file
        </summary>
        */
        protected async Task LoadModelAsync()
        {
            // Load a machine learning model
            var model_file = await StorageFile.GetFileFromApplicationUriAsync(
                new Uri("ms-appx:///Assets/" + ModelAssetFile)
                );
            model = await LearningModel.LoadFromStorageFileAsync(model_file);
            var device = new LearningModelDevice(LearningModelDeviceKind.DirectXHighPerformance);
            session = new LearningModelSession(model, device);
            binding = new LearningModelBinding(session);
        }

        /**
        <summary>
        Good source: https://pyimagesearch.com/2016/11/07/intersection-over-union-iou-for-object-detection/
        </summary>
        <param name="DRa">
            First detection result            
        </param>
        <param name="DRb">
            Second detection result
        </param>
        */
        protected float ComputeIOU(DetectionResult DRa, DetectionResult DRb)
        {

            // Be aware of layout ( top has low values!)
            // ****************************
            // (0,0) ---------------- (1,0)                         (x1,y1) - - - - (x2,y1)
            // ----                 -------                         -                   -
            // ----                 -------  bb layout as follows   -                   -
            // ----                 -------  ------------------->   -                   -
            // ----                 -------                         -                   -
            // ----                 -------                         -                   -
            // ----                 -------                         -                   -
            // (0,1)                 ---(1,1)                       (x1,y2)- - - - - (x2,y2)

            float ay1 = DRa.bbox[0];
            float ax1 = DRa.bbox[1];
            float ay2 = DRa.bbox[2];
            float ax2 = DRa.bbox[3];

            float by1 = DRb.bbox[0];
            float bx1 = DRb.bbox[1];
            float by2 = DRb.bbox[2];
            float bx2 = DRb.bbox[3];

            Debug.Assert(ay1 < ay2);
            Debug.Assert(ax1 < ax2);
            Debug.Assert(by1 < by2);
            Debug.Assert(bx1 < bx2);

            // determine the coordinates of the intersection rectangle
            float x_left = Math.Max(ax1, bx1);
            float y_top = Math.Max(ay1, by1);
            float x_right = Math.Min(ax2, bx2);
            float y_bottom = Math.Min(ay2, by2);

            if (x_right < x_left || y_bottom < y_top)
                return 0;
            float intersection_area = (x_right - x_left) * (y_bottom - y_top);
            float bb1_area = (ax2 - ax1) * (ay2 - ay1);
            float bb2_area = (bx2 - bx1) * (by2 - by1);
            float iou = intersection_area / (bb1_area + bb2_area - intersection_area);

            Debug.Assert(iou >= 0 && iou <= 1);
            return iou;
        }

        /**
        <summary>
        Non maximum surpression
        </summary>
        <param name="detections">
            All found detections from a first pass; apply max surpession on them            
        </param>
        */
        protected List<DetectionResult> NMS(IReadOnlyList<DetectionResult> detections,
            float IOU_threshold = 0.5f)
        {
            List<DetectionResult> final_detections = new List<DetectionResult>();
            for (int i = 0; i < detections.Count; i++)
            {
                int j = 0;
                for (j = 0; j < final_detections.Count; j++)
                {
                    if (ComputeIOU(final_detections[j], detections[i]) > IOU_threshold)
                    {
                        break;
                    }
                }
                if (j == final_detections.Count)
                {
                    final_detections.Add(detections[i]);
                }
            }
            return final_detections;
        }

    }
}
