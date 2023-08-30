// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
#define WINDOWS_UWP
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;

namespace SharedResultsBetweenServerAndHoloLens
{
    public class PersistenceHelper
    {
        // Convert an object to a byte array
        public static byte[] DetectionResultListToByteArray<T>(List<T> obj) where T : DetectionResult
        {
            try
            {
                //MemoryStream ms = new MemoryStream();
                //using (BsonDataWriter writer = new BsonDataWriter(ms))
                //{
                //    JsonSerializer serializer = new JsonSerializer();
                //    serializer.Serialize(writer, obj);
                //}
                //return ms.ToArray();
                return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(obj));

            } catch (System.Exception e)  
            {
                // if for some reason the object can not be converted to a byte[]
                // just return the byte array of an empty result object
                Debug.WriteLine("Exception caught: {0}", e);
                return null;
            }
        }

        // Convert a byte array to an Object
        public static List<T> ByteArrayToDetectionResultList<T>(byte[] arrBytes) where T : DetectionResult
        {
            return JsonConvert.DeserializeObject<List<T>>(Encoding.ASCII.GetString(arrBytes));
            //try
            //{
            //MemoryStream ms = new MemoryStream(arrBytes);
            //using (BsonDataReader reader = new BsonDataReader(ms))
            //{
            //    reader.ReadRootValueAsArray = true;

            //    JsonSerializer serializer = new JsonSerializer();

            //    return serializer.Deserialize<List<T>>(reader);

            //}

            //            } catch(System.Exception e) 
            //            {
            //                // if for some reason the bytes can not be converted to a list of detection results
            //                // just return an empty list

            //#if WINDOWS_UWP
            //                Debug.WriteLine($"{e.Message}", e);
            //#else
            //                Debug.Log($"Error in deserializing results: {e.Message}");
            //#endif
            //                return new List<T>(); 
            //            }
}
    }
}
