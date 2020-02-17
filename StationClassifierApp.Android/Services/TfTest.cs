using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;

using StationCustomVisionApp.Services;
using Android.App;

using Xamarin.TensorFlow.Lite;

using Java.Nio;
using Java.Nio.Channels;
using Java.IO;
using Android.Content.Res;

// Derived from:
// https://github.com/tensorflow/tensorflow/blob/master/tensorflow/lite/java/demo/app/src/main/java/com/example/android/tflitecamerademo

namespace StationCustomVisionApp.Droid
{
    public class TfTest : IClassifierService
    {
        // Mobilenet normalization constants
        static readonly float MN_MEAN = 127.5f;
        static readonly float MN_STD = 127.5f;

        MappedByteBuffer model;
        int modelInputSize;

        public List<string> Labels { get; private set; }

        Interpreter interpreter;
        Interpreter.Options interpOptions;
        
        byte[] scaledArgbData;
        IntPtr scaledArgbPtr;

        byte[] input;
        IntPtr inputPtr;

        float[][] results;

        ByteBuffer inputBuffer;
        Java.Lang.Object outputArr;


        static LibYuvService libYuv;
        

        public TfTest()
        {
            //Specific to mobilenet
            modelInputSize = 224;

            var t = TensorFlowLite.Version();

            LoadLabels();
            LoadModel();

            interpOptions = new Interpreter.Options();
            interpOptions.SetNumThreads(Environment.ProcessorCount);
            interpreter = new Interpreter(model, interpOptions);

            // Num of channels (RGB) => 3
            // Bytes per channel => 4 (each channel is float value)
            //inputBuffer = ByteBuffer.AllocateDirect(modelInputSize * modelInputSize * 3 * 4);
            inputBuffer.Order(ByteOrder.NativeOrder());

            input = new byte[modelInputSize * modelInputSize * 3 * 4];
            inputPtr = GCHandle.Alloc(input, GCHandleType.Pinned).AddrOfPinnedObject();
            //inputBuffer = ByteBuffer.Wrap(input);

            // Num of channels (ARGB) => 4
            // Bytes per channel => 1 
            scaledArgbData = new byte[modelInputSize * modelInputSize * 4];
            scaledArgbPtr = GCHandle.Alloc(scaledArgbData, GCHandleType.Pinned).AddrOfPinnedObject();

            libYuv = new LibYuvService();

            results = new float[1][];
            results[0] = new float[7];

            outputArr = Java.Lang.Object.FromArray(results);
            
        }



        void LoadModel()
        {
            // Edit config file to leave .tflite uncompressed

            //AssetFileDescriptor fd = Application.Context.Assets.OpenFd("test.tflite");
            
            AssetFileDescriptor fd = Application.Context.Assets.OpenFd("test.tflite");
            var fs = new FileInputStream(fd.FileDescriptor);
            model = fs.Channel.Map(FileChannel.MapMode.ReadOnly, fd.StartOffset, fd.DeclaredLength);
        }


        void LoadLabels()
        {
            Labels = new List<string>();

            using (var sr = new StreamReader(Application.Context.Assets.Open("labels.txt")))
            {
                string line;
                while ((line = sr.ReadLine()) != null) Labels.Add(line);
            }

        }

        private unsafe void CopyRgbToBuffer()
        {
            inputBuffer.Rewind();

            // 32-bit pixel depth (BGRA)
            int* pixelsPtr = (int*)scaledArgbPtr;
            int pixelsCount = modelInputSize*modelInputSize;

            float* destPtr = (float*)inputPtr;


            for (var i = 0; i < pixelsCount; ++i)
            {
                int val = pixelsPtr[i];

                // ignore alpha component, normalize for mobilenet
                float r = (((val >> 16) & 0xFF) - MN_MEAN) / MN_STD;
                float g = (((val >> 8) & 0xFF) - MN_MEAN) / MN_STD;
                float b = (((val) & 0xFF) - MN_MEAN) / MN_STD;

                /*inputBuffer.PutFloat(r);
                inputBuffer.PutFloat(g);
                inputBuffer.PutFloat(b);*/

                *(destPtr + (i * 3) + 0) = r;
                *(destPtr + (i * 3) + 1) = g;
                *(destPtr + (i * 3) + 2) = b;

            }

        }


        //TODO: make async
        public float[] Classify(IntPtr pixelData, int width, int height)
        {
            // scale image to fit model input size
            libYuv.ScaleARGB(pixelData, width, height, scaledArgbPtr, modelInputSize, modelInputSize);

            CopyRgbToBuffer();

            interpreter.Run(ByteBuffer.Wrap(input), outputArr);

            results = outputArr.ToArray<float[]>();
            
            return results[0];
        }

    }
}