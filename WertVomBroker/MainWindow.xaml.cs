using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Text;
using System.Timers;
using System.Windows;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace WertVomBroker
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MqttClient client;
        private string clientId;
        private static System.Timers.Timer aTimer;
        private void SetTimer()
        {
            aTimer = new Timer(250);
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            double random = new Random().NextDouble() * 10;
            Dispatcher.Invoke(delegate { textBox.Text = random.ToString(); });
            if (Values.Count >= 10)
                Values.RemoveAt(0);
            Values.Add(random);
        }
        public ChartValues<double> Values { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            DoChartStuff();
            client = new MqttClient("iot.eclipse.org");
            client.MqttMsgPublishReceived += MqttMsgReceived;
            clientId = Guid.NewGuid().ToString();
            client.Connect(clientId);
            client.Subscribe(new string[] { "villach/4BHIF/Temperatur" }, new byte[] { 2 });
            textBox.Text = "";
            SetTimer();
        }
        private void DoChartStuff()
        {
            Values = new ChartValues<double>();
            chart.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Title = "Temperatur",
                    Values = Values
                }
            };
            chart.AxisY.Add(new Axis
            {
                Title = "Temperatur",
                LabelFormatter = value => value.ToString() + " °C"
            });
            chart.LegendLocation = LegendLocation.None;
        }
        private void ButtonAus_Click(object sender, RoutedEventArgs e)
        {
            //if (Values.Count >= 10)
            //    Values.RemoveAt(0);
            //Values.Add(new Random().NextDouble()*10);
            client.Publish("villach/4BHIF/Schülername/Luefter", Encoding.ASCII.GetBytes("off"));
        }
        private void ButtonEin_Click(object sender, RoutedEventArgs e)
        {
            //if (Values.Count >= 10)
            //    Values.RemoveAt(0);
            //Values.Add(new Random().NextDouble() * 10);
            client.Publish("villach/4BHIF/Schülername/Luefter", Encoding.ASCII.GetBytes("on"));

        }
        private void MqttMsgReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string ReceivedMessage = Encoding.UTF8.GetString(e.Message);
            double newChartValue = Convert.ToDouble(ReceivedMessage);
            if (Values.Count >= 10)
                Values.RemoveAt(0);
            Values.Add(newChartValue);
            Dispatcher.Invoke(delegate { textBox.Text = ReceivedMessage; });
        }
    }
}