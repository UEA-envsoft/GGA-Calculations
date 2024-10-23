using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;


public class SolveLinearRegression
{
    /* Based on Simon's SolveMultipointLinear, derives slope intcpt and rsq from a bunch of co-ords */

    /* instance variables */

    ArrayList pointArray = new ArrayList(); 
    private int numOfEntries; 
    private double[] pointpair;          

    /*constructor */
    public SolveLinearRegression()
    {
        numOfEntries = 0;
        pointpair = new double[2];
    }

    /*instance methods */
    public void AddPoints(double x, double y)
    {
        if (double.IsNaN(x) || double.IsNaN(y)) return;
        pointpair = new double[2]; 
        numOfEntries +=1; 
        pointpair[0] = x; 
        pointpair[1] = y;
        pointArray.Add(pointpair);
    }

    public double Slope() // get slope
    {
        if (numOfEntries < 2) return 0;
        return ((this.numOfEntries * this.getSxy()) - (this.getSx() * this.getSy())) /
                ((this.numOfEntries * this.getSxx()) - (this.getSx() * this.getSx()));
    }

    public double Intercept()  // get intercept
    {
        if (numOfEntries < 2) return 0;
        return (this.getSy() - (this.Slope() * this.getSx())) / this.numOfEntries;
    }

    public double RSquare() // get rsquare
    {
        if (numOfEntries < 2) return 0;
        double denom = (((this.numOfEntries * this.getSxx()) - (this.getSx() * this.getSx())) *
                        ((this.numOfEntries * this.getSyy()) - (this.getSy() * this.getSy())));
        denom = Math.Sqrt(denom);
        double r = ((this.numOfEntries * this.getSxy()) - (this.getSx() * this.getSy())) / denom;
        return r * r;
    }

    /*helper methods*/
    private double getSx() // get sum of x
    {
        double Sx = 0;
        foreach (double[] ppair in pointArray)
        {
            Sx += ppair[0];
        }
        return Sx;
    }

    private double getSy() // get sum of y
    {
        double Sy = 0;
        foreach (double[] ppair in pointArray)
        {
            Sy += ppair[1];
        }
        return Sy;
    }

    private double getSxx() // get sum of x*x
    {
        double Sxx = 0;
        foreach (double[] ppair in pointArray)
        {
            Sxx += ppair[0] * ppair[0]; // sum of x*x
        }
        return Sxx;
    }

    private double getSyy() // get sum of y*y
    {
        double Syy = 0;
        foreach (double[] ppair in pointArray)
        {
            Syy += ppair[1] * ppair[1]; // sum of y*y
        }
        return Syy;
    }

    private double getSxy() // get sum of x*y
    {
        double Sxy = 0;
        foreach (double[] ppair in pointArray)
        {
            Sxy += ppair[0] * ppair[1]; // sum of x*y
        }
        return Sxy;
    }



   
}

