using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Diagnostics;


namespace AudioSpectrum
{
    internal class UDP_client
    {
        private static string remoteAddress = MainWindow.esp_uri;
        private static int remotePort = 3333;
        private static UdpClient _udp_client;


        internal void openUDPclient()
        {
            _udp_client = new UdpClient(remoteAddress,remotePort);

        }

        internal void closeUDPclient()
        {
            _udp_client.Close();
        }

        internal bool SendData(byte[] data)
        {
            try
            {
                _udp_client.Send(data, data.Length);
                return true;
                
            }
            catch (Exception ex)
            {
                MainWindow.showMessageBox(ex.Message, "UDP send error");
                Debug.WriteLine(ex.Message);
                _udp_client.Close();
                return false;
            }
        }
    }
}
