using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Input;
using Xamarin.Forms;

namespace StationCustomVisionApp
{
    public class CameraView : View
    {
        //TODO: configuration parameters
        public static int RESOLUTION = 80;
        public static int FORMAT = 0;
        public static int FRAME_WIDTH = 0;
        public static int FRAME_HEIGHT = 0;

        // Pre-allocated buffer for RGB data
        public byte[] FrameData;
        public IntPtr FramePtr;

        public static readonly BindableProperty CaptureCommandProperty = BindableProperty.Create(
            propertyName: "CaptureCommand",
            returnType: typeof(ICommand),
            declaringType: typeof(CameraView)
        );

        public ICommand CaptureCommand {
            get { return (ICommand) GetValue(CaptureCommandProperty); }
            set { SetValue(CaptureCommandProperty, value); }
        }

    }

    
}
