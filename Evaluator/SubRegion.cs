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
        public double[] Histogram { get; private set; }
        public int Pixels { get; private set; }
        public int ID { get; private set; }
        public List<int> PixelsIndexes { get; private set; }


        public List<Rectangle> Blocks { get; private set; }
        public List<SubRegion> Neighbors { get; private set; }

        private static int BmpWidth { get; set; }
        private static byte[] GreyValues { get; set; }

        public SubRegion(Rectangle block, int id)
        {
            ID = id;

            Blocks = new List<Rectangle>();
            Neighbors = new List<SubRegion>();
            PixelsIndexes = new List<int>();

            Blocks.Add(block);
            Pixels = block.Width * block.Height;

            Histogram = GetHistogramFrom(block);

            NormalizeHistogram();
        }

        public static void Init(byte[] greyValues, int bmpWidth)
        {
            BmpWidth = bmpWidth;
            GreyValues = greyValues;
        }
        
        public List<int> GetNeighborsIDs()
        {
            return new List<int>(Neighbors.Select(n => n.ID));
        }
        
        public void AddNeighbor(SubRegion newNeighbor)
        {
            if (!Neighbors.Contains(newNeighbor) && newNeighbor.ID != ID)
            {
                Neighbors.Add(newNeighbor);
            }
        }

        public void RemoveNeighbor(int neighborID)
        {
            Neighbors.Remove(Neighbors.Find(n => n.ID == neighborID));
        }

        public void UpdateNeighbors(List<SubRegion> newNeighbors)
        {
            Neighbors = newNeighbors;
        }

        public void AddBlocks(List<Rectangle> blocks)
        {
            Blocks.AddRange(blocks);

            foreach (var block in blocks)
            {
                Pixels += block.Width * block.Height;
            }
            
            CalculateNormalizedHistogram();         
        }

        private void CalculateNormalizedHistogram()
        {
            Histogram = new double[(Params.MaxLBP + 1) * Params.Bins];

            foreach (var block in Blocks)
            {
                var blockHistogram = GetHistogramFrom(block);

                for (int i = 0; i < Histogram.Length; i++)
                {
                    Histogram[i] += blockHistogram[i];
                }
            }

            NormalizeHistogram();
        }

        public void SavePixelsIndexesInArray() 
        {
            var blockPixels = Params.SMin * Params.SMin;
            PixelsIndexes = new List<int>();

            //for (int i = 0; i < PixelsIndexes.Count(); i += blockPixels)
            //{
            //    SaveBlockPixelsIndexesInArray(block, BmpWidth, PixelsIndexes, i);
            //}

            //int i = 0;
            foreach (var block in Blocks)
            {
                SaveBlockPixelsIndexesInArray(block, BmpWidth);
            }
        }

        private double[] GetHistogramFrom(Rectangle block)
        {
            var histogram = new double[(Params.MaxLBP + 1) * Params.Bins];

            for (int i = block.Y; i < block.Y + block.Height; i++)
            {
                for (int j = block.X; j < block.X + block.Width; j++)
                {
                    var LBPC = CountLBPC(GreyValues, BmpWidth, BmpWidth * i + j);
                    int b = GetBinFor(LBPC.C);

                    histogram[(LBPC.LBP) * Params.Bins + b]++;
                }
            }

            return histogram;
        }
        
        private void NormalizeHistogram()
        {
            for (int i = 0; i < Histogram.Length; i++)
            {
                Histogram[i] /= Pixels;
            }
        }

        public void SaveIDsInArray(int[] IDs)
        {
            foreach (var block in Blocks)
            {
                SaveBlockIDInArray(block, IDs, BmpWidth);
            }
        }

        private void SaveBlockIDInArray(Rectangle block, int[] IDs, int bmpWidth)
        {
            var histogram = new double[(Params.MaxLBP + 1) * Params.Bins];

            for (int i = block.Y; i < block.Y + block.Height; i++)
            {
                for (int j = block.X; j < block.X + block.Width; j++)
                {
                    IDs[bmpWidth * i + j] = ID;
                }
            }
        }

        private void SaveBlockPixelsIndexesInArray(Rectangle block, int bmpWidth)
        {           

            for (int i = block.Y; i < block.Y + block.Height; i++)
            {
                for (int j = block.X; j < block.X + block.Width; j++)
                {
                    PixelsIndexes.Add(bmpWidth * i + j);
                }
            }
        }

        private int GetBinFor(double c)
        {
            c += Params.MaxC; // move to <0,MaxC*2> range from <-MaxC,MaxC>             
            int bin = (int)Math.Floor(c / Params.BinSize);

            return bin;
        }

        private LBPC CountLBPC(byte[] greyValues, int width, int pixelIdx)
        {
            byte LBP = 0;
            double C = 0;

            int biggerOrEqualNeighborsSum = 0;
            int biggerOrEqualNeighborsCount = 0;
            int smallerNeighborsSum = 0;
            int smallerNeighborsCount = 0;

            var NeighborsIndexes = new List<int>
            {
                pixelIdx - width - 1,
                pixelIdx - width,
                pixelIdx - width + 1,
                pixelIdx + 1,
                pixelIdx - 1,
                pixelIdx + width - 1,
                pixelIdx + width,
                pixelIdx + width + 1
            };

            for (int i = 0; i < 8; i++)
            {
                if (greyValues[pixelIdx] <= greyValues[NeighborsIndexes[i]])
                {
                    LBP += (byte)Math.Pow(2, i);

                    biggerOrEqualNeighborsSum += greyValues[NeighborsIndexes[i]];
                    biggerOrEqualNeighborsCount++;
                }
                else
                {
                    smallerNeighborsSum += greyValues[NeighborsIndexes[i]];
                }
            }

            smallerNeighborsCount = Params.Neighbors - biggerOrEqualNeighborsCount;

            if (smallerNeighborsCount == 0)
            {
                C = biggerOrEqualNeighborsSum / biggerOrEqualNeighborsCount;
            }
            else if (biggerOrEqualNeighborsCount == 0)
            {
                C = smallerNeighborsSum / smallerNeighborsCount;
            }
            else
            {
                C = (biggerOrEqualNeighborsSum / biggerOrEqualNeighborsCount) - (smallerNeighborsSum / smallerNeighborsCount);
            }

            return new LBPC { LBP = LBP, C = C };
        }
    }
}
