using MeoReaderJustForAndroid.Services.MeoBeolvasasApp.Services;
using MeoReaderJustForAndroid.Services;
using MeoReaderJustForAndroid.Models;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Runtime.CompilerServices;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.Intrinsics.X86;
using Android.Text;

namespace MeoReaderJustForAndroid
{
    [Activity(Label = "MeoBeolvasas", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private EditText _dolgozoSzamInput, _vonalkodInput, _darabszamInput, _selejtInput, _megjegyzesInput;
        private TextView _dolgozoNevText, _mlTetelAzText;
        private Button _saveButton;
        private MeoService _databaseService;
        private IToastService _toastService;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            _databaseService = new MeoService();
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
            _darabszamInput.TextChanged += ValidalDarabszam;
            _selejtInput.TextChanged += ValidalDarabszam;
            _saveButton.Click += async (sender, e) => await OnSaveClicked();
        }

        string dolgozoszamLast = null, vonalkodLast = null;

        private void SubscribeDolgozoSzamChange()
        {
            _dolgozoSzamInput.FocusChange += async (sender, e) =>
            {
                string dolgozoSzam = _dolgozoSzamInput.Text.Trim();
                if (!e.HasFocus
                 && (dolgozoszamLast is null || !dolgozoszamLast.Equals(dolgozoSzam))) // Csak akkor fut, ha a mez� elvesz�ti a f�kuszt
                {

                    dolgozoszamLast = dolgozoSzam;
                    if (!string.IsNullOrEmpty(dolgozoSzam))
                    {
                        if (!string.IsNullOrEmpty(dolgozoSzam))
                        {
                            _dolgozoNevText.SetTextColor(Android.Graphics.Color.Gray);

                            string dolgozoNev = await Task.Run(() => _databaseService.GetDolgozoNev(dolgozoSzam)); // H�tt�rben futtat�s

                            RunOnUiThread(() =>
                            {
                                //string dolgozoNev = await _databaseService.GetDolgozoNev(dolgozoSzam);
                                if (!string.IsNullOrEmpty(dolgozoNev))
                                {
                                    _dolgozoNevText.Text = $"Dolgoz� neve: {dolgozoNev}";
                                    _dolgozoNevText.SetTextColor(Android.Graphics.Color.Black);
                                }
                                else
                                {
                                    _dolgozoNevText.Text = "Nincs ilyen dolgoz�!";
                                    _dolgozoNevText.SetTextColor(Android.Graphics.Color.Red);
                                }
                            });
                        }
                        else _dolgozoNevText.Text = "(�res)";
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
                 && (vonalkodLast is null || !dolgozoszamLast.Equals(vonalkod))) // Csak akkor fut, ha a mez� elvesz�ti a f�kuszt
                {
                    vonalkodLast = vonalkod;
                    _mlTetelAzText.SetTextColor(Android.Graphics.Color.Gray);

                    if (!string.IsNullOrEmpty(vonalkod))
                    {
                        int? mlTetelAz = await Task.Run(() => _databaseService.GetMlTetelAZ(vonalkod));
                        RunOnUiThread(() =>
                        {
                            if (mlTetelAz is int)
                            {
                                _mlTetelAzText.Text = $"MlTetelAZ: {mlTetelAz}";
                                _mlTetelAzText.SetTextColor(Android.Graphics.Color.Black);
                            }
                            else
                            {
                                _mlTetelAzText.Text = "Nincs a beolvasott vonalk�dalapj�n lek�rhet� munkalap m�velet!";
                                _mlTetelAzText.SetTextColor(Android.Graphics.Color.Red);
                            }
                        });
                    }
                    else _mlTetelAzText.Text = "(�res)";
                }
            };
        }

        private void ValidalDarabszam(object sender, TextChangedEventArgs e)
        {
            int darabszam = int.TryParse(_darabszamInput.Text, out int d) ? d : 0;
            int selejt = int.TryParse(_selejtInput.Text, out int s) ? s : 0;

            if (darabszam <= 0)
            {
                _darabszamInput.Error = "A darabsz�mnak nagyobbnak kell lennie 0-n�l!";
                _selejtInput.Error = null; // Selejt mez� t�rl�se, ha el�z�leg volt benne hiba
            }
            else if (darabszam < selejt)
            {
                _selejtInput.Error = "A selejt nem lehet nagyobb, mint a darabsz�m!";
                _darabszamInput.Error = null; // Darabsz�m mez� t�rl�se, ha el�z�leg volt benne hiba
            }
            else
            {
                _darabszamInput.Error = null; // Minden hiba t�rl�se, ha az �rt�kek helyesek
                _selejtInput.Error = null;
            }
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
        _hibaUzenet.Text = "A darabsz�mnak nagyobbnak kell lennie 0-n�l!";
        _hibaUzenet.SetTextColor(Android.Graphics.Color.Red);
    }
    else if (darabszam < selejt)
    {
        _hibaUzenet.Text = "A darabsz�m nem lehet kisebb a selejtn�l!";
        _hibaUzenet.SetTextColor(Android.Graphics.Color.Red);
    }
    else
    {
        _hibaUzenet.Text = "";
    }
}*/
    }

