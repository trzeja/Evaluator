using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Evaluator
{
    public static class HelperMethods
    {
        

        static public int GetBinFor(double c)
        {
            c += Consts.MaxC; // move to <0,MaxC*2> range from <-MaxC,MaxC>             
            int bin = (int)Math.Floor(c / Consts.BinSize);
            
            return bin;
        }

        public static LBPC CountLBPC(byte[] greyValues, int width, int pixelIdx)
        {
            //TODO na razie nie baw sie w refactor ilno brutal
            byte LBP = 0;
            double C = 0;
            int biggerOrEqualNeighborsSum = 0;
            int biggerOrEqualNeighborsCount = 0;
            int smallerNeighborsSum = 0;
            int smallerNeighborsCount = 0;

            #region NeighboursIndexes

            int northWestNeighborIdx = pixelIdx - width - 1;
            int northNeighborIdx = pixelIdx - width;
            int northEastNeighborIdx = pixelIdx - width + 1;

            int eastNeighborIdx = pixelIdx + 1;
            int westNeighborIdx = pixelIdx - 1;

            int southWestNeighborIdx = pixelIdx + width - 1;
            int southNeighborIdx = pixelIdx + width;
            int southEastNeighborIdx = pixelIdx + width + 1;

            #endregion
            
            if (greyValues[pixelIdx] <= greyValues[northWestNeighborIdx])
            {
                LBP += 1;

                biggerOrEqualNeighborsSum += greyValues[northWestNeighborIdx];
                biggerOrEqualNeighborsCount++;
            }
            else
            {
                smallerNeighborsSum += greyValues[northWestNeighborIdx];
            }

            if (greyValues[pixelIdx] <= greyValues[northNeighborIdx])
            {
                LBP += 2;

                biggerOrEqualNeighborsSum += greyValues[northNeighborIdx];
                biggerOrEqualNeighborsCount++;
            }
            else
            {
                smallerNeighborsSum += greyValues[northNeighborIdx];
            }

            if (greyValues[pixelIdx] <= greyValues[northEastNeighborIdx])
            {
                LBP += 4;

                biggerOrEqualNeighborsSum += greyValues[northEastNeighborIdx];
                biggerOrEqualNeighborsCount++;
            }
            else
            {
                smallerNeighborsSum += greyValues[northEastNeighborIdx];
            }

            if (greyValues[pixelIdx] <= greyValues[westNeighborIdx])
            {
                LBP += 8;

                biggerOrEqualNeighborsSum += greyValues[westNeighborIdx];
                biggerOrEqualNeighborsCount++;
            }
            else
            {
                smallerNeighborsSum += greyValues[westNeighborIdx];
            }

            if (greyValues[pixelIdx] <= greyValues[eastNeighborIdx])
            {
                LBP += 16;

                biggerOrEqualNeighborsSum += greyValues[eastNeighborIdx];
                biggerOrEqualNeighborsCount++;
            }
            else
            {
                smallerNeighborsSum += greyValues[eastNeighborIdx];
            }

            if (greyValues[pixelIdx] <= greyValues[southWestNeighborIdx])
            {
                LBP += 32;

                biggerOrEqualNeighborsSum += greyValues[southWestNeighborIdx];
                biggerOrEqualNeighborsCount++;
            }
            else
            {
                smallerNeighborsSum += greyValues[southWestNeighborIdx];
            }

            if (greyValues[pixelIdx] <= greyValues[southNeighborIdx])
            {
                LBP += 64;

                biggerOrEqualNeighborsSum += greyValues[southNeighborIdx];
                biggerOrEqualNeighborsCount++;
            }
            else
            {
                smallerNeighborsSum += greyValues[southNeighborIdx];
            }

            if (greyValues[pixelIdx] <= greyValues[southEastNeighborIdx])
            {
                LBP += 128;

                biggerOrEqualNeighborsSum += greyValues[southEastNeighborIdx];
                biggerOrEqualNeighborsCount++;
            }
            else
            {
                smallerNeighborsSum += greyValues[southEastNeighborIdx];
            }

            smallerNeighborsCount = Consts.Neighbors - biggerOrEqualNeighborsCount;

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
