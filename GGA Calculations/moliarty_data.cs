using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;

class moliarty_data
{
    #region instance variables
    private MySqlConnection connection;
    private string server;
    private string database;
    private string uid;
    private string password;
    private string lastSQL;
    private string lastError;
    #endregion

    //Constructor
    public moliarty_data()
    {
        initialise();
    }

    //Initialize values
    private void initialise()
    {
        server = "localhost";
        database = "";
        uid = "";
        password = "";
        string connectionString;
        connectionString = "SERVER=" + server + ";" + "DATABASE=" +
        database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

        connection = new MySqlConnection(connectionString);
    }

    #region database functions
    //open connection to database
    private bool openConnection()
    {
        try
        {
            connection.Open();
            return true;
        }
        catch (MySqlException ex)
        {
            //The two most common error numbers when connecting are as follows:
            //0: Cannot connect to server.
            //1045: Invalid user name and/or password.
            switch (ex.Number)
            {
                case 0:
                    //MessageBox.Show("Cannot connect to server.  Contact administrator");
                    throw new Exception("Cannot connect to server.  Contact administrator", ex);
                    break;

                case 1045:
                    //MessageBox.Show("Invalid username/password, please try again");
                    throw new Exception("Invalid username/password, please try again", ex);
                    break;

                default:
                    throw new Exception("Connection Error", ex);
                    break;

            }
            return false;
        }
    }

    //Close connection
    public bool closeConnection()
    {
        try
        {
            connection.Close();
            return true;
        }
        catch (MySqlException ex)
        {
            //MessageBox.Show(ex.Message);
            throw new Exception("Connection close problem", ex);
            return false;
        }
    }
    #endregion

    public string lastSQLcmd
    {
        get
        {
            return lastSQL;
        }
    }

    public string lastErrorMsg
    {
        get
        {
            return lastError;
        }
    }

    //Insert Dory Data

    public void insInstrumentData(DateTime timestamp, double ggaAmbT, double ggaGasT, double ggaGasP, DateTime ggaTime, double n2oAmbT, double n2oGasT, double n2oGasP, DateTime n2oTime)
    {
        ////N2O is reporting wrong date
        //DateTime thisYear = Convert.ToDateTime("01/01/" + DateTime.Now.Year.ToString());
        //if (timestamp < thisYear) timestamp = DateTime.Now;

        string sqlDtime = timestamp.ToString("yyyy-MM-dd HH:mm:ss.ff");
        string sqlGGAtime = ggaTime.ToString("yyyy-MM-dd HH:mm:ss");
        string sqlN2Otime = n2oTime.ToString("yyyy-MM-dd HH:mm:ss");

        string query = "INSERT INTO instrument (timestamp, ggaAmbT, ggaGasT, ggaGasP, ggaTimestamp, n2oAmbT, n2oGasT, n2oGasP, n2oTimestamp) VALUES('" +
                        sqlDtime + "'," + ggaAmbT.ToString() + "," + ggaGasT.ToString() + "," + ggaGasP.ToString() + ",'" + sqlGGAtime + "'," +
                        n2oAmbT.ToString() + "," + n2oGasT.ToString() + "," + n2oGasP.ToString() + ",'" + sqlN2Otime + "')";

        query = query.Replace("NaN", "NULL");
        lastSQL = query;
        //open connection
        if (this.openConnection() == true)
        {
            try
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //Execute command
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                try
                {
                    this.closeConnection();
                }
                catch { }
                //MessageBox.Show(ex.Message);
                throw new Exception("Query problem: " + ex.Message, ex);
            }

            //close connection
            try
            {
                this.closeConnection();
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                //MessageBox.Show(ex.Message);
                throw new Exception("Connection close problem", ex);
            }
        }
        else
        {
            string debug = "dbg";
        }
    }

    public void insMeasurements(DateTime timestamp, string curgas, string id_curgas, string id_wt, 
                               double ch4ppb, double co2ppm, double ggah2oppm, double ch4dryppb, double co2dryppm,
                               double ch4corrppb, double co2corrppm, DateTime ch4caldate, DateTime co2caldate,
                               double n2oppb, double coppb, double n2oh2oppm, double n2odryppb, double codryppb,
                               double n2ocorrppb, double cocorrppb, DateTime n2ocaldate, DateTime cocaldate)
    {
        if (ch4corrppb > 9999 || ch4corrppb < -9999) ch4corrppb = double.NaN;
        if (co2corrppm > 999 || co2corrppm < -999) co2corrppm = double.NaN;
        if (n2ocorrppb > 999 || n2ocorrppb < -999) n2ocorrppb = double.NaN;
        if (cocorrppb > 999 || cocorrppb < -999) cocorrppb = double.NaN;


        if (ch4dryppb > 9999 || ch4dryppb < -9999) ch4dryppb = double.NaN;
        if (co2dryppm > 999 || co2dryppm < -999) co2dryppm = double.NaN;
        if (n2odryppb > 999 || n2odryppb < -999) n2odryppb = double.NaN;
        if (codryppb > 999 || codryppb < -999) codryppb = double.NaN;



        string query = "INSERT INTO measurement (timestamp, curgas, ID_curgas, ID_WT, " +
                       "CH4_ppb, CO2_ppm, ggaH2O_ppm, CH4_dry_ppb, CO2_dry_ppm, CH4_corr_ppb, CO2_corr_ppm, " +
                       "CH4calDate, CO2calDate, " +
                       "N2O_ppb, CO_ppb, n2oH2O_ppm, N2O_dry_ppb, CO_dry_ppb, N2O_corr_ppb, CO_corr_ppb, " +
                       "N2OcalDate, COcalDate) VALUES('" +
                        timestamp.ToString("yyyy-MM-dd HH:mm:ss.ff") + "','" + curgas + "','" + id_curgas + "','" + id_wt + "'," +
                        ch4ppb.ToString() + "," + co2ppm.ToString() + "," + ggah2oppm.ToString() + "," + ch4dryppb.ToString() + "," +
                        co2dryppm.ToString() + "," + ch4corrppb.ToString() + "," +
                        co2corrppm.ToString() + ",'" + ch4caldate.ToString("yyyy-MM-dd HH:mm:ss") + "','" +
                        co2caldate.ToString("yyyy-MM-dd HH:mm:ss") + "'," +
                        n2oppb.ToString() + "," + coppb.ToString() + "," + n2oh2oppm.ToString() + "," + n2odryppb.ToString() + "," +
                        codryppb.ToString() + "," + n2ocorrppb.ToString() + "," +
                        cocorrppb.ToString() + ",'" + n2ocaldate.ToString("yyyy-MM-dd HH:mm:ss") + "','" +
                        cocaldate.ToString("yyyy-MM-dd HH:mm:ss") + "')";

        query = query.Replace("NaN", "NULL");
        lastSQL = query;
        //open connection
        if (this.openConnection() == true)
        {
            try
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //Execute command
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                try
                {
                    this.closeConnection();
                }
                catch { }
                //MessageBox.Show(ex.Message);
                throw new Exception("Query problem: " + ex.Message, ex);
            }

            //close connection
            try
            {
                this.closeConnection();
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                //MessageBox.Show(ex.Message);
                throw new Exception("Connection close problem", ex);
            }
        }
        else
        {
            string debug = "dbg";
        }
    }

    public void insCalibration(DateTime timestamp,
                              string gas1ID, double gas1CH4conc, double gas1CO2conc, double gas1CH4meas, double gas1CO2meas,
                              double gas1CH4ratio, double gas1CO2ratio, 
                              double gas1N2Oconc, double gas1COconc, double gas1N2Omeas, double gas1COmeas,
                              double gas1N2Oratio, double gas1COratio,
                              string gas2ID, double gas2CH4conc, double gas2CO2conc, double gas2CH4meas, double gas2CO2meas,
                              double gas2CH4ratio, double gas2CO2ratio,
                              double gas2N2Oconc, double gas2COconc, double gas2N2Omeas, double gas2COmeas,
                              double gas2N2Oratio, double gas2COratio,
                              string gas3ID, double gas3CH4conc, double gas3CO2conc, double gas3CH4meas, double gas3CO2meas,
                              double gas3CH4ratio, double gas3CO2ratio, 
                              double gas3N2Oconc, double gas3COconc, double gas3N2Omeas, double gas3COmeas,
                              double gas3N2Oratio, double gas3COratio,
                              string gas4ID, double gas4CH4conc, double gas4CO2conc, double gas4CH4meas, double gas4CO2meas,
                              double gas4CH4ratio, double gas4CO2ratio,
                              double gas4N2Oconc, double gas4COconc, double gas4N2Omeas, double gas4COmeas,
                              double gas4N2Oratio, double gas4COratio,
                              double CH4aterm, double CH4bterm, double CH4cterm, double CH4rsq,
                              double CO2aterm, double CO2bterm, double CO2cterm, double CO2rsq,
                              double N2Oaterm, double N2Obterm, double N2Octerm, double N2Orsq,
                              double COaterm, double CObterm, double COcterm, double COrsq)
    {

        if (CH4aterm > 99999 || CH4aterm < -99999) CH4aterm = double.NaN;
        if (CH4bterm > 99999 || CH4bterm < -99999) CH4bterm = double.NaN;
        if (CH4cterm > 99999 || CH4cterm < -99999) CH4cterm = double.NaN;

        if (CO2aterm > 9999 || CO2aterm < -9999) CO2aterm = double.NaN;
        if (CO2bterm > 9999 || CO2bterm < -9999) CO2bterm = double.NaN;
        if (CO2cterm > 9999 || CO2cterm < -9999) CO2cterm = double.NaN;

        if (N2Oaterm > 9999 || N2Oaterm < -9999) N2Oaterm = double.NaN;
        if (N2Obterm > 9999 || N2Obterm < -9999) N2Obterm = double.NaN;
        if (N2Octerm > 9999 || N2Octerm < -9999) N2Octerm = double.NaN;

        if (COaterm > 9999 || COaterm < -9999) COaterm = double.NaN;
        if (CObterm > 9999 || CObterm < -9999) CObterm = double.NaN;
        if (COcterm > 9999 || COcterm < -9999) COcterm = double.NaN;



        string sqlDtime = timestamp.ToString("yyyy-MM-dd HH:mm:ss.ff");

        string query = "INSERT INTO calibration (timestamp,  " +
                         "gas1ID, gas1CH4conc, gas1CO2conc, gas1CH4meas, gas1CO2meas, gas1CH4ratio, gas1CO2ratio,  " +
                         "gas1N2Oconc, gas1COconc, gas1N2Omeas, gas1COmeas, gas1N2Oratio, gas1COratio,  " +
                         "gas2ID, gas2CH4conc, gas2CO2conc, gas2CH4meas, gas2CO2meas, gas2CH4ratio, gas2CO2ratio,  " +
                         "gas2N2Oconc, gas2COconc, gas2N2Omeas, gas2COmeas, gas2N2Oratio, gas2COratio,  " +
                         "gas3ID, gas3CH4conc, gas3CO2conc, gas3CH4meas, gas3CO2meas, gas3CH4ratio, gas3CO2ratio,  " +
                         "gas3N2Oconc, gas3COconc, gas3N2Omeas, gas3COmeas, gas3N2Oratio, gas3COratio,  " +
                         "gas4ID, gas4CH4conc, gas4CO2conc, gas4CH4meas, gas4CO2meas, gas4CH4ratio, gas4CO2ratio,  " +
                         "gas4N2Oconc, gas4COconc, gas4N2Omeas, gas4COmeas, gas4N2Oratio, gas4COratio,  " +
                         "CH4aterm, CH4bterm, CH4cterm, CH4rsq," +
                         "CO2aterm, CO2bterm, CO2cterm, CO2rsq," +
                         "N2Oaterm, N2Obterm, N2Octerm, N2Orsq," +
                         "COaterm, CObterm, COcterm, COrsq) VALUES('" +
                         sqlDtime + "','" + 
                         gas1ID + "'," + gas1CH4conc.ToString() + "," + gas1CO2conc.ToString() + "," +
                         gas1CH4meas.ToString() + "," + gas1CO2meas.ToString() + "," +
                         gas1CH4ratio.ToString() + "," + gas1CO2ratio.ToString() + "," + 
                         gas1N2Oconc.ToString() + "," + gas1COconc.ToString() + "," +
                         gas1N2Omeas.ToString() + "," + gas1COmeas.ToString() + "," +
                         gas1N2Oratio.ToString() + "," + gas1COratio.ToString() + ",'" + 
                         gas2ID + "'," +
                         gas2CH4conc.ToString() + "," + gas2CO2conc.ToString() + "," + gas2CH4meas.ToString() + "," +
                         gas2CO2meas.ToString() + "," +
                         gas2CH4ratio.ToString() + "," + gas2CO2ratio.ToString() + "," +
                         gas2N2Oconc.ToString() + "," + gas2COconc.ToString() + "," +
                         gas2N2Omeas.ToString() + "," + gas2COmeas.ToString() + "," +
                         gas2N2Oratio.ToString() + "," + gas2COratio.ToString() + ",'" + 
                         gas3ID + "'," + gas3CH4conc.ToString() + "," +
                         gas3CO2conc.ToString() + "," + gas3CH4meas.ToString() + "," + gas3CO2meas.ToString() + "," +
                         gas3CH4ratio.ToString() + "," + gas3CO2ratio.ToString() + "," +
                         gas3N2Oconc.ToString() + "," + gas3COconc.ToString() + "," +
                         gas3N2Omeas.ToString() + "," + gas3COmeas.ToString() + "," +
                         gas3N2Oratio.ToString() + "," + gas3COratio.ToString() + ",'" + 
                         gas4ID + "'," + gas4CH4conc.ToString() + "," + gas4CO2conc.ToString() + "," +
                         gas4CH4meas.ToString() + "," + gas4CO2meas.ToString() + "," +
                         gas4CH4ratio.ToString() + "," + gas4CO2ratio.ToString() + "," +
                         gas4N2Oconc.ToString() + "," + gas4COconc.ToString() + "," +
                         gas4N2Omeas.ToString() + "," + gas4COmeas.ToString() + "," +
                         gas4N2Oratio.ToString() + "," + gas4COratio.ToString() + "," + 
                         CH4aterm.ToString() + "," + CH4bterm.ToString() + "," +
                         CH4cterm.ToString() + "," + CH4rsq.ToString() + "," + 
                         CO2aterm.ToString() + "," + CO2bterm.ToString() + "," +
                         CO2cterm.ToString() + "," + CO2rsq.ToString() + "," +
                         N2Oaterm.ToString() + "," + N2Obterm.ToString() + "," +
                         N2Octerm.ToString() + "," + N2Orsq.ToString() + "," +
                         COaterm.ToString() + "," + CObterm.ToString() + "," +
                         COcterm.ToString() + "," + COrsq.ToString() + ")";

        query = query.Replace("NaN", "NULL");
        lastSQL = query;
        //open connection
        if (this.openConnection() == true)
        {
            try
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //Execute command
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                try
                {
                    this.closeConnection();
                }
                catch { }
                //MessageBox.Show(ex.Message);
                throw new Exception("Query problem", ex);
            }

            //close connection
            try
            {
                this.closeConnection();
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                //MessageBox.Show(ex.Message);
                throw new Exception("Connection close problem", ex);
            }
        }
        else
        {
            string debug = "dbg";
        }
    }


    public void insWTCalibration(DateTime timestamp, string gasWTID,
                                 double gasWTCH4conc, double gasWTCO2conc, double gasWTCH4meas, double gasWTCO2meas,
                                 double gasWTN2Oconc, double gasWTCOconc, double gasWTN2Omeas, double gasWTCOmeas)
    {
        if (gasWTCH4conc > 9999 || gasWTCH4conc < -9999) gasWTCH4conc = double.NaN;
        if (gasWTCO2conc > 999 || gasWTCO2conc < -999) gasWTCO2conc = double.NaN;
        if (gasWTN2Oconc > 999 || gasWTN2Oconc < -999) gasWTN2Oconc = double.NaN;
        if (gasWTCOconc > 9999 || gasWTCOconc < -9999) gasWTCOconc = double.NaN;

        if (gasWTCH4meas > 9999 || gasWTCH4meas < -9999) gasWTCH4meas = double.NaN;
        if (gasWTCO2meas > 999 || gasWTCO2meas < -999) gasWTCO2meas = double.NaN;
        if (gasWTN2Omeas > 999 || gasWTN2Omeas < -999) gasWTN2Omeas = double.NaN;
        if (gasWTCOmeas > 9999 || gasWTCOmeas < -9999) gasWTCOmeas = double.NaN;




        string sqlDtime = timestamp.ToString("yyyy-MM-dd HH:mm:ss.ff");

        string query = "INSERT INTO calibration (timestamp, gasWTID, " +
                       "gasWTCH4conc, gasWTCO2conc, gasWTCH4meas, gasWTCO2meas, " +
                       "gasWTN2Oconc, gasWTCOconc, gasWTN2Omeas, gasWTCOmeas) VALUES('" +
                         sqlDtime + "','" + gasWTID + "'," + 
                         gasWTCH4conc.ToString("#.##") + "," + gasWTCO2conc.ToString("#.##") + "," +
                         gasWTCH4meas.ToString("#.##") + "," + gasWTCO2meas.ToString("#.##") + "," +
                         gasWTN2Oconc.ToString("#.##") + "," + gasWTCOconc.ToString("#.##") + "," +
                         gasWTN2Omeas.ToString("#.##") + "," + gasWTCOmeas.ToString("#.##") + ")";

        query = query.Replace("NaN", "NULL");
        lastSQL = query;
        //open connection
        if (this.openConnection() == true)
        {
            try
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //Execute command
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                try
                {
                    this.closeConnection();
                }
                catch { }
                //MessageBox.Show(ex.Message);
                throw new Exception("Query problem", ex);
            }

            //close connection
            try
            {
                this.closeConnection();
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                //MessageBox.Show(ex.Message);
                throw new Exception("Connection close problem", ex);
            }
        }
        else
        {
            string debug = "dbg";
        }
    }


    public void insTarget(DateTime timestamp,
                              string ID, double CH4conc, double CO2conc, double CH4meas, double CO2meas,
                              double CH4range, double CO2range,
                              double N2Oconc, double COconc, double N2Omeas, double COmeas,
                              double N2Orange, double COrange)
    {
        string sqlDtime = timestamp.ToString("yyyy-MM-dd HH:mm:ss");
        if (CH4conc > 9999 || CH4conc < -9999) CH4conc = double.NaN;
        if (CO2conc > 999 || CO2conc < -999) CO2conc = double.NaN;
        if (CH4meas > 9999 || CH4meas < -9999) CH4meas = double.NaN;
        if (CO2meas > 999 || CO2meas < -999) CO2meas = double.NaN;
        if (N2Oconc > 999 || N2Oconc < -999) N2Oconc = double.NaN;
        if (COconc > 999 || COconc < -999) COconc = double.NaN;
        if (N2Omeas > 999 || N2Omeas < -999) N2Omeas = double.NaN;
        if (COmeas > 999 || COmeas < -999) COmeas = double.NaN;

        string query = "INSERT INTO target (timestamp, ID, " +
                       "CH4conc, CO2conc, CH4meas, CO2meas, CH4range, CO2range, " +
                       "N2Oconc, COconc, N2Omeas, COmeas, N2Orange, COrange) VALUES('" +
                         sqlDtime + "','" + ID + "'," + 
                         CH4conc.ToString() + "," + CO2conc.ToString() + "," +
                         CH4meas.ToString() + "," + CO2meas.ToString() + "," + CH4range.ToString() + "," + CO2range.ToString() + "," +
                         N2Oconc.ToString() + "," + COconc.ToString() + "," +
                         N2Omeas.ToString() + "," + COmeas.ToString() + "," + N2Orange.ToString() + "," + COrange.ToString() + ")";

        query = query.Replace("NaN", "NULL");
        lastSQL = query;
        //open connection
        if (this.openConnection() == true)
        {
            try
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //Execute command
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                try
                {
                    this.closeConnection();
                }
                catch { }
                //MessageBox.Show(ex.Message);
                throw new Exception("Query problem", ex);
            }

            //close connection
            try
            {
                this.closeConnection();
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                //MessageBox.Show(ex.Message);
                throw new Exception("Connection close problem", ex);
            }
        }
        else
        {
            string debug = "dbg";
        }
    }

}
