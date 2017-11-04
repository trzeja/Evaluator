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
            //var h1 = GetNormalizedHistogramfromFile(@"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lena_gray514.gif");

            //var h2 = GetNormalizedHistogramfromFile(@"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lena_gray514.gif");
            var h2 = GetNormalizedHistogramfromFile(@"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lena_gray514LR.gif");

           // SaveResults(h1);

            //var MSE = CalculateMSE(h1, h2);
        }

       
        private static double CalculateMSE(double[] histogram1, double[] histogram2)
        {
            double sum = 0;

            for (int i = 0; i < histogram1.Length; i++)
            {
                sum += Math.Pow((histogram1[i] - histogram2[i]),2);
            }

            return sum / histogram1.Length;
        }

        static private double[] GetNormalizedHistogramfromFile(string path)
        {
            Bitmap bmp = new Bitmap(path);
                       
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite,
                bmp.PixelFormat);
     
            IntPtr ptr = bmpData.Scan0;

            int stride = Math.Abs(bmpData.Stride);
            int extraStrideBytesPerLine = stride - bmp.Width;
        
            int bytes = stride * bmp.Height;
          
            byte[] greyValues = new byte[bytes];
            byte[] ID = new byte[bytes];

            double[] histogram = new double[(Consts.MaxLBP + 1) * Consts.Bins]; 

            System.Runtime.InteropServices.Marshal.Copy(ptr, greyValues, 0, bytes);

            bmp.UnlockBits(bmpData);




            int blocksInRow = (int)Math.Ceiling((double)bmp.Width / (double)Consts.minimumBlockSize);
            int blocksInCol = (int)Math.Ceiling((double)bmp.Height / (double)Consts.minimumBlockSize);
            
            int blocksTotal = blocksInRow * blocksInRow;

            //Rectangle[] blocks = new Rectangle[blocksTotal]; //tablioca podobszarow
            var blocks = new List<Rectangle>(); // do debugowania
            var regions = new List<SubRegion>();

            int[] IDs = new int[bmp.Width * bmp.Height];


            Rectangle mainBlock = new Rectangle(0, 0, stride, bmp.Height);

            int id = 0;

            for (int i = mainBlock.Y + 1; i < mainBlock.Height; i+= Consts.minimumBlockSize)
            {
                for (int j = 0; j  < mainBlock.Width + 1; j += Consts.minimumBlockSize)
                {
                    int newBlockWidth = Consts.minimumBlockSize;
                    int newBlockHeight = Consts.minimumBlockSize;

                    if (j + Consts.minimumBlockSize > mainBlock.Width)
                    {
                        newBlockWidth = mainBlock.Width - j;
                    }

                    if (i + Consts.minimumBlockSize > mainBlock.Height)
                    {
                        newBlockHeight = mainBlock.Height - i;
                    }

                    var newBlock = new Rectangle(i, j, newBlockWidth, newBlockHeight);
                    blocks.Add(newBlock);
                    var newSubRegion = new SubRegion();
                    newSubRegion.Blocks.Add(newBlock);
                    newSubRegion.ID = id++;

                    regions.Add(newSubRegion);
                }
            }





            
                        
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
