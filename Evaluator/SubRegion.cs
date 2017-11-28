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

        public SubRegion(Rectangle block)
        {
            Blocks = new List<Rectangle>();
            Neighbors = new List<SubRegion>();

            Blocks.Add(block);
            SubRegionHistogram = GetHistogramFrom(block);
            Pixels = block.Width * block.Height;
        }
                
        public double[] SubRegionHistogram { get; private set; }
        public int Pixels { get; private set; }


        public static void Init(byte[] greyValues, int bmpWidth)
        {
            _bmpWidth = bmpWidth;
            _greyValues = greyValues;
        }

        public int ID { get; set; }

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
        //na razie dodawanie poj. bloku tylko w CreateSubregions() wiec mzoe byc histogramem subRegionu hist bloku, ale jak
        //hirarchical splitting to trzeba zmienic na dodawnie histogrmu do obecnego
        //public void AddBlock(Rectangle block)
        //{
        //    Blocks.Add(block);
        //    _subRegionHistogram = GetHistogramFrom(block);
        //}

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

        //public int GetPixelCount()
        //{
        //    int count = 0;

        //    foreach (var block in Blocks)
        //    {
        //        count += block.Width * block.Height;
        //    }

        //    return count;
        //}

        public double[] CalculateNormalizedHistogram()
        {
            SubRegionHistogram = new double[(Consts.MaxLBP + 1) * Consts.Bins];
            
            foreach (var block in Blocks)
            {
                var blockHistogram = GetHistogramFrom(block);

                for (int i = 0; i < SubRegionHistogram.Length; i++)
                {
                    SubRegionHistogram[i] += blockHistogram[i];
                }
            }

            NormalizeHistogram(SubRegionHistogram, Pixels);

            return SubRegionHistogram;
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
