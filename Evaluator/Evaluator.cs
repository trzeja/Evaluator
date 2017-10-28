using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing; // dodac nalezy referencje
using System.Drawing.Imaging;

namespace Evaluator
{

    public static class Evaluator
    {

        static public void ProcessImages()
        {
            var h1 = GetNormalizedHistogramfromFile(@"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lena_gray514.gif");
            //var h2 = GetNormalizedHistogramfromFile(@"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lena_gray514.gif");

            SaveResults(h1);

            //var MSE = CalculateMSE(h1, h2);
        }

       
        private static double CalculateMSE(double[] h1, double[] h2)
        {
            throw new NotImplementedException(); //todo
        }

        static private double[] GetNormalizedHistogramfromFile(string path)
        {

            Bitmap bmp = new Bitmap(path);
            //Bitmap bmp = new Bitmap(@"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lena_gray2.gif");
                       
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite,
                bmp.PixelFormat);
     
            IntPtr ptr = bmpData.Scan0;

            int stride = Math.Abs(bmpData.Stride);
            int extraStrideBytesPerLine = stride - bmp.Width;
        
            int bytes = stride * bmp.Height;
          
            byte[] greyValues = new byte[bytes];

            double[] histogram = new double[(Consts.MaxLBP + 1) * Consts.Bins]; 

            System.Runtime.InteropServices.Marshal.Copy(ptr, greyValues, 0, bytes);

            bmp.UnlockBits(bmpData);
                

            Rectangle mainBlock = new Rectangle(0, 0, stride, bmp.Height);
            
            int blocksInRow = mainBlock.Width / Consts.minimumBlockSize;
            int blocksTotal = blocksInRow * blocksInRow;

            for (int i = mainBlock.Y + 1; i < mainBlock.Height - 1; i++)
            {
                for (int j = mainBlock.X + 1; j < mainBlock.Width - 1 - extraStrideBytesPerLine; j++)
                {
                    var LBPC = HelperMethods.CountLBPC(greyValues, mainBlock.Width, mainBlock.Width * i + j);
                    byte LBP = LBPC.LBP;
                    double C = LBPC.C;
                    int b = HelperMethods.GetBinFor(C);

                    histogram[(LBP) * Consts.Bins + b]++;
                    //greyValues[mainBlock.Width * i + j] = 255;

                    //tu problem bo nie mam obrazka 514x514 (wpx na ramke)
                    //if ((i%Consts.minimumBlockSize == 0) && (j % Consts.minimumBlockSize == 0))
                    //{
                    //    greyValues[mainBlock.width * i + j] = 255;
                    //}

                }
            }

            NormalizeHistogram(histogram, bmp.Width * bmp.Height);
            
            return histogram;
        }

        private static void NormalizeHistogram(double[] histogram, int pixels)
        {
            for (int i = 0; i < histogram.Length; i++)
            {
                histogram[i] /= pixels;
            }
        }

        //private void MainLoop()
        //{
        //    extraStrideBytesPerLine
        //}

        //public static byte[] ReadBmpBytesFromFile(string path)
        //{
        //    Bitmap bmp = new Bitmap(path);

        //    Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

        //    BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, bmp.PixelFormat);

        //    IntPtr ptr = bmpData.Scan0;

        //    int stride = Math.Abs(bmpData.Stride);
        //    int extraStrideBytesPerLine = stride - bmp.Width;

        //    int bytes = stride * bmp.Height;

        //    byte[] greyValues = new byte[bytes];

        //    int[] histogram = new int[(Consts.MaxLBP + 1) * Consts.Bins];

        //    System.Runtime.InteropServices.Marshal.Copy(ptr, greyValues, 0, bytes);

        //    bmp.UnlockBits(bmpData);
        //}



        static private void SaveResults(double[] results)
        {
            string[] positions = new string[results.Length];

            for (int i = 0; i < results.Length; i++)
            {
                positions[i] = i + " " + results[i];
            }

            System.IO.File.WriteAllLines(@"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\results.txt", positions);
        }
    }
}
