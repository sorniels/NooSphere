﻿/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using System.IO;
using System.ServiceModel;
using System;
using System.Collections.Generic;
using System.Threading;
using System.ServiceModel.Description;
using NooSphere.ActivitySystem.Contracts;
using NooSphere.Core.ActivityModel;
using NooSphere.Core.Devices;
using NooSphere.Helpers;
using Newtonsoft.Json;
using NooSphere.ActivitySystem.FileServer;

namespace NooSphere.ActivitySystem.Base
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ActivityClient : NetEventHandler,IActivityNode
    {
        #region Events
        public event ConnectionEstablishedHandler ConnectionEstablished = null;
        public event InitializedHandler Initialized = null;
        #endregion

        #region Private Members
        private ServiceHost _callbackService;
        private FileService _fileServer;
        #endregion

        #region Properties
        public string Ip { get; set; }
        public string ClientName { get; set; }
        public string DeviceId { get; private set; }
        public string ServiceAddress { get; set; }
        public User CurrentUser { get; set; }
        public string LocalPath { get { return _fileServer.BasePath; } }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="address">The address of the service the client needs to connect to</param>
        /// <param name="localFileDirectory">The local file directory of the client</param>
        public ActivityClient(string address,string localFileDirectory)
        {
            Connect(address);
            InitializeFileService(localFileDirectory);
            OnInitializedEvent(new EventArgs());

            FileUploadRequest += ActivityClientFileUploadRequest;
        }

        /// <summary>
        /// Initializes the File Service
        /// </summary>
        /// <param name="localPath">Path where the file service stores files</param>
        private void InitializeFileService(string localPath)
        {
            _fileServer = new FileService(localPath);
            //_fileServer.FileAdded += FileServerFileAdded;
            //_fileServer.FileChanged += FileServerFileChanged;
            //_fileServer.FileRemoved += FileServerFileRemoved;
            //_fileServer.FileDownloadedFromCloud += FileServerFileDownloaded;
            Console.WriteLine("ActivityClient: FileStore initialized at {0}", _fileServer.BasePath);
        }
        private void ActivityClientFileUploadRequest(object sender, FileEventArgs e)
        {
            UploadResource(e.Resource);
        }

        #endregion

        #region Private Methods


        private void UploadResource(Resource r)
        {
            Rest.SendStreamingRequest(ServiceAddress + "Files/" + r.ActivityId + "/" + r.Id,
                                      _fileServer.BasePath + r.RelativePath);
            Console.WriteLine("ActivityClient: Received Request to upload {0}", r.Name);
        }
        /// <summary>
        /// Tests the connection to the service
        /// </summary>
        /// <param name="addr">The address of the service</param>
        private void TestConnection(string addr)
        {
            Console.WriteLine("ActivityClient: Attempt to connect to {0}",addr);
            bool res;
            var attempts = 0;
            const int maxAttemps = 20;
            do
            {
                ServiceAddress = addr;
                res = JsonConvert.DeserializeObject<bool>(Rest.Get(ServiceAddress));
                Console.WriteLine("ActivityClient: Service active? -> {0}", res);
                Thread.Sleep(100);
                attempts++;
            }
            while (res == false && attempts<maxAttemps);
            if (res)
                OnConnectionEstablishedEvent(new EventArgs());
            else
                throw new Exception("ActivityClient: Could not connect to: " + addr);
        }

        /// <summary>
        /// Connects the client to the activity service
        /// </summary>
        /// <param name="address">The address of the service</param>
        private void Connect(string address)
        {
            Ip = Net.GetIp(IPType.All);
            TestConnection(address);
        }

        /// <summary>
        /// Starts a callback service. The activity manager uses this service to publish
        /// events.
        /// </summary>
        /// <param name="service">The type of callback service</param>
        /// <returns>The port of the deployed service</returns>
        private int StartCallbackService()
        {
            var port = Net.FindPort();

            _callbackService = new ServiceHost(this);
            var se = _callbackService.AddServiceEndpoint(typeof(INetEvent), new WebHttpBinding(), Net.GetUrl(Ip, port, ""));
            se.Behaviors.Add(new WebHttpBehavior());
            try
            {
                _callbackService.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ActivityClient: error launching callback service: " + ex);
                throw new ApplicationException(ex.ToString());
            }
            return port;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Register a given device with the activity client
        /// </summary>
        /// <param name="d">The device that needs to be registered with the activity client</param>
        public void Register(Device d)
        {
            d.BaseAddress = Net.GetUrl(Net.GetIp(IPType.All), StartCallbackService(),"").ToString();
            DeviceId = JsonConvert.DeserializeObject<String>(Rest.Post(ServiceAddress + Url.Devices, d));
            Console.WriteLine("ActivityClient: Received device id: " + DeviceId);
        }
        
        /// <summary>
        /// Unregister main device from the activity client
        /// </summary>
        public void Unregister()
        {
            Rest.Delete(ServiceAddress + Url.Devices, DeviceId);
        }

        /// <summary>
        /// Sends an "add activity" request to the activity manager
        /// </summary>
        /// <param name="act">The activity that needs to be included in the request</param>
        public void AddActivity(Activity act)
        {
            Rest.Post(ServiceAddress + Url.Activities, new {act,deviceId=DeviceId});
        }

        /// <summary>
        /// Get the file byte array from the resource
        /// </summary>
        /// <param name="resource">The resource describing the file</param>
        /// <returns>A byte array representing the file</returns>
        private byte[] GetFile(Resource resource)
        {
            var fi = new FileInfo(_fileServer.BasePath + resource.RelativePath);
            var buffer = new byte[fi.Length];

            using (var fs = new FileStream(_fileServer.BasePath + resource.RelativePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                fs.Read(buffer, 0, (int)fs.Length);
            return buffer;
        }


        /// <summary>
        /// Sends a "Remove activity" request to the activity manager
        /// </summary>
        /// <param name="activityId">The id (of the activity) that needs to be included in the request</param>
        public void RemoveActivity(Guid activityId)
        {
            Rest.Delete(ServiceAddress + Url.Activities, new { activityId, deviceId = DeviceId });
        }

        /// <summary>
        /// Sends an "Update activity" request to the activity manager
        /// </summary>
        /// <param name="act">The activity that needs to be included in the request</param>
        public void UpdateActivity(Activity act)
        {
            Rest.Put(ServiceAddress + Url.Activities, new { act, deviceId = DeviceId });
        }

        /// <summary>
        /// Sends a "Get Activities" request to the activity manager
        /// </summary>
        /// <returns>A list of retrieved activities</returns>
        public List<Activity> GetActivities()
        {
            var res = Rest.Get(ServiceAddress + Url.Activities);
            return JsonConvert.DeserializeObject<List<Activity>>(res);
        }

        /// <summary>
        /// Sends a "Get Activity" request to the activity manager
        /// </summary>
        /// <param name="activityId">The id (of the activity) that needs to be included in the request</param>
        /// <returns></returns>
        public Activity GetActivity(string activityId)
        {
            return JsonConvert.DeserializeObject<Activity>(Rest.Get(ServiceAddress + Url.Activities + "/" + activityId));
        }

        /// <summary>
        /// Sends a "Send Message" request to the activity manager
        /// </summary>
        /// <param name="msg">The message that needs to be included in the request</param>
        public void SendMessage(string msg)
        {
            Rest.Post(ServiceAddress + Url.Messages, new { message = msg, deviceId = DeviceId });
        }

        /// <summary>
        /// Gets all users in the friendlist
        /// </summary>
        /// <returns>A list with all users in the friendlist</returns>
        public List<User> GetUsers()
        {
            return JsonConvert.DeserializeObject<List<User>>(Rest.Get(ServiceAddress + Url.Users)); 
        }

        /// <summary>
        /// Request friendship with another user
        /// </summary>
        /// <param name="email">The email of the user that needs to be friended</param>
        public void RequestFriendShip(string email)
        {
            JsonConvert.DeserializeObject<List<User>>(Rest.Post(ServiceAddress + Url.Users, new { email, deviceId = DeviceId })); 
        }

        /// <summary>
        /// Removes a user from the friendlist
        /// </summary>
        /// <param name="friendId">The id of the friend that needs to be removed</param>
        public void RemoveFriend(Guid friendId)
        {
            JsonConvert.DeserializeObject<List<User>>(Rest.Delete(ServiceAddress + Url.Users, new { friendId, deviceId = DeviceId })); 
        }

        /// <summary>
        /// Sends a response on a friend request from another user
        /// </summary>
        /// <param name="friendId">The id of the friend that is requesting friendship</param>
        /// <param name="approval">Bool that indicates if the friendship was approved</param>
        public void RespondToFriendRequest(Guid friendId, bool approval)
        {
            Rest.Put(ServiceAddress + Url.Users, new{friendId, approval, deviceId = DeviceId});
        }

        #endregion

        #region Internal Event Handlers
        protected void OnConnectionEstablishedEvent(EventArgs e)
        {
            if (ConnectionEstablished != null)
                ConnectionEstablished(this, e);
        }
        protected void OnInitializedEvent(EventArgs e)
        {
            if (Initialized != null)
                Initialized(this, e);
        }
        #endregion

    }
    public enum Url
    {
        Activities,
        Devices,
        Subscribers,
        Messages,
        Users,
        Files
    }
}
