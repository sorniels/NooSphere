﻿/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using NooSphere.Core.Devices;

namespace NooSphere.ActivitySystem.Base
{
    public class DeviceEventArgs
    {
        public Device Device { get; set; }
        public DeviceEventArgs() { }
        public DeviceEventArgs(Device device)
        {
            Device = device;
        }
    }
    public class DeviceRemovedEventArgs
    {
        public string Id { get; set; }
        public DeviceRemovedEventArgs() { }
        public DeviceRemovedEventArgs(string id)
        {
            Id = id;
        }
    }
}
