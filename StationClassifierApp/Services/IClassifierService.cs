using System;
using System.Collections.Generic;
using System.Text;

namespace StationCustomVisionApp.Services
{
    public interface IClassifierService
    {
        List<string> Labels { get; }

        float[] Classify(IntPtr pixelData, int width, int height);
    }
}
