/* 
Application for evaluation of facial symmetry using Microsoft Kinect v2.
Copyright(C) 2017  Sedlák Vojtěch (Vojta.sedlak@gmail.com)

This file is part of FaceSymmetry. 

FaceSymmetry is free software: you can redistribute it and/or modify 
it under the terms of the GNU General Public License as published by 
the Free Software Foundation, either version 3 of the License, or 
(at your option) any later version. 

FaceSymmetry is distributed in the hope that it will be useful, 
but WITHOUT ANY WARRANTY; without even the implied warranty of 
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
GNU General Public License for more details. 

You should have received a copy of the GNU General Public License 
along with Application for evaluation of facial symmetry using Microsoft Kinect v2.. If not, see <http://www.gnu.org/licenses/>.
 */ 

using Common;
using System;
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.ComponentModel;

namespace Common
{
    public enum MessageBoxFSImage
    {
        Warning,
        Error,
        Information,
        Question,
    }

    public enum MessageBoxFSButton
    {
        OK,
        YESNO,

    }
    
   
    public partial class MessageBoxFS : Window
    {
        private bool _canClose = false;
        public ProgressBar ProgressBar { get; set; }

        public MessageBoxFS(string title, string message, MessageBoxFSButton button, MessageBoxFSImage image)
        {
            InitializeComponent();
            _canClose = true;

            this.Title = title;
            messageTxtBox.Text = message;

            IntPtr imageHandle = GetImageHandle(image);
            this.image.Source = Imaging.CreateBitmapSourceFromHIcon(imageHandle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            switch (button)
            {
                case MessageBoxFSButton.OK:
                    CreateButtonOK();
                    break;
                case MessageBoxFSButton.YESNO:
                    CreateButtonYES();
                    CreateButtonNO();
                    break;
                default:
                    break;
            }
        }


        /// <summary>
        /// Progress bar
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        public MessageBoxFS(string title, string message)
        {
            InitializeComponent();

            WindowStyle = WindowStyle.None;
            this.Title = title;
            messageTxtBox.Text = message;

            IntPtr imageHandle = GetImageHandle(MessageBoxFSImage.Information);
            this.image.Source = Imaging.CreateBitmapSourceFromHIcon(imageHandle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());


            ProgressBar = new ProgressBar();
            ProgressBar.IsIndeterminate = true;
            ProgressBar.MinHeight = 35;
            ProgressBar.MinWidth = 300;
            ProgressBar.VerticalAlignment = VerticalAlignment.Center;
            ProgressBar.HorizontalAlignment = HorizontalAlignment.Center;
            ProgressBar.Minimum = 0;
            ProgressBar.Maximum = 100;
            ProgressBar.Value = 0;
            ProgressBar.Margin = new Thickness(10, 5, 10, 10);
            buttonStackPanel.Children.Add(ProgressBar);
        }

        public void UpdateProgressBar(int value)
        {
            ProgressBar.Dispatcher.Invoke(new Action(() => { ProgressBar.Value = value; }), DispatcherPriority.Render);
        }

        public void UpdateText(string value)
        {
            Dispatcher.BeginInvoke(new Action(() => { messageTxtBox.Text = value; }), DispatcherPriority.Background);
        }


        private Button CreateButton()
        {
            Button butt = new Button();
            butt.Height = 35;
            butt.Margin = new Thickness(5);
            butt.VerticalAlignment = VerticalAlignment.Center;
            butt.HorizontalAlignment = HorizontalAlignment.Center;
            butt.Style = new Style(typeof(Button), (Style)FindResource("ButtonStyleGreen"));
            return butt;
        }

        private void CreateButtonOK()
        {
            Button buttonOK = CreateButton();
            buttonOK.Content = "OK";
            buttonOK.Click += OKButton_Click;
            buttonOK.HorizontalAlignment = HorizontalAlignment.Center;
            buttonStackPanel.Children.Add(buttonOK);

        }
        private void CreateButtonYES()
        {
            Button buttonYES = CreateButton();
            buttonYES.Content = "YES";
            buttonYES.Click += YESButton_Click;
            buttonYES.HorizontalAlignment = HorizontalAlignment.Center;
            buttonYES.VerticalAlignment = VerticalAlignment.Center;
            buttonStackPanel.Children.Add(buttonYES);
        }

        private void CreateButtonNO()
        {
            Button buttonNO = CreateButton();
            buttonNO.Content = "NO";
            buttonNO.Click += NOButton_Click;
            buttonNO.HorizontalAlignment = HorizontalAlignment.Center;
            buttonNO.VerticalAlignment = VerticalAlignment.Center;
            buttonStackPanel.Children.Add(buttonNO);
        }


        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
        private void YESButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private void NOButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }       

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_canClose)
                e.Cancel = true;

            base.OnClosing(e);
        }


        public void Close(bool canClose)
        {
            _canClose = canClose;
            this.Close();
        }

        private IntPtr GetImageHandle(MessageBoxFSImage image)
        {
            switch (image)
            {
                case MessageBoxFSImage.Warning:
                    return SystemIcons.Warning.Handle;
                case MessageBoxFSImage.Error:
                    return SystemIcons.Error.Handle;
                case MessageBoxFSImage.Information:
                    return SystemIcons.Information.Handle;
                case MessageBoxFSImage.Question:
                    return SystemIcons.Question.Handle;
                default:
                    return SystemIcons.Error.Handle;
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            IconHelper.RemoveIcon(this);
        }



    }
}
