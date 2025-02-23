using Java.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeoReaderJustForAndroid.Models
{
    public class MeoEllenorzes
    {
        public MeoEllenorzes(int dolgozoszam, string munkalap, int muveletszam, int darabszam, int selejt, string megjegyzes, DateTime datum)
        {
            Dolgozoszam = dolgozoszam;
            Munkalap = munkalap;
            Muveletszam = muveletszam;
            Darabszam = darabszam;
            Selejt = selejt;
            Megjegyzes = megjegyzes;
            Datum = datum;
        }

        //public int MeoID { get; set; }
        public int Dolgozoszam { get; set; }
        // public string Vonalkod { get; set; }
        public string Munkalap { get; set; }
        public int Muveletszam { get; set; }
        public int Darabszam { get; set; }
        public int Selejt { get; set; }
        public string Megjegyzes { get; set; }
        public DateTime Datum { get; set; }
    } 
}
