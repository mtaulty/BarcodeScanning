using System;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.PointOfService;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Storage;

namespace BarcodeScanning
{
    public static class BarcodeScanningHelper
    {
        static string StoredBarcodeDeviceId
        {
            get
            {
                object value = null;

                ApplicationData.Current.LocalSettings.Values.TryGetValue(
                    SETTINGS_KEY, out value);

                return (value as string);
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values.Add(
                    SETTINGS_KEY, value);
            }
        }
        static string CachedBarcodeDeviceId
        {
            get
            {
                if (string.IsNullOrEmpty(cachedBarcodeDeviceId))
                {
                    cachedBarcodeDeviceId = StoredBarcodeDeviceId;
                }
                return (cachedBarcodeDeviceId);
            }
            set
            {
                cachedBarcodeDeviceId = value;

                if (StoredBarcodeDeviceId == null)
                {
                    StoredBarcodeDeviceId = cachedBarcodeDeviceId;
                }
            }
        }
        static async Task<BarcodeScanner> FindBarcodeScannerAsync()
        {
            BarcodeScanner locatedScanner = null;

            if (CachedBarcodeDeviceId == null)
            {
                // NB: changed to 'local' here in the hope of speeding up this call as
                // it seems to take a long time.
                var deviceSelector = BarcodeScanner.GetDeviceSelector(
                    PosConnectionTypes.Local);

                var devices = await DeviceInformation.FindAllAsync(deviceSelector);

                if (devices != null)
                {
                    foreach (var device in devices)
                    {
                        var scanner = await BarcodeScanner.FromIdAsync(device.Id);

                        if (scanner.VideoDeviceId != null)
                        {
                            CachedBarcodeDeviceId = device.Id;
                            break;
                        }
                    }
                }
            }
            if (CachedBarcodeDeviceId != null)
            {
                locatedScanner = await BarcodeScanner.FromIdAsync(CachedBarcodeDeviceId);
            }
            return (locatedScanner);
        }
        public static async Task<BarcodeScannerReport> ScanOnceNoPreviewAsync(
            TimeSpan timeout, params uint[] barcodeSymbologies)
        {
            var scanner = await FindBarcodeScannerAsync();
            BarcodeScannerReport report = null;

            if (scanner != null)
            {
                using (var capture = new MediaCapture())
                {
                    await capture.InitializeAsync(
                        new MediaCaptureInitializationSettings()
                        {
                            VideoDeviceId = scanner.VideoDeviceId,
                            StreamingCaptureMode = StreamingCaptureMode.Video,
                            PhotoCaptureSource = PhotoCaptureSource.VideoPreview
                        }
                    );
                    using (var claimedScanner = await scanner.ClaimScannerAsync())
                    {
                        // TODO: need to check all these flags
                        claimedScanner.IsVideoPreviewShownOnEnable = false;
                        claimedScanner.IsDecodeDataEnabled = true;
                        claimedScanner.IsDisabledOnDataReceived = true;

                        if (barcodeSymbologies?.Length > 0)
                        {
                            await claimedScanner.SetActiveSymbologiesAsync(barcodeSymbologies);
                        }
                        TaskCompletionSource<bool> completed = new TaskCompletionSource<bool>();

                        TypedEventHandler<ClaimedBarcodeScanner, BarcodeScannerDataReceivedEventArgs> handler =
                            (s, e) =>
                            {
                                report = e.Report;
                                completed.SetResult(true);
                            };

                        claimedScanner.DataReceived += handler;

                        await claimedScanner.EnableAsync();
                        await claimedScanner.StartSoftwareTriggerAsync();

                        await Task.WhenAny(completed.Task, Task.Delay(timeout));

                        await claimedScanner.StopSoftwareTriggerAsync();

                        claimedScanner.DataReceived -= handler;
                    }
                }
            }
            return (report);
        }
        static string cachedBarcodeDeviceId;
        static readonly string SETTINGS_KEY = "CACHED_BARCODE_SCANNER_ID";
    }
}
