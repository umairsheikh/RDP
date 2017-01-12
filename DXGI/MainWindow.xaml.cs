using Providers.Nova.Modules;
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

        public NovaManager NovaManager { get { return Managers.NovaServer.Instance.NovaManager; } }
        public Managers.LiveControl.Server.LiveControlManager LiveControlManagerServer { get { return Managers.NovaServer.Instance.LiveControlManager; } }
        public LiveControlManager LiveControlManagerClient { get { return Managers.NovaClient.Instance.LiveControlManager; } }


        public MainWindow()
        {
            InitializeComponent();

         

            //LiveControlManagerServer

            RefreshUI = UpdateImage;
            //duplicateThread = new Thread(Demo);

            //test code here
            Console.WriteLine("{0}, {1}", SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height);
            Console.WriteLine("{0}, {1}", SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
            Console.WriteLine(Marshal.SizeOf(typeof(Vertex)));

            // duplicationManager = DuplicationManager.GetInstance(Dispatcher);
            //  LiveControlManagerServer.

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


        public async Task InitNetworkManager()
        {

            NovaManager.OnIntroducerRegistrationResponded += NovaManager_OnIntroducerRegistrationResponded;
            NovaManager.OnNewPasswordGenerated += new EventHandler<PasswordGeneratedEventArgs>(ServerManager_OnNewPasswordGenerated);
            NovaManager.Network.OnConnected += new EventHandler<Network.ConnectedEventArgs>(Network_OnConnected);

            PasswordGeneratedEventArgs passArgs = await NovaManager.GeneratePassword();
            LabelPassword.Content = passArgs.NewPassword;
            IntroducerRegistrationResponsedEventArgs regArgs = await NovaManager.RegisterWithIntroducerAsTask();
            LabelNovaId.Content = regArgs.NovaId;
            Status.Content = "Host is live";

           // LiveControlManagerServer.RunThreadToSendFrames(this.Dispatcher);

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
            IntPtr pointer = bitmap.GetHbitmap();

            Image.Source = Imaging.CreateBitmapSourceFromHBitmap(pointer, IntPtr.Zero, Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(pointer);
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

        private void ConnectRemote_Click(object sender, RoutedEventArgs e)
        {
            LiveControlManagerClient.OnScreenshotReceived += new EventHandler<ScreenshotMessageEventArgs>(LiveControlManager_OnScreenshotReceived);


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
            // var screenshot = e.Screenshot;

            var screenshot = e.Screenshot;
            using (var stream = new System.IO.MemoryStream(screenshot.Image))
            {

                //this.Dispatcher.BeginInvoke(MainWindow.RefreshUI,LiveControlManagerServer.);

                //Show image with current render code

                // Image image = Image.FromStream(stream);
                //Application.DoEvents();
                //this.BackgroundImage = image;
                /*   if (ShowRegionOutlines)
                   {
                       var gfx = gdiScreen1.CreateGraphics();
                       gfx.DrawLine(pen, new Point(e.Screenshot.Region.X, e.Screenshot.Region.Y), new Point(e.Screenshot.Region.X + e.Screenshot.Region.Width, e.Screenshot.Region.Y));
                       gfx.DrawLine(pen, new Point(e.Screenshot.Region.X + e.Screenshot.Region.Width, e.Screenshot.Region.Y), new Point(e.Screenshot.Region.X + e.Screenshot.Region.Width, e.Screenshot.Region.Y + e.Screenshot.Region.Y));
                       gfx.DrawLine(pen, new Point(e.Screenshot.Region.X + e.Screenshot.Region.Width, e.Screenshot.Region.Y + e.Screenshot.Region.Y), new Point(e.Screenshot.Region.X, e.Screenshot.Region.Y + e.Screenshot.Region.Y));
                       gfx.DrawLine(pen, new Point(e.Screenshot.Region.X, e.Screenshot.Region.Y + e.Screenshot.Region.Y), new Point(e.Screenshot.Region.X, e.Screenshot.Region.Y));
                       gfx.Dispose();
                   }
                   gdiScreen1.Draw(image, screenshot.Region);
               }

               LiveControlManager.RequestScreenshot();
               */
                //Trace.WriteLine(String.Format("Processed Image #{0}, Size: {1} KB", e.Screenshot.Number, GetKBFromBytes(e.Screenshot.Image.Length)));
            }
        }
    }
}