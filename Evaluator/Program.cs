using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace Evaluator
{
    class Program
    {
        static void Main(string[] args)
        {
            //string path1 = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\base.gif";
            //string path1 = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\mosaic3.gif";
            //string path1 = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lena_gray515.gif";

            //string path1 = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lake50.gif";
            //string path2 = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lake50b50.gif";

            //string path1 = @" C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lake50.gif";
            //string path2 = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lake50.gif";

            //string resultsFilePath = args[0];
            //string image1FilePath = args[1];
            //string image2FilePath = args[2];

            string resultsFilePath = @" C:\Users\trzej_000\Documents\Visual Studio 2017\Projects\Evaluator\Evaluator\bin\Debug\results.txt";
            string image1FilePath = @" C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lake50.gif";
            string image2FilePath = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lake50.gif";


            if (File.Exists(image1FilePath) && File.Exists(image2FilePath))
            {
                var evaluator = new Evaluator();
                var PSNR = evaluator.CalculatePSNR(image1FilePath, image2FilePath);
                var similarity = evaluator.CalculateSimilarityBySegmentation(image1FilePath, image2FilePath);
  
                //SaveResults(PSNR, similarity, resultsFilePath);
            }
            else
            {
                Console.WriteLine("Incorrect path - image file not found");
            }
            
        }

        static void SaveResults(double PSNR, double similarity, string path)
        {
            File.AppendAllText(path,PSNR.ToString() +  " " + similarity.ToString() + Environment.NewLine);
        }
    }
}
