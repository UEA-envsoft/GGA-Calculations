using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

/*******************************************************************
 *      Class to generate mean and sd from a bunch of numbers      *
 *                      Alex Etchells 2008                         *
 *            converted from vb6 class of same name                *
 *            addition of trimmed functions and minvalue 2/10/11   *
 *           additon of maxvalue and percentile functions 10/07/23 *
 ******************************************************************/


public class DataAverage
{

    #region instance variables
    private double[] resultArray;
    private int numOfEntries;
    private double[] daMeanAndSD = new double[3];
    private double daMaxValue = -1;
    private double daMinValue = -1;
    #endregion



    #region constructor
    public DataAverage(int maxEntries)
    {
        numOfEntries = 0;
        resultArray = new double[maxEntries];
    }
    #endregion

    #region instance methods
    /// <summary>
    /// Resets the DataAverage would you believe?
    /// </summary>
    private void Reset()
    {
        numOfEntries = 0;
        daMaxValue = -1;
        daMinValue = -1;
        for (int i = 0; i < resultArray.Length; i++)
        {
            resultArray[i] = 0;
        }
    }

    public void AddDataPoint(double dPoint)
    {
        if (double.IsNaN(dPoint))
        {
            return;
        }
        if ((numOfEntries + 1) > resultArray.Length)
        {
            this.Reset();
        }
        numOfEntries++;
        resultArray[numOfEntries - 1] = dPoint;
        if (dPoint > daMaxValue || numOfEntries == 1) daMaxValue = dPoint;
        if (dPoint < daMinValue || numOfEntries == 1) daMinValue = dPoint;
    }

    /// <summary>
    /// Returns trimmed mean, sd, no. of entries
    /// </summary>
    public double[] TrimmedMeanSdNumEntries
    {
        get
        {
            //establish boundaries
            //there are numOfEntries  values
            //25% of entries = (numOfEntries/4)  = chop25

            // exclude entries 0 to chop25 -1  (first 25%)
            // exclude entries (numOfEntries - chop25) to (numOfEntries -1 )  (last 25%)
            int chop25 = (int)numOfEntries / 4;
            //we need an array that is only the size of the result set so that we can sort it.
            double[] newArray = new double[numOfEntries];
            for (int i = 0; i < numOfEntries ; i++)
            {
                newArray[i] = resultArray[i];
            }
            //sort the array
            Array.Sort(newArray);
            double[] trResultArray = new double[numOfEntries];
            int trNumOfEntries = 0;
            double trRunningTotal = 0;
            //collect trimmed values
            for (int i = chop25; i < (numOfEntries - chop25); i++)
            {
                trResultArray[trNumOfEntries] = newArray[i];
                trRunningTotal += newArray[i];
                trNumOfEntries++;
            }
                                                /*    METHOD for 25% of spread
                                                //establish boundaries
                                                double spread = daMaxValue - daMinValue;
                                                double trMinValue = daMinValue + spread / 4;
                                                double trMaxValue = daMaxValue - spread / 4;
                                                //build new array based on those boundaries
                                                double[] trResultArray = new double[numOfEntries];
                                                int trNumOfEntries = 0;
                                                double trRunningTotal = 0;
                                                //collect trimmed values
                                                 for (int i = 0; i < numOfEntries; i++)
                                                 {
                                                     if (resultArray[i] > trMinValue && resultArray[i] < trMaxValue)
                                                     {
                                                         trResultArray[trNumOfEntries] = resultArray[i];
                                                         trRunningTotal += resultArray[i];
                                                         trNumOfEntries++;
                                                     }
                                                 }
                                                */
            //mean
             double trMean = 0;
             if (trNumOfEntries > 0)
             {
                 trMean = trRunningTotal / trNumOfEntries;
             }
             //SD
             double trSumOfSquares = 0;
             double trSD = 0;
             if (trNumOfEntries > 1)
             {
                 for (int i = 0; i < trNumOfEntries; i++)
                 {
                     trSumOfSquares += Math.Pow((trMean - trResultArray[i]), 2);
                 }
                 trSD = Math.Sqrt(trSumOfSquares / (trNumOfEntries - 1));
             }
            //build return array
             double[] trMeanAndSD = new double[3];
            trMeanAndSD[0] = trMean;
            trMeanAndSD[1] = trSD;
            trMeanAndSD[2] = trNumOfEntries;
            return trMeanAndSD;
        }
    }


    /// <summary>
    /// Returns mean, sd, no. of entries
    /// </summary>
    public double[] MeanSdNumEntries
    {
        get
        {
            daMeanAndSD[0] = this.GetMean();
            daMeanAndSD[1] = this.GetSD();
            daMeanAndSD[2] = this.numOfEntries;
            return daMeanAndSD;
        }
    }

    /// <summary>
    /// Returns mean, sd, no. of entries. Then resets the DataAverage
    /// </summary>
    public double[] GetAndReset
    {
        get
        {
            double[] retRes = this.MeanSdNumEntries;
            this.Reset();
            return retRes;
        }
    }

    /// <summary>
    /// Returns trimmed mean, sd, no. of entries. Then resets the DataAverage
    /// </summary>
    public double[] GetTrimmedAndReset
    {
        get
        {
            double[] retRes = this.TrimmedMeanSdNumEntries;
            this.Reset();
            return retRes;
        }
    }
    #endregion

    /// <summary>
    /// Returns max value
    /// </summary>
    public double MaxValue
    {
        get
        {
            if (numOfEntries == 0) return 0; //perhaps replace this with an exception?
            return daMaxValue;
        }
    }

    /// <summary>
    /// Returns min value
    /// </summary>
    public double MinValue
    {
        get
        {
            if (numOfEntries == 0) return 0; //perhaps replace this with an exception?
            return daMinValue;
        }
    }

    /// <summary>
    /// Returns percetile (linear interpolation between closest ranks)
    /// </summary>
    public double GetPercentile(double percentile)
    {
        if (numOfEntries == 0) return double.NaN;

        if (numOfEntries == 1) return resultArray[0];


        //we need an array that is only the size of the result set so that we can sort it.
        double[] valuesArray = new double[numOfEntries];
        for (int i = 0; i < numOfEntries; i++)
        {
            valuesArray[i] = resultArray[i];
        }
        //sort the array
        Array.Sort(valuesArray);

        /*for N sorted values   V1=<V2<=V3<=......<=VN
        percent rank (pr) for nth value:
        prn = (100/N)(n - 1/2)
        eg for 5 values   N=5    pr for third value..
        pr3 = (100/5)(3 - 1/2)   =    20 x  2.5  = 50
        
        the value pf the  P-th percentile can be calc as follows:
        if   P <  pr1   V  = V1
        if   P >  prN   V  = VN
         
        if there is an integer k for which  P = prk    then  V  =  Vk

        otherwise find integer k for which   prk < P < prk+1 and take
        V = Vk + ((P-prk)/(prk+1-pk))(Vk+1 - Vk)
          = Vk + N((P-prk)/100)(Vk+1-Vk) 
          */

        //create percentrannk array
        double[] prArray = new double[numOfEntries];
        for (int i = 0; i < numOfEntries; i++)
        {
            int n = i+1;
            prArray[i] = (100 / numOfEntries) * (n - 0.5);
        }

        //if   P <  pr1   V  = V1
        if (percentile < prArray[0]) return valuesArray[0];
        //if   P >  prN   V  = VN
        if (percentile > prArray[numOfEntries - 1]) return valuesArray[numOfEntries-1];

        // if there is an integer k for which  P = prk    then  V  =  Vk
        //otherwise find integer k for which   prk < P < prk+1
        int k = -1;
        for (int i = 0; i < numOfEntries; i++)
        {
            if (prArray[i] == percentile) return valuesArray[i];
            if (i > 0 && prArray[i - 1] < percentile && prArray[i] > percentile) k = i - 1;
        }

        if (k == -1) return double.NaN;

        // V = Vk + N((P-prk)/100)(Vk+1-Vk)

        return valuesArray[k] + (numOfEntries * ((percentile - prArray[k]) / 100) * (valuesArray[k + 1] - valuesArray[k]));
    }






    #region helper methods
    private double GetMean()
    {
        double runningTotal = 0;
        if (numOfEntries == 0) return 0;
        for (int i = 0; i < numOfEntries; i++)
        {
            runningTotal += resultArray[i];
        }
        return runningTotal / numOfEntries;
    }

    private double GetSD()
    {
        double sumOfSquares = 0;
        if (numOfEntries < 2) return 0;
        for (int i = 0; i < numOfEntries; i++)
        {
            sumOfSquares += Math.Pow((this.GetMean() - resultArray[i]), 2);
        }
        return Math.Sqrt(sumOfSquares / (numOfEntries - 1));
    }
    #endregion
}
