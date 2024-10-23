using System.Runtime;
using System.Threading;
using System;

namespace GGA_Calculations
{
    public partial class Form1 : Form
    {

        #region instance variables

        //State Machine
        enum States { Init, Running, Quitting, Error, Quit };

        //version
        string curSW_Version;

        //are we developing?
        bool simulatingForDev = false;

        const int CURV1POSITIONS = 10;

        private Panel[] cylRegs = new Panel[10];

        //Va1
        private ViciMultiPosV4 va1 = new ViciMultiPosV4(CURV1POSITIONS);
        private int v1DesiredPos = 6; //resting position
        private int v1CurrentPos = -1;

        //labjack
        envSoftLabJack ljack = new envSoftLabJack(0);
        bool cyclingValco = false;
        DateTime restartValcoTime = DateTime.Now;

        //file locations
        private string outputFilePath = "";

        //program settings
        private ProgSettings pSettings;

        //Program Settings file location
        String settingsFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\envSoft\\Moliarty";


        //auto running in thanos
        private bool measuringCalOrUnk = false;


        //Backup
        String dropboxFrequency = "Inactive";
        DateTime dropboxUpdateTime;

        //ports
        private SerialPort n2oPort;//  = new SerialPort(pSettings.GetString("N2OPort"), 9600, Parity.None, 8, StopBits.One);
        private SerialPort ggaPort;// = new SerialPort(pSettings.GetString("GGAPort"), 9600, Parity.None, 8, StopBits.One);
        //private bool assigningPorts = false;


        private Random randomNumber = new Random();
        private bool forceResting = false;

        //Times and Timing
        private DateTime previousTime = DateTime.Now;
        private double lastRecordAndCalculate;

        //PicarroGGA
        private Picarro2301_streaming_Interpreter picGGA = new Picarro2301_streaming_Interpreter();
        private string ggaOutput = "";
        private double ggaTimeCount;
        private double wtCH4 = 1900;
        private double wtCO2 = 390;

        //LosGatosN2O
        private LosGatosN2O_Interpreter lgN2O = new LosGatosN2O_Interpreter();
        private string n2oOutput = "";
        private double n2oTimeCount;
        private double wtN2O = 131;
        private double wtCO = 115;

        //DataAverages
        private DataAverage av_GGACH4_30sec = new DataAverage(65); //space for a min
        private DataAverage av_GGACO2_30sec = new DataAverage(65);

        private DataAverage av_GGACH4dry_30sec = new DataAverage(65); //space for a min
        private DataAverage av_GGAH2O_30sec = new DataAverage(65);
        private DataAverage av_GGACO2dry_30sec = new DataAverage(65);

        private DataAverage av_CO2 = new DataAverage(600); //space for 10 mins data
        private DataAverage av_CH4 = new DataAverage(600); //space for 10 mins data
        private DataAverage av_CO = new DataAverage(600); //space for 10 mins data
        private DataAverage av_N2O = new DataAverage(600); //space for 10 mins data


        private DataAverage av_N2OCO_30sec = new DataAverage(65);
        private DataAverage av_N2ON2O_30sec = new DataAverage(65);
        private DataAverage av_N2OCOdry_30sec = new DataAverage(65);
        private DataAverage av_N2ON2Odry_30sec = new DataAverage(65);
        private DataAverage av_N2OH2O_30sec = new DataAverage(65);

        private DataAverage av_CH4_aTerm = new DataAverage(4);
        private DataAverage av_CH4_bTerm = new DataAverage(4);
        private DataAverage av_CH4_cTerm = new DataAverage(4);
        private DataAverage av_CH4_rSquare = new DataAverage(4);
        private DataAverage av_CO2_aTerm = new DataAverage(4);
        private DataAverage av_CO2_bTerm = new DataAverage(4);
        private DataAverage av_CO2_cTerm = new DataAverage(4);
        private DataAverage av_CO2_rSquare = new DataAverage(4);
        private DataAverage av_N2O_aTerm = new DataAverage(4);
        private DataAverage av_N2O_bTerm = new DataAverage(4);
        private DataAverage av_N2O_cTerm = new DataAverage(4);
        private DataAverage av_N2O_rSquare = new DataAverage(4);
        private DataAverage av_CO_aTerm = new DataAverage(4);
        private DataAverage av_CO_bTerm = new DataAverage(4);
        private DataAverage av_CO_cTerm = new DataAverage(4);
        private DataAverage av_CO_rSquare = new DataAverage(4);

        private const int MEAN = 0;
        private const int SD = 1;
        private const int COUNT = 2;

        private Bitmap[] va1Images = new Bitmap[10];

        //MACROS
        Thread macroThread;
        private bool macroRunning = false;
        /// <summary>
        /// Filename and path of the currently selected macro
        /// </summary>
        private string currentMacro;
        private bool abortMacro = false;
        private bool testMacro = false;
        private bool macroCompleted = false;
        private int macroPause = -1;
        private int pauseInterval = 0;
        private DateTime macroStartTime;
        private bool pauseMacro = false;
        private int[] macroCounter = new int[10];
        private bool macroStep = false;
        Brush myBrush = Brushes.Black;
        private string macroType = " ";
        private bool recordCal = false;
        private bool recordUnk = false;
        private const int STEP = 0;
        private const int WT1MONITOR = 1;
        private const int G1MONITOR = 2;
        private const int WT2MONITOR = 3;
        private const int G2MONITOR = 4;
        private const int WT3MONITOR = 5;
        private const int G3MONITOR = 6;
        private const int WT4MONITOR = 7;
        private const int G4MONITOR = 8;
        private const int WT5MONITOR = 9;
        private int[] calStatus = new int[10] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        private double pre_WSS_WT_CO2_AVG = -1;
        private double post_WSS_WT_CO2_AVG = -1;
        private double pre_WSS_WT_CH4_AVG = -1;
        private double post_WSS_WT_CH4_AVG = -1;
        private double pre_WSS_WT_CO_AVG = -1;
        private double post_WSS_WT_CO_AVG = -1;
        private double pre_WSS_WT_N2O_AVG = -1;
        private double post_WSS_WT_N2O_AVG = -1;
        //private int calRetry = 0;
        private bool calRequired = false;
        private int calCount = 0;
        private bool wtRequired = false;
        private bool unkCylsRequired = false;
        private int unkCylCount = 0;


        //cylinder Info
        private System.Windows.Forms.Label[] cylLabels = new System.Windows.Forms.Label[10];
        private TextBox[] cylIDTxtBoxes = new TextBox[10];
        private TextBox[] cylCH4TxtBoxes = new TextBox[10];
        private TextBox[] cylCodeTxtBoxes = new TextBox[10];
        private TextBox[] cylCO2TxtBoxes = new TextBox[10];
        private TextBox[] cylN2OTxtBoxes = new TextBox[10];
        private TextBox[] cylCOTxtBoxes = new TextBox[10];

        //gases
        private Thanos_Gas gasWss1 = new Thanos_Gas();
        private Thanos_Gas gasWss2 = new Thanos_Gas();
        private Thanos_Gas gasWss3 = new Thanos_Gas();
        private Thanos_Gas gasWss4 = new Thanos_Gas();
        private Thanos_Gas gasWt = new Thanos_Gas();
        private Thanos_Gas gasUnk = new Thanos_Gas();
        private Thanos_Gas[] cylinders = new Thanos_Gas[10];
        private string curGasID = "N.O.N.E.";

        private delegate void setTextBoxTextDelegate(TextBox theTextBox, string text);
        private delegate void setLabelTextDelegate(System.Windows.Forms.Label theLabel, string text);
        private delegate void clrClistBoxDelegate(DotNetListBox theClistBox);
        private delegate void addToClistBoxDelegate(DotNetListBox theClistBox, string text, Color txtColour);
        private delegate void setButtonTextDelegate(Button theButton, string text);
        private delegate void setButtonEnableDelegate(Button theButton, bool state);
        private delegate void setPanelVisibleDelegate(Panel thePanel, bool vis);

        //email warnings
        bool lostN2OSignalSent = false;
        bool lostGGASignalSent = false;
        //bool N2OgoneHatstand = false;
        bool ggaLowGasPressureSent = false;
        bool n2oLowGasPressureSent = false;
        bool lostLabJackSent = false;
        bool lostValcoSent = false;
        private double valcoTimeCount;

        //email
        Thread emailThread;
        Int32 emailSendCount = 0;
        DateTime lastEmailSent = DateTime.Now;
        DateTime startOfEmailHour = DateTime.Now;

        //water correction
        private double ggaH2O;
        private double ggaWetCH4;
        private double ggaWetCO2;
        private double ggaDryCH4 = 1900;
        private double ggaDryCO2 = 390;
        private double userDryCH4;
        private double userDryCO2;
        private double n2oH2O;
        private double n2oWetN2O;
        private double n2oWetCO;
        private double n2oDryN2O = 331;
        private double n2oDryCO = 115;
        private double userDryN2O;
        private double userDryCO;

        //Database
        moliarty_data database = new moliarty_data();


        #endregion

        private States progState { get; set; }  //can always expand in the future if we need more functionality






        public Form1()
        {
            InitializeComponent();
        }



        private void accumulate4GasData()
        {
            //cylinders are dry so always use the uncorrected 'wet' data
            double CH4data = ggaWetCH4;
            double CO2data = ggaWetCO2;
            double N2Odata = n2oWetN2O;
            double COdata = n2oWetCO;

            double[] dataArray;
            //are we measuring WSS1, WSS2, WSS3 or WSS4 OR WT?

            //if first occurrance advance to step 1
            if (calStatus[STEP] == 0 &&
                calStatus[WT1MONITOR] == 0 &&
                calStatus[G1MONITOR] == 0 &&
                calStatus[WT2MONITOR] == 0 &&
                calStatus[G2MONITOR] == 0 &&
                calStatus[WT3MONITOR] == 0 &&
                calStatus[G3MONITOR] == 0 &&
                calStatus[WT4MONITOR] == 0) calStatus[STEP] = 1;


            switch (calStatus[STEP])
            {
                case 1:// WT 1st pass
                    if (calStatus[WT1MONITOR] == 0) //first pass so initialise the DataAverages
                    {
                        dataArray = av_CH4.GetAndReset;
                        dataArray = av_CO2.GetAndReset;
                        dataArray = av_N2O.GetAndReset;
                        dataArray = av_CO.GetAndReset;
                    }
                    calStatus[WT1MONITOR]++;
                    //collect WT1 data-- 
                    av_CH4.AddDataPoint(CH4data);
                    av_CO2.AddDataPoint(CO2data);
                    av_N2O.AddDataPoint(N2Odata);
                    av_CO.AddDataPoint(COdata);
                   
                    try
                    {
                        database.insMeasurements(DateTime.Now, "WT", gasWt.ID, gasWt.ID, Convert.ToDouble(textBoxGGACH4.Text), Convert.ToDouble(textBoxGGACO2.Text),
                                                 Convert.ToDouble(textBoxGGAH2O.Text), Convert.ToDouble(textBoxGGACH4dry.Text), Convert.ToDouble(textBoxGGACO2dry.Text),
                                                 Convert.ToDouble(textBoxCorrCH4.Text), Convert.ToDouble(textBoxCorrCO2.Text), pSettings.GetDateTime("LastOKCH4CalDate"),
                                                 pSettings.GetDateTime("LastOKCO2CalDate"),
                                                 Convert.ToDouble(textBoxN2ON2O.Text), Convert.ToDouble(textBoxN2OCO.Text),
                                                 Convert.ToDouble(textBoxN2OH2O.Text), Convert.ToDouble(textBoxN2ON2Odry.Text), Convert.ToDouble(textBoxN2OCOdry.Text),
                                                 Convert.ToDouble(textBoxCorrN2O.Text), Convert.ToDouble(textBoxCorrCO.Text), pSettings.GetDateTime("LastOKN2OCalDate"),
                                                 pSettings.GetDateTime("LastOKCOCalDate"));
                    }
                    catch (Exception ex)
                    {
                        string e = ex.Message;
                    }
                    break;
                case 2:// WSS1
                    //first off we need to deal with the WT data
                    if (calStatus[WT1MONITOR] != 0) //we may not be measuring WT now, but we have been
                    {
                        //so store the data
                        dataArray = av_CH4.GetAndReset;
                        pre_WSS_WT_CH4_AVG = dataArray[MEAN];
                        gasWt.AvCH4Sd = dataArray[SD];
                        gasWt.AvCH4n = Convert.ToInt32(dataArray[COUNT]);
                        dataArray = av_CO2.GetAndReset;
                        pre_WSS_WT_CO2_AVG = dataArray[MEAN];
                        gasWt.AvCO2Sd = dataArray[SD];
                        gasWt.AvCO2n = Convert.ToInt32(dataArray[COUNT]);
                        dataArray = av_N2O.GetAndReset;
                        pre_WSS_WT_N2O_AVG = dataArray[MEAN];
                        gasWt.AvN2OSd = dataArray[SD];
                        gasWt.AvN2On = Convert.ToInt32(dataArray[COUNT]);
                        dataArray = av_CO.GetAndReset;
                        pre_WSS_WT_CO_AVG = dataArray[MEAN];
                        gasWt.AvCOSd = dataArray[SD];
                        gasWt.AvCOn = Convert.ToInt32(dataArray[COUNT]);
                        calStatus[WT1MONITOR] = 0;
                        //update WT data
                        gasWt.AvCH4Meas = pre_WSS_WT_CH4_AVG;
                        gasWt.AvCO2Meas = pre_WSS_WT_CO2_AVG;
                        gasWt.AvN2OMeas = pre_WSS_WT_N2O_AVG;
                        gasWt.AvCOMeas = pre_WSS_WT_CO_AVG;
                        wtCH4 = gasWt.AvCH4Meas;
                        wtCO2 = gasWt.AvCO2Meas;
                        wtN2O = gasWt.AvN2OMeas;
                        wtCO = gasWt.AvCOMeas;

                        //prevent a crash - we will still have RAW data
                        if (wtCH4 == 0 || double.IsNaN(wtCH4)) wtCH4 = -1;
                        if (wtCO2 == 0 || double.IsNaN(wtCO2)) wtCO2 = -1;
                        if (wtN2O == 0 || double.IsNaN(wtN2O)) wtN2O = -1;
                        if (wtCO == 0 || double.IsNaN(wtCO)) wtCO = -1;

                        labelWTrefCH4.Text = MyString.setDP(wtCH4.ToString(), 4);
                        labelWTrefCO2.Text = MyString.setDP(wtCO2.ToString(), 4);
                        labelWTrefN2O.Text = MyString.setDP(wtN2O.ToString(), 4);
                        labelWTrefCO.Text = MyString.setDP(wtCO.ToString(), 4);

                        pSettings.AddSetting("WtCH4", wtCH4);
                        pSettings.AddSetting("WtCO2", wtCO2);
                        pSettings.AddSetting("WtN2O", wtN2O);
                        pSettings.AddSetting("WtCO", wtCO);
                        pSettings.Save();
                       
                        database.insWTCalibration(DateTime.Now, gasWt.ID,
                                                  gasWt.CH4Conc, gasWt.CO2Conc, gasWt.AvCH4Meas, gasWt.AvCO2Meas,
                                                  gasWt.N2OConc, gasWt.COConc, gasWt.AvN2OMeas, gasWt.AvCOMeas);
                    }
                    //now deal with WSS1 data
                    if (calStatus[G1MONITOR] == 0) //first pass so initialise the DataAverages
                    {
                        dataArray = av_CH4.GetAndReset;
                        dataArray = av_CO2.GetAndReset;
                        dataArray = av_N2O.GetAndReset;
                        dataArray = av_CO.GetAndReset;
                        //and make this gas wss1
                        gasWss1 = cylinders[v1CurrentPos - 1];
                    }
                    calStatus[G1MONITOR]++;
                    //collect WSS1 data-- 
                    av_CH4.AddDataPoint(CH4data);
                    av_CO2.AddDataPoint(CO2data);
                    av_N2O.AddDataPoint(N2Odata);
                    av_CO.AddDataPoint(COdata);
                    //TODO bung it in a log file

              
                    try
                    {
                        database.insMeasurements(DateTime.Now, "W1", gasWss1.ID, gasWt.ID, Convert.ToDouble(textBoxGGACH4.Text), Convert.ToDouble(textBoxGGACO2.Text),
                                                 Convert.ToDouble(textBoxGGAH2O.Text), Convert.ToDouble(textBoxGGACH4dry.Text), Convert.ToDouble(textBoxGGACO2dry.Text),
                                                 Convert.ToDouble(textBoxCorrCH4.Text), Convert.ToDouble(textBoxCorrCO2.Text), pSettings.GetDateTime("LastOKCH4CalDate"),
                                                 pSettings.GetDateTime("LastOKCO2CalDate"),
                                                 Convert.ToDouble(textBoxN2ON2O.Text), Convert.ToDouble(textBoxN2OCO.Text),
                                                 Convert.ToDouble(textBoxN2OH2O.Text), Convert.ToDouble(textBoxN2ON2Odry.Text), Convert.ToDouble(textBoxN2OCOdry.Text),
                                                 Convert.ToDouble(textBoxCorrN2O.Text), Convert.ToDouble(textBoxCorrCO.Text), pSettings.GetDateTime("LastOKN2OCalDate"),
                                                 pSettings.GetDateTime("LastOKCOCalDate"));
                    }
                    catch (Exception ex)
                    {
                        string e = ex.Message;
                    }
                    break;
                case 3:// WT 2nd pass
                    //first off we need to deal with the WSS1 data
                    if (calStatus[G1MONITOR] != 0) //we may not be measuring WSS1 now, but we have been
                    {
                        //so store the data
                        dataArray = av_CH4.GetAndReset;
                        gasWss1.AvCH4Meas = dataArray[MEAN];
                        gasWss1.AvCH4n = Convert.ToInt32(dataArray[COUNT]);
                        dataArray = av_CO2.GetAndReset;
                        gasWss1.AvCO2Meas = dataArray[MEAN];
                        gasWss1.AvCO2n = Convert.ToInt32(dataArray[COUNT]);
                        dataArray = av_N2O.GetAndReset;
                        gasWss1.AvN2OMeas = dataArray[MEAN];
                        gasWss1.AvN2On = Convert.ToInt32(dataArray[COUNT]);
                        dataArray = av_CO.GetAndReset;
                        gasWss1.AvCOMeas = dataArray[MEAN];
                        gasWss1.AvCOn = Convert.ToInt32(dataArray[COUNT]);
                        calStatus[G1MONITOR] = 0;
                    }
                    //now deal with WT data
                    if (calStatus[WT2MONITOR] == 0) //first pass so initialise the DataAverages
                    {
                        dataArray = av_CH4.GetAndReset;
                        dataArray = av_CO2.GetAndReset;
                        dataArray = av_N2O.GetAndReset;
                        dataArray = av_CO.GetAndReset;
                    }
                    calStatus[WT2MONITOR]++;
                    //collect WT2 data--  
                    av_CH4.AddDataPoint(CH4data);
                    av_CO2.AddDataPoint(CO2data);
                    av_N2O.AddDataPoint(N2Odata);
                    av_CO.AddDataPoint(COdata);
                    //TODO bung it in a log file

                    try
                    {
                        database.insMeasurements(DateTime.Now, "WT", gasWt.ID, gasWt.ID, Convert.ToDouble(textBoxGGACH4.Text), Convert.ToDouble(textBoxGGACO2.Text),
                                                 Convert.ToDouble(textBoxGGAH2O.Text), Convert.ToDouble(textBoxGGACH4dry.Text), Convert.ToDouble(textBoxGGACO2dry.Text),
                                                 Convert.ToDouble(textBoxCorrCH4.Text), Convert.ToDouble(textBoxCorrCO2.Text), pSettings.GetDateTime("LastOKCH4CalDate"),
                                                 pSettings.GetDateTime("LastOKCO2CalDate"),
                                                 Convert.ToDouble(textBoxN2ON2O.Text), Convert.ToDouble(textBoxN2OCO.Text),
                                                 Convert.ToDouble(textBoxN2OH2O.Text), Convert.ToDouble(textBoxN2ON2Odry.Text), Convert.ToDouble(textBoxN2OCOdry.Text),
                                                 Convert.ToDouble(textBoxCorrN2O.Text), Convert.ToDouble(textBoxCorrCO.Text), pSettings.GetDateTime("LastOKN2OCalDate"),
                                                 pSettings.GetDateTime("LastOKCOCalDate"));
                    }
                    catch (Exception ex)
                    {
                        string e = ex.Message;
                    }
                    break;
                case 4:// WSS2
                    //first off we need to deal with the WT data
                    if (calStatus[WT2MONITOR] != 0) //we may not be measuring WT now, but we have been
                    {
                        //so store the data
                        dataArray = av_CH4.GetAndReset;
                        post_WSS_WT_CH4_AVG = dataArray[MEAN];
                        gasWt.AvCH4Sd = dataArray[SD];
                        gasWt.AvCH4n = Convert.ToInt32(dataArray[COUNT]);
                        dataArray = av_CO2.GetAndReset;
                        post_WSS_WT_CO2_AVG = dataArray[MEAN];
                        gasWt.AvCO2Sd = dataArray[SD];
                        gasWt.AvCO2n = Convert.ToInt32(dataArray[COUNT]);
                        dataArray = av_N2O.GetAndReset;
                        post_WSS_WT_N2O_AVG = dataArray[MEAN];
                        gasWt.AvN2OSd = dataArray[SD];
                        gasWt.AvN2On = Convert.ToInt32(dataArray[COUNT]);
                        dataArray = av_CO.GetAndReset;
                        post_WSS_WT_CO_AVG = dataArray[MEAN];
                        gasWt.AvCOSd = dataArray[SD];
                        gasWt.AvCOn = Convert.ToInt32(dataArray[COUNT]);
                        calStatus[WT2MONITOR] = 0;
                        //make WSS! result into a ratio
                        //gasWss1.AvCH4Meas = gasWss1.AvCH4Meas / ((pre_WSS_WT_CH4_AVG + post_WSS_WT_CH4_AVG) / 2);
                        //gasWss1.AvCO2Meas = gasWss1.AvCO2Meas / ((pre_WSS_WT_CO2_AVG + post_WSS_WT_CO2_AVG) / 2);
                        gasWss1.AvCH4WT = (pre_WSS_WT_CH4_AVG + post_WSS_WT_CH4_AVG) / 2;
                        gasWss1.AvCO2WT = (pre_WSS_WT_CO2_AVG + post_WSS_WT_CO2_AVG) / 2;
                        gasWss1.AvN2OWT = (pre_WSS_WT_N2O_AVG + post_WSS_WT_N2O_AVG) / 2;
                        gasWss1.AvCOWT = (pre_WSS_WT_CO_AVG + post_WSS_WT_CO_AVG) / 2;
                        //post measurement will be pre wrt WSS2
                        pre_WSS_WT_CH4_AVG = post_WSS_WT_CH4_AVG;
                        pre_WSS_WT_CO2_AVG = post_WSS_WT_CO2_AVG;
                        pre_WSS_WT_N2O_AVG = post_WSS_WT_N2O_AVG;
                        pre_WSS_WT_CO_AVG = post_WSS_WT_CO_AVG;
                        //update WT data
                        gasWt.AvCH4Meas = pre_WSS_WT_CH4_AVG;
                        gasWt.AvCO2Meas = pre_WSS_WT_CO2_AVG;
                        gasWt.AvN2OMeas = pre_WSS_WT_N2O_AVG;
                        gasWt.AvCOMeas = pre_WSS_WT_CO_AVG;
                        wtCH4 = gasWt.AvCH4Meas;
                        wtCO2 = gasWt.AvCO2Meas;
                        wtN2O = gasWt.AvN2OMeas;
                        wtCO = gasWt.AvCOMeas;

                        //prevent a crash - we will still have RAW data
                        if (wtCH4 == 0 || double.IsNaN(wtCH4)) wtCH4 = -1;
                        if (wtCO2 == 0 || double.IsNaN(wtCO2)) wtCO2 = -1;
                        if (wtN2O == 0 || double.IsNaN(wtN2O)) wtN2O = -1;
                        if (wtCO == 0 || double.IsNaN(wtCO)) wtCO = -1;

                        labelWTrefCH4.Text = MyString.setDP(wtCH4.ToString(), 4);
                        labelWTrefCO2.Text = MyString.setDP(wtCO2.ToString(), 4);
                        labelWTrefN2O.Text = MyString.setDP(wtN2O.ToString(), 4);
                        labelWTrefCO.Text = MyString.setDP(wtCO.ToString(), 4);

                        pSettings.AddSetting("WtCH4", wtCH4);
                        pSettings.AddSetting("WtCO2", wtCO2);
                        pSettings.AddSetting("WtN2O", wtN2O);
                        pSettings.AddSetting("WtCO", wtCO);
                        pSettings.Save();
                    
                        database.insWTCalibration(DateTime.Now, gasWt.ID,
                                                  gasWt.CH4Conc, gasWt.CO2Conc, gasWt.AvCH4Meas, gasWt.AvCO2Meas,
                                                  gasWt.N2OConc, gasWt.COConc, gasWt.AvN2OMeas, gasWt.AvCOMeas);
                    }
                    //now deal with WSS2 data
                    if (calStatus[G2MONITOR] == 0) //first pass so initialise the DataAverages
                    {
                        dataArray = av_CH4.GetAndReset;
                        dataArray = av_CO2.GetAndReset;
                        dataArray = av_N2O.GetAndReset;
                        dataArray = av_CO.GetAndReset;
                        //and make this gas wss2
                        gasWss2 = cylinders[v1CurrentPos - 1];
                    }
                    calStatus[G2MONITOR]++;
                    //collect Wss2 data--  
                    av_CH4.AddDataPoint(CH4data);
                    av_CO2.AddDataPoint(CO2data);
                    av_N2O.AddDataPoint(N2Odata);
                    av_CO.AddDataPoint(COdata);
           
                    try
                    {
                        database.insMeasurements(DateTime.Now, "W2", gasWss2.ID, gasWt.ID, Convert.ToDouble(textBoxGGACH4.Text), Convert.ToDouble(textBoxGGACO2.Text),
                                                 Convert.ToDouble(textBoxGGAH2O.Text), Convert.ToDouble(textBoxGGACH4dry.Text), Convert.ToDouble(textBoxGGACO2dry.Text),
                                                 Convert.ToDouble(textBoxCorrCH4.Text), Convert.ToDouble(textBoxCorrCO2.Text), pSettings.GetDateTime("LastOKCH4CalDate"),
                                                 pSettings.GetDateTime("LastOKCO2CalDate"),
                                                 Convert.ToDouble(textBoxN2ON2O.Text), Convert.ToDouble(textBoxN2OCO.Text),
                                                 Convert.ToDouble(textBoxN2OH2O.Text), Convert.ToDouble(textBoxN2ON2Odry.Text), Convert.ToDouble(textBoxN2OCOdry.Text),
                                                 Convert.ToDouble(textBoxCorrN2O.Text), Convert.ToDouble(textBoxCorrCO.Text), pSettings.GetDateTime("LastOKN2OCalDate"),
                                                 pSettings.GetDateTime("LastOKCOCalDate"));
                    }
                    catch (Exception ex)
                    {
                        string e = ex.Message;
                    }
                    break;
                case 5:// WT 3rd pass
                    //first off we need to deal with the Wss2 data
                    if (calStatus[G2MONITOR] != 0) //we may not be measuring Wss2 now, but we have been
                    {
                        //so store the data
                        dataArray = av_CH4.GetAndReset;
                        gasWss2.AvCH4Meas = dataArray[0];
                        gasWss2.AvCH4n = Convert.ToInt32(dataArray[COUNT]);
                        dataArray = av_CO2.GetAndReset;
                        gasWss2.AvCO2Meas = dataArray[0];
                        gasWss2.AvCO2n = Convert.ToInt32(dataArray[COUNT]);
                        dataArray = av_N2O.GetAndReset;
                        gasWss2.AvN2OMeas = dataArray[0];
                        gasWss2.AvN2On = Convert.ToInt32(dataArray[COUNT]);
                        dataArray = av_CO.GetAndReset;
                        gasWss2.AvCOMeas = dataArray[0];
                        gasWss2.AvCOn = Convert.ToInt32(dataArray[COUNT]);
                        calStatus[G2MONITOR] = 0;
                    }
                    //now deal with WT data
                    if (calStatus[WT3MONITOR] == 0) //first pass so initialise the DataAverages
                    {
                        dataArray = av_CH4.GetAndReset;
                        dataArray = av_CO2.GetAndReset;
                        dataArray = av_N2O.GetAndReset;
                        dataArray = av_CO.GetAndReset;
                    }
                    calStatus[WT3MONITOR]++;
                    //collect WT3 data--  
                    av_CH4.AddDataPoint(CH4data);
                    av_CO2.AddDataPoint(CO2data);
                    av_N2O.AddDataPoint(N2Odata);
                    av_CO.AddDataPoint(COdata);
                
                    try
                    {
                        database.insMeasurements(DateTime.Now, "WT", gasWt.ID, gasWt.ID, Convert.ToDouble(textBoxGGACH4.Text), Convert.ToDouble(textBoxGGACO2.Text),
                                                 Convert.ToDouble(textBoxGGAH2O.Text), Convert.ToDouble(textBoxGGACH4dry.Text), Convert.ToDouble(textBoxGGACO2dry.Text),
                                                 Convert.ToDouble(textBoxCorrCH4.Text), Convert.ToDouble(textBoxCorrCO2.Text), pSettings.GetDateTime("LastOKCH4CalDate"),
                                                 pSettings.GetDateTime("LastOKCO2CalDate"),
                                                 Convert.ToDouble(textBoxN2ON2O.Text), Convert.ToDouble(textBoxN2OCO.Text),
                                                 Convert.ToDouble(textBoxN2OH2O.Text), Convert.ToDouble(textBoxN2ON2Odry.Text), Convert.ToDouble(textBoxN2OCOdry.Text),
                                                 Convert.ToDouble(textBoxCorrN2O.Text), Convert.ToDouble(textBoxCorrCO.Text), pSettings.GetDateTime("LastOKN2OCalDate"),
                                                 pSettings.GetDateTime("LastOKCOCalDate"));
                    }
                    catch (Exception ex)
                    {
                        string e = ex.Message;
                    }
                    break;





                case 6:// WSS3
                    //first off we need to deal with the WT data
                    if (calStatus[WT3MONITOR] != 0) //we may not be measuring WT now, but we have been
                    {
                        //so store the data
                        dataArray = av_CH4.GetAndReset;
                        post_WSS_WT_CH4_AVG = dataArray[MEAN];
                        gasWt.AvCH4Sd = dataArray[SD];
                        gasWt.AvCH4n = Convert.ToInt32(dataArray[COUNT]);
                        dataArray = av_CO2.GetAndReset;
                        post_WSS_WT_CO2_AVG = dataArray[MEAN];
                        gasWt.AvCO2Sd = dataArray[SD];
                        gasWt.AvCO2n = Convert.ToInt32(dataArray[COUNT]);
                        dataArray = av_N2O.GetAndReset;
                        post_WSS_WT_N2O_AVG = dataArray[MEAN];
                        gasWt.AvN2OSd = dataArray[SD];
                        gasWt.AvN2On = Convert.ToInt32(dataArray[COUNT]);
                        dataArray = av_CO.GetAndReset;
                        post_WSS_WT_CO_AVG = dataArray[MEAN];
                        gasWt.AvCOSd = dataArray[SD];
                        gasWt.AvCOn = Convert.ToInt32(dataArray[COUNT]);
                        calStatus[WT3MONITOR] = 0;
                        //make WSS! result into a ratio
                        //gasWss1.AvCH4Meas = gasWss1.AvCH4Meas / ((pre_WSS_WT_CH4_AVG + post_WSS_WT_CH4_AVG) / 2);
                        //gasWss1.AvCO2Meas = gasWss1.AvCO2Meas / ((pre_WSS_WT_CO2_AVG + post_WSS_WT_CO2_AVG) / 2);
                        gasWss2.AvCH4WT = (pre_WSS_WT_CH4_AVG + post_WSS_WT_CH4_AVG) / 2;
                        gasWss2.AvCO2WT = (pre_WSS_WT_CO2_AVG + post_WSS_WT_CO2_AVG) / 2;
                        gasWss2.AvN2OWT = (pre_WSS_WT_N2O_AVG + post_WSS_WT_N2O_AVG) / 2;
                        gasWss2.AvCOWT = (pre_WSS_WT_CO_AVG + post_WSS_WT_CO_AVG) / 2;
                        //post measurement will be pre wrt WSS3
                        pre_WSS_WT_CH4_AVG = post_WSS_WT_CH4_AVG;
                        pre_WSS_WT_CO2_AVG = post_WSS_WT_CO2_AVG;
                        pre_WSS_WT_N2O_AVG = post_WSS_WT_N2O_AVG;
                        pre_WSS_WT_CO_AVG = post_WSS_WT_CO_AVG;
                        //update WT data
                        gasWt.AvCH4Meas = pre_WSS_WT_CH4_AVG;
                        gasWt.AvCO2Meas = pre_WSS_WT_CO2_AVG;
                        gasWt.AvN2OMeas = pre_WSS_WT_N2O_AVG;
                        gasWt.AvCOMeas = pre_WSS_WT_CO_AVG;
                        wtCH4 = gasWt.AvCH4Meas;
                        wtCO2 = gasWt.AvCO2Meas;
                        wtN2O = gasWt.AvN2OMeas;
                        wtCO = gasWt.AvCOMeas;

                        //prevent a crash - we will still have RAW data
                        if (wtCH4 == 0 || double.IsNaN(wtCH4)) wtCH4 = -1;
                        if (wtCO2 == 0 || double.IsNaN(wtCO2)) wtCO2 = -1;
                        if (wtN2O == 0 || double.IsNaN(wtN2O)) wtN2O = -1;
                        if (wtCO == 0 || double.IsNaN(wtCO)) wtCO = -1;

                        labelWTrefCH4.Text = MyString.setDP(wtCH4.ToString(), 4);
                        labelWTrefCO2.Text = MyString.setDP(wtCO2.ToString(), 4);
                        labelWTrefN2O.Text = MyString.setDP(wtN2O.ToString(), 4);
                        labelWTrefCO.Text = MyString.setDP(wtCO.ToString(), 4);

                        pSettings.AddSetting("WtCH4", wtCH4);
                        pSettings.AddSetting("WtCO2", wtCO2);
                        pSettings.AddSetting("WtN2O", wtN2O);
                        pSettings.AddSetting("WtCO", wtCO);
                        pSettings.Save();
                 
                        database.insWTCalibration(DateTime.Now, gasWt.ID,
                                                  gasWt.CH4Conc, gasWt.CO2Conc, gasWt.AvCH4Meas, gasWt.AvCO2Meas,
                                                  gasWt.N2OConc, gasWt.COConc, gasWt.AvN2OMeas, gasWt.AvCOMeas);
                    }
                    //now deal with WSS3 data
                    if (calStatus[G3MONITOR] == 0) //first pass so initialise the DataAverages
                    {
                        dataArray = av_CH4.GetAndReset;
                        dataArray = av_CO2.GetAndReset;
                        dataArray = av_N2O.GetAndReset;
                        dataArray = av_CO.GetAndReset;
                        //and make this gas wss3
                        gasWss3 = cylinders[v1CurrentPos - 1];
                    }
                    calStatus[G3MONITOR]++;
                    //collect Wss3 data--  
                    av_CH4.AddDataPoint(CH4data);
                    av_CO2.AddDataPoint(CO2data);
                    av_N2O.AddDataPoint(N2Odata);
                    av_CO.AddDataPoint(COdata);
                  
                    try
                    {
                        database.insMeasurements(DateTime.Now, "W3", gasWss3.ID, gasWt.ID, Convert.ToDouble(textBoxGGACH4.Text), Convert.ToDouble(textBoxGGACO2.Text),
                                                 Convert.ToDouble(textBoxGGAH2O.Text), Convert.ToDouble(textBoxGGACH4dry.Text), Convert.ToDouble(textBoxGGACO2dry.Text),
                                                 Convert.ToDouble(textBoxCorrCH4.Text), Convert.ToDouble(textBoxCorrCO2.Text), pSettings.GetDateTime("LastOKCH4CalDate"),
                                                 pSettings.GetDateTime("LastOKCO2CalDate"),
                                                 Convert.ToDouble(textBoxN2ON2O.Text), Convert.ToDouble(textBoxN2OCO.Text),
                                                 Convert.ToDouble(textBoxN2OH2O.Text), Convert.ToDouble(textBoxN2ON2Odry.Text), Convert.ToDouble(textBoxN2OCOdry.Text),
                                                 Convert.ToDouble(textBoxCorrN2O.Text), Convert.ToDouble(textBoxCorrCO.Text), pSettings.GetDateTime("LastOKN2OCalDate"),
                                                 pSettings.GetDateTime("LastOKCOCalDate"));
                    }
                    catch (Exception ex)
                    {
                        string e = ex.Message;
                    }
                    break;
                case 7:// WT 4th pass
                    //first off we need to deal with the Wss3 data
                    if (calStatus[G3MONITOR] != 0) //we may not be measuring Wss2 now, but we have been
                    {
                        //so store the data
                        dataArray = av_CH4.GetAndReset;
                        gasWss3.AvCH4Meas = dataArray[0];
                        gasWss3.AvCH4n = Convert.ToInt32(dataArray[COUNT]);
                        dataArray = av_CO2.GetAndReset;
                        gasWss3.AvCO2Meas = dataArray[0];
                        gasWss3.AvCO2n = Convert.ToInt32(dataArray[COUNT]);
                        dataArray = av_N2O.GetAndReset;
                        gasWss3.AvN2OMeas = dataArray[0];
                        gasWss3.AvN2On = Convert.ToInt32(dataArray[COUNT]);
                        dataArray = av_CO.GetAndReset;
                        gasWss3.AvCOMeas = dataArray[0];
                        gasWss3.AvCOn = Convert.ToInt32(dataArray[COUNT]);
                        calStatus[G3MONITOR] = 0;
                    }
                    //now deal with WT data
                    if (calStatus[WT4MONITOR] == 0) //first pass so initialise the DataAverages
                    {
                        dataArray = av_CH4.GetAndReset;
                        dataArray = av_CO2.GetAndReset;
                        dataArray = av_N2O.GetAndReset;
                        dataArray = av_CO.GetAndReset;
                    }
                    calStatus[WT4MONITOR]++;
                    //collect WT4 data--  
                    av_CH4.AddDataPoint(CH4data);
                    av_CO2.AddDataPoint(CO2data);
                    av_N2O.AddDataPoint(N2Odata);
                    av_CO.AddDataPoint(COdata);
                  
                    try
                    {
                        database.insMeasurements(DateTime.Now, "WT", gasWt.ID, gasWt.ID, Convert.ToDouble(textBoxGGACH4.Text), Convert.ToDouble(textBoxGGACO2.Text),
                                                 Convert.ToDouble(textBoxGGAH2O.Text), Convert.ToDouble(textBoxGGACH4dry.Text), Convert.ToDouble(textBoxGGACO2dry.Text),
                                                 Convert.ToDouble(textBoxCorrCH4.Text), Convert.ToDouble(textBoxCorrCO2.Text), pSettings.GetDateTime("LastOKCH4CalDate"),
                                                 pSettings.GetDateTime("LastOKCO2CalDate"),
                                                 Convert.ToDouble(textBoxN2ON2O.Text), Convert.ToDouble(textBoxN2OCO.Text),
                                                 Convert.ToDouble(textBoxN2OH2O.Text), Convert.ToDouble(textBoxN2ON2Odry.Text), Convert.ToDouble(textBoxN2OCOdry.Text),
                                                 Convert.ToDouble(textBoxCorrN2O.Text), Convert.ToDouble(textBoxCorrCO.Text), pSettings.GetDateTime("LastOKN2OCalDate"),
                                                 pSettings.GetDateTime("LastOKCOCalDate"));
                    }
                    catch (Exception ex)
                    {
                        string e = ex.Message;
                    }
                    break;




                case 8:// WSS4    
                    //first off we need to deal with the WT data
                    if (calStatus[WT4MONITOR] != 0) //we may not be measuring WT now, but we have been
                    {
                        //so store the data
                        dataArray = av_CH4.GetAndReset;
                        post_WSS_WT_CH4_AVG = dataArray[MEAN];
                        gasWt.AvCH4Sd = dataArray[SD];
                        gasWt.AvCH4n = Convert.ToInt32(dataArray[COUNT]);
                        dataArray = av_CO2.GetAndReset;
                        post_WSS_WT_CO2_AVG = dataArray[MEAN];
                        gasWt.AvCO2Sd = dataArray[SD];
                        gasWt.AvCO2n = Convert.ToInt32(dataArray[COUNT]);
                        dataArray = av_N2O.GetAndReset;
                        post_WSS_WT_N2O_AVG = dataArray[MEAN];
                        gasWt.AvN2OSd = dataArray[SD];
                        gasWt.AvN2On = Convert.ToInt32(dataArray[COUNT]);
                        dataArray = av_CO.GetAndReset;
                        post_WSS_WT_CO_AVG = dataArray[MEAN];
                        gasWt.AvCOSd = dataArray[SD];
                        gasWt.AvCOn = Convert.ToInt32(dataArray[COUNT]);
                        calStatus[WT4MONITOR] = 0;
                        //make WSS3 result into a ratio
                        //gasWss2.AvCH4Meas = gasWss2.AvCH4Meas / ((pre_WSS_WT_CH4_AVG + post_WSS_WT_CH4_AVG) / 2);
                        //gasWss2.AvCO2Meas = gasWss2.AvCO2Meas / ((pre_WSS_WT_CO2_AVG + post_WSS_WT_CO2_AVG) / 2);
                        gasWss3.AvCH4WT = (pre_WSS_WT_CH4_AVG + post_WSS_WT_CH4_AVG) / 2;
                        gasWss3.AvCO2WT = (pre_WSS_WT_CO2_AVG + post_WSS_WT_CO2_AVG) / 2;
                        gasWss3.AvN2OWT = (pre_WSS_WT_N2O_AVG + post_WSS_WT_N2O_AVG) / 2;
                        gasWss3.AvCOWT = (pre_WSS_WT_CO_AVG + post_WSS_WT_CO_AVG) / 2;
                        //post measurement will be pre wrt WSS4
                        pre_WSS_WT_CH4_AVG = post_WSS_WT_CH4_AVG;
                        pre_WSS_WT_CO2_AVG = post_WSS_WT_CO2_AVG;
                        pre_WSS_WT_N2O_AVG = post_WSS_WT_N2O_AVG;
                        pre_WSS_WT_CO_AVG = post_WSS_WT_CO_AVG;
                        //update WT data
                        gasWt.AvCH4Meas = pre_WSS_WT_CH4_AVG;
                        gasWt.AvCO2Meas = pre_WSS_WT_CO2_AVG;
                        gasWt.AvN2OMeas = pre_WSS_WT_N2O_AVG;
                        gasWt.AvCOMeas = pre_WSS_WT_CO_AVG;
                        wtCH4 = gasWt.AvCH4Meas;
                        wtCO2 = gasWt.AvCO2Meas;
                        wtN2O = gasWt.AvN2OMeas;
                        wtCO = gasWt.AvCOMeas;

                        //prevent a crash - we will still have RAW data
                        if (wtCH4 == 0 || double.IsNaN(wtCH4)) wtCH4 = -1;
                        if (wtCO2 == 0 || double.IsNaN(wtCO2)) wtCO2 = -1;
                        if (wtN2O == 0 || double.IsNaN(wtN2O)) wtN2O = -1;
                        if (wtCO == 0 || double.IsNaN(wtCO)) wtCO = -1;

                        labelWTrefCH4.Text = MyString.setDP(wtCH4.ToString(), 4);
                        labelWTrefCO2.Text = MyString.setDP(wtCO2.ToString(), 4);
                        labelWTrefN2O.Text = MyString.setDP(wtN2O.ToString(), 4);
                        labelWTrefCO.Text = MyString.setDP(wtCO.ToString(), 4);

                        pSettings.AddSetting("WtCH4", wtCH4);
                        pSettings.AddSetting("WtCO2", wtCO2);
                        pSettings.AddSetting("WtN2O", wtN2O);
                        pSettings.AddSetting("WtCO", wtCO);
                        pSettings.Save();
                     
                        database.insWTCalibration(DateTime.Now, gasWt.ID,
                                                  gasWt.CH4Conc, gasWt.CO2Conc, gasWt.AvCH4Meas, gasWt.AvCO2Meas,
                                                  gasWt.N2OConc, gasWt.COConc, gasWt.AvN2OMeas, gasWt.AvCOMeas);
                    }
                    //now deal with Wss4 data
                    if (calStatus[G4MONITOR] == 0) //first pass so initialise the DataAverages
                    {
                        dataArray = av_CH4.GetAndReset;
                        dataArray = av_CO2.GetAndReset;
                        dataArray = av_N2O.GetAndReset;
                        dataArray = av_CO.GetAndReset;
                        //and make this gas wss3
                        gasWss4 = cylinders[v1CurrentPos - 1];
                    }
                    calStatus[G4MONITOR]++;
                    //collect Wss4 data--               
                    av_CH4.AddDataPoint(CH4data);
                    av_CO2.AddDataPoint(CO2data);
                    av_N2O.AddDataPoint(N2Odata);
                    av_CO.AddDataPoint(COdata);
                
                    try
                    {
                        database.insMeasurements(DateTime.Now, "W4", gasWss4.ID, gasWt.ID, Convert.ToDouble(textBoxGGACH4.Text), Convert.ToDouble(textBoxGGACO2.Text),
                                                 Convert.ToDouble(textBoxGGAH2O.Text), Convert.ToDouble(textBoxGGACH4dry.Text), Convert.ToDouble(textBoxGGACO2dry.Text),
                                                 Convert.ToDouble(textBoxCorrCH4.Text), Convert.ToDouble(textBoxCorrCO2.Text), pSettings.GetDateTime("LastOKCH4CalDate"),
                                                 pSettings.GetDateTime("LastOKCO2CalDate"),
                                                 Convert.ToDouble(textBoxN2ON2O.Text), Convert.ToDouble(textBoxN2OCO.Text),
                                                 Convert.ToDouble(textBoxN2OH2O.Text), Convert.ToDouble(textBoxN2ON2Odry.Text), Convert.ToDouble(textBoxN2OCOdry.Text),
                                                 Convert.ToDouble(textBoxCorrN2O.Text), Convert.ToDouble(textBoxCorrCO.Text), pSettings.GetDateTime("LastOKN2OCalDate"),
                                                 pSettings.GetDateTime("LastOKCOCalDate"));
                    }
                    catch (Exception ex)
                    {
                        string e = ex.Message;
                    }
                    break;
                case 9:// WT 5th pass
                    //first off we need to deal with the Wss3 data
                    if (calStatus[G4MONITOR] != 0) //we may not be measuring Wss3 now, but we have been
                    {
                        //so store the data
                        dataArray = av_CH4.GetAndReset;
                        gasWss4.AvCH4Meas = dataArray[0];
                        gasWss4.AvCH4n = Convert.ToInt32(dataArray[COUNT]);
                        dataArray = av_CO2.GetAndReset;
                        gasWss4.AvCO2Meas = dataArray[0];
                        gasWss4.AvCO2n = Convert.ToInt32(dataArray[COUNT]);
                        dataArray = av_N2O.GetAndReset;
                        gasWss4.AvN2OMeas = dataArray[0];
                        gasWss4.AvN2On = Convert.ToInt32(dataArray[COUNT]);
                        dataArray = av_CO.GetAndReset;
                        gasWss4.AvCOMeas = dataArray[0];
                        gasWss4.AvCOn = Convert.ToInt32(dataArray[COUNT]);
                        calStatus[G4MONITOR] = 0;
                    }
                    //now deal with WT data
                    if (calStatus[WT5MONITOR] == 0) //first pass so initialise the DataAverages
                    {
                        dataArray = av_CH4.GetAndReset;
                        dataArray = av_CO2.GetAndReset;
                        dataArray = av_N2O.GetAndReset;
                        dataArray = av_CO.GetAndReset;
                    }
                    calStatus[WT5MONITOR]++;
                    //collect WT5 data--  
                    av_CH4.AddDataPoint(CH4data);
                    av_CO2.AddDataPoint(CO2data);
                    av_N2O.AddDataPoint(N2Odata);
                    av_CO.AddDataPoint(COdata);
                   
                    try
                    {
                        database.insMeasurements(DateTime.Now, "WT", gasWt.ID, gasWt.ID, Convert.ToDouble(textBoxGGACH4.Text), Convert.ToDouble(textBoxGGACO2.Text),
                                                 Convert.ToDouble(textBoxGGAH2O.Text), Convert.ToDouble(textBoxGGACH4dry.Text), Convert.ToDouble(textBoxGGACO2dry.Text),
                                                 Convert.ToDouble(textBoxCorrCH4.Text), Convert.ToDouble(textBoxCorrCO2.Text), pSettings.GetDateTime("LastOKCH4CalDate"),
                                                 pSettings.GetDateTime("LastOKCO2CalDate"),
                                                 Convert.ToDouble(textBoxN2ON2O.Text), Convert.ToDouble(textBoxN2OCO.Text),
                                                 Convert.ToDouble(textBoxN2OH2O.Text), Convert.ToDouble(textBoxN2ON2Odry.Text), Convert.ToDouble(textBoxN2OCOdry.Text),
                                                 Convert.ToDouble(textBoxCorrN2O.Text), Convert.ToDouble(textBoxCorrCO.Text), pSettings.GetDateTime("LastOKN2OCalDate"),
                                                 pSettings.GetDateTime("LastOKCOCalDate"));
                    }
                    catch (Exception ex)
                    {
                        string e = ex.Message;
                    }
                    break;
                default:
                    break;
            }
        }

        private void accumulateWTData()
        {
            //cylinders are dry so always use the uncorrected 'wet' data
            double CH4data = ggaWetCH4;
            double CO2data = ggaWetCO2;
            double N2Odata = n2oWetN2O;
            double COdata = n2oWetCO;
            double[] dataArray;

            if (calStatus[G1MONITOR] == 0) //first pass so initialise the DataAverages
            {
                dataArray = av_CH4.GetAndReset; //now collecting O2 data also
                dataArray = av_CO2.GetAndReset;
                dataArray = av_N2O.GetAndReset;
                dataArray = av_CO.GetAndReset;
            }
            calStatus[G1MONITOR]++;
            //collect Zero data--  
            av_CH4.AddDataPoint(CH4data); //note this is the WT so dont divide by WT to get a ration  
            av_CO2.AddDataPoint(CO2data);  //THAT WOULD BE SILLY!
            av_N2O.AddDataPoint(N2Odata);
            av_CO.AddDataPoint(COdata);
            //TODO bung it in a log file
            try
            {
                database.insMeasurements(DateTime.Now, "WT", gasWt.ID, gasWt.ID, Convert.ToDouble(textBoxGGACH4.Text), Convert.ToDouble(textBoxGGACO2.Text),
                                         Convert.ToDouble(textBoxGGAH2O.Text), Convert.ToDouble(textBoxGGACH4dry.Text), Convert.ToDouble(textBoxGGACO2dry.Text),
                                         Convert.ToDouble(textBoxCorrCH4.Text), Convert.ToDouble(textBoxCorrCO2.Text), pSettings.GetDateTime("LastOKCH4CalDate"),
                                         pSettings.GetDateTime("LastOKCO2CalDate"),
                                         Convert.ToDouble(textBoxN2ON2O.Text), Convert.ToDouble(textBoxN2OCO.Text),
                                         Convert.ToDouble(textBoxN2OH2O.Text), Convert.ToDouble(textBoxN2ON2Odry.Text), Convert.ToDouble(textBoxN2OCOdry.Text),
                                         Convert.ToDouble(textBoxCorrN2O.Text), Convert.ToDouble(textBoxCorrCO.Text), pSettings.GetDateTime("LastOKN2OCalDate"),
                                         pSettings.GetDateTime("LastOKCOCalDate"));
            }
            catch (Exception ex)
            {
                string e = ex.Message;
            }
        }



        private void calculate4GasCalibration()
        {
            double[] dataArray;

            //first off we need to deal with the WT data

            //store the WT data
            dataArray = av_CH4.GetAndReset;
            post_WSS_WT_CH4_AVG = dataArray[MEAN];
            gasWt.AvCH4Sd = dataArray[SD];
            gasWt.AvCH4n = Convert.ToInt32(dataArray[COUNT]);
            dataArray = av_CO2.GetAndReset;
            post_WSS_WT_CO2_AVG = dataArray[MEAN];
            gasWt.AvCO2Sd = dataArray[SD];
            gasWt.AvCO2n = Convert.ToInt32(dataArray[COUNT]);
            dataArray = av_N2O.GetAndReset;
            post_WSS_WT_N2O_AVG = dataArray[MEAN];
            gasWt.AvN2OSd = dataArray[SD];
            gasWt.AvN2On = Convert.ToInt32(dataArray[COUNT]);
            dataArray = av_CO.GetAndReset;
            post_WSS_WT_CO_AVG = dataArray[MEAN];
            gasWt.AvCOSd = dataArray[SD];
            gasWt.AvCOn = Convert.ToInt32(dataArray[COUNT]);
            calStatus[WT5MONITOR] = 0;
            //make WSS4 result into a ratio
            //gasWss3.AvCH4Meas = gasWss3.AvCH4Meas / ((pre_WSS_WT_CH4_AVG + post_WSS_WT_CH4_AVG) / 2);
            //gasWss3.AvCO2Meas = gasWss3.AvCO2Meas / ((pre_WSS_WT_CO2_AVG + post_WSS_WT_CO2_AVG) / 2);
            //gasWss3.AvN2OMeas = gasWss3.AvN2OMeas / ((pre_WSS_WT_N2O_AVG + post_WSS_WT_N2O_AVG) / 2);
            //gasWss3.AvCOMeas = gasWss3.AvCOMeas / ((pre_WSS_WT_CO_AVG + post_WSS_WT_CO_AVG) / 2);
            gasWss4.AvCH4WT = (pre_WSS_WT_CH4_AVG + post_WSS_WT_CH4_AVG) / 2;
            gasWss4.AvCO2WT = (pre_WSS_WT_CO2_AVG + post_WSS_WT_CO2_AVG) / 2;
            gasWss4.AvN2OWT = (pre_WSS_WT_N2O_AVG + post_WSS_WT_N2O_AVG) / 2;
            gasWss4.AvCOWT = (pre_WSS_WT_CO_AVG + post_WSS_WT_CO_AVG) / 2;

            //update WT data
            gasWt.AvCH4Meas = post_WSS_WT_CH4_AVG;
            gasWt.AvCO2Meas = post_WSS_WT_CO2_AVG;
            gasWt.AvN2OMeas = pre_WSS_WT_N2O_AVG;
            gasWt.AvCOMeas = pre_WSS_WT_CO_AVG;
            wtCH4 = gasWt.AvCH4Meas;
            wtCO2 = gasWt.AvCO2Meas;
            wtN2O = gasWt.AvN2OMeas;
            wtCO = gasWt.AvCOMeas;

            //prevent a crash - we will still have RAW data
            if (wtCH4 == 0 || double.IsNaN(wtCH4)) wtCH4 = -1;
            if (wtCO2 == 0 || double.IsNaN(wtCO2)) wtCO2 = -1;
            if (wtN2O == 0 || double.IsNaN(wtN2O)) wtN2O = -1;
            if (wtCO == 0 || double.IsNaN(wtCO)) wtCO = -1;

            labelWTrefCH4.Text = MyString.setDP(wtCH4.ToString(), 4);
            labelWTrefCO2.Text = MyString.setDP(wtCO2.ToString(), 4);
            labelWTrefN2O.Text = MyString.setDP(wtN2O.ToString(), 4);
            labelWTrefCO.Text = MyString.setDP(wtCO.ToString(), 4);

            pSettings.AddSetting("WtCH4", wtCH4);
            pSettings.AddSetting("WtCO2", wtCO2);
            pSettings.AddSetting("WtN2O", wtN2O);
            pSettings.AddSetting("WtCO", wtCO);
            pSettings.Save();
           
            database.insWTCalibration(DateTime.Now, gasWt.ID,
                                      gasWt.CH4Conc, gasWt.CO2Conc, gasWt.AvCH4Meas, gasWt.AvCO2Meas,
                                      gasWt.N2OConc, gasWt.COConc, gasWt.AvN2OMeas, gasWt.AvCOMeas);
            //clear stored values
            pre_WSS_WT_CH4_AVG = -1;
            post_WSS_WT_CH4_AVG = -1;
            pre_WSS_WT_CO2_AVG = -1;
            post_WSS_WT_CO2_AVG = -1;
            pre_WSS_WT_N2O_AVG = -1;
            post_WSS_WT_N2O_AVG = -1;
            pre_WSS_WT_CO_AVG = -1;
            post_WSS_WT_CO_AVG = -1;

            //now perform the calculations (quadratic)
            calculateSpansQuadraticOrLinear();
            calStatus[STEP] = 0;

        }


        private void calculateSpansQuadraticOrLinear()
        {
            string data = "";
            string calCH4Flag = "REJECTED";
            string calCO2Flag = "REJECTED";
            string calN2OFlag = "REJECTED";
            string calCOFlag = "REJECTED";

            double CH4_aTerm = 0;
            double CH4_bTerm = 0;
            double CH4_cTerm = 0;
            double CH4_rSquare = 0;
            double CO2_aTerm = 0;
            double CO2_bTerm = 0;
            double CO2_cTerm = 0;
            double CO2_rSquare = 0;
            double N2O_aTerm = 0;
            double N2O_bTerm = 0;
            double N2O_cTerm = 0;
            double N2O_rSquare = 0;
            double CO_aTerm = 0;
            double CO_bTerm = 0;
            double CO_cTerm = 0;
            double CO_rSquare = 0;

            //enter values to determine calibration factors

            //++++++++++++++++++++++++++++++++++++++++++++
            //                 CH4
            //++++++++++++++++++++++++++++++++++++++++++++
            if (cbCH4linear.Checked)
            {
                SolveLinearRegression spansCH4 = new SolveLinearRegression();
                spansCH4.AddPoints(gasWss1.AvCH4Ratio, gasWss1.CH4Conc);
                spansCH4.AddPoints(gasWss2.AvCH4Ratio, gasWss2.CH4Conc);
                spansCH4.AddPoints(gasWss3.AvCH4Ratio, gasWss3.CH4Conc);
                spansCH4.AddPoints(gasWss4.AvCH4Ratio, gasWss4.CH4Conc);
                CH4_aTerm = 0;
                CH4_bTerm = spansCH4.Slope();
                CH4_cTerm = spansCH4.Intercept();
                CH4_rSquare = spansCH4.RSquare();
            }
            else
            {
                LstSquQuadRegr spansCH4 = new LstSquQuadRegr();
                spansCH4.AddPoints(gasWss1.AvCH4Ratio, gasWss1.CH4Conc);
                spansCH4.AddPoints(gasWss2.AvCH4Ratio, gasWss2.CH4Conc);
                spansCH4.AddPoints(gasWss3.AvCH4Ratio, gasWss3.CH4Conc);
                spansCH4.AddPoints(gasWss4.AvCH4Ratio, gasWss4.CH4Conc);
                //logErr.LogData("CH4 values ratio then conc," + gasWss1.AvCH4Ratio.ToString() + "," + gasWss1.CH4Conc.ToString() + "," + gasWss2.AvCH4Ratio.ToString() + "," +
                //    gasWss2.CH4Conc.ToString() + "," + gasWss3.AvCH4Ratio.ToString() + "," + gasWss3.CH4Conc.ToString() + "," + gasWss4.AvCH4Ratio.ToString() + "," + gasWss4.CH4Conc.ToString());
                CH4_aTerm = spansCH4.aTerm();
                CH4_bTerm = spansCH4.bTerm();
                CH4_cTerm = spansCH4.cTerm();
                CH4_rSquare = spansCH4.rSquare();
                //logErr.LogData("CH4_aTerm CH4_bTerm CH4_cTerm CH4_rsq, " + CH4_aTerm.ToString() + "," + CH4_bTerm.ToString() + "," + CH4_cTerm.ToString() + "," + CH4_rSquare.ToString());
            }
            //display these values
            labelCH4aLC.Text = MyString.setDpOrExpDp(CH4_aTerm.ToString(), 4, 2, 1);
            labelCH4bLC.Text = MyString.setDpOrExpDp(CH4_bTerm.ToString(), 4, 2, 1);
            labelCH4cLC.Text = MyString.setDpOrExpDp(CH4_cTerm.ToString(), 4, 2, 1);
            labelCalDateLC.Text = "Last CH4 Calibration Date: " + MyString.alphaNumDate(macroStartTime, "", 2) + " " + macroStartTime.ToShortTimeString();

            //++++++++++++++++++++++++++++++++++++++++++++
            //                 CO2
            //++++++++++++++++++++++++++++++++++++++++++++
            if (cbCO2linear.Checked)
            {
                SolveLinearRegression spansCO2 = new SolveLinearRegression();
                spansCO2.AddPoints(gasWss1.AvCO2Ratio, gasWss1.CO2Conc);
                spansCO2.AddPoints(gasWss2.AvCO2Ratio, gasWss2.CO2Conc);
                spansCO2.AddPoints(gasWss3.AvCO2Ratio, gasWss3.CO2Conc);
                spansCO2.AddPoints(gasWss4.AvCO2Ratio, gasWss4.CO2Conc);
                CO2_aTerm = 0;
                CO2_bTerm = spansCO2.Slope();
                CO2_cTerm = spansCO2.Intercept();
                CO2_rSquare = spansCO2.RSquare();
            }
            else
            {
                LstSquQuadRegr spansCO2 = new LstSquQuadRegr();
                spansCO2.AddPoints(gasWss1.AvCO2Ratio, gasWss1.CO2Conc);
                spansCO2.AddPoints(gasWss2.AvCO2Ratio, gasWss2.CO2Conc);
                spansCO2.AddPoints(gasWss3.AvCO2Ratio, gasWss3.CO2Conc);
                spansCO2.AddPoints(gasWss4.AvCO2Ratio, gasWss4.CO2Conc);
                //logErr.LogData("CO2 values ratio then conc," + gasWss1.AvCO2Ratio.ToString() + "," + gasWss1.CO2Conc.ToString() + "," + gasWss2.AvCO2Ratio.ToString() + "," +
                //    gasWss2.CO2Conc.ToString() + "," + gasWss3.AvCO2Ratio.ToString() + "," + gasWss3.CO2Conc.ToString() + "," + gasWss4.AvCO2Ratio.ToString() + "," + gasWss4.CO2Conc.ToString());
                CO2_aTerm = spansCO2.aTerm();
                CO2_bTerm = spansCO2.bTerm();
                CO2_cTerm = spansCO2.cTerm();
                CO2_rSquare = spansCO2.rSquare();
                //logErr.LogData("CO2_aTerm CO2_bTerm CO2_cTerm CO2_rsq, " + CO2_aTerm.ToString() + "," + CO2_bTerm.ToString() + "," + CO2_cTerm.ToString() + "," + CO2_rSquare.ToString());
            }
            //display these values
            labelCO2aLC.Text = MyString.setDpOrExpDp(CO2_aTerm.ToString(), 4, 2, 1);
            labelCO2bLC.Text = MyString.setDpOrExpDp(CO2_bTerm.ToString(), 4, 2, 1);
            labelCO2cLC.Text = MyString.setDpOrExpDp(CO2_cTerm.ToString(), 4, 2, 1);

            //++++++++++++++++++++++++++++++++++++++++++++
            //                 N2O
            //++++++++++++++++++++++++++++++++++++++++++++
            if (cbN2Olinear.Checked)
            {
                SolveLinearRegression spansN2O = new SolveLinearRegression();
                spansN2O.AddPoints(gasWss1.AvN2ORatio, gasWss1.N2OConc);
                spansN2O.AddPoints(gasWss2.AvN2ORatio, gasWss2.N2OConc);
                spansN2O.AddPoints(gasWss3.AvN2ORatio, gasWss3.N2OConc);
                spansN2O.AddPoints(gasWss4.AvN2ORatio, gasWss4.N2OConc);
                N2O_aTerm = 0;
                N2O_bTerm = spansN2O.Slope();
                N2O_cTerm = spansN2O.Intercept();
                N2O_rSquare = spansN2O.RSquare();
            }
            else
            {
                LstSquQuadRegr spansN2O = new LstSquQuadRegr();
                spansN2O.AddPoints(gasWss1.AvN2ORatio, gasWss1.N2OConc);
                spansN2O.AddPoints(gasWss2.AvN2ORatio, gasWss2.N2OConc);
                spansN2O.AddPoints(gasWss3.AvN2ORatio, gasWss3.N2OConc);
                spansN2O.AddPoints(gasWss4.AvN2ORatio, gasWss4.N2OConc);
                //logErr.LogData("N2O values ratio then conc," + gasWss1.AvN2ORatio.ToString() + "," + gasWss1.N2OConc.ToString() + "," + gasWss2.AvN2ORatio.ToString() + "," +
                //  gasWss2.N2OConc.ToString() + "," + gasWss3.AvN2ORatio.ToString() + "," + gasWss3.N2OConc.ToString() + "," + gasWss4.AvN2ORatio.ToString() + "," + gasWss4.N2OConc.ToString());
                N2O_aTerm = spansN2O.aTerm();
                N2O_bTerm = spansN2O.bTerm();
                N2O_cTerm = spansN2O.cTerm();
                N2O_rSquare = spansN2O.rSquare();
                //logErr.LogData("N2O_aTerm N2O_bTerm N2O_cTerm N2O_rsq, " + N2O_aTerm.ToString() + "," + N2O_bTerm.ToString() + "," + N2O_cTerm.ToString() + "," + N2O_rSquare.ToString());
            }
            //display these values
            labelN2OaLC.Text = MyString.setDpOrExpDp(N2O_aTerm.ToString(), 4, 2, 1);
            labelN2ObLC.Text = MyString.setDpOrExpDp(N2O_bTerm.ToString(), 4, 2, 1);
            labelN2OcLC.Text = MyString.setDpOrExpDp(N2O_cTerm.ToString(), 4, 2, 1);

            //++++++++++++++++++++++++++++++++++++++++++++
            //                 CO
            //++++++++++++++++++++++++++++++++++++++++++++
            if (cbCOlinear.Checked)
            {
                SolveLinearRegression spansCO = new SolveLinearRegression();
                spansCO.AddPoints(gasWss1.AvCORatio, gasWss1.COConc);
                spansCO.AddPoints(gasWss2.AvCORatio, gasWss2.COConc);
                spansCO.AddPoints(gasWss3.AvCORatio, gasWss3.COConc);
                spansCO.AddPoints(gasWss4.AvCORatio, gasWss4.COConc);
                CO_aTerm = 0;
                CO_bTerm = spansCO.Slope();
                CO_cTerm = spansCO.Intercept();
                CO_rSquare = spansCO.RSquare();
            }
            else
            {
                LstSquQuadRegr spansCO = new LstSquQuadRegr();
                spansCO.AddPoints(gasWss1.AvCORatio, gasWss1.COConc);
                spansCO.AddPoints(gasWss2.AvCORatio, gasWss2.COConc);
                spansCO.AddPoints(gasWss3.AvCORatio, gasWss3.COConc);
                spansCO.AddPoints(gasWss4.AvCORatio, gasWss4.COConc);
                //logErr.LogData("CO values ratio then conc," + gasWss1.AvCORatio.ToString() + "," + gasWss1.COConc.ToString() + "," + gasWss2.AvCORatio.ToString() + "," +
                //    gasWss2.COConc.ToString() + "," + gasWss3.AvCORatio.ToString() + "," + gasWss3.COConc.ToString() + "," + gasWss4.AvCORatio.ToString() + "," + gasWss4.COConc.ToString());
                CO_aTerm = spansCO.aTerm();
                CO_bTerm = spansCO.bTerm();
                CO_cTerm = spansCO.cTerm();
                CO_rSquare = spansCO.rSquare();
                //logErr.LogData("CO_aTerm CO_bTerm CO_cTerm CO_rsq, " + CO_aTerm.ToString() + "," + CO_bTerm.ToString() + "," + CO_cTerm.ToString() + "," + CO_rSquare.ToString());
            }
            //display these values
            labelCOaLC.Text = MyString.setDpOrExpDp(CO_aTerm.ToString(), 4, 2, 1);
            labelCObLC.Text = MyString.setDpOrExpDp(CO_bTerm.ToString(), 4, 2, 1);
            labelCOcLC.Text = MyString.setDpOrExpDp(CO_cTerm.ToString(), 4, 2, 1);

            //are these values ok?
            //calRetry = 0;
            calCH4Flag = "1";
            if (cH4CalAcceptable(CH4_aTerm, CH4_bTerm, CH4_cTerm))
            {
                //CH4
                pSettings.AddSetting("TermA_CH4", CH4_aTerm);
                pSettings.AddSetting("TermB_CH4", CH4_bTerm);
                pSettings.AddSetting("TermC_CH4", CH4_cTerm);
                pSettings.AddSetting("CalCH4rSq", CH4_rSquare);
                labelCH4a.Text = MyString.setDpOrExpDp(pSettings.GetDouble("TermA_CH4").ToString(), 4, 2, 1);
                labelCH4b.Text = MyString.setDpOrExpDp(pSettings.GetDouble("TermB_CH4").ToString(), 4, 2, 1);
                labelCH4c.Text = MyString.setDpOrExpDp(pSettings.GetDouble("TermC_CH4").ToString(), 4, 2, 1);
                pSettings.AddSetting("LastOKCH4CalDate", macroStartTime);
                labelCH4CalDate.Text = "Last Accept CH4 Cal: " + MyString.alphaNumDate(pSettings.GetDateTime("LastOKCH4CalDate"), "", 2) + " " + pSettings.GetDateTime("LastOKCH4CalDate").ToShortTimeString();
                calCH4Flag = "0";
            }
            calCO2Flag = "1";
            if (cO2CalAcceptable(CO2_aTerm, CO2_bTerm, CO2_cTerm))
            {
                pSettings.AddSetting("LastOKCO2CalDate", macroStartTime);
                pSettings.AddSetting("TermA_CO2", CO2_aTerm);
                pSettings.AddSetting("TermB_CO2", CO2_bTerm);
                pSettings.AddSetting("TermC_CO2", CO2_cTerm);
                pSettings.AddSetting("CalCO2rSq", CO2_rSquare);
                labelCO2a.Text = MyString.setDpOrExpDp(pSettings.GetDouble("TermA_CO2").ToString(), 4, 2, 1);
                labelCO2b.Text = MyString.setDpOrExpDp(pSettings.GetDouble("TermB_CO2").ToString(), 4, 2, 1);
                labelCO2c.Text = MyString.setDpOrExpDp(pSettings.GetDouble("TermC_CO2").ToString(), 4, 2, 1);
                labelCO2CalDate.Text = "Last Accept CO2 Cal: " + MyString.alphaNumDate(pSettings.GetDateTime("LastOKCO2CalDate"), "", 2) + " " + pSettings.GetDateTime("LastOKCO2CalDate").ToShortTimeString();
                calCO2Flag = "0";
            }
            calN2OFlag = "1";
            if (n2OCalAcceptable(N2O_aTerm, N2O_bTerm, N2O_cTerm))
            {
                //N2O
                pSettings.AddSetting("TermA_N2O", N2O_aTerm);
                pSettings.AddSetting("TermB_N2O", N2O_bTerm);
                pSettings.AddSetting("TermC_N2O", N2O_cTerm);
                pSettings.AddSetting("CalN2OrSq", N2O_rSquare);
                labelN2Oa.Text = MyString.setDpOrExpDp(pSettings.GetDouble("TermA_N2O").ToString(), 4, 2, 1);
                labelN2Ob.Text = MyString.setDpOrExpDp(pSettings.GetDouble("TermB_N2O").ToString(), 4, 2, 1);
                labelN2Oc.Text = MyString.setDpOrExpDp(pSettings.GetDouble("TermC_N2O").ToString(), 4, 2, 1);
                pSettings.AddSetting("LastOKN2OCalDate", macroStartTime);
                labelN2OCalDate.Text = "Last Accept N2O Cal: " + MyString.alphaNumDate(pSettings.GetDateTime("LastOKN2OCalDate"), "", 2) + " " + pSettings.GetDateTime("LastOKN2OCalDate").ToShortTimeString();
                calN2OFlag = "0";
            }
            calCOFlag = "1";
            if (cOCalAcceptable(CO_aTerm, CO_bTerm, CO_cTerm))
            {
                pSettings.AddSetting("LastOKCOCalDate", macroStartTime);
                pSettings.AddSetting("TermA_CO", CO_aTerm);
                pSettings.AddSetting("TermB_CO", CO_bTerm);
                pSettings.AddSetting("TermC_CO", CO_cTerm);
                pSettings.AddSetting("CalCOrSq", CO_rSquare);
                labelCOa.Text = MyString.setDpOrExpDp(pSettings.GetDouble("TermA_CO").ToString(), 4, 2, 1);
                labelCOb.Text = MyString.setDpOrExpDp(pSettings.GetDouble("TermB_CO").ToString(), 4, 2, 1);
                labelCOc.Text = MyString.setDpOrExpDp(pSettings.GetDouble("TermC_CO").ToString(), 4, 2, 1);
                labelCOCalDate.Text = "Last Accept CO Cal: " + MyString.alphaNumDate(pSettings.GetDateTime("LastOKCOCalDate"), "", 2) + " " + pSettings.GetDateTime("LastOKCOCalDate").ToShortTimeString();
                calCOFlag = "0";
            }
            pSettings.Save();

            if (calCH4Flag == "1" || calCO2Flag == "1" || calN2OFlag == "1" || calCOFlag == "1")
            {
                //what to do if not accepted
                //calRetry++;
            }

           
            try
            {
                calDate = DateTime.Now;   //.AddSeconds(1); //to avoid clash with logging of past WT
                database.insCalibration(calDate,
                                        gasWss1.ID, gasWss1.CH4Conc, gasWss1.CO2Conc, gasWss1.AvCH4Meas, gasWss1.AvCO2Meas,
                                        gasWss1.AvCH4Ratio, gasWss1.AvCO2Ratio,
                                        gasWss1.N2OConc, gasWss1.COConc, gasWss1.AvN2OMeas, gasWss1.AvCOMeas,
                                        gasWss1.AvN2ORatio, gasWss1.AvCORatio,

                                        gasWss2.ID, gasWss2.CH4Conc, gasWss2.CO2Conc, gasWss2.AvCH4Meas, gasWss2.AvCO2Meas,
                                        gasWss2.AvCH4Ratio, gasWss2.AvCO2Ratio,
                                        gasWss2.N2OConc, gasWss2.COConc, gasWss2.AvN2OMeas, gasWss2.AvCOMeas,
                                        gasWss2.AvN2ORatio, gasWss2.AvCORatio,

                                        gasWss3.ID, gasWss3.CH4Conc, gasWss3.CO2Conc, gasWss3.AvCH4Meas, gasWss3.AvCO2Meas,
                                        gasWss3.AvCH4Ratio, gasWss3.AvCO2Ratio,
                                        gasWss3.N2OConc, gasWss3.COConc, gasWss3.AvN2OMeas, gasWss3.AvCOMeas,
                                        gasWss3.AvN2ORatio, gasWss3.AvCORatio,

                                        gasWss4.ID, gasWss4.CH4Conc, gasWss4.CO2Conc, gasWss4.AvCH4Meas, gasWss4.AvCO2Meas,
                                        gasWss4.AvCH4Ratio, gasWss4.AvCO2Ratio,
                                        gasWss4.N2OConc, gasWss4.COConc, gasWss4.AvN2OMeas, gasWss4.AvCOMeas,
                                        gasWss4.AvN2ORatio, gasWss4.AvCORatio,
                                        CH4_aTerm, CH4_bTerm, CH4_cTerm, CH4_rSquare,
                                        CO2_aTerm, CO2_bTerm, CO2_cTerm, CO2_rSquare,
                                        N2O_aTerm, N2O_bTerm, N2O_cTerm, N2O_rSquare,
                                        CO_aTerm, CO_bTerm, CO_cTerm, CO_rSquare);
                logErr.LogData("DB cal table,No Error \n" + database.lastSQLcmd);


                //"-", double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN,
                //                        double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN,

            }
            catch (Exception ex)
            {
                logErr.LogData("DB cal table," + database.lastErrorMsg + "\n" + database.lastSQLcmd);
                string alex = ex.Message;
            }

            //Multiple Cal Run Code


            //reduce calCount and save the data
            if (calCount > 0) calCount--;
            if (calCount < 0)  //once we exit here with calCount of 0 requitecalibration becomes false sowe shouldn't enter this routine again
            {
                MessageBox.Show("Error with calCount");
                string alex = "buggrit";
            }
            if (calCount == 3) //first run is a good time to reinitialise the data averages
            {
                double[] discard = av_CH4_aTerm.GetAndReset;
                discard = av_CH4_bTerm.GetAndReset;
                discard = av_CH4_cTerm.GetAndReset;
                discard = av_CH4_rSquare.GetAndReset;
                discard = av_CO2_aTerm.GetAndReset;
                discard = av_CO2_bTerm.GetAndReset;
                discard = av_CO2_cTerm.GetAndReset;
                discard = av_CO2_rSquare.GetAndReset;
                discard = av_N2O_aTerm.GetAndReset;
                discard = av_N2O_bTerm.GetAndReset;
                discard = av_N2O_cTerm.GetAndReset;
                discard = av_N2O_rSquare.GetAndReset;
                discard = av_CO_aTerm.GetAndReset;
                discard = av_CO_bTerm.GetAndReset;
                discard = av_CO_cTerm.GetAndReset;
                discard = av_CO_rSquare.GetAndReset;
            }
            if (calCount < 3) //last 3 runs
            {
                if (cH4CalAcceptable(CH4_aTerm, CH4_bTerm, CH4_cTerm))
                {
                    av_CH4_aTerm.AddDataPoint(CH4_aTerm);
                    av_CH4_bTerm.AddDataPoint(CH4_bTerm);
                    av_CH4_cTerm.AddDataPoint(CH4_cTerm);
                    av_CH4_rSquare.AddDataPoint(CH4_rSquare);
                }
                if (cO2CalAcceptable(CO2_aTerm, CO2_bTerm, CO2_cTerm))
                {
                    av_CO2_aTerm.AddDataPoint(CO2_aTerm);
                    av_CO2_bTerm.AddDataPoint(CO2_bTerm);
                    av_CO2_cTerm.AddDataPoint(CO2_cTerm);
                    av_CO2_rSquare.AddDataPoint(CO2_rSquare);
                }
                if (n2OCalAcceptable(N2O_aTerm, N2O_bTerm, N2O_cTerm))
                {
                    av_N2O_aTerm.AddDataPoint(N2O_aTerm);
                    av_N2O_bTerm.AddDataPoint(N2O_bTerm);
                    av_N2O_cTerm.AddDataPoint(N2O_cTerm);
                    av_N2O_rSquare.AddDataPoint(N2O_rSquare);
                }
                if (cOCalAcceptable(CO_aTerm, CO_bTerm, CO_cTerm))
                {
                    av_CO_aTerm.AddDataPoint(CO_aTerm);
                    av_CO_bTerm.AddDataPoint(CO_bTerm);
                    av_CO_cTerm.AddDataPoint(CO_cTerm);
                    av_CO_rSquare.AddDataPoint(CO_rSquare);
                }
            }

            //if it's the last calibration take an average of the last 3 runs and save the values
            if (calCount == 0)
            {
                CH4_aTerm = av_CH4_aTerm.GetAndReset[MEAN];
                CH4_bTerm = av_CH4_bTerm.GetAndReset[MEAN];
                CH4_cTerm = av_CH4_cTerm.GetAndReset[MEAN];
                CH4_rSquare = av_CH4_rSquare.GetAndReset[MEAN];
                CO2_aTerm = av_CO2_aTerm.GetAndReset[MEAN];
                CO2_bTerm = av_CO2_bTerm.GetAndReset[MEAN];
                CO2_cTerm = av_CO2_cTerm.GetAndReset[MEAN];
                CO2_rSquare = av_CO2_rSquare.GetAndReset[MEAN];
                N2O_aTerm = av_N2O_aTerm.GetAndReset[MEAN];
                N2O_bTerm = av_N2O_bTerm.GetAndReset[MEAN];
                N2O_cTerm = av_N2O_cTerm.GetAndReset[MEAN];
                N2O_rSquare = av_N2O_rSquare.GetAndReset[MEAN];
                CO_aTerm = av_CO_aTerm.GetAndReset[MEAN];
                CO_bTerm = av_CO_bTerm.GetAndReset[MEAN];
                CO_cTerm = av_CO_cTerm.GetAndReset[MEAN];
                CO_rSquare = av_CO_rSquare.GetAndReset[MEAN];

                calCH4Flag = "1";
                if (cH4CalAcceptable(CH4_aTerm, CH4_bTerm, CH4_cTerm))
                {
                    pSettings.AddSetting("TermA_CH4", CH4_aTerm);
                    pSettings.AddSetting("TermB_CH4", CH4_bTerm);
                    pSettings.AddSetting("TermC_CH4", CH4_cTerm);
                    pSettings.AddSetting("CalCH4rSq", CH4_rSquare);
                    labelCH4a.Text = MyString.setDpOrExpDp(pSettings.GetDouble("TermA_CH4").ToString(), 4, 2, 1);
                    labelCH4b.Text = MyString.setDpOrExpDp(pSettings.GetDouble("TermB_CH4").ToString(), 4, 2, 1);
                    labelCH4c.Text = MyString.setDpOrExpDp(pSettings.GetDouble("TermC_CH4").ToString(), 4, 2, 1);
                    pSettings.AddSetting("LastOKCH4CalDate", macroStartTime);
                    labelCH4CalDate.Text = "Last Accept CH4 Cal: " + MyString.alphaNumDate(pSettings.GetDateTime("LastOKCH4CalDate"), "", 2) + " " + pSettings.GetDateTime("LastOKCH4CalDate").ToShortTimeString();
                    calCH4Flag = "0";
                }
                calCO2Flag = "1";
                if (cO2CalAcceptable(CO2_aTerm, CO2_bTerm, CO2_cTerm))
                {
                    pSettings.AddSetting("LastOKCO2CalDate", macroStartTime);
                    pSettings.AddSetting("TermA_CO2", CO2_aTerm);
                    pSettings.AddSetting("TermB_CO2", CO2_bTerm);
                    pSettings.AddSetting("TermC_CO2", CO2_cTerm);
                    pSettings.AddSetting("CalCO2rSq", CO2_rSquare);
                    labelCO2a.Text = MyString.setDpOrExpDp(pSettings.GetDouble("TermA_CO2").ToString(), 4, 2, 1);
                    labelCO2b.Text = MyString.setDpOrExpDp(pSettings.GetDouble("TermB_CO2").ToString(), 4, 2, 1);
                    labelCO2c.Text = MyString.setDpOrExpDp(pSettings.GetDouble("TermC_CO2").ToString(), 4, 2, 1);
                    labelCO2CalDate.Text = "Last Accept CO2 Cal: " + MyString.alphaNumDate(pSettings.GetDateTime("LastOKCO2CalDate"), "", 2) + " " + pSettings.GetDateTime("LastOKCO2CalDate").ToShortTimeString();
                    calCO2Flag = "0";
                }
                calN2OFlag = "1";
                if (n2OCalAcceptable(N2O_aTerm, N2O_bTerm, N2O_cTerm))
                {
                    //N2O
                    pSettings.AddSetting("TermA_N2O", N2O_aTerm);
                    pSettings.AddSetting("TermB_N2O", N2O_bTerm);
                    pSettings.AddSetting("TermC_N2O", N2O_cTerm);
                    pSettings.AddSetting("CalN2OrSq", N2O_rSquare);
                    labelN2Oa.Text = MyString.setDpOrExpDp(pSettings.GetDouble("TermA_N2O").ToString(), 4, 2, 1);
                    labelN2Ob.Text = MyString.setDpOrExpDp(pSettings.GetDouble("TermB_N2O").ToString(), 4, 2, 1);
                    labelN2Oc.Text = MyString.setDpOrExpDp(pSettings.GetDouble("TermC_N2O").ToString(), 4, 2, 1);
                    pSettings.AddSetting("LastOKN2OCalDate", macroStartTime);
                    labelN2OCalDate.Text = "Last Accept N2O Cal: " + MyString.alphaNumDate(pSettings.GetDateTime("LastOKN2OCalDate"), "", 2) + " " + pSettings.GetDateTime("LastOKN2OCalDate").ToShortTimeString();
                    calN2OFlag = "0";
                }
                calCOFlag = "1";
                if (cOCalAcceptable(CO_aTerm, CO_bTerm, CO_cTerm))
                {
                    pSettings.AddSetting("LastOKCOCalDate", macroStartTime);
                    pSettings.AddSetting("TermA_CO", CO_aTerm);
                    pSettings.AddSetting("TermB_CO", CO_bTerm);
                    pSettings.AddSetting("TermC_CO", CO_cTerm);
                    pSettings.AddSetting("CalCOrSq", CO_rSquare);
                    labelCOa.Text = MyString.setDpOrExpDp(pSettings.GetDouble("TermA_CO").ToString(), 4, 2, 1);
                    labelCOb.Text = MyString.setDpOrExpDp(pSettings.GetDouble("TermB_CO").ToString(), 4, 2, 1);
                    labelCOc.Text = MyString.setDpOrExpDp(pSettings.GetDouble("TermC_CO").ToString(), 4, 2, 1);
                    labelCOCalDate.Text = "Last Accept CO Cal: " + MyString.alphaNumDate(pSettings.GetDateTime("LastOKCOCalDate"), "", 2) + " " + pSettings.GetDateTime("LastOKCOCalDate").ToShortTimeString();
                    calCOFlag = "0";
                }
                pSettings.Save();

            
                try
                {
                    calDate = DateTime.Now.AddSeconds(1);   //.AddSeconds(1); //to avoid clash with logging of 4th cal
                    database.insCalibration(calDate,
                                            gasWss1.ID, gasWss1.CH4Conc, gasWss1.CO2Conc, gasWss1.AvCH4Meas, gasWss1.AvCO2Meas,
                                            gasWss1.AvCH4Ratio, gasWss1.AvCO2Ratio,
                                            gasWss1.N2OConc, gasWss1.COConc, gasWss1.AvN2OMeas, gasWss1.AvCOMeas,
                                            gasWss1.AvN2ORatio, gasWss1.AvCORatio,

                                            gasWss2.ID, gasWss2.CH4Conc, gasWss2.CO2Conc, gasWss2.AvCH4Meas, gasWss2.AvCO2Meas,
                                            gasWss2.AvCH4Ratio, gasWss2.AvCO2Ratio,
                                            gasWss2.N2OConc, gasWss2.COConc, gasWss2.AvN2OMeas, gasWss2.AvCOMeas,
                                            gasWss2.AvN2ORatio, gasWss2.AvCORatio,

                                            gasWss3.ID, gasWss3.CH4Conc, gasWss3.CO2Conc, gasWss3.AvCH4Meas, gasWss3.AvCO2Meas,
                                            gasWss3.AvCH4Ratio, gasWss3.AvCO2Ratio,
                                            gasWss3.N2OConc, gasWss3.COConc, gasWss3.AvN2OMeas, gasWss3.AvCOMeas,
                                            gasWss3.AvN2ORatio, gasWss3.AvCORatio,

                                            gasWss4.ID, gasWss4.CH4Conc, gasWss4.CO2Conc, gasWss4.AvCH4Meas, gasWss4.AvCO2Meas,
                                            gasWss4.AvCH4Ratio, gasWss4.AvCO2Ratio,
                                            gasWss4.N2OConc, gasWss4.COConc, gasWss4.AvN2OMeas, gasWss4.AvCOMeas,
                                            gasWss4.AvN2ORatio, gasWss4.AvCORatio,
                                            CH4_aTerm, CH4_bTerm, CH4_cTerm, CH4_rSquare,
                                            CO2_aTerm, CO2_bTerm, CO2_cTerm, CO2_rSquare,
                                            N2O_aTerm, N2O_bTerm, N2O_cTerm, N2O_rSquare,
                                            CO_aTerm, CO_bTerm, CO_cTerm, CO_rSquare);
                    logErr.LogData("DB cal table,No Error \n" + database.lastSQLcmd);

                }
                catch (Exception ex)
                {
                    logErr.LogData("DB cal table," + database.lastErrorMsg + "\n" + database.lastSQLcmd);
                    string alex = ex.Message;
                }
            }

        }

        private void calculateWTvalues()
        {
            double[] dataArray;
            string data;

            //store WT data
            dataArray = av_CH4.GetAndReset;
            gasWt.AvCH4Meas = dataArray[MEAN];
            gasWt.AvCH4Sd = dataArray[SD];
            gasWt.AvCH4n = Convert.ToInt32(dataArray[COUNT]);
            dataArray = av_CO2.GetAndReset;
            gasWt.AvCO2Meas = dataArray[MEAN];
            gasWt.AvCO2Sd = dataArray[SD];
            gasWt.AvCO2n = Convert.ToInt32(dataArray[COUNT]);
            dataArray = av_N2O.GetAndReset;
            gasWt.AvN2OMeas = dataArray[MEAN];
            gasWt.AvN2OSd = dataArray[SD];
            gasWt.AvN2On = Convert.ToInt32(dataArray[COUNT]);
            dataArray = av_CO.GetAndReset;
            gasWt.AvCOMeas = dataArray[MEAN];
            gasWt.AvCOSd = dataArray[SD];
            gasWt.AvCOn = Convert.ToInt32(dataArray[COUNT]);
            calStatus[G1MONITOR] = 0;
            calStatus[STEP] = 0;

            wtCH4 = gasWt.AvCH4Meas;
            wtCO2 = gasWt.AvCO2Meas;
            wtN2O = gasWt.AvN2OMeas;
            wtCO = gasWt.AvCOMeas;

            //prevent a crash - we will still have RAW data
            if (wtCH4 == 0 || double.IsNaN(wtCH4)) wtCH4 = -1;
            if (wtCO2 == 0 || double.IsNaN(wtCO2)) wtCO2 = -1;
            if (wtN2O == 0 || double.IsNaN(wtN2O)) wtN2O = -1;
            if (wtCO == 0 || double.IsNaN(wtCO)) wtCO = -1;

            labelWTrefCH4.Text = MyString.setDP(wtCH4.ToString(), 4);
            labelWTrefCO2.Text = MyString.setDP(wtCO2.ToString(), 4);
            labelWTrefN2O.Text = MyString.setDP(wtN2O.ToString(), 4);
            labelWTrefCO.Text = MyString.setDP(wtCO.ToString(), 4);

            pSettings.AddSetting("WtCH4", wtCH4);
            pSettings.AddSetting("WtCO2", wtCO2);
            pSettings.AddSetting("WtN2O", wtN2O);
            pSettings.AddSetting("WtCO", wtCO);
            pSettings.Save();

            database.insWTCalibration(DateTime.Now, gasWt.ID,
                                      gasWt.CH4Conc, gasWt.CO2Conc, gasWt.AvCH4Meas, gasWt.AvCO2Meas,
                                      gasWt.N2OConc, gasWt.COConc, gasWt.AvN2OMeas, gasWt.AvCOMeas);
        }

    }
}