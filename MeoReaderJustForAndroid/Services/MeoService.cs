using Microsoft.Data.SqlClient;
using Dapper;
using MeoReaderJustForAndroid.Models;
using System.Reflection.Metadata.Ecma335;

namespace MeoReaderJustForAndroid.Services
{
    public class MeoService
    {
        private readonly string _connectionString;

        public MeoService()
        {
            _connectionString = "Server=ISTI13-PC\\sqlexpress2022;Database=Tekszol_DEV;User Id = sa; Password = sql;Encrypt=False; TrustServerCertificate=True;";
            //_connectionString = "Server=ISTI13-PC\\SQLEXPRESS2022;Database=Tekszol_DEV;Integrated security=True";
            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();
            }
            catch (Exception ex)
            {
                ;
            }
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
            return await connection.QueryFirstOrDefaultAsync<string>(query, new { DolgozoSzam = dolgozoSzam });
        }
        

        // SQL lekérdezés: technológia megnevezése vonalkód alapján
        //private async Task<string> GetTechnologiaMegnevezes(string vonalkod)
        public async Task<int?> GetMlTetelAZ(string vonalkod)
        {            
            if (!vonalkod.Contains("/"))
                throw new Exception($"A beolvasót vonalkoód ({vonalkod})rossz formátumú!\n 7 számjegy / majd 3-5 szám!");
            string[] vonalkodParts = vonalkod.Split("/");
            if (vonalkodParts.Count() != 2 || !int.TryParse(vonalkodParts[1], out int muveletszam))
                throw new Exception($"A beolvasót vonalkoód ({vonalkod})rossz formátumú!\n 7 számjegy / majd 3-5 szám!"); ;

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "select [Mltetel AZ] from [Munkalapok részletek] where Munkalap = '" + vonalkodParts[0] +$"' and Műveletszám = {muveletszam}";
            
            return await connection.QueryFirstOrDefaultAsync<int?>(query);
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
