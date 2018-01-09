using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Evaluator
{
    public class Evaluator
    {
        private byte[] GreyValues { get; set; }

        private int[] ID { get; set; }

        private Bitmap Bmp { get; set; }

        private List<SubRegion> SubRegions { get; set; }

        private int SubRegionsNumber { get; set; }

        public double CalculatePSNR(string image1Path, string image2Path)
        {
            ReadFile(image1Path);
            var greyValues1 = GreyValues;
            ReadFile(image2Path);
            var greyValues2 = GreyValues;

            double sum = 0;

            for (int i = 0; i < greyValues1.Length; i++)
            {
                sum += Math.Pow((greyValues1[i] - greyValues2[i]), 2);
            }

            var MSE = sum / greyValues1.Length;

            return 10 * Math.Log10((Math.Pow(Params.SignalMax, 2)) / MSE);
        }

        public double CalculateSimilarityBySegmentation(string image1Path, string image2Path)
        {
            int imageID = -1;

            var frontierPixelsIndexes1 = SegmentImage(image1Path, ++imageID);

            var frontierPixelsIndexes2 = SegmentImage(image2Path, ++imageID);

            return CalculateFrontiersSimilarity(frontierPixelsIndexes1, frontierPixelsIndexes2);
        }

        private List<int> SegmentImage(string path, int imageID)
        {
            ReadFile(path);

            CreateSubRegions();

            Merge(imageID);

            SaveIDsInArray();
            //DrawBoundariesInFile(path, imageID); // TO DEL

            return GetFrontierPixelsIndexes();
        }

        private void ReadFile(string path)
        {
            Bmp = new Bitmap(path);

            int pixelsToTrim = Bmp.Width % 4;
            if (pixelsToTrim != 0)
            {
                Bmp = Bmp.Clone(new Rectangle(0, 0, Bmp.Width - pixelsToTrim, Bmp.Height), Bmp.PixelFormat);
            }

            Rectangle rect = new Rectangle(0, 0, Bmp.Width, Bmp.Height);
            BitmapData bmpData = Bmp.LockBits(rect, ImageLockMode.ReadWrite,
                Bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            var bytes = Bmp.Width * Bmp.Height;
            GreyValues = new byte[bytes];
            ID = Enumerable.Repeat(-1, bytes).ToArray();

            System.Runtime.InteropServices.Marshal.Copy(ptr, GreyValues, 0, bytes);

            SubRegion.Init(GreyValues, Bmp.Width);

            Bmp.UnlockBits(bmpData);
        }

        private void DrawBoundariesInFile(string path, int imageID) //TO DEL
        {
            Bmp = new Bitmap(path);

            int pixelsToTrim = Bmp.Width % 4;
            if (pixelsToTrim != 0)
            {
                Bmp = Bmp.Clone(new Rectangle(0, 0, Bmp.Width - pixelsToTrim, Bmp.Height), Bmp.PixelFormat);
            }

            Rectangle rect = new Rectangle(0, 0, Bmp.Width, Bmp.Height);
            BitmapData bmpData = Bmp.LockBits(rect, ImageLockMode.ReadWrite,
                Bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            var bytes = Bmp.Width * Bmp.Height;
            GreyValues = new byte[bytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, GreyValues, 0, bytes);

            Rectangle mainBlock = new Rectangle(1, 1, Bmp.Width - 1, Bmp.Height - 1);

            for (int i = mainBlock.Y; i < mainBlock.Height; i++)
            {
                for (int j = mainBlock.X; j < mainBlock.Width; j++)
                {
                    ChangePixelColorIfFrontier(Bmp.Width * i + j); //spr czy bmp.width dobre
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(GreyValues, 0, ptr, bytes);

            Bmp.UnlockBits(bmpData);

            string output = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\frontiersDrawed" + imageID.ToString() + ".gif";

            Bmp.Save(output);

            System.Diagnostics.Process.Start(output);
        }

        private void ChangePixelColorIfFrontier(int pixelIdx) //TO DEL
        {
            #region NeighborsIndexes

            var neighborsIndexes = new List<int>
            {
                pixelIdx - Bmp.Width - 1,
                pixelIdx - Bmp.Width,
                pixelIdx - Bmp.Width + 1,
                pixelIdx + 1,
                pixelIdx - 1,
                pixelIdx + Bmp.Width - 1,
                pixelIdx + Bmp.Width,
                pixelIdx + Bmp.Width + 1
            };

            #endregion

            foreach (var neighborIdx in neighborsIndexes)
            {
                if (ID[pixelIdx] != ID[neighborIdx])
                {
                    GreyValues[pixelIdx] = 0;
                }
            }
        }

        private void CreateSubRegions()
        {
            SubRegions = new List<SubRegion>();
            SubRegionsNumber = 0;

            Rectangle mainBlock = new Rectangle(1, 1, Bmp.Width - 1, Bmp.Height - 1);

            int subRegionID = 0;

            for (int i = mainBlock.Y; i <= mainBlock.Height; i += Params.SMin)
            {
                for (int j = mainBlock.X; j <= mainBlock.Width; j += Params.SMin)
                {
                    int newBlockWidth = Params.SMin;
                    int newBlockHeight = Params.SMin;

                    if (j + Params.SMin > mainBlock.Width)
                    {
                        newBlockWidth = mainBlock.Width - j;
                    }

                    if (i + Params.SMin > mainBlock.Height)
                    {
                        newBlockHeight = mainBlock.Height - i;
                    }

                    var newBlock = new Rectangle(j, i, newBlockWidth, newBlockHeight);

                    var newSubRegion = new SubRegion(newBlock, subRegionID++);
                    SubRegions.Add(newSubRegion);
                    SubRegionsNumber++;
                }
            }

            SetSubRegionsNeighbors();
        }

        private void SetSubRegionsNeighbors()
        {
            foreach (var region in SubRegions)
            {
                var block = region.Blocks.FirstOrDefault();
                //create block enlarged by 1 in each direction
                var enlargedBlock = new Rectangle(block.X - 1, block.Y - 1, Params.SMin + 2, Params.SMin + 2);

                var regionNeighbors = SubRegions
                    .Where(s => s.Blocks.FirstOrDefault().IntersectsWith(enlargedBlock) && !s.Equals(region))
                    .ToList();

                region.UpdateNeighbors(regionNeighbors);
            }
        }

        private void Merge(int imageID)
        {
            var MIRs = new List<string>(); // TO DEL

            double MImax = double.MinValue;
            double MIcur;
            double MIR = double.MinValue;

            var mergers = CreateMergeList();
            CalculateMIsFor(mergers);

            Merge smallestMIMerge;

            var totalIterations = SubRegionsNumber - Params.RegionsToRemain;

            for (int i = 0; i < imageID; i++)
            {
                Console.Write(Environment.NewLine);
            }

            while (SubRegionsNumber > Params.RegionsToRemain && MIR < Params.Y)
            {
                Console.Write("\rImage " + (imageID + 1).ToString() + " predicted segmentation state: " +
                ((int)(100 - 100 * (SubRegionsNumber / (double)(totalIterations)))).ToString() + "%        ");

                smallestMIMerge = mergers.FirstOrDefault();

                foreach (var merge in mergers)
                {
                    if (merge.MI < smallestMIMerge.MI)
                    {
                        smallestMIMerge = merge;
                    }
                    else if (merge.MI > MImax)
                    {
                        MImax = merge.MI;
                    }
                }

                var pairToMarge = smallestMIMerge;
                MIcur = pairToMarge.MI;
                MIR = MIcur / MImax;

                var newMergePairs = MergePair(pairToMarge);

                RemoveOldMergers(mergers, pairToMarge);

                CalculateMIsFor(newMergePairs);

                mergers.AddRange(newMergePairs);

                MIRs.Add(MIR.ToString()); //TO DEL 
            }

            SaveMIRsInFile(MIRs); //TO DEL
        }

        private List<Merge> MergePair(Merge pair)
        {
            var subRegionToRemain = SubRegions[pair.SubRegion1ID];
            var subRegionToDelete = SubRegions[pair.SubRegion2ID];

            var subRegionToDeleteNeighbors = subRegionToDelete.Neighbors;

            foreach (var neighbor in subRegionToDeleteNeighbors)
            {
                neighbor.RemoveNeighbor(subRegionToDelete.ID);
                neighbor.AddNeighbor(subRegionToRemain);
                subRegionToRemain.AddNeighbor(neighbor);
            }

            subRegionToRemain.AddBlocks(subRegionToDelete.Blocks);

            SubRegions[subRegionToDelete.ID] = null;
            SubRegionsNumber--;

            var newMergePairs = new List<Merge>();
            foreach (var neighbor in subRegionToRemain.Neighbors)
            {
                newMergePairs.Add(new Merge()
                {
                    SubRegion1ID = subRegionToRemain.ID,
                    SubRegion2ID = neighbor.ID,
                    MI = double.MaxValue
                });
            }

            return newMergePairs;
        }

        private void RemoveOldMergers(List<Merge> mergers, Merge pairToMarge)
        {
            mergers.RemoveAll(m => m.SubRegion1ID == pairToMarge.SubRegion1ID
                || m.SubRegion1ID == pairToMarge.SubRegion2ID
                || m.SubRegion2ID == pairToMarge.SubRegion1ID
                || m.SubRegion2ID == pairToMarge.SubRegion2ID);
        }

        private List<Merge> CreateMergeList()
        {
            var mergers = new List<Merge>();

            for (int i = 0; i < SubRegions.Count; i++)
            {
                var neighborsIDs = SubRegions[i].GetNeighborsIDs();
                foreach (var neighborID in neighborsIDs)
                {
                    if (neighborID < i)
                    {
                        continue; // not adding pair with ID of already processed subRegion
                    }

                    var mergePair = new Merge()
                    {
                        SubRegion1ID = SubRegions[i].ID,
                        SubRegion2ID = neighborID,
                        MI = double.MaxValue
                    };

                    mergers.Add(mergePair);
                }
            }

            return mergers;
        }

        private void CalculateMIsFor(List<Merge> mergers)
        {
            foreach (var merge in mergers)
            {
                var subRegion1 = SubRegions[merge.SubRegion1ID];
                var subRegion2 = SubRegions[merge.SubRegion2ID];

                int pixels1 = subRegion1.Pixels;
                int pixels2 = subRegion2.Pixels;

                int p = pixels1 > pixels2 ? pixels2 : pixels1; // p is the number of pixels in smaller subregion

                var sr1h = subRegion1.Histogram;
                var sr2h = subRegion2.Histogram;

                double MSE = CalculateMSE(sr1h, sr2h);

                var MI = p * MSE;
                merge.MI = MI;
            }
        }

        private double CalculateMSE(double[] histogram1, double[] histogram2)
        {
            double sum = 0;

            for (int i = 0; i < histogram1.Length; i++)
            {
                sum += Math.Pow((histogram1[i] - histogram2[i]), 2);
            }

            return sum / histogram1.Length;
        }

        private void SaveIDsInArray()
        {
            foreach (var region in SubRegions)
            {
                if (region == null)
                {
                    continue;
                }

                region.SaveIDsInArray(ID);
            }
        }

        private void SaveMIRsInFile(List<string> MIRs) // TO DEL
        {
            string path1 = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\map\MIRs.txt";

            var sb = new StringBuilder();
            foreach (var MIR in MIRs)
            {
                sb.Append(MIR + Environment.NewLine);
            }

            File.WriteAllText(path1, sb.ToString().Replace('.', ','));
        }

        private double CalculateFrontiersSimilarity(List<int> indexes1, List<int> indexes2)
        {
            int matchedIndexes = 0;

            if (indexes2.Count() > indexes1.Count())
            {
                var temp = indexes1;
                indexes1 = indexes2;
                indexes2 = temp;
            }

            foreach (var idx in indexes1)
            {
                if (indexes2.Contains(idx))
                {
                    matchedIndexes++;
                }
            }

            return matchedIndexes / (double)indexes1.Count() * 100;

        }

        private List<int> GetFrontierPixelsIndexes()
        {
            var frontierPixelsIndexes = new List<int>();

            Rectangle mainBlock = new Rectangle(3, 3, Bmp.Width - 3, Bmp.Height - 3); // omit picture frame border

            for (int i = mainBlock.Y; i < mainBlock.Height; i++)
            {
                for (int j = mainBlock.X; j < mainBlock.Width; j++)
                {
                    int pixelIndex = Bmp.Width * i + j;
                    if (IsOnFrontier(pixelIndex))
                    {
                        frontierPixelsIndexes.Add(pixelIndex);
                    }
                }
            }

            return frontierPixelsIndexes;
        }

        private bool IsOnFrontier(int pixelIndex)
        {
            #region NeighborsIndexes

            var neighborsIndexes = new List<int>
            {
                pixelIndex - Bmp.Width - 1,
                pixelIndex - Bmp.Width,
                pixelIndex - Bmp.Width + 1,
                pixelIndex + 1,
                pixelIndex - 1,
                pixelIndex + Bmp.Width - 1,
                pixelIndex + Bmp.Width,
                pixelIndex + Bmp.Width + 1
            };

            #endregion

            foreach (var neighborIdx in neighborsIndexes)
            {
                if (ID[pixelIndex] != ID[neighborIdx])
                {
                    return true;
                }
            }

            return false;
        }
    }
}
