using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace AzureIOTTrackerSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Device device1, device2, device3, device4;

        public MainWindow()
        {
            InitializeComponent();
            Progress1.Visibility = Visibility.Hidden;
            Progress2.Visibility = Visibility.Hidden;
            Progress3.Visibility = Visibility.Hidden;
            Progress4.Visibility = Visibility.Hidden;
            currentTime.FontSize = 16;

            currentTime.Content = DateTime.Now.ToLongTimeString();
            DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();

            Origin1.Text = "Hyderabad, India";
            Destination1.Text = "Delhi, India";
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            currentTime.Content = DateTime.Now.ToLongTimeString();
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            await InitializeAll();
            Mouse.OverrideCursor = Cursors.Arrow;
            MessageBox.Show("Connected Successfully");
        }

        private async Task InitializeAll()
        {
            //initialization of devices
            device1 = await InitializeDevice("Device1");
            device2 = await InitializeDevice("Device2");
            device3 = await InitializeDevice("Device3");
            device4 = await InitializeDevice("Device4");
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;

            int sleepTime = 20; // seconds
            //int sleepTime = 1; // seconds
            int sleepCounter = 0;

            if(device1 == null)
            {
                await InitializeAll();
            }

            //initialization of devices
            await InitializeLocations(device1, Progress1, Origin1.Text, Destination1.Text);
            await InitializeLocations(device2, Progress2, Origin2.Text, Destination2.Text);
            await InitializeLocations(device3, Progress3, Origin3.Text, Destination3.Text);
            await InitializeLocations(device4, Progress4, Origin4.Text, Destination4.Text);            

            while (true)
            {
                Thread.Sleep(500);
                DoEvents();
                if (sleepCounter++ <= (sleepTime * 2)) continue;
                sleepCounter = 0;

                SendNextMessage(device1, Progress1);
                SendNextMessage(device2, Progress2);
                SendNextMessage(device3, Progress3);
                SendNextMessage(device4, Progress4);

                if (device1.Complete && device2.Complete && device3.Complete && device4.Complete)
                {
                    MessageBox.Show("Messages sent successfully");
                    break;
                }
            }

            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private async Task<Device> InitializeDevice(string deviceId)
        {
            Device device = new Device(deviceId);
            await device.Initialize();
            return device;
        }

        private async Task InitializeLocations(Device device, ProgressBar progressBar, string origin, string destination)
        {
            await device.InitializeLocations(origin, destination);
            progressBar.Visibility = Visibility.Visible;
            progressBar.Value = 0;
            progressBar.Maximum = device.MessageCount;
        }

        private void SendNextMessage(Device device, ProgressBar progressBar)
        {
            device.SendMessage();
            progressBar.Value++;
        }

        public void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }

    }
}
