using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.AI.MachineLearning;
using Windows.Media;
using Windows.Storage;

namespace AIServer.Src.UserGuidance
{
    public class UserGuidance
    {
        protected LearningModel model;
        protected LearningModelSession session;
        protected LearningModelBinding binding;

        private string[] _inputs;
        protected string[] _outputs = new string[] { "output_probability", "output_label" };
        private string[] _userIntentionLabel = null;

        public UserGuidance() {

            _userIntentionLabel = UserIntention.getClasses();
            _inputs = new string[] { "eye_data",
                                     "hand_data",
                                     "head_data",
                                     "class_labels"};
        }

        public string Evaluate(UserData userData)
        {
            // all important stuff is listet here: https://learn.microsoft.com/de-de/windows/ai/windows-ml/bind-a-model
            // we have to normalize our frame input first; so you HAVE to first create a normalized tensor!
            // Tensor layout allways follows: NxCxHxW
            // N=Batch size, C=#Channels, H=Height, W=Width

            string detection_label = "NoIntention";

            if (userData.isFull())
            {

                float[] eyeDataBurstWindow = userData.getEyeDataBurstWindowPresentation();
                float[] handDataBurstWindow = userData.getHandDataBurstWindowPresentation();
                float[] headDataBurstWindow = userData.getHeadDataBurstWindowPresentation();
                float[] nnDataBurstWindow = userData.getNNDataBurstWindowPresentation();

                var eyeBurstTensor = TensorFloat.CreateFromArray(new[] { (long)1, (long)eyeDataBurstWindow.Length },
                                                                 eyeDataBurstWindow);

                var handBurstTensor = TensorFloat.CreateFromArray(new[] { (long)1, (long)handDataBurstWindow.Length },
                                                                  handDataBurstWindow);

                var headBurstTensor = TensorFloat.CreateFromArray(new[] { (long)1, (long)headDataBurstWindow.Length },
                                                                  headDataBurstWindow);

                var nnBurstTensor = TensorFloat.CreateFromArray(new[] { (long)1, (long)nnDataBurstWindow.Length },
                                                                nnDataBurstWindow);

                binding.Clear();

                binding.Bind(_inputs[0], eyeBurstTensor);
                binding.Bind(_inputs[1], handBurstTensor);
                binding.Bind(_inputs[2], headBurstTensor);
                binding.Bind(_inputs[3], nnBurstTensor);

                var results = session.Evaluate(binding, "");

                TensorInt64Bit result = results.Outputs[_outputs[1]] as TensorInt64Bit;
                var data = result.GetAsVectorView();
                var data_arr = data.ToArray();

                uint label_index = (uint)data_arr[0];

                detection_label = _userIntentionLabel[label_index];

            }

            return detection_label;

        }

        public async Task LoadModelAsync()
        {
            string ModelAssetFile = "UserGuidanceAIMLP.onnx";
            // Load a machine learning model
            var model_file = await StorageFile.GetFileFromApplicationUriAsync(
                new Uri("ms-appx:///Assets/" + ModelAssetFile)
                );
            model = await LearningModel.LoadFromStorageFileAsync(model_file);
            var device = new LearningModelDevice(LearningModelDeviceKind.Cpu);
            session = new LearningModelSession(model, device);
            binding = new LearningModelBinding(session);
        }
    }
}
