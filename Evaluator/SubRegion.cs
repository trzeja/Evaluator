using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Evaluator
{
    public class SubRegion
    {
        public SubRegion()
        {
            Blocks = new List<Rectangle>();
            Neighbors = new List<SubRegion>();
        }

        public int ID { get; set; }

        public List<Rectangle> Blocks { get; set; }//to bedzie prywatne docelowo chyba
        public List<SubRegion> Neighbors { get; set; } //to bedzie prywatne docelowo chyba

        public List<int> GetNeighboursIDs()
        {
            return new List<int>(Neighbors.Select(n => n.ID));
        }

        public int GetPixelCount()
        {
            int count = 0;

            foreach (var block in Blocks)
            {
                count += block.Width * block.Height;
            }

            return count;
        }

        public double[] GetNormalizedHistogram(byte[] greyValues, int bmpWidth)
        {
            var subRegionHistogram = new double[(Consts.MaxLBP + 1) * Consts.Bins];
            int pixels = GetPixelCount();

            foreach (var block in Blocks)
            {
                var blockHistogram = GetHistogramFrom(block, greyValues, bmpWidth);

                for (int i = 0; i < subRegionHistogram.Length; i++)
                {
                    subRegionHistogram[i] += blockHistogram[i];
                }               
            }

            NormalizeHistogram(subRegionHistogram, pixels);

            return subRegionHistogram;
        }

        private double[] GetHistogramFrom(Rectangle block, byte[] greyValues, int bmpWidth)
        {
            var histogram = new double[(Consts.MaxLBP + 1) * Consts.Bins];

            for (int i = block.Y; i < block.Y + block.Height; i++)
            {
                for (int j = block.X; j < block.X + block.Width; j++)
                {
                    var LBPC = HelperMethods.CountLBPC(greyValues, bmpWidth, bmpWidth * i + j);
                    int b = HelperMethods.GetBinFor(LBPC.C);

                    histogram[(LBPC.LBP) * Consts.Bins + b]++;
                }
            }
            
            return histogram;
        }

        //private double[] GetNormalizedHistogramFrom(Rectangle block)
        //{
        //    var histogram = new double[(Consts.MaxLBP + 1) * Consts.Bins];

        //    for (int i = block.Y; i < block.Y + block.Height; i++)
        //    {
        //        for (int j = block.X; j < block.X + block.Width; j++)
        //        {
        //            var LBPC = HelperMethods.CountLBPC(_greyValues, _bmp.Width, _bmp.Width * i + j);
        //            int b = HelperMethods.GetBinFor(LBPC.C);

        //            histogram[(LBPC.LBP) * Consts.Bins + b]++;
        //        }
        //    }

        //    NormalizeHistogram(histogram, block.Width * block.Height);

        //    return histogram;
        //}

        private void NormalizeHistogram(double[] histogram, int pixels)
        {
            for (int i = 0; i < histogram.Length; i++)
            {
                histogram[i] /= pixels;
            }
        }

        private void AddHistogrms(double[] sourceHistogram, double[] sourceAndDestinationHistogram)
        {

        }
    }
}
