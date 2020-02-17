using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Drawing;
using System.Runtime.InteropServices;

using Android.App;
using Android.Content;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using StationCustomVisionApp;
using StationCustomVisionApp.Droid;

using Android.Hardware;

using Xam.Plugins.OnDeviceCustomVision;
using ApxLabs.FastAndroidCamera;


[assembly: ExportRenderer(typeof(CameraView), typeof(CameraViewRenderer))]
namespace StationCustomVisionApp.Droid
{
    [Obsolete]
    public class CameraViewRenderer : ViewRenderer<CameraView, CameraControlNative>, INonMarshalingPreviewCallback
    {

        //Main control
        CameraControlNative cameraViewNative;
        LibYuvService libYuv = new LibYuvService();

        int cWidth;
        int cHeight;

        //remove this and use ispreviewing
        bool cameraRunning = true;  

        public CameraViewRenderer(Context context) : base(context)
        {
        }


        protected override void OnElementChanged(ElementChangedEventArgs<CameraView> eArgs)
        {
            base.OnElementChanged(eArgs);

            // assign native control when xamarin element is first created
            if (eArgs.NewElement != null)
            {
                if (Control == null)
                {
                    cameraViewNative = new CameraControlNative(Context);
                    cameraViewNative.CameraMain = Camera.Open();
                    SetNativeControl(cameraViewNative);
                }

                cameraViewNative.Click += OnCameraClicked;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                cameraViewNative.Click -= OnCameraClicked;
                Control.CameraMain.Release();
            }
            
            base.Dispose(disposing);
        }




        public void OnCameraClicked(object s, EventArgs e)
        {

            if(Element.FrameData == null)
            {
                cWidth = CameraControlNative.PreviewSize.Width;
                cHeight = CameraControlNative.PreviewSize.Height;

                // 4 bytes per pixel
                Element.FrameData = new byte[cWidth * cHeight * 4];
                Element.FramePtr = GCHandle.Alloc(Element.FrameData, GCHandleType.Pinned).AddrOfPinnedObject();

            }
                


            if (cameraRunning)
            {
                using (var buffer1 = new FastJavaByteArray(CameraControlNative.BufferSize))
                    Control.CameraMain.AddCallbackBuffer(buffer1);

                Control.CameraMain.SetNonMarshalingPreviewCallback(this);
                cameraRunning = false;
            }
            else
            {
                Control.CameraMain.SetNonMarshalingPreviewCallback(null);
                cameraRunning = true;
            }
        }

        public void OnPreviewFrame(IntPtr data, Camera camera)
        {

            FastJavaByteArray frameBuffer;

            if (Element.CaptureCommand.CanExecute(null))
            {
                Benchmark.Timers["Total"] = new Stopwatch();
                Benchmark.Timers["Total"].Restart();

                frameBuffer = new FastJavaByteArray(data);

                CaptureFrame(frameBuffer);
                Element.CaptureCommand.Execute(Element.FramePtr);
   
            } else { frameBuffer = new FastJavaByteArray(CameraControlNative.BufferSize); }

            Control.CameraMain.AddCallbackBuffer(frameBuffer);
            frameBuffer.Dispose();
        }


        public unsafe void CaptureFrame(FastJavaByteArray frameBuffer)
        {
            Benchmark.Timers["Transform"].Restart();

            //TODO: rename FramePtr to ArgbPtr
            // Expected output: Bgra8888
            libYuv.NV21ToARGB_90((IntPtr)frameBuffer.Raw, cWidth, cHeight, Element.FramePtr);

            Benchmark.Timers["Transform"].Stop();
        }


        // Source: http://www.wordsaretoys.com/2013/10/25/roll-that-camera-zombie-rotation-and-coversion-from-yv12-to-yuv420planar/
       /* public unsafe void Rotate90NV21(FastJavaByteArray frameBuffer, byte[] output)
        {
            int width = CameraControlNative.PreviewSize.Width;
            int height = CameraControlNative.PreviewSize.Height;

            var input = frameBuffer;

            bool swap = true;
            bool yflip = true;
            bool xflip = false;

            //step through output array selecting correct input coordinate
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int xo = x, yo = y;
                    int w = width, h = height;
                    int xi = xo, yi = yo;

                    //swap x and y coordinates
                    //(with scaling for non-square preview)

                    if (swap)
                    {
                        xi = w * yo / h;
                        yi = h * xo / w;
                    }

                    if (yflip) yi = h - yi - 1;
                    if (xflip) xi = w - xi - 1;

                    output[w * yo + xo] = (byte)input[w * yi + xi];

                    // transforming uv plane
                    int fs = w * h;
                    int qs = (fs >> 2);
                    xi = (xi >> 1);
                    yi = (yi >> 1);
                    xo = (xo >> 1);
                    yo = (yo >> 1);
                    w = (w >> 1);
                    h = (h >> 1);
                    int ui = fs + (w * yi + xi) * 2;
                    int uo = fs + (w * yo + xo) * 2;
                    int vi = ui + 1;
                    int vo = uo + 1;
                    output[uo] = input[ui];
                    output[vo] = input[vi];
                }
            }


        }*/

    }
}
