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
        private int _bytes;
        private byte[] _greyValues;

        private int[] _ID;
        private Bitmap _bmp;

        private int _subRegionIDCounter = 0;

        private List<Rectangle> _blocks; // do debugowania
        private List<SubRegion> _subRegions;

        public void ProcessImages()
        {
            //string path = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\base.gif";
            //string path = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\mosaic3.gif";
            string path = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lena_gray515.gif";
            //string path = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lake.gif";

            ReadFile(path);
            

            CreateSubRegions();
            //var h1 = GetNormalizedHistogramfromFile();
            SaveIDsInArray();
            DrawBoundariesInFile(path);


            Merge();
            DrawBoundariesInFile(path);

            //ReadFile(path);
            //var h2 = GetNormalizedHistogramfromFile();

            //SaveResults(h1);

            // var MSE = CalculateMSE(h1, h2);
        }

        private void ReadFile(string path)
        {
            _bmp = new Bitmap(path);

            Rectangle rect = new Rectangle(0, 0, _bmp.Width, _bmp.Height);
            BitmapData bmpData = _bmp.LockBits(rect, ImageLockMode.ReadWrite,
                _bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            int pixelsToTrim = _bmp.Width % 4;


            //bmpData.Stride

            _bytes = _bmp.Width * _bmp.Height;
            _greyValues = new byte[_bytes];
            _ID = Enumerable.Repeat(-1, _bytes).ToArray();

            System.Runtime.InteropServices.Marshal.Copy(ptr, _greyValues, 0, _bytes);








            SubRegion.Init(_greyValues, _bmp.Width);

            _bmp.UnlockBits(bmpData);

            _subRegionIDCounter = 0;
        }

        private void DrawBoundariesInFile(string path)
        {
            _bmp = new Bitmap(path);

            Rectangle rect = new Rectangle(0, 0, _bmp.Width, _bmp.Height);
            BitmapData bmpData = _bmp.LockBits(rect, ImageLockMode.ReadWrite,
                _bmp.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            _bytes = _bmp.Width * _bmp.Height;
            _greyValues = new byte[_bytes];
            //_ID = Enumerable.Repeat(-1, _bytes).ToArray();

            System.Runtime.InteropServices.Marshal.Copy(ptr, _greyValues, 0, _bytes);

            //SubRegion.Init(_greyValues, _bmp.Width);
                      
            Rectangle mainBlock = new Rectangle(1, 1, _bmp.Width - 1, _bmp.Height - 1);

            for (int i = mainBlock.Y; i < mainBlock.Height; i ++)
            {
                for (int j = mainBlock.X; j < mainBlock.Width; j ++)
                {                    
                    ChangePixelToWhiteIfFrontier(_bmp.Width * i + j); //spr czy bmp.width dobre
                }
            }
                        
            System.Runtime.InteropServices.Marshal.Copy(_greyValues, 0, ptr, _bytes);
            
            _bmp.UnlockBits(bmpData);
            
            string output = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lena_grayDrawed.gif";
            
            _bmp.Save(output);
            
            System.Diagnostics.Process.Start(output);
        }

        private void ChangePixelToWhiteIfFrontier(int pixelIdx)
        {
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

            for (int i = mainBlock.Y; i <= mainBlock.Height; i += Consts.SMax)
            {
                for (int j = mainBlock.X; j <= mainBlock.Width; j += Consts.SMax)
                {
                    int newBlockWidth = Consts.SMax;
                    int newBlockHeight = Consts.SMax;

                    if (j + Consts.SMax > mainBlock.Width)
                    {
                        newBlockWidth = mainBlock.Width - j;
                    }

                    if (i + Consts.SMax > mainBlock.Height)
                    {
                        newBlockHeight = mainBlock.Height - i;
                    }

                    var newBlock = new Rectangle(j, i, newBlockWidth, newBlockHeight);
                                        
                    //SplitHierarchically(newBlock);
                    SplitAll(newBlock);
                                       
                }
            }

            SetSubRegionsNeighbors();
        }

        private void SplitAll(Rectangle block)
        {
            var newSubRegion = new SubRegion(block, _subRegionIDCounter++);
            _subRegions.Add(newSubRegion);
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

                region.Neighbors = regionNeighbors;
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

            //int oneTenthOfAllPossibleMergers = mergers.Count() / 10;
            int oneTenthOfAllPossibleMergers = 0;
            Merge smallestMIMerge;
                        
            //while (oneTenthOfAllPossibleMergers-- > Consts.ForceStop /*|| MIR < Consts.Y*/)
                while (MIR < Consts.Y)
                {
                Console.WriteLine(oneTenthOfAllPossibleMergers);

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

                var newMergePairs = MergePair(pairToMarge); //tutaj licza sie na nowo histogramy z uwzgl nowych blokow

                RemoveOldMergers(mergers, pairToMarge);
                //smallestMIMerge.MI = double.MaxValue;

                CalculateMIsFor(newMergePairs); //tutaj odwoje sie do tych policzonych juz

                mergers.AddRange(newMergePairs);

                SaveIDsInArray();

                MIRs.Add(MIR.ToString());
            }

            SaveMIRsInFile(MIRs);
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

            _subRegions[subRegionToDelete.ID] = null; //po usunietym subregionie zostaje null

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
                        continue; //not adding pair with ID of already processed subRegion
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

                int p = pixels1 > pixels2 ? pixels2 : pixels1; //p is number of pixels in smaller subregion

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

        private void SaveHistogramInFile(double[] results)
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
                region.SaveIDInArray(_ID, _bmp.Width);
            }
        }

        private void SaveMIRsInFile(List<string> MIRs)
        {
            string path1 = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\map\MIRs.txt";

            var sb = new StringBuilder();
            foreach (var MIR in MIRs)
            {
                sb.Append(MIR + Environment.NewLine);
            }

            File.WriteAllText(path1, sb.ToString().Replace('.', ','));
        }

        private double[] GetHistogramFrom(Rectangle block)
        {
            var histogram = new double[(Consts.MaxLBP + 1) * Consts.Bins];

            for (int i = block.Y; i < block.Y + block.Height; i++)
            {
                for (int j = block.X; j < block.X + block.Width; j++)
                {
                    var LBPC = HelperMethods.CountLBPC(_greyValues, _bmp.Width, _bmp.Width * i + j);
                    int b = HelperMethods.GetBinFor(LBPC.C);

                    histogram[(LBPC.LBP) * Consts.Bins + b]++;
                }
            }

            return histogram;
        }

    }
}
