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
        double[] _histogram;

        private List<Rectangle> _blocks; // do debugowania
        private List<SubRegion> _subRegions;


        private IntPtr _ptr; //debug draw
        private BitmapData _bmpData; //debug draw

        public void ProcessImages()
        {
            string path = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lena_gray64.gif";

            //debug
            //ReadFileWithSaveOption(path);
            //CreateSubRegions();
            //MergeSubRegions();
            //DrawPictureAndSave();
            //debug


            ReadFile(path);
            CreateSubRegions();
            //var h1 = GetNormalizedHistogramfromFile();

            Merge();

            SaveIDsInArray();
            SaveIDArrayInFile();
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

            _bytes = _bmp.Width * _bmp.Height;            

            _greyValues = new byte[_bytes];
            _ID = new int[_bytes];

            //_histogram = new double[(Consts.MaxLBP + 1) * Consts.Bins];

            System.Runtime.InteropServices.Marshal.Copy(ptr, _greyValues, 0, _bytes);

            _bmp.UnlockBits(bmpData);
        }

        private void ReadFileWithSaveOption(string path)
        {
            _bmp = new Bitmap(path);

            Rectangle rect = new Rectangle(0, 0, _bmp.Width, _bmp.Height);
            _bmpData = _bmp.LockBits(rect, ImageLockMode.ReadWrite,
                _bmp.PixelFormat);

            //IntPtr ptr = bmpData.Scan0;
            _ptr = _bmpData.Scan0;

            _bytes = _bmp.Width * _bmp.Height;

            _greyValues = new byte[_bytes];
            _ID = new int[_bytes];

            _histogram = new double[(Consts.MaxLBP + 1) * Consts.Bins];

            System.Runtime.InteropServices.Marshal.Copy(_ptr, _greyValues, 0, _bytes);

            //_bmp.UnlockBits(bmpData);
        }

        private void DrawPictureAndSave()
        {
            System.Runtime.InteropServices.Marshal.Copy(_greyValues, 0, _ptr, _bytes);

            // Unlock the bits.
            _bmp.UnlockBits(_bmpData);

            // Draw the modified image.
            //e.Graphics.DrawImage(bmp, 0, 150);
            string output = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lena_grayDrawed.gif";

            _bmp.Save(output);

            System.Diagnostics.Process.Start(output);
        }
             

        //private double[] GetNormalizedHistogramfromFile()
        //{
        //    Rectangle mainBlock = new Rectangle(1, 1, _bmp.Width - 2, _bmp.Height - 2);
        //    var h = GetNormalizedHistogramFrom(mainBlock);
        //    return h;
        //}

        private void CreateSubRegions()
        {
            _blocks = new List<Rectangle>();
            _subRegions = new List<SubRegion>();

            Rectangle mainBlock = new Rectangle(1, 1, _bmp.Width - 1, _bmp.Height - 1);

            int id = 0;

            for (int i = mainBlock.Y; i <= mainBlock.Height; i += Consts.minimumBlockSize)
            {
                for (int j = mainBlock.X; j <= mainBlock.Width; j += Consts.minimumBlockSize)
                {
                    int newBlockWidth = Consts.minimumBlockSize;
                    int newBlockHeight = Consts.minimumBlockSize;

                    if (j + Consts.minimumBlockSize > mainBlock.Width)
                    {
                        newBlockWidth = mainBlock.Width - j;
                    }

                    if (i + Consts.minimumBlockSize > mainBlock.Height)
                    {
                        newBlockHeight = mainBlock.Height - i;
                    }

                    var newBlock = new Rectangle(j, i, newBlockWidth, newBlockHeight);
                    _blocks.Add(newBlock);
                    var newSubRegion = new SubRegion();
                    newSubRegion.Blocks.Add(newBlock);
                    newSubRegion.ID = id++;

                    _subRegions.Add(newSubRegion);
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
                var enlargedBlock = new Rectangle(block.X - 1, block.Y - 1, Consts.minimumBlockSize + 2, Consts.minimumBlockSize + 2);

                var regionNeighbors = _subRegions
                    .Where(s => s.Blocks.FirstOrDefault().IntersectsWith(enlargedBlock) && !s.Equals(region))
                    .ToList();

                region.Neighbors = regionNeighbors;
            }
        }

        private void Merge()
        {
            var mergers = CreateMergeList();
            CalculateMIsFor(mergers);
            
            //tu bedzie for i warunek stopu Y
            var smallestMIMergeIdx = 0;
            for (int i = 1; i < mergers.Count; i++)
            {
                if (mergers[i].MI < mergers[smallestMIMergeIdx].MI)
                {
                    smallestMIMergeIdx = i;
                }
            }

            var pairToMarge = mergers[smallestMIMergeIdx];
            MergePair(pairToMarge);
            // Merge 2 regions of smallestMIMergeIdx
            //...                   
        }

        private void MergePair(Merge pair)
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
            //trzbea przekazac bloki usuwanego zjadajacemu
            subRegionToRemain.AddBlocks(subRegionToDelete.Blocks);                
            
            _subRegions.Remove(subRegionToDelete);
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
                    var mergePair = new Merge() { SubRegion1ID = _subRegions[i].ID , SubRegion2ID = neighborID, MI = double.MaxValue };
                    
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

                int pixels1 = subRegion1.GetPixelCount();
                int pixels2 = subRegion2.GetPixelCount();

                int p = pixels1 > pixels2 ? pixels2 : pixels1; //p is number of pixels in smaller subregion
                var sr1h = subRegion1.GetNormalizedHistogram(_greyValues, _bmp.Width);
                var sr2h = subRegion2.GetNormalizedHistogram(_greyValues, _bmp.Width);

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

        private void SaveResults(double[] results)
        {
            string[] positions = new string[results.Length];

            for (int i = 0; i < results.Length; i++)
            {
                positions[i] = i + " " + results[i];
            }

            System.IO.File.WriteAllLines(@"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\results.txt", positions);
        }

        private void SaveIDsInArray()
        {
            foreach (var region in _subRegions)
            {
                region.SaveIDInArray(_ID, _bmp.Width);
            }
        }

        private void SaveIDArrayInFile()
        {
            string path = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\IDmap.txt";

            var sb = new StringBuilder();
            for (int i = 0; i < _ID.Length; i++)
            {
                var id = _ID[i].ToString("X");                
                sb.Append(id);
                if ((i + 1)%_bmp.Width == 0 && i !=0)
                {
                    sb.Append(Environment.NewLine);
                }
            }
            
            File.WriteAllText(path, sb.ToString());
        }
        
    }
}
