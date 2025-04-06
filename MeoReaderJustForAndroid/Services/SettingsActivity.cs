using Android.Content;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeoReaderJustForAndroid.Services
{
    [Activity(Label = "Beállítások")]
    public class SettingsActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.settings_layout);

            var ipEditText = FindViewById<EditText>(Resource.Id.ipEditText);
            var dbNameEditText = FindViewById<EditText>(Resource.Id.dbNameEditText);
            var userEditText = FindViewById<EditText>(Resource.Id.userEditText);
            var passEditText = FindViewById<EditText>(Resource.Id.passEditText);
            var saveButton = FindViewById<Button>(Resource.Id.saveButton);

            // Betöltés a SharedPreferences-ból
            // Beállítások betöltése
            var prefs = GetSharedPreferences("AppSettings", FileCreationMode.Private);
            ipEditText.Text = prefs.GetString("server_ip", "192.168.1.117");
            dbNameEditText.Text = prefs.GetString("db_name", "Tekszol_DEV");
            userEditText.Text = prefs.GetString("db_user", "sa");
            passEditText.Text = prefs.GetString("db_pass", "sql");

            saveButton.Click += async (sender, e) =>
            {
                string ip = ipEditText.Text.Trim();
                string db = dbNameEditText.Text.Trim();
                string user = userEditText.Text.Trim();
                string pass = passEditText.Text.Trim();

                string testConnStr = $"Server={ip};Database={db};User Id={user};Password={pass};TrustServerCertificate=True;";
                bool success = await TestConnectionAsync(testConnStr, timeoutSeconds: MeoService.TIMEOUT_SECONDS);
                if (success)
                {                  
                    var editor = prefs.Edit();
                    editor.PutString("server_ip", ip);
                    editor.PutString("db_name", db);
                    editor.PutString("db_user", user);
                    editor.PutString("db_pass", pass);
                    editor.Apply();

                    Toast.MakeText(this, "Beállítások mentve és kapcsolat sikeres!", ToastLength.Long).Show();
                    Finish();
                }
                else
                {
                    Toast.MakeText(this, "Sikertelen kapcsolódás az adatbázishoz!", ToastLength.Long).Show();
                }
            };
        }

        private async Task<bool> TestConnectionAsync(string connectionString, int timeoutSeconds)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));

            try
            {
                using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync(cts.Token);
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.settings); // saját layoutod
        }

        private void OnSaveClicked()
        {
            // beállítások elmentése után
            SetResult(Result.Ok);
            Finish();
        }

        private void OnCancelClicked()
        {
            SetResult(Result.Canceled);
            Finish();
        }
    }
}
