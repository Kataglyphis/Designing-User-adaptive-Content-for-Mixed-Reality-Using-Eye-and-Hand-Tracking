﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

namespace HoloLensUserGuidance.EyeTracking.Logging
{
    [AddComponentMenu("Scripts/HoloLensUserGuidance/UserInputRecorder")]
    public class UserInputRecorder : CustomInputLogger
    {
        public string FilenameToUse = $"test{Path.DirectorySeparatorChar}folder";

        [SerializeField]
        private LogStructure logStructure = null;

        private bool automaticLogging = true;

        #region Singleton
        private static UserInputRecorder instance;
        public static UserInputRecorder Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<UserInputRecorder>();
                }
                return instance;
            }
        }
        #endregion      

        EyeTrackingTarget prevTarget = null;

        public override string GetHeader()
        {
            if (logStructure != null)
            {
                string[] header_columns = logStructure.GetHeaderColumns();
                string header_format = GetStringFormat(header_columns);
                return String.Format(header_format, header_columns);
            }
            else
                return "";
        }

        public override string GetDescription()
        {
            if (logStructure != null)
            {
                string[] description_columns = logStructure.GetDescriptionColumns();
                string header_format = GetStringFormat(description_columns);
                return String.Format(header_format, description_columns);
            }
            else
                return "";
        }

        // Todo: Put into BasicLogger?
        protected object[] GetData_Part1()
        {

            var now = DateTime.UtcNow;

            object[] data = new object[]
            {
                // UserId
                UserName,
                // SessionType
                sessionDescr,
                // Timestamp
                (now - TimerStart).TotalMilliseconds
            };

            TimerStart = DateTime.UtcNow;

            return data;
        }

        // Todo: Put into generic utils class?
        public object[] MergeObjArrays(object[] part1, object[] part2)
        {
            object[] data = new object[part1.Length + part2.Length];
            part1.CopyTo(data, 0);
            part2.CopyTo(data, part1.Length);
            return data;
        }

        protected override string GetFileName()
        {
            if (!string.IsNullOrEmpty(FilenameToUse))
            {
                return FilenameToUse;
            }

            return String.Format("{0}-{1}", sessionDescr, UserName);
        }

        private string LimitStringLength(string str, int maxLength)
        {
            if (str.Length < maxLength)
                return str;
            else
            {
                return str.Substring(0, maxLength);
            }
        }

        public static string GetStringFormat(object[] data)
        {
            string strFormat = "";
            for (int i = 0; i < data.Length - 1; i++)
            {
                strFormat += ("{" + i + "}" + System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator + " ");
            }
            strFormat += ("{" + (data.Length - 1) + "}");
            return strFormat;
        }

        public void UpdateLog(string inputType, string inputStatus, EyeTrackingTarget intendedTarget)
        {
            if ((Instance != null) && (isLogging))
            {
                if (logStructure != null)
                {
                    object[] data = MergeObjArrays(GetData_Part1(), logStructure.GetData(inputType, inputStatus, intendedTarget));
                    string data_format = GetStringFormat(data);
                    Instance.CustomAppend(String.Format(data_format, data));
                    prevTarget = intendedTarget;
                }
            }
        }

        #region Remains the same across different loggers
        protected override void CustomAppend(string msg)
        {
            base.CustomAppend(msg);
        }
        #endregion

        public void UpdateLog()
        {
            UpdateLog("", "", null);
        }

        void Update()
        {
            if (automaticLogging)
            {
                UpdateLog();
            }
        }

        public override void OnDestroy()
        {
            // Disable listening to user input
            if (UserInputRecorder.Instance != null)
            {
                UserInputRecorder.Instance.StopLoggingAndSave();
            }

            base.OnDestroy();
        }
    }
}