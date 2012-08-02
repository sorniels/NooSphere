﻿/// <licence>
/// 
/// (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)
/// 
/// Pervasive Interaction Technology Laboratory (pIT lab)
/// IT University of Copenhagen
///
/// This library is free software; you can redistribute it and/or 
/// modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
/// as published by the Free Software Foundation. Check 
/// http://www.gnu.org/licenses/gpl.html for details.
/// 
/// </licence>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NooSphere.ActivitySystem.Events
{
    /// <summary>
    /// Events used to distributed activity model
    /// </summary>
    public delegate void ActivityAddedHandler(Object sender, ActivityEventArgs e);
    public delegate void ActivityRemovedHandler(Object sender, ActivityRemovedEventArgs e);
    public delegate void ActivityChangedHandler(Object sender, ActivityEventArgs e);

    /// <summary>
    /// Device events
    /// </summary>
    public delegate void DeviceRoleChangedHandler(Object sender, DeviceEventArgs e);
    public delegate void DeviceAddedHandler(Object sender, DeviceEventArgs e);
    public delegate void DeviceRemovedHandler(Object sender, DeviceRemovedEventArgs e);

    /// <summary>
    /// File Events used by local File Server
    /// </summary>
    public delegate void FileChangedHandler(Object sender, FileEventArgs e);
    public delegate void FileAddedHandler(Object sender, FileEventArgs e);
    public delegate void FileRemovedHandler(Object sender, FileEventArgs e);
    public delegate void FileLockedHandler(Object sender, FileEventArgs e);

    /// <summary>
    /// File events used by cloud infrastructure
    /// </summary>
    public delegate void FileUploadedHandler(Object sender, FileEventArgs e);
    public delegate void FileDownloadedHandler(Object sender,FileEventArgs e);
    public delegate void FileDeletedHandler(Object sender, FileEventArgs e);

    /// <summary>
    /// Message event
    /// </summary>
    public delegate void MessageReceivedHandler(Object sender, ComEventArgs e);

    /// <summary>
    /// Connection event
    /// </summary>>
    public delegate void ConnectionEstablishedHandler(Object sender,EventArgs e);
    
    /// <summary>
    /// Participant event
    /// </summary>
    public delegate void ParticipantAddedHandler(Object sender, ParticipantEventArgs e);
    public delegate void ParticipantRemovedHandler(Object sender, ParticipantEventArgs e);

    /// <summary>
    /// Friendlist event
    /// </summary>
    public delegate void FriendAddedHandler(Object sender, FriendEventArgs e);
    public delegate void FriendRemovedHander(Object sender, FriendEventArgs e);
    public delegate void FriendDeletedHandler(Object sender, FriendDeletedEventArgs e);
    public delegate void FriendRequestReceivedHandler(Object sender, FriendEventArgs e);

}
