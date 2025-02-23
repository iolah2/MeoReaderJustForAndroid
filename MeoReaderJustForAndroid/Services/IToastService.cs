using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeoReaderJustForAndroid.Services
{
    // IToastService.cs
    namespace MeoBeolvasasApp.Services
    {
        public interface IToastService
        {
            void ShowToast(string message);
        }

          public class ToastService : IToastService
        {
            public void ShowToast(string message)
            {
                //MainThread.BeginInvokeOnMainThread(() =>
                //{
                //    Toast.MakeText(Application.Context, message, ToastLength.Short).Show();
                //});
            }
        }
    }
}
