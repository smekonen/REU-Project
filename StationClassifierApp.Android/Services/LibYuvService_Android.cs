using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using Xamarin.Forms;
using StationCustomVisionApp.Droid;
using StationCustomVisionApp.Services;

namespace StationCustomVisionApp.Droid
{
    public unsafe class LibYuvService : ILibYuvService
    {
        static int ARGB_BPP = 32;

         
        /**         ConvertToARGB from convert_argb.h (LibYUV)
        
        Convert camera sample to ARGB with cropping, rotation and vertical flip.

        "sample_size" is needed to parse MJPG.

        "dst_stride_argb" number of bytes in a row of the dst_argb plane.
        Normally this would be the same as dst_width, with recommended alignment
        to 16 bytes for better efficiency.
        If rotation of 90 or 270 is used, stride is affected. The caller should
        allocate the I420 buffer according to rotation.

        "dst_stride_u" number of bytes in a row of the dst_u plane.
        Normally this would be the same as (dst_width + 1) / 2, with
        recommended alignment to 16 bytes for better efficiency.
        If rotation of 90 or 270 is used, stride is affected.

        "crop_x" and "crop_y" are starting position for cropping.
        To center, crop_x = (src_width - dst_width) / 2
                    crop_y = (src_height - dst_height) / 2

        "src_width" / "src_height" is size of src_frame in pixels.
        "src_height" can be negative indicating a vertically flipped image source.

        "crop_width" / "crop_height" is the size to crop the src to.
        Must be less than or equal to src_width/src_height
        Cropping parameters are pre-rotation.

        "rotation" can be 0, 90, 180 or 270.
        "fourcc" is a fourcc. ie 'I420', 'YUY2'

        Returns 0 for successful; -1 for invalid parameter. Non-zero for failure.*/

        [DllImport("libyuv")]
        private static extern int ConvertToARGB(byte* sample, UIntPtr sample_size,
                                               byte* dst_argb, int dst_stride_argb,
                                               int crop_x, int crop_y,
                                               int src_width, int src_height,
                                               int crop_width, int crop_height,
                                               RotationMode rotation, UInt32 fourcc);

        [DllImport("libyuv")]
        private static extern int ARGBScale(byte* src_argb, int src_stride_argb, int src_width, int src_height,
                                           byte* dst_argb, int dst_stride_argb, int dst_width, int dst_height,
                                           int filterMode);


        // Convert NV21 to RBG (format: BGRA_8888) with 90 degrees rotation
        public void NV21ToARGB_90(IntPtr src, int srcWidth, int srcHeight, IntPtr dstARGB)
        {
            int dstStride = srcWidth * ARGB_BPP / 8;

            int status = ConvertToARGB((byte*)src, (UIntPtr)null,
                                       (byte*)dstARGB, dstStride,
                                       0, 0, srcWidth, srcHeight, srcWidth, srcHeight,
                                       RotationMode.kRotate90, (uint)FourCharCode.NV21);

            if (status < 0) throw new Exception("NV21 conversion failed.");
        }

        public void MJPGToARGB(IntPtr src, int srcWidth, int srcHeight, IntPtr dstARGB)
        {
            throw new NotImplementedException();
        }

        public void ScaleARGB(IntPtr src, int srcWidth, int srcHeight, IntPtr dst, int dstWidth, int dstHeight)
        {
            int srcStride = srcWidth * ARGB_BPP / 8;
            int dstStride = dstWidth * ARGB_BPP / 8;

            int status =  ARGBScale((byte*)src, srcStride, srcWidth, srcHeight,
                                    (byte*)dst, dstStride, dstWidth, dstHeight, 0);

           // if (status < 0) throw new Exception("ARGB scale failed.");
        }


        enum FourCharCode
        {
            NV21 = ('N') | ('V' << 8) | ('2' << 16) | ('1' << 24),
            MJPG = ('M') | ('J' << 8) | ('P' << 16) | ('G' << 24),
        }

        enum RotationMode
        {
            kRotate0 = 0,      // No rotation.
            kRotate90 = 90,    // Rotate 90 degrees clockwise.
            kRotate180 = 180,  // Rotate 180 degrees.
            kRotate270 = 270,  // Rotate 270 degrees clockwise.
        }
    }
      
}