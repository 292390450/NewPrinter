using Knd.Printer.CoreLibFrame45;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;

namespace Knd.Printer.Common.Helper
{
    public class AudioHelper
    {
        private static SpeechSynthesizer _speech;

        public static void Speak(string content)
        {
            try
            {
                if (_speech == null)
                {
                    _speech = new SpeechSynthesizer
                    {
                        Rate = -2,
                        Volume = 100
                    };
                }
                if (!string.IsNullOrEmpty(content))
                {
                    _speech.SpeakAsync(content);
                }
            }
            catch (Exception ex)
            {
                LogManager.AddLog("语音错误" + ex);
            }
        }
    }
    public class LightHelp : IDisposable
    {
        private readonly SerialPort _serialPort;

        private readonly byte[] LightComand = new byte[9] { 255, 248, 0, 16, 7, 0, 0, 0, 254 };

        private List<byte> _datas = new List<byte>();

        private bool _disposed;

        private int _index;

        private readonly object _lock = new object();

        private readonly AutoResetEvent _reset;

        public LightHelp(string port, int rate)
        {
            try
            {
                _serialPort = new SerialPort(port)
                {
                    BaudRate = rate,
                    DataBits = 8,
                    StopBits = StopBits.One,
                    Parity = Parity.None,
                    ReceivedBytesThreshold = 1
                };
                _reset = new AutoResetEvent(initialState: false);
                Start();
            }
            catch (Exception ex)
            {
                LogManager.AddLog("灯光控制,打开串口异常" + ex.Message);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            if (_serialPort != null && _serialPort.IsOpen)
            {
                try
                {
                    _serialPort.Close();
                }
                catch (Exception)
                {
                }
                _serialPort.Dispose();
            }
            _reset.Dispose();
            _disposed = true;
        }

        private void Start()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
            _serialPort.DataReceived += SerialPortOnDataReceived;
            _serialPort.ErrorReceived += SerialPortOnErrorReceived;
            _serialPort.Open();
        }

        protected byte[] Sent(byte[] command)
        {
            lock (_lock)
            {
                if (_serialPort == null)
                {
                    return null;
                }
                _datas = new List<byte>();
                _serialPort.Write(command, 0, command.Length);
                bool b = _reset.WaitOne(1000);
                return _datas.ToArray();
            }
        }

        private void SerialPortOnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                Thread.Sleep(500);
                int len = _serialPort.BytesToRead;
                byte[] buf = new byte[len];
                _serialPort.Read(buf, 0, len);
                _datas.AddRange(buf);
                _reset.Set();
            }
            catch (Exception)
            {
            }
        }

        private void SerialPortOnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            try
            {
                LogManager.AddLog("灯光控制,串口接收数据错误");
                _index = 0;
                _datas = new List<byte>();
                _datas[0] = 0;
                _reset.Set();
            }
            catch (Exception)
            {
            }
        }

        private string ConvertByteToAscii(byte[] data)
        {
            string r = string.Empty;
            foreach (byte d in data)
            {
                string s = Convert.ToString(d, 16);
                s = s.PadLeft(2, '0');
                r += s;
            }
            return r;
        }

        public void OpenLight(byte num, byte op)
        {
            LightComand[5] = num;
            LightComand[6] = op;
            LightComand[7] = ComputerLightXy(LightComand);
            LogManager.AddLog("灯光控制,操作灯指令:{0}" + ConvertByteToAscii(LightComand));
            if (!_serialPort.IsOpen)
            {
                _serialPort.Open();
            }
            Sent(LightComand);
        }

        private byte ComputerLightXy(byte[] bytes)
        {
            return (byte)((uint)(bytes[1] + bytes[3] + bytes[4] + bytes[5] + bytes[6]) & 0xFu);
        }

        public void CheckLight()
        {
            try
            {
                OpenLight(0, 1);
                OpenLight(1, 1);
                Thread.Sleep(500);
                OpenLight(0, 0);
                OpenLight(1, 0);
            }
            catch (Exception ex)
            {
                LogManager.AddLog("灯光检测异常" + ex);
            }
        }

        public void PrintingLight()
        {
            try
            {
                OpenLight(0, 1);
                OpenLight(0, 1);
                OpenLight(1, 2);
            }
            catch (Exception ex)
            {
                LogManager.AddLog("打印中灯光异常" + ex);
            }
        }

        public void PrintingFinishLight()
        {
            try
            {
                OpenLight(1, 1);
            }
            catch (Exception ex)
            {
                LogManager.AddLog("打印完成灯光异常" + ex);
            }
        }

        public void WaitingPrintLight()
        {
            try
            {
                OpenLight(0, 0);
                OpenLight(1, 0);
            }
            catch (Exception ex)
            {
                LogManager.AddLog("等待打印灯光异常" + ex);
            }
        }
    }
    public class MD5Helper
    {
        public static string GetMd5Hash(MD5 md5Hash, string input)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();
        }
    }
    public class PasswordBoxHelper
    {
        public static readonly DependencyProperty PasswordProperty = DependencyProperty.RegisterAttached("Password", typeof(string), typeof(PasswordBoxHelper), new FrameworkPropertyMetadata(string.Empty, OnPasswordPropertyChanged));

        public static readonly DependencyProperty AttachProperty = DependencyProperty.RegisterAttached("Attach", typeof(bool), typeof(PasswordBoxHelper), new PropertyMetadata(false, Attach));

        private static readonly DependencyProperty IsUpdatingProperty = DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordBoxHelper));

        public static void SetAttach(DependencyObject dp, bool value)
        {
            dp.SetValue(AttachProperty, value);
        }

        public static bool GetAttach(DependencyObject dp)
        {
            return (bool)dp.GetValue(AttachProperty);
        }

        public static string GetPassword(DependencyObject dp)
        {
            return (string)dp.GetValue(PasswordProperty);
        }

        public static void SetPassword(DependencyObject dp, string value)
        {
            dp.SetValue(PasswordProperty, value);
        }

        private static bool GetIsUpdating(DependencyObject dp)
        {
            return (bool)dp.GetValue(IsUpdatingProperty);
        }

        private static void SetIsUpdating(DependencyObject dp, bool value)
        {
            dp.SetValue(IsUpdatingProperty, value);
        }

        private static void OnPasswordPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            passwordBox.PasswordChanged -= PasswordChanged;
            if (!GetIsUpdating(passwordBox))
            {
                passwordBox.Password = (string)e.NewValue;
            }
            passwordBox.PasswordChanged += PasswordChanged;
        }

        private static void Attach(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                if ((bool)e.OldValue)
                {
                    passwordBox.PasswordChanged -= PasswordChanged;
                }
                if ((bool)e.NewValue)
                {
                    passwordBox.PasswordChanged += PasswordChanged;
                }
            }
        }

        private static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox passwordBox = sender as PasswordBox;
            SetIsUpdating(passwordBox, value: true);
            SetPassword(passwordBox, passwordBox.Password);
            SetIsUpdating(passwordBox, value: false);
        }
    }
    public class QrCodeHelper
    {
        public static Bitmap GeneratCodeImage(string content, int width, int height)
        {
            EncodingOptions options = new QrCodeEncodingOptions
            {
                DisableECI = true,
                CharacterSet = "UTF-8",
                Width = width,
                Height = height,
                Margin = 0
            };
         
            BarcodeWriter<Bitmap> writer = new BarcodeWriter<Bitmap>();
            writer.Format = BarcodeFormat.QR_CODE;
            writer.Options = options;
            return writer.Write(content);
        }

        public static BitmapImage GetBitmapImage(Bitmap bit)
        {
            try
            {
                using MemoryStream ms = new MemoryStream();
                bit.Save(ms, ImageFormat.Bmp);
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                RenderOptions.SetBitmapScalingMode(bitmap, BitmapScalingMode.LowQuality);
                bitmap.DecodePixelWidth = 150;
                bitmap.EndInit();
                bitmap.Freeze();
                bit.Dispose();
                return bitmap;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
    public class VisualHelper
    {
        public static T FindVisualParent<T>(DependencyObject obj) where T : DependencyObject
        {
            try
            {
                T baseParent = null;
                DependencyObject parent = VisualTreeHelper.GetParent(obj);
                if (parent != null && parent is T)
                {
                    return (T)parent;
                }
                if (parent != null)
                {
                    return FindVisualParent<T>(parent);
                }
                return baseParent;
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
                return null;
            }
        }
    }

}
