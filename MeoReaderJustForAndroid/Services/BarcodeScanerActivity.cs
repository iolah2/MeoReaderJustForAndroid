using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using ZXing.Mobile;

namespace MeoReaderJustForAndroid.Services
{

    [Activity(Label = "Vonalkód Olvasó")]
    public class BarcodeScannerActivity : Activity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            MobileBarcodeScanner.Initialize(Application);
            var scanner = new MobileBarcodeScanner();

            var result = await scanner.Scan();
            if (result != null)
            {
                Intent returnIntent = new Intent();
                returnIntent.PutExtra("SCANNED_BARCODE", result.Text);
                SetResult(Result.Ok, returnIntent);
            }
            else
            {
                SetResult(Result.Canceled);
            }

            Finish(); // Visszatérés az előző activity-re
        }

        /*protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            MobileBarcodeScanner.Initialize(Application);
            var scanner = new MobileBarcodeScanner();

            var result = await scanner.Scan();
            if (result != null)
            {
                Toast.MakeText(this, $"Vonalkód: {result.Text}", ToastLength.Long).Show();
            }
        }*/
    }

}
