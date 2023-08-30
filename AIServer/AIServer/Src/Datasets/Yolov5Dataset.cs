// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using YamlDotNet.Serialization.NamingConventions;

namespace AIServer.Datasets
{
    /**
    <summary>
    This abstraction is important. The Yolov5 Dataset format reads a Yaml file.
    In this project we use Roboflow for automatically creating such a dataset.
    </summary>
    */
    public abstract class Yolov5Dataset : Dataset
    {

        private Yolov5DatasetConfig _config;

        protected Yolov5Dataset(string yamlFileName)
        {
            var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                                        .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                        .Build();

            _config = deserializer.Deserialize<Yolov5DatasetConfig>(File.ReadAllText("Assets/" + yamlFileName));

        }

        public override string[] Labels
        {
            get
            {
                return _config.names;
            }
        }
    }
}
