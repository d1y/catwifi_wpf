using System;
using System.Collections.Generic;
using System.IO;
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
using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using Gma.QrCodeNet;
using Microsoft.Win32;
using ZXing;
using System.Drawing;

namespace Hello
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{
		public BitmapImage img;
		public MainWindow()
		{
			InitializeComponent();
		}

        public static BitmapSource BitmapFromBase64(string b64string)
		{
			var bytes = Convert.FromBase64String(b64string);

			using (var stream = new MemoryStream(bytes))
			{
				return BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
			}
		}

		private static BitmapImage Source2image(BitmapSource x)
        {
			JpegBitmapEncoder encoder = new JpegBitmapEncoder();
			MemoryStream memoryStream = new MemoryStream();
			BitmapImage bImg = new BitmapImage();

			encoder.Frames.Add(BitmapFrame.Create(x));
			encoder.Save(memoryStream);

			memoryStream.Position = 0;
			bImg.BeginInit();
			bImg.StreamSource = memoryStream;
			bImg.EndInit();

			memoryStream.Close();

			return bImg;
		}

		private static WriteableBitmap RenderQrcodeImage(string data)
        {
            try
            {
				QrEncoder encoder = new QrEncoder(ErrorCorrectionLevel.M);
				QrCode qrCode;
				encoder.TryEncode(data, out qrCode);
				WriteableBitmapRenderer wRenderer = new WriteableBitmapRenderer(new FixedModuleSize(2, QuietZoneModules.Two), Colors.Black, Colors.White);
				WriteableBitmap wBitmap = new WriteableBitmap(70, 70, 96, 96, PixelFormats.Gray8, null);
				wRenderer.Draw(wBitmap, qrCode.Matrix);

				return wBitmap;
			}
            catch (Exception)
            {

				QrEncoder encoder = new QrEncoder(ErrorCorrectionLevel.M);
				QrCode qrCode;
				encoder.TryEncode("test", out qrCode);
				WriteableBitmapRenderer wRenderer = new WriteableBitmapRenderer(new FixedModuleSize(2, QuietZoneModules.Two), Colors.Black, Colors.White);
				WriteableBitmap wBitmap = new WriteableBitmap(70, 70, 96, 96, PixelFormats.Gray8, null);
				wRenderer.Draw(wBitmap, qrCode.Matrix);

				return wBitmap;
			}
			
		}

		private string GetContent()
        {
			string password = wifiPassword.Text;
			string name = wifiName.Text;
			ComboBoxItem typeItem = (ComboBoxItem)wifiType.SelectedItem;
			Console.WriteLine(typeItem);
			object Ctype = typeItem.Content;
			string type = "WPA";
			if (Ctype != null)
            {
				type = Ctype.ToString();
            };
			//Console.WriteLine("name: {0}", name);
			//Console.WriteLine("password: {0}", password);
			//Console.WriteLine("type: {0}", type);
			//Console.WriteLine("type: {0}", type.Length);
            string result = string.Format("WIFI:T:{0};S:{1};P:{2};;", type, name, password);
			Console.WriteLine("结果: {0}", result);
			return result;
        }

		private static string GetQrcodeContentByImageFile(string filename)
        {
			// create a barcode reader instance
			IBarcodeReader reader = new BarcodeReader();
			// load a bitmap
			var barcodeBitmap = (Bitmap)System.Drawing.Image.FromFile(filename);
			// detect and decode the barcode inside the bitmap
			var result = reader.Decode(barcodeBitmap);
			// do something with the result
			if (result != null)
			{
				string t = result.Text;
				return t;
				//Console.WriteLine(t);
				//txtDecoderType.Text = result.BarcodeFormat.ToString();
				//txtDecoderContent.Text = result.Text;
			}
			return "";
		}

		private static string GetWifiMessage(string wifi)
        {
            try
            {
				string u = wifi.Remove(0, "WIFI:".Length);
				u = u.Remove(u.Length - 2);
				string[] b = u.Split(';');
				string type = b[0].Substring(2);
                string name = b[1].Substring(2);
                string password = b[2].Substring(2);
                return string.Format("名称: {0}\n密码: {1}\n加密类型: {2}", name, password, type);
            }
            catch (Exception)
            {

				return "解析失败";
            }
        }


		private void OpenFileButton(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openfiledialog = new OpenFileDialog
			{
				Filter = "图像文件|*.jpg;*.png;*.jpeg;*.bmp;*.gif|所有文件|*.*"
			};
            try
            {
				if ((bool)openfiledialog.ShowDialog())
				{
					string filename = openfiledialog.FileName;
					string data = GetQrcodeContentByImageFile(filename);
					if (data.IndexOf("WIFI:") == 0)
                    {
						string b = GetWifiMessage(data);
						MessageBox.Show(b);
                    } else
                    {
						MessageBox.Show("读取错误");
                    }
				}
			}
            catch (Exception)
            {
				MessageBox.Show("请选择图片!");
			}
			
		}

		private void ClickHelp(object sender, RoutedEventArgs e)
		{
			string helpText = "输入wifi名称和密码和加密方法即可自动生成二维码, 接下来用手机连接即可\n选择文件可解析wifi二维码";
			System.Windows.MessageBox.Show(helpText);
		}
		

		private void WifiNameChanged(object sender, TextChangedEventArgs e)
		{
			string t = GetContent();
			WriteableBitmap b = RenderQrcodeImage(t);
			qrcodeImg.Source = b;
		}

		private void WifiPasswordChanged(object sender, TextChangedEventArgs e)
		{
			string t = GetContent();
			WriteableBitmap b = RenderQrcodeImage(t);
			qrcodeImg.Source = b;
		}

        private void wifiType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string t = GetContent();
            WriteableBitmap b = RenderQrcodeImage(t);
			if (b != null && qrcodeImg != null)
            {
				qrcodeImg.Source = b;
            }
        }
    }
}
