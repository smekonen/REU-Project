using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Windows.Input;
using System.Threading;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Essentials;
using Xam.Plugins.OnDeviceCustomVision;
using StationCustomVisionApp.Services;
using System.Runtime.InteropServices;

using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace StationCustomVisionApp
{
    public class MainViewModel : SimpleViewModel
    {

        ImageSource img;
        public ImageSource Img
        {
            get => img;
            set => SetPropertyValue(ref img, value);
        }

        List<string> poiList;
        public List<string> PoiList
        {
            get => poiList ?? new List<string>();
            set => SetPropertyValue(ref poiList, value);
        }
        
        public string SelectedPoi { get; set; }
        
        public ICommand CaptureCommand { get; private set; }

        public Task<List<ImageClassification>> CurrentClassify { get; private set; }

        ILibYuvService libYuvService;
        IVibrateService vibrateService;
        IClassifierService classifierService;

        /*        bool compareMode;
        public bool CompareMode
        {
            get => compareMode;

            set
            {
                SetPropertyValue(ref compareMode, value);
                CameraView.MulticaptureMode = value;

                if (!compareMode)
                {
                    OutputLabel1 = "---";
                    OutputLabel2 = "---";
                    OutputLabel3 = "---";
                }

                CaptureCommand = value ? classifyCompareCommand : classifyCommand;
                RaisePropertyChanged("CaptureCommand");
            }
        }*/
        /*        string outputLabel2;
        public string OutputLabel2
        {
            get => outputLabel2 ?? "";
            set => SetPropertyValue(ref outputLabel2, value);
        }*/
        /*        string outputLabel3;
                public string OutputLabel3
                {
                    get => outputLabel3 ?? "";
                    set => SetPropertyValue(ref outputLabel3, value);
                }*/

        string outputLabel1;
        public string OutputLabel1
        {
            get => outputLabel1 ?? "---";
            set => SetPropertyValue(ref outputLabel1, value);
        }

        public bool Switch
        {
            get;set;

        }


        Command classifyCommand;
        //Command classifyCompareCommand;

        public MainViewModel()
        {
            
            classifyCommand = new Command(async (object data) => await Classify(data), ClassifyCanExecute);
            //classifyCompareCommand = new Command(async (object data) => await ClassifyCompare(data), ClassifyCanExecute);
            CaptureCommand = classifyCommand;

            PoiList = new List<string> { "Escalator", "Elevator", "Platform", "Stairs", "Ticket", "Fare"};

            vibrateService = DependencyService.Get<IVibrateService>();
            libYuvService = DependencyService.Get<ILibYuvService>();
            classifierService = DependencyService.Get<IClassifierService>();
        }


        private bool ClassifyCanExecute(object arg)
        {
            if (CurrentClassify == null) return true;
            return CurrentClassify.IsCompleted;
        }


        //TODO: Implement saving/loading frames for testing

        bool debug = true;
        void LogFrame(Stream jpgStream)
        {
            if (!debug) return;

            //var filesDirectory = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);


            var dirPath = @"storage/emulated/0/StationVisionTemp";

            var filePath = Path.Combine(dirPath, "frame1.jpg");


/*            using (FileStream fw = File.Open(filePath, FileMode.OpenOrCreate))
            {
                jpgStream.CopyTo(fw);

            }*/

            debug = false;
        }


        static int counter = 1;

        void CaptureVideoFrame(Stream frameStream)
        {

            var dirPath = @"storage/emulated/0/StationVisionTemp";

            var filePath = Path.Combine(dirPath, $"scene{counter += 6:D5}.jpg");

            try {
                using (FileStream fw = File.OpenRead(filePath))
                {
                    frameStream.Position = 0;
                    frameStream.SetLength(0);
                    fw.CopyTo(frameStream);
                }
            } catch { counter = 1; }
            
        }

        void PreviewFrame(object data)
        {
            SKBitmap bmp = new SKBitmap();
            var info = new SKImageInfo(1088, 1088, SKImageInfo.PlatformColorType);
            info.ColorType = SKColorType.Bgra8888;


            bmp.InstallPixels(info, (IntPtr)data, info.RowBytes);

            SKBitmapImageSource scaledBmp = new SKBitmap(400, 400);
            bmp.ScalePixels(scaledBmp, SKFilterQuality.High);

            Img = scaledBmp;
        }

        async Task Classify(object data)
        {
            Benchmark.Timers["Classify"].Restart();

            //PreviewFrame(data);

            var results = classifierService.Classify((IntPtr)data, 1088, 1088);
            
            Benchmark.Timers["Classify"].Stop();

            string output = "";
            var labels = classifierService.Labels;

            for(int i = 0; i < labels.Count; i++)
            {
                output += $"{labels[i] + ":",-10}{results[i],7:P1}\n";
            }
         
            OutputLabel1 = output + "\n" + Benchmark.GetTimes();

            Benchmark.Timers["Total"].Stop();

            //UpdateHapticFeedback(null);
        }

        private void UpdateHapticFeedback(List<ImageClassification> classifications)
        {
            //if (classifications == null) { vibrateService.Vibrate(VibrateMode.Low); return; }

            double prob = SelectedPoi == null ? 
                classifications.Max(c => c.Probability) :
                classifications.First(c => c.Tag == SelectedPoi).Probability;

            switch (prob)
            {
                
                case var _ when prob >= 0.5:
                    vibrateService.Vibrate(VibrateMode.High);
                    break;

                case var _ when prob >= 0.2:
                    vibrateService.Vibrate(VibrateMode.Low);
                    break;

                default:
                    vibrateService.Cancel();
                    break;
                
            }
        }
    }
}

