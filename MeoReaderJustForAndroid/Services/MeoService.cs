using Microsoft.Data.SqlClient;
using Dapper;
using MeoReaderJustForAndroid.Models;
using System.Reflection.Metadata.Ecma335;
using Android.Content;

namespace MeoReaderJustForAndroid.Services
{
    public class MeoService
    {
        public const int TIMEOUT_SECONDS = 30;
        private string _connectionString;
        private bool _connectionAvailable;
        public bool IsConnectionAvailable => _connectionAvailable;
        /*dolgozó tárolása
TODO
  🧠 Hogy tárolhatod le?
🔹 Például, amikor a dolgozót kiválasztják (akár MainActivity-ben):
csharp
Másolás
Szerkesztés
var prefs = Application.Context.GetSharedPreferences("AppSettings", FileCreationMode.Private);
var editor = prefs.Edit();
editor.PutInt("dolgozo_id", 12345); // vagy amit kiválasztott a felhasználó
editor.Apply();
🔹 Majd amikor újra elindul az app:
csharp
Másolás
Szerkesztés
var prefs = Application.Context.GetSharedPreferences("AppSettings", FileCreationMode.Private);
int dolgozoId = prefs.GetInt("dolgozo_id", -1); // -1 ha nincs még tárolva
Így az alkalmazás megjegyzi a korábban kiválasztott dolgozót, és akár automatikusan be is töltheti, vagy előválaszthatja.


  */
        public MeoService(Context context)
        {
            SetConnesctionString(context);

            // Szinkron módon, timeout-tal teszteljük a kapcsolatot
            _connectionAvailable = TestConnectionWithTimeout(_connectionString, timeoutSeconds: TIMEOUT_SECONDS);

            //if (!_connectionAvailable)
            //{
            //    Android.Util.Log.Warn("MeoService", "Nem sikerült csatlakozni az adatbázishoz. Beállítások megnyitása...");

            //    var intent = new Intent(Application.Context, typeof(SettingsActivity));
            //    intent.AddFlags(ActivityFlags.NewTask);
            //    Application.Context.StartActivity(intent);
            //    SetConnesctionString(context);
            //    _connectionAvailable = TestConnectionWithTimeout(_connectionString, timeoutSeconds: TIMEOUT_SECONDS);
            //    if (!_connectionAvailable)
            //    {
            //        throw new Exception("A kapcsolat nem lett helyesen beállítva, az alkalmazás bezár.");
            //    }
            //}
        }

        public bool Get_connectionAvailable()
        {
            return _connectionAvailable;
        }

        public bool TryReconnect(Context context)
        {
            SetConnesctionString(context);
            _connectionAvailable = TestConnectionWithTimeout(_connectionString, timeoutSeconds: TIMEOUT_SECONDS);
            return _connectionAvailable;
        }

        private void SetConnesctionString(Context context)
        {
            var prefs = context.GetSharedPreferences("AppSettings", FileCreationMode.Private);
            string ip = prefs.GetString("server_ip", "192.168.1.117");
            string db = prefs.GetString("db_name", "Tekszol_DEV");
            string user = prefs.GetString("db_user", "sa");
            string pass = prefs.GetString("db_pass", "sql");

            _connectionString = $"Server={ip};Database={db};User Id={user};Password={pass};TrustServerCertificate=True;";
        }

        private bool TestConnectionWithTimeout(string connectionString, int timeoutSeconds)
        {
            try
            {
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
                using var connection = new SqlConnection(connectionString);
                var task = connection.OpenAsync(cts.Token);
                task.Wait(cts.Token);
                connection.Close();
                return true;
            }
            catch (Exception ex)
            {
                Android.Util.Log.Error("MeoService", $"Kapcsolódási hiba: {ex.Message}");
                return false;
            }
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public async Task<List<MeoEllenorzes>> GetAllRecordsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            IEnumerable<MeoEllenorzes> records = await connection.QueryAsync<MeoEllenorzes>("SELECT * FROM MeoEllenorzes");
            return records.AsList();
        }

        public async Task SaveRecordAsync(MeoEllenorzes record)
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "INSERT INTO MeoEllenorzes(DolgozoSzam, Munkalap, Muveletszam, Darabszam, Selejt, Megjegyzes, Datum) " +
                        "VALUES(@DolgozoSzam, @Munkalap, @Muveletszam, @Darabszam, @Selejt, @Megjegyzes, @Datum)";
            await connection.ExecuteAsync(query, record);
        }

        // https://chatgpt.com/c/6797de9f-161c-8009-85dc-2b109faca5df



        // Adatbázisból dolgozó neve lekérdezése (példa async SQL kapcsolat)
      

        public async Task<int?> GetFirstDolgozoszam()
        {
            using var connection = new SqlConnection(_connectionString);
            var query = "select top 1 Dolgozószám from Dolgozók";

            var res = await connection.QueryFirstOrDefaultAsync<int?>(query);

            return res;//await                   connection.QueryFirstOrDefault<int?>(query);

        }

        public async Task<string> GetDolgozoNev(string dolgozoSzam)
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT Név FROM Dolgozók WHERE Dolgozószám = @DolgozoSzam";
            var result = await connection.QueryFirstOrDefaultAsync<string>(query, new { DolgozoSzam = dolgozoSzam });
            return result ?? "Nincs ilyen dolgozó";
        }
        

        // SQL lekérdezés: technológia megnevezése vonalkód alapján
        //private async Task<string> GetTechnologiaMegnevezes(string vonalkod)
        public async Task<(int?, int?)> GetMlTetelAZ(string vonalkod)
        {            
            if (!vonalkod.Contains("/"))
                throw new Exception($"A beolvasót vonalkoód ({vonalkod})rossz formátumú!\n 7 számjegy / majd 3-5 szám!");
            string[] vonalkodParts = vonalkod.Split("/");
            if (vonalkodParts.Count() != 2 || !int.TryParse(vonalkodParts[1], out int muveletszam))
                throw new Exception($"A beolvasót vonalkoód ({vonalkod})rossz formátumú!\n 7 számjegy / majd 3-5 szám!"); ;

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "select [Mltetel AZ],Gyartando from [Munkalapok részletek] where Munkalap = '" + vonalkodParts[0] +$"' and Műveletszám = {muveletszam}";
            
            return await connection.QueryFirstOrDefaultAsync<(int?, int?)>(query);
        }

        /*
          string vonalkod = MunkalapPicker.SelectedItem.ToString();
            if (!vonalkod.Contains("/"))
                return;
            string[] vonalkodParts = vonalkod.Split("/");
            if (vonalkodParts.Count() != 2)
                return;

            MeoEllenorzes record = new MeoEllenorzes
            (
                Dolgozoszam: (int)DolgozoPicker.SelectedItem,
                Munkalap: vonalkodParts[0],
                Muvelet: Convert.ToInt32(vonalkodParts[1]),
                Darabszam: int.Parse(DarabszamEntry.Text),
                Selejt: int.Parse(SelejtEntry.Text),
                Megjegyzes: MegjegyzesEditor.Text,
                Datum: DateTime.Now
            );*/
    }
}
