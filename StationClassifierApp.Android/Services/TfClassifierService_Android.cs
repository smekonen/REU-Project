using System;
using System.IO;

using Emgu.TF.Lite;

using System.Runtime.InteropServices;
using StationCustomVisionApp.Droid;
using StationCustomVisionApp.Services;

using Android.App;
using System.Collections.Generic;

namespace StationCustomVisionApp.Droid
{
    public class TfClassifierService : IClassifierService
    {
        static List<string> labels;
        public List<string> Labels => labels;

        static FlatBufferModel model;
        static int modelInputSize;
        static Interpreter interp;

        // ARGB raw pixel data
        static byte[] inputData;
        static IntPtr inputPtr;

        static Tensor inputTensor;
        static Tensor[] outputTensors;

        static LibYuvService libYuv;

        public static void Initialize(byte[] tfLiteModel)
        {
            //TODO: compute from model
            modelInputSize = 300;

            model = new FlatBufferModel(tfLiteModel);

            inputData = new byte[modelInputSize * modelInputSize * 4];
            inputPtr = GCHandle.Alloc(inputData, GCHandleType.Pinned).AddrOfPinnedObject();

            var op = new BuildinOpResolver();
            interp = new Interpreter(model, op);

            interp.AllocateTensors();            

            var input = interp.GetInput();
            inputTensor = interp.GetTensor(input[0]);

            var output = interp.GetOutput();
            var outputIndex = output[0];

            outputTensors = new Tensor[output.Length];
            for (var i = 0; i < output.Length; i++)
            {
                outputTensors[i] = interp.GetTensor(outputIndex + i);
            }

            libYuv = new LibYuvService();


            labels = new List<string>();
            using (var sr = new StreamReader(Application.Context.Assets.Open("labels.txt")))
            {
                string line;
                while ((line = sr.ReadLine()) != null) labels.Add(line);
            }

        }


        //TODO: make async, return labels
        public float[] Classify(IntPtr pixelData, int width, int height)
        {

            if (!model.Initialized) throw new Exception("Initialize before classification.");

            libYuv.ScaleARGB(pixelData, width, height, inputPtr, modelInputSize, modelInputSize);
            SetInputTensorPixels(inputPtr, modelInputSize * modelInputSize);

            interp.Invoke();
            

            return (float[])outputTensors[0].GetData();
        }
        
        private unsafe void SetInputTensorPixels(IntPtr pixels, int pixelCount)
        {
            // 32-bit pixel depth (BGRA)
            int* pixelsPtr = (int*)pixels;
            byte* destPtr = (byte*)inputTensor.DataPointer;

            for (var i = 0; i < pixelCount; ++i)
            {
                int val = pixelsPtr[i];

                // ignore alpha component
                var b = (byte)((val >> 24) & 0xFF);
                var g = (byte)((val >> 16) & 0xFF);
                var r = (byte)((val >> 8) & 0xFF);

                *(destPtr + (i * 3) + 0) = r;
                *(destPtr + (i * 3) + 1) = g;
                *(destPtr + (i * 3) + 2) = b;
            }
        }

    }
}
