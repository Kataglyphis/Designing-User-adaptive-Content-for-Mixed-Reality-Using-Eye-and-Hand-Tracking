using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharedResultsBetweenServerAndHoloLens;

namespace AIServer.Src.UserGuidance
{
    // https://stackoverflow.com/questions/5852863/fixed-size-queue-which-automatically-dequeues-old-values-upon-new-enqueues
    internal class FixedSizedQueue<T>
    {
        public ConcurrentQueue<T> queue = new ConcurrentQueue<T>();

        public int Size { get; private set; }

        public FixedSizedQueue(int size)
        {
            Size = size;
        }

        public bool isFull()
        {
            return queue.Count == Size;
        }

        public void Enqueue(T obj)
        {
            queue.Enqueue(obj);

            while (queue.Count > Size)
            {
                T outObj;
                queue.TryDequeue(out outObj);
            }
        }
    }

    public class UserData
    {
        public static readonly int maxResults = 4;
        // sliding window size in #samplePoints
        public static readonly int slidingWindowSize = 150;
        static readonly int values = 16;

        private FixedSizedQueue<float> _userDataEyePosX;
        private FixedSizedQueue<float> _userDataEyePosY;

        private FixedSizedQueue<float> _userDataRightIndexTipPosX;
        private FixedSizedQueue<float> _userDataRightIndexTipPosY;

        private FixedSizedQueue<float> _userDataLeftIndexTipPosX;
        private FixedSizedQueue<float> _userDataLeftIndexTipPosY;

        private FixedSizedQueue<float> _userDataHeadMovementX;
        private FixedSizedQueue<float> _userDataHeadMovementY;
        private FixedSizedQueue<float> _userDataHeadMovementZ;

        private FixedSizedQueue<float> _userDataHeadVelocityX;
        private FixedSizedQueue<float> _userDataHeadVelocityY;
        private FixedSizedQueue<float> _userDataHeadVelocityZ;

        private FixedSizedQueue<float> _classLabel1;
        private FixedSizedQueue<float> _classLabel2;
        private FixedSizedQueue<float> _classLabel3;
        private FixedSizedQueue<float> _classLabel4;

        private FixedSizedQueue<float> _eyeHitPosInOut1;
        private FixedSizedQueue<float> _eyeHitPosInOut2;
        private FixedSizedQueue<float> _eyeHitPosInOut3;
        private FixedSizedQueue<float> _eyeHitPosInOut4;

        private FixedSizedQueue<float> _rightIndexTipPosInOut1;
        private FixedSizedQueue<float> _rightIndexTipPosInOut2;
        private FixedSizedQueue<float> _rightIndexTipPosInOut3;
        private FixedSizedQueue<float> _rightIndexTipPosInOut4;

        private FixedSizedQueue<float> _leftIndexTipPosInOut1;
        private FixedSizedQueue<float> _leftIndexTipPosInOut2;
        private FixedSizedQueue<float> _leftIndexTipPosInOut3;
        private FixedSizedQueue<float> _leftIndexTipPosInOut4;

        public UserData() {

            _userDataEyePosX = new FixedSizedQueue<float>(slidingWindowSize);
            _userDataEyePosY = new FixedSizedQueue<float>(slidingWindowSize);

            _userDataRightIndexTipPosX = new FixedSizedQueue<float>(slidingWindowSize);
            _userDataRightIndexTipPosY = new FixedSizedQueue<float>(slidingWindowSize);

            _userDataLeftIndexTipPosX = new FixedSizedQueue<float>(slidingWindowSize);
            _userDataLeftIndexTipPosY = new FixedSizedQueue<float>(slidingWindowSize);

            _userDataHeadMovementX = new FixedSizedQueue<float>(slidingWindowSize);
            _userDataHeadMovementY = new FixedSizedQueue<float>(slidingWindowSize);
            _userDataHeadMovementZ = new FixedSizedQueue<float>(slidingWindowSize);

            _userDataHeadVelocityX = new FixedSizedQueue<float>(slidingWindowSize);
            _userDataHeadVelocityY = new FixedSizedQueue<float>(slidingWindowSize);
            _userDataHeadVelocityZ = new FixedSizedQueue<float>(slidingWindowSize);

            _classLabel1 = new FixedSizedQueue<float>(slidingWindowSize);
            _classLabel2 = new FixedSizedQueue<float>(slidingWindowSize);
            _classLabel3 = new FixedSizedQueue<float>(slidingWindowSize);
            _classLabel4 = new FixedSizedQueue<float>(slidingWindowSize);

            _eyeHitPosInOut1 = new FixedSizedQueue<float>(slidingWindowSize);
            _eyeHitPosInOut2 = new FixedSizedQueue<float>(slidingWindowSize);
            _eyeHitPosInOut3 = new FixedSizedQueue<float>(slidingWindowSize);
            _eyeHitPosInOut4 = new FixedSizedQueue<float>(slidingWindowSize);

            _rightIndexTipPosInOut1 = new FixedSizedQueue<float>(slidingWindowSize);
            _rightIndexTipPosInOut2 = new FixedSizedQueue<float>(slidingWindowSize);
            _rightIndexTipPosInOut3 = new FixedSizedQueue<float>(slidingWindowSize);
            _rightIndexTipPosInOut4 = new FixedSizedQueue<float>(slidingWindowSize);

            _leftIndexTipPosInOut1 = new FixedSizedQueue<float>(slidingWindowSize);
            _leftIndexTipPosInOut2 = new FixedSizedQueue<float>(slidingWindowSize);
            _leftIndexTipPosInOut3 = new FixedSizedQueue<float>(slidingWindowSize);
            _leftIndexTipPosInOut4 = new FixedSizedQueue<float>(slidingWindowSize);

        }

        public bool isFull()
        {
            return _userDataEyePosX.isFull() && _userDataRightIndexTipPosX.isFull()
                                             && _userDataRightIndexTipPosY.isFull()
                                             && _userDataLeftIndexTipPosX.isFull()
                                             && _userDataLeftIndexTipPosY.isFull()
                                             && _userDataHeadMovementX.isFull()
                                             && _userDataHeadMovementY.isFull()
                                             && _userDataHeadMovementZ.isFull()
                                             && _userDataHeadVelocityX.isFull()
                                             && _userDataHeadVelocityY.isFull()
                                             && _userDataHeadVelocityZ.isFull()
                                             && _classLabel1.isFull();
        }

        public void addSample(UserSamplePoint new_user_sample_point,
                              List<InstanceSegmentationResult> last_result)
        {
            _userDataEyePosX.Enqueue(new_user_sample_point.eyeHitposScreenSpace[0]);
            _userDataEyePosY.Enqueue(new_user_sample_point.eyeHitposScreenSpace[1]);

            _userDataRightIndexTipPosX.Enqueue(new_user_sample_point.rightIndexTipPosScreenSpace[0]);
            _userDataRightIndexTipPosY.Enqueue(new_user_sample_point.rightIndexTipPosScreenSpace[1]);

            _userDataLeftIndexTipPosX.Enqueue(new_user_sample_point.leftIndexTipPosScreenSpace[0]);
            _userDataLeftIndexTipPosY.Enqueue(new_user_sample_point.leftIndexTipPosScreenSpace[1]);

            _userDataHeadMovementX.Enqueue(new_user_sample_point.headMovement[0]);
            _userDataHeadMovementY.Enqueue(new_user_sample_point.headMovement[1]);
            _userDataHeadMovementZ.Enqueue(new_user_sample_point.headMovement[2]);

            _userDataHeadVelocityX.Enqueue(new_user_sample_point.headVelocity[0]);
            _userDataHeadVelocityY.Enqueue(new_user_sample_point.headVelocity[1]);
            _userDataHeadVelocityZ.Enqueue(new_user_sample_point.headVelocity[2]);

            string[] data_labels = new string[] { "", "cap", "frame", "platform" };

            for (int index = 0; index < last_result.Count; index++)
            {
                new_user_sample_point.classLabel[index] = Array.IndexOf(data_labels, last_result[index].label);

            }

            _classLabel1.Enqueue(new_user_sample_point.classLabel[0]);
            _classLabel2.Enqueue(new_user_sample_point.classLabel[1]);
            _classLabel3.Enqueue(new_user_sample_point.classLabel[2]);
            _classLabel4.Enqueue(new_user_sample_point.classLabel[3]);

            _eyeHitPosInOut1.Enqueue(new_user_sample_point.eyeHitPosInOrOut[0]);
            _eyeHitPosInOut2.Enqueue(new_user_sample_point.eyeHitPosInOrOut[1]);
            _eyeHitPosInOut3.Enqueue(new_user_sample_point.eyeHitPosInOrOut[2]);
            _eyeHitPosInOut4.Enqueue(new_user_sample_point.eyeHitPosInOrOut[3]);

            _rightIndexTipPosInOut1.Enqueue(new_user_sample_point.rightIndexTipPosInOrOut[0]);
            _rightIndexTipPosInOut2.Enqueue(new_user_sample_point.rightIndexTipPosInOrOut[1]);
            _rightIndexTipPosInOut3.Enqueue(new_user_sample_point.rightIndexTipPosInOrOut[2]);
            _rightIndexTipPosInOut4.Enqueue(new_user_sample_point.rightIndexTipPosInOrOut[3]);

            _leftIndexTipPosInOut1.Enqueue(new_user_sample_point.leftIndexTipPosInOrOut[0]);
            _leftIndexTipPosInOut2.Enqueue(new_user_sample_point.leftIndexTipPosInOrOut[1]);
            _leftIndexTipPosInOut3.Enqueue(new_user_sample_point.leftIndexTipPosInOrOut[2]);
            _leftIndexTipPosInOut4.Enqueue(new_user_sample_point.leftIndexTipPosInOrOut[3]);


        }
        public float[] getEyeDataBurstWindowPresentation()
        {

            return _userDataEyePosX.queue.ToArray()
                                   .Concat(_userDataEyePosY.queue.ToArray())
                                   .Take(slidingWindowSize * 2)
                   .ToArray();
        }

        public float[] getHandDataBurstWindowPresentation()
        {

            return _userDataRightIndexTipPosX.queue.ToArray()
                                   .Concat(_userDataRightIndexTipPosY.queue.ToArray())
                                   .Concat(_userDataLeftIndexTipPosX.queue.ToArray())
                                   .Concat(_userDataLeftIndexTipPosY.queue.ToArray())
                                   .Take(slidingWindowSize * 4)
                    .ToArray();
        }

        public float[] getHeadDataBurstWindowPresentation()
        {

            return _userDataHeadMovementX.queue.ToArray()
                                   .Concat(_userDataHeadMovementY.queue.ToArray())
                                   .Concat(_userDataHeadMovementZ.queue.ToArray())
                                   .Concat(_userDataHeadVelocityX.queue.ToArray())
                                   .Concat(_userDataHeadVelocityY.queue.ToArray())
                                   .Concat(_userDataHeadVelocityZ.queue.ToArray())
                                   .Take(slidingWindowSize * 6)
                    .ToArray();
        }

        public float[] getNNDataBurstWindowPresentation()
        {

            return _classLabel1.queue.ToArray()
                                   .Concat(_classLabel2.queue.ToArray())
                                   .Concat(_classLabel3.queue.ToArray())
                                   .Concat(_classLabel4.queue.ToArray())
                                   .Take(slidingWindowSize * 4)

                   //.Union(_eyeHitPosInOut1.queue.ToArray())
                   //.Union(_eyeHitPosInOut2.queue.ToArray())
                   //.Union(_eyeHitPosInOut3.queue.ToArray())
                   //.Union(_eyeHitPosInOut4.queue.ToArray())
                   //.Union(_rightIndexTipPosInOut1.queue.ToArray())
                   //.Union(_rightIndexTipPosInOut2.queue.ToArray())
                   //.Union(_rightIndexTipPosInOut3.queue.ToArray())
                   //.Union(_rightIndexTipPosInOut4.queue.ToArray())
                   //.Union(_leftIndexTipPosInOut1.queue.ToArray())
                   //.Union(_leftIndexTipPosInOut2.queue.ToArray())
                   //.Union(_leftIndexTipPosInOut3.queue.ToArray())
                   //.Union(_leftIndexTipPosInOut4.queue.ToArray())
                   .ToArray();

        }
    }
}
