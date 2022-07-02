using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Un4seen.Bass;
using Un4seen.BassWasapi;


namespace AudioSpectrum
{

    internal class Analyzer
    {

        private bool _lightMusicFlag;               //enabled status
        private DispatcherTimer _t;         //timer that refreshes the display
        private float[] _fft;               //buffer for fft data
        private ProgressBar _l, _r;         //progressbars for left and right channel intensity
        private WASAPIPROC _process;        //callback function to obtain data
        private int _lastlevel;             //last output level
        private int _hanctr;                //last output level counter
        private List<byte> _spectrumdata;   //spectrum data buffer
        private ComboBox _devicelist;       //device list
        private bool _initialized;          //initialized flag
        private int devindex;               //used device index
        private UDP_client _udp_client;
        static private int samplesNum = 0; // number of samples for VU metr
        static private int maxSamplesNum = 5;
        static private int maxLevel = 0;

        private int _lines = 16;            // number of spectrum lines

        //ctor
        public Analyzer(ProgressBar left, ProgressBar right, ComboBox devicelist)
        {
            _fft = new float[1024];
            _lastlevel = 0;
            _hanctr = 0;
            _t = new DispatcherTimer();
            _t.Tick += _t_Tick;
            _t.Interval = TimeSpan.FromMilliseconds(25); //40hz refresh rate
            _t.IsEnabled = false;
            _l = left;
            _r = right;
            _l.Minimum = 0;
            _r.Minimum = 0;
            _r.Maximum = byte.MaxValue;
            _l.Maximum = byte.MaxValue;
            _process = new WASAPIPROC(Process);
            _spectrumdata = new List<byte>();
            _devicelist = devicelist;
            _initialized = false;
            _lightMusicFlag = false;
            _udp_client = new UDP_client();
            Init();
        }

        // flag for display enable
        public bool DisplayEnable { get; set; }

        //flag for enabling and disabling program functionality
        public bool LightMusicFlag
        {
            get { return _lightMusicFlag; }
            set
            {
                _lightMusicFlag = value;
                if (value)
                {
                    if (!_initialized)
                    {
                        var array = (_devicelist.Items[_devicelist.SelectedIndex] as string).Split(' ');
                        devindex = Convert.ToInt32(array[0]);
                        bool result = BassWasapi.BASS_WASAPI_Init(devindex, 0, 0, BASSWASAPIInit.BASS_WASAPI_BUFFER, 1f, 0.05f, _process, IntPtr.Zero);
                        if (!result)
                        {
                            var error = Bass.BASS_ErrorGetCode();
                            MessageBox.Show(error.ToString());
                        }
                        else
                        {
                            _initialized = true;
                            _devicelist.IsEnabled = false;
                        }
                    }
                    _udp_client.openUDPclient();
                    BassWasapi.BASS_WASAPI_Start();
                }
                else{
                    BassWasapi.BASS_WASAPI_Stop(true);
                    _devicelist.IsEnabled = true;

                    _udp_client.closeUDPclient();
                }

                System.Threading.Thread.Sleep(500);
                _t.IsEnabled = value;
            }
        }

        // initialization
        private void Init()
        {
            bool result = false;
            for (int i = 0; i < BassWasapi.BASS_WASAPI_GetDeviceCount(); i++)
            {
                var device = BassWasapi.BASS_WASAPI_GetDeviceInfo(i);
                if (device.IsEnabled && device.IsLoopback)
                {
                    _devicelist.Items.Add(string.Format("{0} - {1}", i, device.name));
                }
            }
            _devicelist.SelectedIndex = 0;
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, false);
            result = Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            if (!result) throw new Exception("Init Error");
        }

        //timer 
        private void _t_Tick(object sender, EventArgs e)
        {

            //SpectrumMode();

            int level = BassWasapi.BASS_WASAPI_GetLevel();
            VUMode(level);


            //if(Utils.LowWord32(level) > Utils.LowWord32(maxLevel) ) maxLevel = level;
            //samplesNum++;
            //if (samplesNum >= maxSamplesNum)
            //{
            //    samplesNum = 0;
            //    VUMode(maxLevel);
            //    maxLevel = 0;
            //}



            //Required, because some programs hang the output. If the output hangs for a 75ms
            //this piece of code re initializes the output so it doesn't make a gliched sound for long.
            //if (_hanctr > 3)
            //{
            //    _hanctr = 0;
            //    _l.Value = 0;
            //    _r.Value = 0;
            //    Free();
            //    Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            //    _initialized = false;
            //    LightMusicFlag = true;
            //}
        }

        // WASAPI callback, required for continuous recording
        private int Process(IntPtr buffer, int length, IntPtr user)
        {
            return length;
        }

        //cleanup
        public void Free()
        {
            BassWasapi.BASS_WASAPI_Free();
            Bass.BASS_Free();
        }

        private void SpectrumMode()
        {
            int ret = BassWasapi.BASS_WASAPI_GetData(_fft, (int)BASSData.BASS_DATA_FFT2048); //get channel fft data
            if (ret < -1)
            {
                Debug.WriteLine(" BASS_WASAPI_GetData ERROR");
                return;
            }
            int x, y;
            int b0 = 0;
            byte[] data_arr = new byte[_lines];

            //computes the spectrum data, the code is taken from a bass_wasapi sample.
            for (x = 0; x < _lines; x++)
            {
                float peak = 0;
                int b1 = (int)Math.Pow(2, x * 10.0 / (_lines - 1));
                if (b1 > 1023) b1 = 1023;
                if (b1 <= b0) b1 = b0 + 1;
                for (; b0 < b1; b0++)
                {
                    if (peak < _fft[1 + b0]) peak = _fft[1 + b0];
                }
                y = (int)(Math.Sqrt(peak) * 3 * 255 - 4);
                if (y > 255) y = 255;
                if (y < 0) y = 0;

                data_arr[x] = (byte)y;
                //Debug.Write(y);
                //Debug.Write(" / ");
            }

            _udp_client.SendData(data_arr);
        }

        private void VUMode(int level)
        {
            byte level_left, level_right;

            level_left = (byte)(Utils.LowWord32(level) / 128);
            if (level_left > 255) level_left = 255;
            if (level_left < 0) level_left = 0;

            level_right = (byte)(Utils.HighWord32(level) / 128);
            if (level_right > 255) level_right = 255;
            if (level_right < 0) level_right = 0;

            byte[] data_arr = new byte[] { level_left, level_right };

            _udp_client.SendData(data_arr);

            Debug.Write(level_left);
            Debug.Write("/");
            Debug.WriteLine(level_left);
            _l.Value = level_left;
            _r.Value = level_right;
            _lastlevel = level;


        }
    }
}
