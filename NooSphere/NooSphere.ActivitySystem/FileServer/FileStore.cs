﻿/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using NooSphere.Core.ActivityModel;
using NooSphere.ActivitySystem.Base;
using System.Threading;

namespace NooSphere.ActivitySystem.FileServer
{
    public class FileStore
    {
        #region Events
        public event FileAddedHandler FileAdded;
        public event FileChangedHandler FileChanged;
        public event FileRemovedHandler FileRemoved;
        public event FileDownloadRequestHandler FileDownloadedFromCloud;     
        #endregion

        #region Properties
        public string BasePath { get; set; }
        #endregion

        #region Private Members
        private readonly Dictionary<Guid, Resource> _files = new Dictionary<Guid, Resource>();
        #endregion

        #region Public Methods
        public FileStore(string path)
        {
            BasePath = path;
        }
        public void AddFile(Resource resource, byte[] fileInBytes,FileSource source)
        {
            var t = new Thread(() =>
            {
                SaveToDisk(fileInBytes, resource);
                _files.Add(resource.Id, resource);

                if (source == FileSource.Cloud)
                {
                    if (FileDownloadedFromCloud != null)
                        FileDownloadedFromCloud(this, new FileEventArgs(resource));
                }
                else if (source == FileSource.Local)
                {
                    if (FileAdded != null)
                        FileAdded(this, new FileEventArgs(resource));
                }
                Console.WriteLine("FileStore: Added file {0} to store", resource.Name); 
            }) {IsBackground = true};
            t.Start();
        }
        public void RemoveFile(Resource resource)
        {
            _files.Remove(resource.Id);
            File.Delete(BasePath+resource.RelativePath);
            if (FileRemoved != null)
                FileRemoved(this, new FileEventArgs(resource));
            Console.WriteLine("FileStore: Removed file {0} from store", resource.Name); 
        }
        public byte[] GetFile(Resource resource)
        {
            var fi = new FileInfo(BasePath + resource.RelativePath);
            var buffer = new byte[fi.Length];

            using (var fs = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read))
                fs.Read(buffer, 0, (int)fs.Length);

            return buffer;
        }
        public void Updatefile(Resource resource, byte[] fileInBytes)
        {
            var t = new Thread(() =>
            {
                _files[resource.Id] = resource;
                SaveToDisk(fileInBytes,resource);
                if (FileChanged != null)
                    FileChanged(this, new FileEventArgs(resource));
                Console.WriteLine("FileStore: Updated file {0} in store", resource.Name); 
            }) {IsBackground = true};
            t.Start();
        }
        #endregion

        #region Private Methods
        private void SaveToDisk(byte[] fileInBytes, Resource resource)
        {
            try
            {
                string path = BasePath + resource.RelativePath;
                using (var fileToupload = new FileStream(path, FileMode.Create))
                {
                    fileToupload.Write(fileInBytes, 0, fileInBytes.Length);
                    fileToupload.Close();
                    fileToupload.Dispose();

                    File.SetCreationTimeUtc(path, DateTime.Parse(resource.CreationTime));
                    File.SetLastWriteTimeUtc(path, DateTime.Parse(resource.LastWriteTime));
                    Console.WriteLine("FileStore: Saved file {0} to disk at {1}", resource.Name,path); 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }

        }
        #endregion
    }
}