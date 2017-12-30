using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Evaluator
{
    class Program
    {
        static void Main(string[] args)
        {
            //string path1 = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\base.gif";
            //string path1 = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\mosaic3.gif";
            //string path1 = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lena_gray515.gif";
            string path1 = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lake50.gif";
            string path2 = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lake50b50.gif";


            var evaluator = new Evaluator();
            var PSNR = evaluator.CalculatePSNR(path1, path2);
            var similarity = evaluator.CalculateSimilarityBySegmentation(path1, path2);
             
        }
    }
}
