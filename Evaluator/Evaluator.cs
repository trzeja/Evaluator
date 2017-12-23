using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing; // dodac nalezy referencje
using System.Drawing.Imaging;
using System.IO;

namespace Evaluator
{
    public class Evaluator
    {
        private byte[] _greyValues;

        private int[] _ID;
        private Bitmap _bmp;

        private List<SubRegion> _subRegions;
        private int _subRegionsNumber;

        public double ProcessImages(string path1, string path2)
        {
            ReadFile(path1);

            CreateSubRegions();
            //var h1 = GetNormalizedHistogramfromFile();
            SaveIDsInArray();
            DrawBoundariesInFile(path1);

            Merge();
            DrawBoundariesInFile(path1);

            //ReadFile(path);
            //var h2 = GetNormalizedHistogramfromFile();

            // var MSE = CalculateMSE(h1, h2);

            return 0;
        }
        //niech na razie zwraca int[] ID
        //private Task<>

        private void ReadFile(string path)
        {
            _bmp = new Bitmap(path);

            int pixelsToTrim = _bmp.Width % 4;
            if (pixelsToTrim != 0)
            {
                _bmp = _bmp.Clone(new Rectangle(0, 0, _bmp.Width - pixelsToTrim, _bmp.Height), _bmp.PixelFormat);                
            }

            Rectangle rect = new Rectangle(0, 0, _bmp.Width, _bmp.Height);
            BitmapData bmpData = _bmp.LockBits(rect, ImageLockMode.ReadWrite,
                _bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;                        

            var bytes = _bmp.Width * _bmp.Height;
            _greyValues = new byte[bytes];
            _ID = Enumerable.Repeat(-1, bytes).ToArray();

            System.Runtime.InteropServices.Marshal.Copy(ptr, _greyValues, 0, bytes);

            SubRegion.Init(_greyValues, _bmp.Width);

            _bmp.UnlockBits(bmpData);
        }
        
        private void DrawBoundariesInFile(string path)
        {
            _bmp = new Bitmap(path);

            int pixelsToTrim = _bmp.Width % 4;
            if (pixelsToTrim != 0)
            {
                _bmp = _bmp.Clone(new Rectangle(0, 0, _bmp.Width - pixelsToTrim, _bmp.Height), _bmp.PixelFormat);
            }

            Rectangle rect = new Rectangle(0, 0, _bmp.Width, _bmp.Height);
            BitmapData bmpData = _bmp.LockBits(rect, ImageLockMode.ReadWrite,
                _bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            var bytes = _bmp.Width * _bmp.Height;
            _greyValues = new byte[bytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, _greyValues, 0, bytes);

            Rectangle mainBlock = new Rectangle(1, 1, _bmp.Width - 1, _bmp.Height - 1);

            for (int i = mainBlock.Y; i < mainBlock.Height; i++)
            {
                for (int j = mainBlock.X; j < mainBlock.Width; j++)
                {
                    ChangePixelColorIfFrontier(_bmp.Width * i + j); //spr czy bmp.width dobre
                }
            }

            System.Runtime.InteropServices.Marshal.Copy(_greyValues, 0, ptr, bytes);

            _bmp.UnlockBits(bmpData);

            string output = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lena_grayDrawed.gif";

            _bmp.Save(output);

            System.Diagnostics.Process.Start(output);
        } // TO DEL

        private void ChangePixelColorIfFrontier(int pixelIdx) //TO DEL
        {
            #region NeighborsIndexes

            var neighborsIndexes = new List<int>();

            int northWestNeighborIdx = pixelIdx - _bmp.Width - 1;
            neighborsIndexes.Add(northWestNeighborIdx);

            int northNeighborIdx = pixelIdx - _bmp.Width;
            neighborsIndexes.Add(northNeighborIdx);

            int northEastNeighborIdx = pixelIdx - _bmp.Width + 1;
            neighborsIndexes.Add(northEastNeighborIdx);

            int eastNeighborIdx = pixelIdx + 1;
            neighborsIndexes.Add(eastNeighborIdx);

            int westNeighborIdx = pixelIdx - 1;
            neighborsIndexes.Add(westNeighborIdx);

            int southWestNeighborIdx = pixelIdx + _bmp.Width - 1;
            neighborsIndexes.Add(southWestNeighborIdx);

            int southNeighborIdx = pixelIdx + _bmp.Width;
            neighborsIndexes.Add(southNeighborIdx);

            int southEastNeighborIdx = pixelIdx + _bmp.Width + 1;
            neighborsIndexes.Add(southEastNeighborIdx);

            #endregion

            foreach (var neighborIdx in neighborsIndexes)
            {
                if (_ID[pixelIdx] != _ID[neighborIdx])
                {
                    _greyValues[pixelIdx] = 0;
                }
            }
        }

        private void CreateSubRegions()
        {
            _subRegions = new List<SubRegion>();

            Rectangle mainBlock = new Rectangle(1, 1, _bmp.Width - 1, _bmp.Height - 1);

            int subRegionID = 0;

            for (int i = mainBlock.Y; i <= mainBlock.Height; i += Consts.SMin)
            {
                for (int j = mainBlock.X; j <= mainBlock.Width; j += Consts.SMin)
                {
                    int newBlockWidth = Consts.SMin;
                    int newBlockHeight = Consts.SMin;

                    if (j + Consts.SMin > mainBlock.Width)
                    {
                        newBlockWidth = mainBlock.Width - j;
                    }

                    if (i + Consts.SMin > mainBlock.Height)
                    {
                        newBlockHeight = mainBlock.Height - i;
                    }

                    var newBlock = new Rectangle(j, i, newBlockWidth, newBlockHeight);

                    var newSubRegion = new SubRegion(newBlock, subRegionID++);
                    _subRegions.Add(newSubRegion);
                    _subRegionsNumber++;
                }
            }

            SetSubRegionsNeighbors();
        }

        private void SetSubRegionsNeighbors()
        {
            foreach (var region in _subRegions)
            {
                var block = region.Blocks.FirstOrDefault();
                //create block enlarged by 1 in each direction
                var enlargedBlock = new Rectangle(block.X - 1, block.Y - 1, Consts.SMin + 2, Consts.SMin + 2);

                var regionNeighbors = _subRegions
                    .Where(s => s.Blocks.FirstOrDefault().IntersectsWith(enlargedBlock) && !s.Equals(region))
                    .ToList();
                
                region.UpdateNeighbours(regionNeighbors);
            }
        }

        private void Merge()
        {
            var MIRs = new List<string>();

            double MImax = double.MinValue;
            double MIcur;
            double MIR = double.MinValue;

            var mergers = CreateMergeList();
            CalculateMIsFor(mergers);

            Merge smallestMIMerge;

            var totalIterations = _subRegionsNumber - Consts.RegionsToRemain;

            while (_subRegionsNumber > Consts.RegionsToRemain && MIR < Consts.Y)
            {
                Console.Write("\rPredicted state: " +  ((int)(100 - 100 * (_subRegionsNumber / (double)(totalIterations)))).ToString() + "%        ");
                
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
            SaveIDsInArray();
        }

        private List<Merge> MergePair(Merge pair)
        {
            var subRegionToRemain = _subRegions[pair.SubRegion1ID];
            var subRegionToDelete = _subRegions[pair.SubRegion2ID];

            var subRegionToDeleteNeighbors = subRegionToDelete.Neighbors;

            foreach (var neighbor in subRegionToDeleteNeighbors)
            {
                neighbor.RemoveNeighbor(subRegionToDelete.ID);
                neighbor.AddNeighbor(subRegionToRemain);
                subRegionToRemain.AddNeighbor(neighbor);
            }

            subRegionToRemain.AddBlocks(subRegionToDelete.Blocks);

            _subRegions[subRegionToDelete.ID] = null;
            _subRegionsNumber--;

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

            for (int i = 0; i < _subRegions.Count; i++)
            {
                var neighborsIDs = _subRegions[i].GetNeighboursIDs();
                foreach (var neighborID in neighborsIDs)
                {
                    if (neighborID < i)
                    {
                        continue; // not adding pair with ID of already processed subRegion
                    }

                    var mergePair = new Merge()
                    {
                        SubRegion1ID = _subRegions[i].ID,
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
                var subRegion1 = _subRegions[merge.SubRegion1ID];
                var subRegion2 = _subRegions[merge.SubRegion2ID];

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

        private void SaveHistogramInFile(double[] results) // TO DEL
        {
            string[] positions = new string[results.Length];

            for (int i = 0; i < results.Length; i++)
            {
                positions[i] = /*i + " " +*/ results[i].ToString().Replace('.', ',');
            }

            System.IO.File.WriteAllLines(@"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\map\histogram.txt", positions);
        }

        private void SaveIDsInArray()
        {
            foreach (var region in _subRegions)
            {
                if (region == null)
                {
                    continue;
                }

                region.SaveIDsInArray(_ID);
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
    }
}
