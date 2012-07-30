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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using NooSphere.Core.ActivityModel;
using System.Windows.Interop;
using NooSphere.Platform.Windows.Interopt;
using NooSphere.Core.Devices;

namespace ActivityUI.PopUp
{
    /// <summary>
    /// Interaction logic for PopUpWindow.xaml
    /// </summary>
    public partial class DeviceWindow : Window
    {
        #region Window Hacks

        private const int GWL_STYLE = -16;
        private const uint WS_SYSMENU = 0x80000;

        protected override void OnSourceInitialized(EventArgs e)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            User32.SetWindowLong(hwnd, GWL_STYLE,
                User32.GetWindowLong(hwnd, GWL_STYLE) & (0xFFFFFFFF ^ WS_SYSMENU));

            base.OnSourceInitialized(e);
        }
        #endregion

        private ActivityBar taskbar;
        public DeviceWindow(ActivityBar bar)
        {
            InitializeComponent();
            this.Topmost = true;
            this.ShowInTaskbar = false;
            this.WindowStyle = System.Windows.WindowStyle.None;
            this.ResizeMode = System.Windows.ResizeMode.CanResize;
            this.MinHeight = this.MaxHeight = this.Height;
            this.MinWidth = this.MaxWidth = this.Width;
            this.taskbar = bar;
        }
        public void Show(int offset,List<Device> devices)
        {
            if (offset + this.Width > System.Windows.SystemParameters.PrimaryScreenWidth)
                this.Left = System.Windows.SystemParameters.PrimaryScreenWidth-this.Width;
            else
                this.Left = offset;
            this.Top = taskbar.Height+5;
            this.Show();
            VisualizeDevices(devices);
        }
        
        private void VisualizeDevices(List<Device> devices)
        {
            btnTabletop.Visibility = System.Windows.Visibility.Hidden;
            btnPhone.Visibility = System.Windows.Visibility.Hidden;
            btnLaptop.Visibility = System.Windows.Visibility.Hidden;
            btnTablet.Visibility = System.Windows.Visibility.Hidden;

            foreach (Device dev in devices)
            {
                if (dev.DeviceType == DeviceType.Tabletop)
                    btnTabletop.Visibility = System.Windows.Visibility.Visible;
                else if (dev.DeviceType == DeviceType.SmartPhone)
                    btnPhone.Visibility = System.Windows.Visibility.Visible;
                else if (dev.DeviceType == DeviceType.Laptop)
                    btnLaptop.Visibility = System.Windows.Visibility.Visible;
                else if (dev.DeviceType == DeviceType.Tablet)
                    btnTablet.Visibility = System.Windows.Visibility.Visible;
            }
        }
        private void btnDone_Click(object sender, RoutedEventArgs e)
        {

            this.Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            taskbar.DeleteActivity();
            this.Hide();
        }
    }
}