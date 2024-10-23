/******************************************************************************
                          Class LstSquQuadRegr
     A C#  Class for Least Squares Regression for Quadratic Curve Fitting
                          Alex Etchells  2010    
          version 2 using decimals to resolve rounding errors
          version 3 returns NaN for div by 0 for a b and c terms
******************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;


public class LstSquQuadRegr
{
    /* instance variables */
    ArrayList pointArray = new ArrayList();
    private int numOfEntries;
    private decimal[] pointpair;

    /*constructor */
    public LstSquQuadRegr()
    {
        numOfEntries = 0;
        pointpair = new decimal[2];
    }

    /*instance methods */
    public void AddPoints(double x, double y)
    {
        if (double.IsNaN(x) || double.IsNaN(y)) return;
        pointpair = new decimal[2];
        //numOfEntries -= 1;
        try
        {
            pointpair[0] = Convert.ToDecimal(x);
            pointpair[1] = Convert.ToDecimal(y);
            pointArray.Add(pointpair);
            numOfEntries += 1;

        }
        catch { }
    }
    /*
      y = ax^2 + bx + c
 
      notation Sjk to mean the sum of x_i^j*y_i^k. 
      2a*S40 + 2c*S20 + 2b*S30 - 2*S21 = 0
      2b*S20 + 2a*S30 + 2c*S10 - 2*S11 = 0
      2a*S20 + 2c*S00 + 2b*S10 - 2*S01 = 0
  
      solve the system of simultaneous equations using Cramer's law
 
      [ S40  S30  S20 ] [ a ]   [ S21 ]
      [ S30  S20  S10 ] [ b ] = [ S11 ]
      [ S20  S10  S00 ] [ c ]   [ S01 ] 
 
      D = [ S40  S30  S20 ] 
          [ S30  S20  S10 ] 
          [ S20  S10  S00 ]  
  
          S40(S20*S00 - S10*S10) - S30(S30*S00 - S10*S20) + S20(S30*S10 - S20*S20)
 
     Da = [ S21  S30  S20 ]
          [ S11  S20  S10 ] 
          [ S01  S10  S00 ]  
 
          S21(S20*S00 - S10*S10) - S11(S30*S00 - S10*S20) + S01(S30*S10 - S20*S20)
 
     Db = [ S40  S21  S20 ] 
          [ S30  S11  S10 ] 
          [ S20  S01  S00 ]  
 
          S40(S11*S00 - S01*S10) - S30(S21*S00 - S01*S20) + S20(S21*S10 - S11*S20)
  
     Dc = [ S40  S30  S21 ] 
          [ S30  S20  S11 ] 
          [ S20  S10  S01 ] 
  
          S40(S20*S01 - S10*S11) - S30(S30*S01 - S10*S21) + S20(S30*S11 - S20*S21)  
 
     */

    private decimal deciATerm()
    {
        if (numOfEntries < 3)
        {
            throw new InvalidOperationException("Insufficient pairs of co-ordinates");
        }
        //notation sjk to mean the sum of x_i^j*y_i^k. 
        decimal s40 = getSxy(4, 0);
        decimal s30 = getSxy(3, 0);
        decimal s20 = getSxy(2, 0);
        decimal s10 = getSxy(1, 0);
        decimal s00 = numOfEntries;

        decimal s21 = getSxy(2, 1);
        decimal s11 = getSxy(1, 1);
        decimal s01 = getSxy(0, 1);

        //   Da / D
        try
        {
            return (s21 * (s20 * s00 - s10 * s10) - s11 * (s30 * s00 - s10 * s20) + s01 * (s30 * s10 - s20 * s20))
                    /
                    (s40 * (s20 * s00 - s10 * s10) - s30 * (s30 * s00 - s10 * s20) + s20 * (s30 * s10 - s20 * s20));
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public double aTerm()
    {
        if (numOfEntries < 3) return 0;
        try
        {
            double result = Convert.ToDouble(deciATerm());
            if (double.IsInfinity(result)) result = double.NaN;
            return result;
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Attempted to divide by zero.")) return double.NaN;
            else throw ex;
        }
    }

    private decimal deciBTerm()
    {
        if (numOfEntries < 3)
        {
            throw new InvalidOperationException("Insufficient pairs of co-ordinates");
        }
        //notation sjk to mean the sum of x_i^j*y_i^k. 
        decimal s40 = getSxy(4, 0);
        decimal s30 = getSxy(3, 0);
        decimal s20 = getSxy(2, 0);
        decimal s10 = getSxy(1, 0);
        decimal s00 = numOfEntries;

        decimal s21 = getSxy(2, 1);
        decimal s11 = getSxy(1, 1);
        decimal s01 = getSxy(0, 1);

        //   Db / D
        try
        {
            return (s40 * (s11 * s00 - s01 * s10) - s30 * (s21 * s00 - s01 * s20) + s20 * (s21 * s10 - s11 * s20))
            /
            (s40 * (s20 * s00 - s10 * s10) - s30 * (s30 * s00 - s10 * s20) + s20 * (s30 * s10 - s20 * s20));
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public double bTerm()
    {
        if (numOfEntries < 3) return 0;
        try
        {
            double result = Convert.ToDouble(deciBTerm());
            if (double.IsInfinity(result)) result = double.NaN;
            return result;
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Attempted to divide by zero.")) return double.NaN;
            else throw ex;
        }
    }

    private decimal deciCTerm()
    {
        if (numOfEntries < 3)
        {
            throw new InvalidOperationException("Insufficient pairs of co-ordinates");
        }
        //notation sjk to mean the sum of x_i^j*y_i^k.  
        decimal s40 = getSxy(4, 0);
        decimal s30 = getSxy(3, 0);
        decimal s20 = getSxy(2, 0);
        decimal s10 = getSxy(1, 0);
        decimal s00 = numOfEntries;

        decimal s21 = getSxy(2, 1);
        decimal s11 = getSxy(1, 1);
        decimal s01 = getSxy(0, 1);

        //   Dc / D
        try
        {
            return (s40 * (s20 * s01 - s10 * s11) - s30 * (s30 * s01 - s10 * s21) + s20 * (s30 * s11 - s20 * s21))
                    /
                    (s40 * (s20 * s00 - s10 * s10) - s30 * (s30 * s00 - s10 * s20) + s20 * (s30 * s10 - s20 * s20));
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public double cTerm()
    {
        if (numOfEntries < 3) return 0;
        try
        {
            double result = Convert.ToDouble(deciCTerm());
            if (double.IsInfinity(result)) result = double.NaN;
            return result;
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("Attempted to divide by zero.")) return double.NaN;
            else throw ex;
        }
    }

    public double rSquare() // get rsquare
    {
        if (numOfEntries < 3) return 0;
        if (numOfEntries < 3)
        {
            throw new InvalidOperationException("Insufficient pairs of co-ordinates");
        }
        try
        {
            return Convert.ToDouble(1 - getSSerr() / getSStot());
        }
        catch
        {
            return double.NaN;
        }
    }


    /*helper methods*/
    private decimal getSxy(int xPower, int yPower) // get sum of x^xPower * y^yPower
    {
        decimal Sxy = 0;
        foreach (decimal[] ppair in pointArray)
        {
            decimal xToPower = 1;
            for (int i = 0; i < xPower; i++)
            {
                xToPower = xToPower * ppair[0];
            }

            decimal yToPower = 1;
            for (int i = 0; i < yPower; i++)
            {
                yToPower = yToPower * ppair[1];
            }
            Sxy += xToPower * yToPower;
        }
        return Sxy;
    }


    private decimal getYMean()
    {
        decimal y_tot = 0;
        foreach (decimal[] ppair in pointArray)
        {
            y_tot += ppair[1];
        }
        return y_tot / numOfEntries;
    }

    private decimal getSStot()
    {
        decimal ss_tot = 0;
        foreach (decimal[] ppair in pointArray)
        {
            ss_tot += (ppair[1] - getYMean()) * (ppair[1] - getYMean());
        }
        return ss_tot;
    }

    private decimal getSSerr()
    {
        decimal ss_err = 0;
        foreach (decimal[] ppair in pointArray)
        {
            ss_err += (ppair[1] - getPredictedY(ppair[0])) * (ppair[1] - getPredictedY(ppair[0]));
        }
        return ss_err;
    }


    private decimal getPredictedY(decimal x)
    {
        return deciATerm() * x * x + deciBTerm() * x + deciCTerm();
    }
}