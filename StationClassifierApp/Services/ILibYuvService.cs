using System;
using System.Collections.Generic;
using System.Text;

namespace StationCustomVisionApp.Services
{
    public interface ILibYuvService
    {
        // Includes 90' rotation
        void NV21ToARGB_90(IntPtr src, int srcWidth, int srcHeight, IntPtr dstARGB);
        void MJPGToARGB(IntPtr src, int srcWidth, int srcHeight, IntPtr dstARGB);
        void ScaleARGB(IntPtr src, int srcWidth, int srcHeight, IntPtr dst, int dstWidth, int dstHeight);
    }
}
