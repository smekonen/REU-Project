using System;
using System.Collections.Generic;
using Android.Content;
using Android.Hardware;
using Android.Runtime;
using Android.Util;
using Android.Views;

//Derived from: https://github.com/xamarin/xamarin-forms-samples/tree/master/CustomRenderers/View/Droid

namespace StationCustomVisionApp.Droid
{
    [Obsolete]
    public sealed class CameraControlNative : ViewGroup, ISurfaceHolderCallback
    {
        private SurfaceView cameraSurface;
        private ISurfaceHolder holder;

        public static Camera.Size PreviewSize { get; private set; }
        IList<Camera.Size> supportedPreviewSizes;

        public static int BufferSize { get; private set; }

        public bool IsPreviewing { get; set; }

        Camera cameraMain;
        public Camera CameraMain
        {
            get { return cameraMain; }
            set
            {
                cameraMain = value;
                RequestLayout();
            }
        }

        //TODO: Start camera, stop camera methods. Two way connect to renderer

        public CameraControlNative(Context context) : base(context)
        {

            cameraSurface = new SurfaceView(context);

            AddView(cameraSurface);

            IsPreviewing = false;
            holder = cameraSurface.Holder;
            holder.AddCallback(this);
        }

        // Viewgroup overrides **********************************************************


        protected override void OnLayout(bool changed, int left, int top, int right, int bottom)
        {
            //measure specifications to bound the measure of the view (measureSpec is encoded as int)
            var msw = MeasureSpec.MakeMeasureSpec(right - left, MeasureSpecMode.Exactly);
            var msh = MeasureSpec.MakeMeasureSpec(bottom - top, MeasureSpecMode.Exactly);

            SetMeasuredDimension(msw, msh);

            supportedPreviewSizes = CameraMain.GetParameters().SupportedPreviewSizes;
            PreviewSize = GetOptimalPreviewSize(supportedPreviewSizes, msw, msh);

            //layout the view in the top left corner of parent
            cameraSurface.Layout(0, 0, right - left, bottom - top);
        }


        // Surface view callbacks ********************************************************

        public void SurfaceCreated(ISurfaceHolder holder) => CameraMain?.SetPreviewDisplay(holder);
        public void SurfaceDestroyed(ISurfaceHolder holder) => CameraMain?.StopPreview();


        public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format format, int width, int height)
        {
            var parameters = CameraMain.GetParameters();
            parameters.PreviewFormat = Android.Graphics.ImageFormatType.Nv21;
            //parameters.FocusMode = Camera.Parameters.FocusModeInfinity;
            parameters.FocusMode = Camera.Parameters.FocusModeContinuousPicture;
            parameters.SetPreviewSize(PreviewSize.Width, PreviewSize.Height);

            // 6 bytes per 4 pixels (12 bpp) for YUV420. NV12 means 12bpp
            BufferSize = (int)(PreviewSize.Height * PreviewSize.Width * 6.0 / 4.0);

            // might not work for different devices
            cameraMain.SetDisplayOrientation(90);

            RequestLayout();
            CameraMain.SetParameters(parameters);
            CameraMain.StartPreview();
            IsPreviewing = true;

            //TODO: remove this
            this.PerformClick();
        }

        // Helper method

        Camera.Size GetOptimalPreviewSize(IList<Camera.Size> sizes, int w, int h)
        {
            const double AspectTolerance = 0.1;
            double targetRatio = (double)h / w;

            if (sizes == null)
            {
                return null;
            }

            Camera.Size optimalSize = null;
            double minDiff = double.MaxValue;

            int targetHeight = h;
            foreach (Camera.Size size in sizes)
            {
                double ratio = (double)size.Width / size.Height;

                if (Math.Abs(ratio - targetRatio) > AspectTolerance)
                    continue;
                if (Math.Abs(size.Height - targetHeight) < minDiff)
                {
                    optimalSize = size;
                    minDiff = Math.Abs(size.Height - targetHeight);
                }
            }

            if (optimalSize == null)
            {
                minDiff = double.MaxValue;
                foreach (Camera.Size size in sizes)
                {
                    if (Math.Abs(size.Height - targetHeight) < minDiff)
                    {
                        optimalSize = size;
                        minDiff = Math.Abs(size.Height - targetHeight);
                    }
                }
            }

            return optimalSize;

        }
    }
}
