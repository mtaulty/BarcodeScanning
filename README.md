# BarcodeScanning
The Universal Windows Platform has a capability whereby a BarcodeScanner can be used with either a specific, physical, specialised device or it can now be used with a webcam.

You can find the details of the support [here](https://docs.microsoft.com/en-us/windows/uwp/devices-sensors/pos-camerabarcode-get-started) in the documentation where it's called a 'camera barcode scanner'.

If you read that document then you'll find that the sample code that it puts together is largely based around using the built-in UI for barcode scanning which you may well not want and, specifically, that's a 2D UI which wouldn't (e.g.) be something that you'd want on a HoloLens.

You can avoid the built-in UI though by providing your own UI and [this article](https://docs.microsoft.com/en-us/windows/uwp/devices-sensors/pos-camerabarcode-hosting-preview) talks about how to go about that.

However, what if you don't want *any* preview at all because (e.g.) you are going to run your code on a device like HoloLens where the user is already looking at the barcode without your UI having to overlay it for them.

That was the intention of the piece of code in this repo - to use the BarcodeScanner from an app which does not want to display preview (including HoloLens) - note that this was build against a preview SDK 17134.
