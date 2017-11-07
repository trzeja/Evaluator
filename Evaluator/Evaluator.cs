using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing; // dodac nalezy referencje
using System.Drawing.Imaging;

namespace Evaluator
{

    public class Evaluator
    {
        private int _bytes;
        private byte[] _greyValues;


        private byte[] _ID;
        private Bitmap _bmp;
        double[] _histogram;

        private List<Rectangle> _blocks; // do debugowania
        private List<SubRegion> _subRegions;


        public void ProcessImages()
        {
            string path = @"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\lena_gray64.gif";

            ReadFile(path);
            CreateSubRegions();
            var h1 = GetNormalizedHistogramfromFile();

            ReadFile(path);
            var h2 = GetNormalizedHistogramfromFile();

           // SaveResults(h1);

            var MSE = CalculateMSE(h1, h2);
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
            _ID = new byte[_bytes];

            _histogram = new double[(Consts.MaxLBP + 1) * Consts.Bins];

            System.Runtime.InteropServices.Marshal.Copy(ptr, _greyValues, 0, _bytes);

            _bmp.UnlockBits(bmpData);
        }
             

        private double[] GetNormalizedHistogramfromFile()
        {
            Rectangle mainBlock = new Rectangle(1, 1, _bmp.Width - 1, _bmp.Height - 1);
            var h = GetNormalizedHistogramFrom(mainBlock);
            return h;
        }

        private void CreateSubRegions()
        {
            _blocks = new List<Rectangle>();
            _subRegions = new List<SubRegion>();

            Rectangle mainBlock = new Rectangle(1, 1, _bmp.Width - 1, _bmp.Height - 1);

            int id = 0;

            for (int i = mainBlock.Y; i < mainBlock.Height; i += Consts.minimumBlockSize)
            {
                for (int j = mainBlock.X; j < mainBlock.Width; j += Consts.minimumBlockSize)
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

                    var newBlock = new Rectangle(i, j, newBlockWidth, newBlockHeight);
                    _blocks.Add(newBlock);
                    var newSubRegion = new SubRegion();
                    newSubRegion.Blocks.Add(newBlock);
                    newSubRegion.ID = id++;

                    _subRegions.Add(newSubRegion);
                }
            }

            //ustaw sasiadow w subRegionach
           
            foreach (var region in _subRegions)
            {
                var block = region.Blocks.FirstOrDefault();
                var enlargedBlock = new Rectangle(block.X - 1, block.Y - 1, Consts.minimumBlockSize + 2, Consts.minimumBlockSize + 2);
                
                //var allRegions = _subRegions.Where(s => s.Blocks.FirstOrDefault())

                var regionNeighbors = _subRegions
                    .Where(s => s.Blocks.FirstOrDefault().IntersectsWith(enlargedBlock) && !s.Equals(region)).ToList();

                region.Neighbors = regionNeighbors;

            }

            var t = 1;

        }

        private double[] GetNormalizedHistogramFrom(Rectangle block)
        {
            for (int i = block.Y; i < block.Height; i++)
            {
                for (int j = block.X; j < block.Width; j++)
                {
                    var LBPC = HelperMethods.CountLBPC(_greyValues, _bmp.Width, _bmp.Width * i + j);                    
                    int b = HelperMethods.GetBinFor(LBPC.C);

                    _histogram[(LBPC.LBP) * Consts.Bins + b]++;
                }
            }

            NormalizeHistogram(_histogram, _bmp.Width * _bmp.Height);

            return _histogram;
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

        private void NormalizeHistogram(double[] histogram, int pixels)
        {
            for (int i = 0; i < histogram.Length; i++)
            {
                histogram[i] /= pixels;
            }
        }

        //private void MainLoop()
        //{
        //    extraStrideBytesPerLine
        //}
        
        private void SaveResults(double[] results)
        {
            string[] positions = new string[results.Length];

            for (int i = 0; i < results.Length; i++)
            {
                positions[i] = i + " " + results[i];
            }

            System.IO.File.WriteAllLines(@"C:\Users\trzej_000\Google Drive\Politechniczne\INZ\results.txt", positions);
        }
    }
}
