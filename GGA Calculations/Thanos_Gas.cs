using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Thanos_Gas
{
    #region instance variables
    private string gasID;
    private string gas2LetterCode;
    private int gasMpvPos;
    private double gasCO2Conc;
    private double gasCH4Conc;
    private double gasAvCH4Meas;
    private double gasAvCO2Meas;
    private double gasAvCH4WT;
    private double gasAvCO2WT;
    private double gasAvCH4Sd;
    private double gasAvCO2Sd;
    private int gasAvCH4n = 0;
    private int gasAvCO2n = 0;
    private double gasCOConc;
    private double gasN2OConc;
    private double gasAvN2OMeas;
    private double gasAvCOMeas;
    private double gasAvN2OWT;
    private double gasAvCOWT;
    private double gasAvN2OSd;
    private double gasAvCOSd;
    private int gasAvN2On = 0;
    private int gasAvCOn = 0;
    private DateTime gasLastRun;
    private bool gasInUse = false;
    #endregion

    #region instance methods

    public string ID
    {
        get
        {
            return gasID;
        }
        set
        {
            gasID = value;
        }
    }

    public int MpvPos
    {
        get
        {
            return gasMpvPos;
        }
        set
        {
            gasMpvPos = value;
        }
    }

    public string TwoLetterCode
    {
        get
        {
            return gas2LetterCode;
        }
        set
        {
            gas2LetterCode = value;
        }
    }

    public double CO2Conc
    {
        get
        {
            return gasCO2Conc;
        }
        set
        {
            gasCO2Conc = value;
        }
    }
    public double CH4Conc
    {
        get
        {
            return gasCH4Conc;
        }
        set
        {
            gasCH4Conc = value;
        }
    }
    public double AvCH4Meas
    {
        get
        {
            return gasAvCH4Meas;
        }
        set
        {
            gasAvCH4Meas = value;
        }
    }
    public double AvCO2Meas
    {
        get
        {
            return gasAvCO2Meas;
        }
        set
        {
            gasAvCO2Meas = value;
        }
    }
    public double AvCH4Sd
    {
        get
        {
            return gasAvCH4Sd;
        }
        set
        {
            gasAvCH4Sd = value;
        }
    }
    public double AvCO2Sd
    {
        get
        {
            return gasAvCO2Sd;
        }
        set
        {
            gasAvCO2Sd = value;
        }
    }
    public int AvCH4n
    {
        get
        {
            return gasAvCH4n;
        }
        set
        {
            gasAvCH4n = value;
        }
    }
    public int AvCO2n
    {
        get
        {
            return gasAvCO2n;
        }
        set
        {
            gasAvCO2n = value;
        }
    }

    public double COConc
    {
        get
        {
            return gasCOConc;
        }
        set
        {
            gasCOConc = value;
        }
    }
    public double N2OConc
    {
        get
        {
            return gasN2OConc;
        }
        set
        {
            gasN2OConc = value;
        }
    }
    public double AvN2OMeas
    {
        get
        {
            return gasAvN2OMeas;
        }
        set
        {
            gasAvN2OMeas = value;
        }
    }
    public double AvCOMeas
    {
        get
        {
            return gasAvCOMeas;
        }
        set
        {
            gasAvCOMeas = value;
        }
    }
    public double AvN2OSd
    {
        get
        {
            return gasAvN2OSd;
        }
        set
        {
            gasAvN2OSd = value;
        }
    }
    public double AvCOSd
    {
        get
        {
            return gasAvCOSd;
        }
        set
        {
            gasAvCOSd = value;
        }
    }
    public int AvN2On
    {
        get
        {
            return gasAvN2On;
        }
        set
        {
            gasAvN2On = value;
        }
    }
    public int AvCOn
    {
        get
        {
            return gasAvCOn;
        }
        set
        {
            gasAvCOn = value;
        }
    }
    public double AvCH4WT
    {
        get
        {
            return gasAvCH4WT;
        }
        set
        {
            gasAvCH4WT = value;
        }
    }
    public double AvCO2WT
    {
        get
        {
            return gasAvCO2WT;
        }
        set
        {
            gasAvCO2WT = value;
        }
    }
    public double AvN2OWT
    {
        get
        {
            return gasAvN2OWT;
        }
        set
        {
            gasAvN2OWT = value;
        }
    }
    public double AvCOWT
    {
        get
        {
            return gasAvCOWT;
        }
        set
        {
            gasAvCOWT = value;
        }
    }
    public double AvCH4Ratio
    {
        get
        {
            return gasAvCH4Meas / gasAvCH4WT;
        }
    }
    public double AvCO2Ratio
    {
        get
        {
            return gasAvCO2Meas / gasAvCO2WT;
        }
    }
    public double AvN2ORatio
    {
        get
        {
            return gasAvN2OMeas / gasAvN2OWT;
        }
    }
    public double AvCORatio
    {
        get
        {
            return gasAvCOMeas / gasAvCOWT;
        }
    }
     
    /// <summary>
    /// Date and Time of Last Calibration Run
    /// </summary>
    public DateTime LastRun
    {
        get
        {
            return gasLastRun;
        }
        set
        {
            gasLastRun = value;
        }
    }

    /// <summary>
    /// Used to count number of passes when averaging
    /// </summary>
    public bool InUse
    {
        get
        {
            return gasInUse;
        }
        set
        {
            InUse = value;
        }
    }
    
    public void Copy(Thanos_Gas newThanos_Gas)
    {
        newThanos_Gas.gasID = this.gasID;
        newThanos_Gas.gasMpvPos = this.gasMpvPos;
        newThanos_Gas.gas2LetterCode = this.gas2LetterCode;
        newThanos_Gas.gasCO2Conc = this.gasCO2Conc;
        newThanos_Gas.gasCH4Conc = this.gasCH4Conc;
        newThanos_Gas.gasAvCH4Meas = this.gasAvCH4Meas;
        newThanos_Gas.gasAvCO2Meas = this.gasAvCO2Meas;
        newThanos_Gas.gasAvCH4Sd = this.gasAvCH4Sd;
        newThanos_Gas.gasAvCO2Sd = this.gasAvCO2Sd;
        newThanos_Gas.gasAvCH4n = this.gasAvCH4n;
        newThanos_Gas.gasAvCO2n = this.gasAvCO2n;
        newThanos_Gas.gasCOConc = this.gasCOConc;
        newThanos_Gas.gasN2OConc = this.gasN2OConc;
        newThanos_Gas.gasAvN2OMeas = this.gasAvN2OMeas;
        newThanos_Gas.gasAvCOMeas = this.gasAvCOMeas;
        newThanos_Gas.gasAvN2OSd = this.gasAvN2OSd;
        newThanos_Gas.gasAvCOSd = this.gasAvCOSd;
        newThanos_Gas.gasAvN2On = this.gasAvN2On;
        newThanos_Gas.gasAvCOn = this.gasAvCOn;
        newThanos_Gas.gasLastRun = this.gasLastRun;
        newThanos_Gas.gasInUse = this.gasInUse;
    }

    #endregion

}
