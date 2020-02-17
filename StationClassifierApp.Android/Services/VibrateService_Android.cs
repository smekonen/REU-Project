using System;
using Android.Content;
using Xamarin.Forms;
using Android.OS;
using StationCustomVisionApp.Services;
using StationCustomVisionApp.Droid;

namespace StationCustomVisionApp.Droid
{
    [Obsolete]
    public class VibrateService : IVibrateService
    {
        public VibrateMode Mode { get; private set; } = VibrateMode.Off;

        Vibrator vibrator = (Vibrator)Android.App.Application.Context.GetSystemService(Context.VibratorService);

        long[] highPattern = { 0, 100, 10 };
        long[] lowPattern = { 0, 10, 10 }; 

        public void Vibrate(VibrateMode mode)
        {
            /*if (Build.VERSION.SdkInt >= BuildVersionCodes.O) vibrator.Vibrate(VibrationEffect.CreateOneShot(1000, 100));*/

            if (mode == VibrateMode.Off) { Cancel(); return; }
            if( mode == Mode) { return; }

            long[] pattern = lowPattern;

            if (mode == VibrateMode.High) pattern = highPattern;
            else if (mode == VibrateMode.Low) pattern = lowPattern;

            vibrator.Vibrate(pattern, 1);

            Mode = mode;
        }

        public void Cancel()
        {
            vibrator.Cancel();
            Mode = VibrateMode.Off;
        }
    }
}