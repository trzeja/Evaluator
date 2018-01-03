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

            string path0 = args[0];
            string path1 = args[1];
            string path2 = args[2];
            
            Console.WriteLine("trying path0: " + path0);

            if (File.Exists(path1) && File.Exists(path2))
            {
                Console.WriteLine("Its working!");

                var evaluator = new Evaluator();
                var PSNR = evaluator.CalculatePSNR(path1, path2);
                var similarity = evaluator.CalculateSimilarityBySegmentation(path1, path2);

                //var PSNR = 1.2;
                //var similarity = 2.1;
                    
                SaveResults(PSNR, similarity, path0);
            }
            else
            {
                Console.WriteLine("Incorrect path - file not found");
            }
            
        }

        static void SaveResults(double PSNR, double similarity, string path)
        {
            File.AppendAllText(path,PSNR.ToString() +  " " + similarity.ToString() + Environment.NewLine);
        }
    }
}
