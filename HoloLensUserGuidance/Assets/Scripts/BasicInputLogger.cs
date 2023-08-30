﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Text;
using UnityEngine;

#if WINDOWS_UWP
using Windows.Storage;
#endif

namespace HoloLensUserGuidance.EyeTracking.Logging
{
    public abstract class BasicInputLogger : MonoBehaviour
    {
        public bool addTimestampToLogfileName = false;

        public void SetUserName(string name)
        {
            UserName = name;
            Debug.Log("New user name: " + name);
        }

        public void SetSessionDescr(string descr)
        {
            sessionDescr = descr;
            Debug.Log("New session name: " + descr);
        }

        public string UserName = "tester";

        [SerializeField]
        protected string sessionDescr = "Session00";

        public string LogDirectory => Path.Combine("UserGuidance", UserName);

        public abstract string GetHeader();

        public abstract string GetDescription();

#if WINDOWS_UWP
        private StorageFile logFile;
        private StorageFolder logRootFolder = KnownFolders.MusicLibrary;
        private StorageFolder sessionFolder;
#endif

        internal bool isLogging = false;
        protected abstract string GetFileName();
        private StringBuilder buffer = null;

#if WINDOWS_UWP
        protected virtual async void CreateNewLogFile()
        {
            try
            {
                Debug.Log(logRootFolder);
                Debug.Log(">> BasicInputLogger.CreateNewLogFile:  " + logRootFolder.ToString());
                if (logRootFolder != null)
                {
                    string fullPath = Path.Combine(logRootFolder.Path, LogDirectory);
                    Debug.LogFormat("Does directory already exist {0} --\nLogRootFolder: {2} \n {1}", Directory.Exists(fullPath), fullPath, logRootFolder.Path);

                    try
                    {
                        if (!Directory.Exists(fullPath))
                        {
                            Debug.LogFormat("Trying to create new directory..");
                            Debug.LogFormat("Full path: " + fullPath);
                            sessionFolder = await logRootFolder.CreateFolderAsync(LogDirectory, CreationCollisionOption.OpenIfExists);
                            
                        }

                        sessionFolder = await logRootFolder.GetFolderAsync(LogDirectory);
                        logFile = await sessionFolder.CreateFileAsync(Filename, CreationCollisionOption.ReplaceExisting);

                        Debug.Log(string.Format("*** Create log file to: {0} -- \n -- {1}", sessionFolder.Name, sessionFolder.Path));
                        Debug.Log(string.Format("*** The log file path is: {0} -- \n -- {1}", logFile.Name, logFile.Path));
                    }
                    catch (FileNotFoundException)
                    {
                        sessionFolder = await logRootFolder.CreateFolderAsync(LogDirectory, CreationCollisionOption.GenerateUniqueName);
                    }
                    catch (DirectoryNotFoundException) { }
                    catch { }
                }
            }
            catch (Exception e)
            {
                Debug.Log(string.Format("Exception in BasicLogger: {0}", e.Message));
            }
        }
#endif

        void CheckIfInitialized()
        {
            if (buffer == null)
                ResetLog();
        }

        public void ResetLog()
        {
            buffer = new StringBuilder();
            buffer.Length = 0;
#if WINDOWS_UWP
            CreateNewLogFile();
#endif
        }

        private string FormattedTimeStamp
        {
            get
            {
                string year = (DateTime.Now.Year - 2000).ToString();
                string month = AddLeadingZeroToSingleDigitIntegers(DateTime.Now.Month);
                string day = AddLeadingZeroToSingleDigitIntegers(DateTime.Now.Day);
                string hour = AddLeadingZeroToSingleDigitIntegers(DateTime.Now.Hour);
                string minute = AddLeadingZeroToSingleDigitIntegers(DateTime.Now.Minute);
                string sec = AddLeadingZeroToSingleDigitIntegers(DateTime.Now.Second);

                return string.Format("{0}{1}{2}-{3}{4}{5}",
                    year,
                    month,
                    day,
                    hour,
                    minute,
                    sec);
            }
        }

        private string AddLeadingZeroToSingleDigitIntegers(int val)
        {
            return (val < 10) ? ("0" + val) : ("" + val);
        }


        #region Append log
        public bool Append(string msg)
        {
            CheckIfInitialized();

            if (isLogging)
            {
                // post IO to a separate thread.
                this.buffer.AppendLine(msg);
                return true;
            }
            return false;
        }
        #endregion

#if WINDOWS_UWP
        public async void LoadLogs()
        {
            try
            {
                if (logRootFolder != null)
                {
                    string fullPath = Path.Combine(logRootFolder.Path, LogDirectory);

                    try
                    {
                        if (!Directory.Exists(fullPath))
                        {
                            return;
                        }

                        sessionFolder = await logRootFolder.GetFolderAsync(LogDirectory);
                        logFile = await sessionFolder.GetFileAsync(Filename);


                    }
                    catch (FileNotFoundException)
                    {
                        sessionFolder = await logRootFolder.CreateFolderAsync(LogDirectory, CreationCollisionOption.GenerateUniqueName);
                    }
                    catch (DirectoryNotFoundException) { }
                    catch (Exception) { }
                }
            }
            catch (Exception e)
            {
                Debug.Log(string.Format("Exception in BasicLogger to load log file: {0}", e.Message));
            }
        }
#endif

#if WINDOWS_UWP
        public async void SaveLogs()
        {
            if (isLogging)
            {
                if (buffer.Length > 0)
                {
                    // Log buffer to the file
                    Debug.Log("SAVE LOGS to " + Filename);
                    CreateNewLogFile();
                    await FileIO.AppendTextAsync(logFile, buffer.ToString());
                }
            }
        }
#else
        public void SaveLogs()
        {
            if (isLogging)
            {
                // Create new Stream writer
                using (var writer = new StreamWriter(Filename))
                {
                    Debug.Log("SAVE LOGS to " + Filename);
                    writer.Write(this.buffer.ToString());
                }
            }
        }
#endif

        private string Filename
        {
            get
            {
                if (addTimestampToLogfileName)
                    return FilenameWithTimestamp;
                else
                    return FilenameNoTimestamp;
            }
        }

        protected string FilenameWithTimestamp
        {
            get { return (FormattedTimeStamp + "_" + FilenameNoTimestamp); }
        }

        protected string FilenameNoTimestamp
        {
            get { return GetFileName() + ".csv"; }
        }

        public virtual void OnDestroy()
        {
            if (isLogging)
                SaveLogs();
        }
    }
}
