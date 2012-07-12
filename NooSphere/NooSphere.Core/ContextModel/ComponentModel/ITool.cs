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
using NooSphere.Core.Primitives;

namespace NooSphere.Core.ContextModel.ComponentModel
{
    public interface ITool:IEntity
    {
        ToolType Type { get; set; }
        ToolState State { get; set; }
    }
    public enum ToolType
    {
        Physical,
        Digital,
        Mediating
    }
    public interface ToolState{}
}
