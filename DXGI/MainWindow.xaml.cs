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

namespace DXGI_DesktopDuplication
{
    /// <summary>
    ///     MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DuplicationManager duplicationManager = null;

        public static UpdateUI RefreshUI;

        private Thread duplicateThread = null;

        public Managers.Nova.Server.NovaManager NovaManager { get { return Managers.NovaServer.Instance.NovaManager; } }
        public Managers.LiveControl.Server.LiveControlManager LiveControlManager { get { return Managers.NovaServer.Instance.LiveControlManager; } }


        public MainWindow()
        {
            InitializeComponent();

            
                
            NovaManager.OnIntroducerRegistrationResponded += NovaManager_OnIntroducerRegistrationResponded;
            NovaManager.OnNewPasswordGenerated += new EventHandler<Providers.Nova.Modules.PasswordGeneratedEventArgs>(ServerManager_OnNewPasswordGenerated);
            NovaManager.Network.OnConnected += new EventHandler<Network.ConnectedEventArgs>(Network_OnConnected);

            RefreshUI = UpdateImage;
            duplicateThread = new Thread(Demo);

            //test code here
            Console.WriteLine("{0}, {1}", SystemParameters.WorkArea.Width, SystemParameters.WorkArea.Height);
            Console.WriteLine("{0}, {1}", SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);

            Console.WriteLine(Marshal.SizeOf(typeof (Vertex)));

            duplicationManager = DuplicationManager.GetInstance(Dispatcher);

            CaptureFrame();
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

        public async void CaptureFrame()
        {

            PasswordGeneratedEventArgs passArgs = await NovaManager.GeneratePassword();
            LabelPassword.Content = passArgs.NewPassword;
            IntroducerRegistrationResponsedEventArgs regArgs = await NovaManager.RegisterWithIntroducerAsTask();
            LabelNovaId.Content = regArgs.NovaId;
            Status.Content = "Host is live";


            FrameData frameData;
            duplicationManager.GetFrame(out frameData);
            duplicationManager.GetChangedRects(ref frameData); //TODO pending
        }


        public void CapturedChangedRects()
        {
            FrameData data = null;
            duplicationManager.GetChangedRects(ref data);
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
            LabelPassword.Content = e.NewPassword;
        }

        private void NovaManager_OnIntroducerRegistrationResponded(object sender, Providers.Nova.Modules.IntroducerRegistrationResponsedEventArgs e)
        {
            LabelNovaId.Content = e.NovaId;
         
        }

        private void StartHosting_Click(object sender, RoutedEventArgs e)
        {
               
        }
    }
}