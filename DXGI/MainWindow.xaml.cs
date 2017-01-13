﻿using Providers.Nova.Modules;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Managers.Nova.Server;
using System.Diagnostics;
using System.Threading.Tasks;
using Managers.LiveControl.Client;

using Providers.LiveControl.Client;
using Network;
using Network.Messages.Nova;
using System.IO;
using System.Drawing.Imaging;

namespace DXGI_DesktopDuplication
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        //private DuplicationManager duplicationManager = null;
        public static UpdateUI RefreshUI;
        private Thread duplicateThread = null;

        public Managers.Nova.Client.NovaManager NovaManagerClient = null;
        public Managers.LiveControl.Client.LiveControlManager LiveControlManagerClient;

        public NovaManager NovaManagerServer = null;// { get { return Managers.NovaServer.Instance.NovaManager; } }
        public Managers.LiveControl.Server.LiveControlManager LiveControlManagerServer = null;// { get { return Managers.NovaServer.Instance.LiveControlManager; } }

        public MainWindow()
        {
            LiveControlManagerClient.OnScreenshotReceived += new EventHandler<ScreenshotMessageEventArgs>(LiveControlManager_OnScreenshotReceived);
            InitializeComponent();
            RefreshUI = UpdateImage;

            //test code here
            Console.WriteLine("{0}, {1}", SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height);
            Console.WriteLine("{0}, {1}", SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
            Console.WriteLine(Marshal.SizeOf(typeof(Vertex)));
        }

        public static void UpdateBimtapWithFrameData(ref Bitmap sourceBitmap, FrameData data)
        {
            Graphics graphics = Graphics.FromImage(sourceBitmap);
            //TODO
        }

        public void Demo()
        {

            while (Thread.CurrentThread.IsAlive)
            {
                CapturedChangedRects();
                Console.WriteLine("Capture");
            }

            Console.WriteLine("Exited");
        }


        public async Task InitNetworkManagerClient()
        {
            NovaManagerClient=Managers.NovaClient.Instance.NovaManager;
            LiveControlManagerClient=  Managers.NovaClient.Instance.LiveControlManager;

            LiveControlManagerClient.OnScreenshotReceived += new EventHandler<ScreenshotMessageEventArgs>(LiveControlManager_OnScreenshotReceived);

            NovaManagerClient.OnIntroductionCompleted += new EventHandler<IntroducerIntroductionCompletedEventArgs>(ClientManager_OnIntroductionCompleted);
            NovaManagerClient.OnNatTraversalSucceeded += new EventHandler<Network.NatTraversedEventArgs>(ClientManager_OnNatTraversalSucceeded);
            NovaManagerClient.OnConnected += new EventHandler<ConnectedEventArgs>(ClientManager_OnConnected);
        }

        public async Task InitNetworkManagerServer()
        {

             NovaManagerServer =  Managers.NovaServer.Instance.NovaManager; 
             LiveControlManagerServer =  Managers.NovaServer.Instance.LiveControlManager;

            NovaManagerServer.OnIntroducerRegistrationResponded += NovaManager_OnIntroducerRegistrationResponded;
            NovaManagerServer.OnNewPasswordGenerated += new EventHandler<PasswordGeneratedEventArgs>(ServerManager_OnNewPasswordGenerated);
            NovaManagerServer.Network.OnConnected += new EventHandler<Network.ConnectedEventArgs>(Network_OnConnected);

            PasswordGeneratedEventArgs passArgs = await NovaManagerServer.GeneratePassword();
            LabelPassword.Content = passArgs.NewPassword;
            IntroducerRegistrationResponsedEventArgs regArgs = await NovaManagerServer.RegisterWithIntroducerAsTask();
            LabelNovaId.Content = regArgs.NovaId;
            Status.Content = "Host is live";

            // LiveControlManagerServer.RunThreadToSendFrames(this.Dispatcher);

        }

        void ClientManager_OnConnected(object sender, ConnectedEventArgs e)
        {
            //  ButtonConnect.Set(() => ButtonConnect.Text, "Connected.");
            remoteConnection.Content = "Connected to remote";
            // this.Dispose();
        }

        private void ClientManager_OnIntroductionCompleted(object sender, IntroducerIntroductionCompletedEventArgs e)
        {
            switch (e.Result)
            {
                case Network.Messages.Nova.ResponseIntroducerIntroductionCompletedMessage.Result.Allowed:
                    // Do nothing, expect OnNatTraversalSucceeded() to be raised shortly
                    break;

                case Network.Messages.Nova.ResponseIntroducerIntroductionCompletedMessage.Result.Denied:
                    switch (e.DenyReason)
                    {
                        case ResponseIntroducerIntroductionCompletedMessage.Reason.WrongPassword:
                            //TextBox_Password.Set(() => TextBox_Password.Text, String.Empty); // clear the password box for re-entry
                            remoteConnection.Content = "Enter correct password/login";
                            MessageBox.Show("Please enter the correct password.");
                            break;
                        case ResponseIntroducerIntroductionCompletedMessage.Reason.Banned:
                            MessageBox.Show("You have been banned for trying to connect too many times.");
                            break;
                    }
                    break;
            }
        }

        private void ClientManager_OnNatTraversalSucceeded(object sender, NatTraversedEventArgs e)
        {
            // ButtonConnect.Set(() => ButtonConnect.Text, "Connecting to " + TextBox_Id.Text + " ...");
            remoteConnection.Content = "Connecting to machine..";
        }


        public async Task CaptureFrame()
        {

            //FrameData frameData;
            //duplicationManager.GetFrame(out frameData);
            //duplicationManager.GetChangedRects(ref frameData); //TODO pending




        }


        public void CapturedChangedRects()
        {
            //FrameData data = null;
            //duplicationManager.GetChangedRects(ref data);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            //TODO test code here

            if (duplicateThread.ThreadState == System.Threading.ThreadState.Unstarted)
            {
                duplicateThread.Start();
                Console.WriteLine("Start");
            }


            CapturedChangedRects();
            Console.WriteLine("Click");
            //CaptureFrame();//TODO 已知bug：只有写成CaptureFrame时不会抛异常
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);


        public void UpdateImage(Bitmap bitmap)
        {
            //IntPtr pointer = bitmap.GetHbitmap();
            //Image.Source = Imaging.CreateBitmapSourceFromHBitmap(pointer, IntPtr.Zero, Int32Rect.Empty,
            //    BitmapSizeOptions.FromEmptyOptions());
            //DeleteObject(pointer);
        }


        //Nova Sending Screenshots over network



        //NOVA Network handshakes
        void Network_OnConnected(object sender, Network.ConnectedEventArgs e)
        {
            Status.Content = "Connected";
            //LabelStatus.Set(() => LabelStatus.Text, "Connected");
            //Timer_KeepAlive.Enabled = false;
        }

        void ServerManager_OnNewPasswordGenerated(object sender, Providers.Nova.Modules.PasswordGeneratedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                LabelPassword.Content = e.NewPassword;
            }));

        }

        private void NovaManager_OnIntroducerRegistrationResponded(object sender, Providers.Nova.Modules.IntroducerRegistrationResponsedEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                LabelNovaId.Content = e.NovaId;
            }));

        }

        private async void startCapture_Click(object sender, RoutedEventArgs e)
        {
            // await CaptureFrame();
            //Start Server Network Registration
            await InitNetworkManager();


        }

        private async void ConnectRemote_Click(object sender, RoutedEventArgs e)
        {
            await NovaManagerClient.RequestIntroductionAsTask(RID.Text, PWD.Text);
            LiveControlManagerClient.RequestScreenshot();

            //if (duplicateThread.ThreadState == System.Threading.ThreadState.Unstarted)
            //{
            //    duplicateThread.Start();
            //    Console.WriteLine("Start");
            //}


            //CapturedChangedRects();
            //Console.WriteLine("Click");
        }

        private void LiveControlManager_OnScreenshotReceived(object sender, ScreenshotMessageEventArgs e)
        {
            var screenshot = e.Screenshot;

            using (var stream = new System.IO.MemoryStream(screenshot.Image))
            {

                   Image image = Image.FromStream(stream);
                //Application.DoEvents();
                var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                bitmap.BeginInit();
                MemoryStream memoryStream = new MemoryStream();
                // Save to a memory stream...
                image.Save(memoryStream, ImageFormat.Bmp);
                // Rewind the stream...
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                bitmap.StreamSource = memoryStream;
                bitmap.EndInit();

                this.BGImage.Source = bitmap;
                   //if (ShowRegionOutlines)
                   //{
                   //    var gfx = gdiScreen1.CreateGraphics();
                   //    gfx.DrawLine(pen, new Point(e.Screenshot.Region.X, e.Screenshot.Region.Y), new Point(e.Screenshot.Region.X + e.Screenshot.Region.Width, e.Screenshot.Region.Y));
                   //    gfx.DrawLine(pen, new Point(e.Screenshot.Region.X + e.Screenshot.Region.Width, e.Screenshot.Region.Y), new Point(e.Screenshot.Region.X + e.Screenshot.Region.Width, e.Screenshot.Region.Y + e.Screenshot.Region.Y));
                   //    gfx.DrawLine(pen, new Point(e.Screenshot.Region.X + e.Screenshot.Region.Width, e.Screenshot.Region.Y + e.Screenshot.Region.Y), new Point(e.Screenshot.Region.X, e.Screenshot.Region.Y + e.Screenshot.Region.Y));
                   //    gfx.DrawLine(pen, new Point(e.Screenshot.Region.X, e.Screenshot.Region.Y + e.Screenshot.Region.Y), new Point(e.Screenshot.Region.X, e.Screenshot.Region.Y));
                   //    gfx.Dispose();
                   //}
                   //gdiScreen1.Draw(image, screenshot.Region);
               }

               //LiveControlManager.RequestScreenshot();
            }
        }
    
}