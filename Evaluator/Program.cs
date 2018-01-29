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
            if (args.Count() != 4)
            {
                Console.WriteLine("Incorrect number of parameters - should be 4");
            }
            else
            {
                string resultsFilePath = args[0];
                string image1FilePath = args[1];
                string image2FilePath = args[2];
                string imageID = args[3];

                if (File.Exists(image1FilePath) && File.Exists(image2FilePath))
                {
                    var evaluator = new Evaluator();
                    var PSNR = evaluator.CalculatePSNR(image1FilePath, image2FilePath);
                    var similarity = evaluator.CalculateSimilarityBySegmentation(image1FilePath, image2FilePath, int.Parse(imageID));

                    SaveResults(PSNR, similarity, resultsFilePath);
                }
                else
                {
                    Console.WriteLine("Incorrect path - image file not found");
                }
            }                  
        }

        static void SaveResults(double PSNR, double similarity, string path)
        {
            File.AppendAllText(path,PSNR.ToString("F2").Replace('.', ',') +  " "
                + similarity.ToString("F2").Replace('.', ',') + Environment.NewLine);
        }
    }
}
