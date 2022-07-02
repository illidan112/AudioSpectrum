using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;

namespace AudioSpectrum
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Analyzer _analyzer;
        private TCP_client _TCPclient;

        public static string esp_uri = "esp_led.local";
        public MainWindow()
        {
            InitializeComponent();
            _TCPclient = new TCP_client();
            _analyzer = new Analyzer(PbL, PbR, DeviceBox);
            _TCPclient.set_analyzer(_analyzer);
        }

        static internal void showMessageBox(string error_string, string caption)
        {
            MessageBox.Show(error_string, caption);
        }


        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            //_client.get_request();
            ResponseText.Text = _TCPclient.response;
        }

        private void BtnUDP_Click(object sender, RoutedEventArgs e)
        {
            //_udpClient.SendData();
        }

        private void BtnOn_Click(object sender, RoutedEventArgs e)
        {
            _TCPclient.LightMusicModeON();
        }

        private void BtnOff_Click(object sender, RoutedEventArgs e)
        {
            _TCPclient.LightMusicModeOFF();
        }

    
    }
}
