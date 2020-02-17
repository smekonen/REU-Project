using System;
using System.Collections.Generic;
using System.Text;

namespace StationCustomVisionApp.Services
{
    public interface IVibrateService
    {
        VibrateMode Mode { get; }
        void Vibrate(VibrateMode mode);
        void Cancel();
    }

    public enum VibrateMode
    {
        High,
        Low,
        Off
    }
}
