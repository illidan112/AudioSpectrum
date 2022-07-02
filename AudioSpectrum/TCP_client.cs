using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace AudioSpectrum
{
    internal class TCP_client
    {
        private TcpClient tcpclient;
        private static string esp_uri = MainWindow.esp_uri;
        private const int port = 3334;
        public string response = "OK!!";
        private Analyzer analyzer;


        static TCP_client()
        {
            
        }

        public void set_analyzer(Analyzer _analyzer)
        {
            analyzer = _analyzer;
        }

        public async Task LightMusicModeOFF()
        {
            if (analyzer.LightMusicFlag)
            {
                if (ModeChange("22"))
                {
                    analyzer.LightMusicFlag = false;
                }
            }
        }


        public async Task LightMusicModeON()
        {
            if (!analyzer.LightMusicFlag)
            {
                if (ModeChange("11"))
                {
                    analyzer.LightMusicFlag = true;
                }
            }
        }

        private bool ModeChange(string cmd)
        {
            try
            {
                tcpclient = new TcpClient();
                tcpclient.Connect(esp_uri, port);
                NetworkStream stream = tcpclient.GetStream();

                byte[] data = System.Text.Encoding.UTF8.GetBytes(cmd);
                stream.Write(data, 0, data.Length);

                // Closing threads
                stream.Close();
                tcpclient.Close();

                return true;
            }
            catch (SocketException e)
            {
                MainWindow.showMessageBox(e.Message, "SocketException");
                return false;
                //Console.WriteLine("SocketException: {0}", e);
            }
            catch (Exception e)
            {
                MainWindow.showMessageBox(e.Message, "Exception");
                return false;
                //Console.WriteLine("Exception: {0}", e.Message);
            }
        }

    }
}
