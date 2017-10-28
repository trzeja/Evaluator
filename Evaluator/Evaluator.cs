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
        private const int white = 0;
        private const int black = 255;


        static public void LockUnlockBits()
        {

            Bitmap bmp = new Bitmap(@"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lena_gray514.gif");
            //Bitmap bmp = new Bitmap(@"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lena_gray2.gif");

            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            BitmapData bmpData =
                bmp.LockBits(rect, ImageLockMode.ReadWrite,
                bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            int stride = Math.Abs(bmpData.Stride);
            int extraStrideBytesPerLine = stride - bmp.Width;
            // Declare an array to hold the bytes of the bitmap.
            int bytes = stride * bmp.Height;
            //int bytes = bmpData.Width * bmp.Height;
            byte[] greyValues = new byte[bytes];
            int[] histogram = new int[(Consts.MaxLBP + 1) * Consts.Bins]; 

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, greyValues, 0, bytes);

            //CountLBP(greyValues, bmp.Width, 20);

            
            Block mainBlock = new Block { x = 0, y = 0, height = bmp.Height, width = stride }; //bo bez ramki jednopikselowej

            int blocksInRow = mainBlock.width / Consts.minimumBlockSize;
            int blocksTotal = blocksInRow * blocksInRow;

            for (int i = mainBlock.y + 1; i < mainBlock.height - 1; i++)
            {
                for (int j = mainBlock.x + 1; j < mainBlock.width - 1 - extraStrideBytesPerLine; j++)
                {
                    //var LBPC = HelperMethods.CountLBPC(greyValues, mainBlock.width, mainBlock.width * i + j);
                    //byte LBP = LBPC.LBP;
                    //double C = LBPC.C;
                    //int b = HelperMethods.GetBinFor(C);
                  
                   // histogram[(LBP)*Consts.Bins + b]++;
                    greyValues[mainBlock.width * i + j] = 255;

                    //tu problem bo nie mam obrazka 514x514 (wpx na ramke)
                    //if ((i%Consts.minimumBlockSize == 0) && (j % Consts.minimumBlockSize == 0))
                    //{
                    //    greyValues[mainBlock.width * i + j] = 255;
                    //}

                }
            }

           

            // Set every third value to 255. A 24bpp bitmap will look red.  
            //for (int counter = 2; counter < rgbValues.Length; counter += 3)

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(greyValues, 0, ptr, bytes);

            // Unlock the bits.
            bmp.UnlockBits(bmpData);

            // Draw the modified image.
            //e.Graphics.DrawImage(bmp, 0, 150);
            string output = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lena_gray1.gif";

            bmp.Save(output);
            System.Diagnostics.Process.Start(output);
            SaveResults(histogram);
        }



        static private void SaveResults(int[] results)
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
