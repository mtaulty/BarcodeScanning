using BarcodeScanning;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Devices.PointOfService;
using Windows.UI.Xaml.Controls;

namespace App8
{
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MainPage()
        {
            this.InitializeComponent();
        }
        async void OnScan()
        {
            this.ScannedCode = "waiting...";

            var newCode = await BarcodeScanningHelper.ScanOnceNoPreviewAsync(
                TimeSpan.FromMilliseconds(-1), BarcodeSymbologies.Code128);

            if (newCode?.ScanDataLabel != null)
            {
                this.ScannedCode = new string(UnicodeEncoding.ASCII.GetChars(newCode.ScanDataLabel.ToArray()));
            }
            else
            {
                this.ScannedCode = null;
            }
        }
        string ScannedCode
        {
            get => this.scannedCode;
            set
            {
                this.scannedCode = value;
                this.FirePropertyChanged();
            }
        }
        string scannedCode;

        void FirePropertyChanged([CallerMemberName] string property = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
