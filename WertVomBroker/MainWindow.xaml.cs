using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Text;
using System.Windows;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace WertVomBroker
{
    public partial class MainWindow : Window
    {
        private MqttClient client;
        private string clientId;
        private static int maxNumberOfValuesDisplayed = 30;
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
            client.Publish("villach/4BHIF/Schülername/Luefter", Encoding.ASCII.GetBytes("off"));
        }
        private void ButtonEin_Click(object sender, RoutedEventArgs e)
        {
            client.Publish("villach/4BHIF/Schülername/Luefter", Encoding.ASCII.GetBytes("on"));
        }
        private void MqttMsgReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string ReceivedMessage = Encoding.UTF8.GetString(e.Message);
            double newChartValue = Convert.ToDouble(ReceivedMessage);
            if (Values.Count >= maxNumberOfValuesDisplayed) //es werden nur die letzten paar Werte angezeigt
                Values.RemoveAt(0);
            Values.Add(newChartValue);
            Dispatcher.Invoke(delegate { textBox.Text = ReceivedMessage; });
        }
    }
}