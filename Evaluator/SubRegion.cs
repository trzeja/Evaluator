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
        //double[] _subRegionHistogram;
            
        private static int _bmpWidth;
        private static byte[] _greyValues;

        public SubRegion(Rectangle block, int id)
        {
            ID = id;

            Blocks = new List<Rectangle>();
            Neighbors = new List<SubRegion>();

            Blocks.Add(block);
            Histogram = GetHistogramFrom(block);
            Pixels = block.Width * block.Height;
        }
                
        public double[] Histogram { get; private set; }
        public int Pixels { get; private set; }


        public static void Init(byte[] greyValues, int bmpWidth)
        {
            _bmpWidth = bmpWidth;
            _greyValues = greyValues;
        }

        public int ID { get; private set; }

        public List<Rectangle> Blocks { get; set; }//to bedzie prywatne docelowo chyba
        public List<SubRegion> Neighbors { get; set; } //to bedzie prywatne docelowo chyba

        //public List<SubRegion> GetNeighbors()
        //{
        //    return Neighbors;
        //}

        public List<int> GetNeighboursIDs()
        {
            return new List<int>(Neighbors.Select(n => n.ID));
        }

        public void RemoveNeighbor(int neighborID)
        {
            Neighbors.Remove(Neighbors.Find(n => n.ID == neighborID));
        }

        public void AddNeighbor(SubRegion newNeighbor)
        {
            if (!Neighbors.Contains(newNeighbor) && newNeighbor.ID != ID)
            {
                Neighbors.Add(newNeighbor);
            }
        }
        
        public void AddBlocks(List<Rectangle> blocks)
        {
            Blocks.AddRange(blocks);

            foreach (var block in blocks)
            {
                Pixels += block.Width * block.Height;
            }
            
            //nieoptymalnie
            CalculateNormalizedHistogram(); //TODO add new histograms to existing or recalculate whole             
        }
        
        public double[] CalculateNormalizedHistogram()
        {
            Histogram = new double[(Consts.MaxLBP + 1) * Consts.Bins];
            
            foreach (var block in Blocks)
            {
                var blockHistogram = GetHistogramFrom(block);

                for (int i = 0; i < Histogram.Length; i++)
                {
                    Histogram[i] += blockHistogram[i];
                }
            }

            NormalizeHistogram(Histogram, Pixels);

            return Histogram;
        }

        public void SaveIDInArray(int[] IDs, int bmpWidth)
        {
            foreach (var block in Blocks)
            {
                SaveBlockIDInArray(block, IDs, bmpWidth);
            }
        }

        private double[] GetHistogramFrom(Rectangle block)
        {
            var histogram = new double[(Consts.MaxLBP + 1) * Consts.Bins];

            for (int i = block.Y; i < block.Y + block.Height; i++)
            {
                for (int j = block.X; j < block.X + block.Width; j++)
                {
                    var LBPC = HelperMethods.CountLBPC(_greyValues, _bmpWidth, _bmpWidth * i + j);
                    int b = HelperMethods.GetBinFor(LBPC.C);

                    histogram[(LBPC.LBP) * Consts.Bins + b]++;
                }
            }

            return histogram;
        }

        private void NormalizeHistogram(double[] histogram, int pixels)
        {
            for (int i = 0; i < histogram.Length; i++)
            {
                histogram[i] /= pixels;
            }
        }
        
        private void SaveBlockIDInArray(Rectangle block, int[] IDs, int bmpWidth)
        {
            var histogram = new double[(Consts.MaxLBP + 1) * Consts.Bins];

            for (int i = block.Y; i < block.Y + block.Height; i++)
            {
                for (int j = block.X; j < block.X + block.Width; j++)
                {
                    IDs[bmpWidth * i + j] = ID;
                }
            }
        }
    }
}
