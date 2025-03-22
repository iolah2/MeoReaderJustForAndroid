using MeoReaderJustForAndroid.Services.MeoBeolvasasApp.Services;
using MeoReaderJustForAndroid.Services;
using MeoReaderJustForAndroid.Models;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Runtime.CompilerServices;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.Intrinsics.X86;
using Android.Text;
using Android.Content;
using Android.Runtime;
using Android.Views;

namespace MeoReaderJustForAndroid
{
    [Activity(Label = "MeoBeolvasas", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private const int REQUEST_CODE_VONALKOD_INPUT = 1001;
        private EditText _dolgozoSzamInput, _vonalkodInput, _darabszamInput, _selejtInput, _megjegyzesInput;
        private TextView _dolgozoNevText, _mlTetelAzText;
        private Button _saveButton;
        private MeoService _databaseService;
        private IToastService _toastService;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);
            try
            {
                _databaseService = new MeoService(this);
            }
            catch (Exception ex)
            {
                Toast.MakeText(ApplicationContext, "Alkalmazás bezárul!\nOka:" + ex.Message, ToastLength.Short).Show();
                FinishAffinity();
            }
            _toastService = new ToastService();

            _dolgozoSzamInput = FindViewById<EditText>(Resource.Id.dolgozoszamInput);
            _dolgozoNevText = FindViewById<TextView>(Resource.Id.dolgozoNevText);
            SubscribeDolgozoSzamChange();

            _vonalkodInput = FindViewById<EditText>(Resource.Id.vonalkodInput);
            _mlTetelAzText = FindViewById<TextView>(Resource.Id.mlTetelAzText);
            SubscribeVonalkodChange();
            _darabszamInput = FindViewById<EditText>(Resource.Id.darabszamInput);
            _selejtInput = FindViewById<EditText>(Resource.Id.selejtInput);
            _megjegyzesInput = FindViewById<EditText>(Resource.Id.megjegyzesInput);
            _saveButton = FindViewById<Button>(Resource.Id.saveButton);


            int? result = await _databaseService.GetFirstDolgozoszam();//.Result;
            _dolgozoSzamInput.Text = result?.ToString() ?? "";
            if(result is int)
            _dolgozoNevText.Text =  await Task.Run(() => _databaseService.GetDolgozoNev(result.ToString()));
            _darabszamInput.TextChanged += ValidalDarabszam;
            _selejtInput.TextChanged += ValidalDarabszam;
            _saveButton.Click += async (sender, e) => await OnSaveClicked();
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent? data)
        {         
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == REQUEST_CODE_VONALKOD_INPUT && resultCode == Result.Ok && data != null)
            {
                string scannedBarcode = data.GetStringExtra("SCANNED_BARCODE");
                _vonalkodInput.Text = scannedBarcode; // Beállítja az olvasott vonalkódot a mezõbe
            }
        }    

        string dolgozoszamLast = null, vonalkodLast = null;

        private void SubscribeDolgozoSzamChange()
        {
            _dolgozoSzamInput.FocusChange += async (sender, e) =>
            {
                string dolgozoSzam = _dolgozoSzamInput.Text.Trim();
                if (!e.HasFocus
                 && (dolgozoszamLast is null || !dolgozoszamLast.Equals(dolgozoSzam))) // Csak akkor fut, ha a mezõ elveszíti a fókuszt
                {

                    dolgozoszamLast = dolgozoSzam;
                    if (!string.IsNullOrEmpty(dolgozoSzam))
                    {
                        if (!string.IsNullOrEmpty(dolgozoSzam))
                        {
                            _dolgozoNevText.SetTextColor(Android.Graphics.Color.Gray);

                            string dolgozoNev = await Task.Run(() => _databaseService.GetDolgozoNev(dolgozoSzam)); // Háttérben futtatás

                            RunOnUiThread(() =>
                            {
                                //string dolgozoNev = await _databaseService.GetDolgozoNev(dolgozoSzam);
                                if (!string.IsNullOrEmpty(dolgozoNev))
                                {
                                    _dolgozoNevText.Text = $"Dolgozó neve: {dolgozoNev}";
                                    _dolgozoNevText.SetTextColor(Android.Graphics.Color.Black);
                                }
                                else
                                {
                                    _dolgozoNevText.Text = "Nincs ilyen dolgozó!";
                                    _dolgozoNevText.SetTextColor(Android.Graphics.Color.Red);
                                }
                            });
                        }
                        else _dolgozoNevText.Text = "(üres)";
                    }
                }
            };
        }

        private void SubscribeVonalkodChange()
        {
            _vonalkodInput.FocusChange += async (sender, e) =>
            {
                string vonalkod = _vonalkodInput.Text.Trim();
                if (!e.HasFocus
                 && (vonalkodLast is null || !dolgozoszamLast.Equals(vonalkod))) // Csak akkor fut, ha a mezõ elveszíti a fókuszt
                {
                    vonalkodLast = vonalkod;
                    _mlTetelAzText.SetTextColor(Android.Graphics.Color.Gray);

                    if (!string.IsNullOrEmpty(vonalkod))
                    {
                        int? mlTetelAz, gyartando;
                        (mlTetelAz, gyartando) = await Task.Run(() => _databaseService.GetMlTetelAZ(vonalkod));
                        RunOnUiThread(() =>
                        {
                            if (mlTetelAz is int)
                            {
                                _mlTetelAzText.Text = $"MlTetelAZ: {mlTetelAz}";
                                _mlTetelAzText.SetTextColor(Android.Graphics.Color.Black);
                            }
                            else
                            {
                                _mlTetelAzText.Text = "Nincs a beolvasott vonalkódalapján lekérhetõ munkalap mûvelet!";
                                _mlTetelAzText.SetTextColor(Android.Graphics.Color.Red);
                            }

                            _darabszamInput.Text = gyartando?.ToString() ?? "1";
                        });
                    }
                    else _mlTetelAzText.Text = "(üres)";
                }
                else if (e.HasFocus)
                {
                    Intent intent = new Intent(this, typeof(BarcodeScannerActivity));
                    StartActivityForResult(intent, REQUEST_CODE_VONALKOD_INPUT);
                }
            };
        }        

        private void ValidalDarabszam(object sender, TextChangedEventArgs e)
        {
            int darabszam = int.TryParse(_darabszamInput.Text, out int d) ? d : 0;
            int selejt = int.TryParse(_selejtInput.Text, out int s) ? s : 0;

            if (darabszam <= 0)
            {
                _darabszamInput.Error = "A darabszámnak nagyobbnak kell lennie 0-nál!";
                _selejtInput.Error = null; // Selejt mezõ törlése, ha elõzõleg volt benne hiba
            }
            else if (darabszam < selejt)
            {
                _selejtInput.Error = "A selejt nem lehet nagyobb, mint a darabszám!";
                _darabszamInput.Error = null; // Darabszám mezõ törlése, ha elõzõleg volt benne hiba
            }
            else
            {
                _darabszamInput.Error = null; // Minden hiba törlése, ha az értékek helyesek
                _selejtInput.Error = null;
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.top_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.menu_settings)
            {
                StartActivity(typeof(SettingsActivity));
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private async Task OnSaveClicked()
        {
            string vonalkod = _vonalkodInput.Text;
            if (!vonalkod.Contains("/"))
                return;
            string[] vonalkodParts = vonalkod.Split("/");
            if (vonalkodParts.Count() != 2)
                return;

            int.TryParse(_darabszamInput.Text, out int darabszam);
            int.TryParse(_dolgozoSzamInput.Text, out int dolgozoszam);
            int.TryParse(_selejtInput.Text, out int selejt);
            int.TryParse(vonalkodParts[1], out int muveletszam);

            MeoEllenorzes record = new MeoEllenorzes
            (
                dolgozoszam: dolgozoszam,
                munkalap: vonalkodParts[0],
                muveletszam: muveletszam,
                darabszam: darabszam,
                selejt: selejt,
                megjegyzes: _megjegyzesInput.Text,
                datum: DateTime.Now
            );

            await _databaseService.SaveRecordAsync(record);
            Toast.MakeText(ApplicationContext, "Adat elmentve!", ToastLength.Short).Show();
            //_toastService.ShowToast("Adat elmentve!");
        }
    }
        /*private EditText _darabszamInput;
private EditText _selejtInput;
private TextView _hibaUzenet;

protected override void OnCreate(Bundle savedInstanceState)
{
    base.OnCreate(savedInstanceState);
    SetContentView(Resource.Layout.activity_main);

    _darabszamInput = FindViewById<EditText>(Resource.Id.darabszamInput);
    _selejtInput = FindViewById<EditText>(Resource.Id.selejtInput);
    _hibaUzenet = FindViewById<TextView>(Resource.Id.hibaUzenet);

    _darabszamInput.TextChanged += ValidalDarabszam;
    _selejtInput.TextChanged += ValidalDarabszam;
}

private void ValidalDarabszam(object sender, TextChangedEventArgs e)
{
    int darabszam = int.TryParse(_darabszamInput.Text, out int d) ? d : 0;
    int selejt = int.TryParse(_selejtInput.Text, out int s) ? s : 0;

    if (darabszam <= 0)
    {
        _hibaUzenet.Text = "A darabszámnak nagyobbnak kell lennie 0-nál!";
        _hibaUzenet.SetTextColor(Android.Graphics.Color.Red);
    }
    else if (darabszam < selejt)
    {
        _hibaUzenet.Text = "A darabszám nem lehet kisebb a selejtnél!";
        _hibaUzenet.SetTextColor(Android.Graphics.Color.Red);
    }
    else
    {
        _hibaUzenet.Text = "";
    }
}*/
    }

