﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Work_Track_Client_v2
{
    class GetInformation
    {
        string conn = ConfigurationManager.ConnectionStrings["Work_Track"].ConnectionString;
        string uninstallKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
        Used_app uApp = new Used_app();
        List<Used_app> uselist = new List<Used_app>();
        List<string> ProcessList = new List<string>();
        List<string> CurrentProcessList = new List<string>();
        List<string[]> AllProcessList = new List<string[]>();
        string pcname = System.Environment.MachineName;
        Installedapp Apps;
        List<Installedapp> InstalledList = new List<Installedapp>();
        //List<Installedapp> InstalledList1;
        public List<Used_app> CheckProcess(List<string> prlist, bool doit, List<Used_app> UsedappBD, bool reboot)
        {            
            uselist.Clear();
            CurrentProcessList.Clear();
            if (doit)
            {
                ProcessList = prlist;
            }

            Process[] pr = Process.GetProcesses();
            //.Where(p => !string.IsNullOrEmpty(p.MainWindowTitle))
            //.Select(p => new { p.MainWindowTitle }).ToString();

            foreach (Process poc in pr)
            {
                if (!string.IsNullOrEmpty(poc.MainWindowTitle))
                {
                    CurrentProcessList.Add(poc.MainWindowTitle.ToString());

                }

            }

            for (int i = 0; i < CurrentProcessList.Count; i++)
            {
                if (CurrentProcessList[i].EndsWith("Google Chrome"))
                {
                    CurrentProcessList[i] = "Google Chrome";
                }
                if (CurrentProcessList[i].EndsWith("Mozila Firefox"))
                {
                    CurrentProcessList[i] = "Mozilla Firefox";
                }
                if (CurrentProcessList[i].StartsWith("Skype"))
                {
                    CurrentProcessList[i] = "Skype";
                }
            }

            for (int i = 0; i < ProcessList.Count; i++)
            {
                bool deleteIS = true;
                for (int j = 0; j < CurrentProcessList.Count; j++)
                {
                    if (ProcessList[i]==CurrentProcessList[j])
                    {
                        deleteIS = false;
                    }

                }
                if (deleteIS)
                {
                    ProcessList.RemoveAt(i);
                    i--;
                }
            }


            if (AllProcessList.Count != 0)
            {
                foreach (string str in ProcessList)
                {
                    bool addnew1 = true;
                    for (int i = 0; i < AllProcessList.Count; i++)
                    {
                        if (str == AllProcessList[i][0])
                        {
                            double CurHour = Convert.ToDouble(AllProcessList[i][1]);
                            AllProcessList[i][1] = (CurHour + 0.5).ToString();
                            addnew1 = false;
                        }

                    }
                    if (addnew1)
                    {
                        string[] newProc = new string[2];
                        newProc[0] = str;
                        newProc[1] = "0,5";
                        AllProcessList.Add(newProc);
                    }

                }
            }
            else
            {
                foreach (string str in ProcessList)
                {
                    string[] newProc1 = new string[2];
                    newProc1[0] = str;
                    newProc1[1] = "0,5";
                    AllProcessList.Add(newProc1);
                }
            }


          

            foreach (string[] str in AllProcessList)
            {
                uApp.App_name = str[0];
                uApp.Used_time = str[1];
                uApp.Used_date = DateTime.Now.ToShortDateString();
                Used_app tempClone = (Used_app)uApp.Clone();
                uselist.Add(tempClone);

            }
            
            //=================================================================
            if (reboot)
            {
                SqlConnection sqlstartapp = new SqlConnection(conn);
                //Open the connection
                sqlstartapp.Open();

                using (var tempcmd2 = new SqlCommand("SELECT App_names FROM Applications WHERE App_ID=@App_id", sqlstartapp))
                {
                    tempcmd2.Parameters.Add(new SqlParameter("@App_id", SqlDbType.Int));
                    foreach (Used_app uappp in UsedappBD)
                    {
                        tempcmd2.Parameters["@App_id"].Value = uappp.App_ID;
                        uappp.App_name = tempcmd2.ExecuteScalar().ToString();
                    }
                }

                sqlstartapp.Close();
                int a = uselist.Count;
                int s = 0;
                foreach (Used_app uaps in UsedappBD)                    
                {
                    Used_app usedtime = new Used_app();
                    bool add = true;
                    for (s = 0; s < a; s++)
                    {
                        if (uaps.App_name == uselist[s].App_name)
                        {
                            add = false;                            
                            uselist[s].Used_time = (0.5 + Convert.ToDouble(uaps.Used_time)).ToString();
                            break;
                        }
                    }
                    if (add)
                    {
                        usedtime = (Used_app)uaps.Clone();
                        uselist.Add(usedtime);
                    }
                }

            }

            ProcessList.Clear();

            if (reboot)
            {
                AllProcessList.Clear();
                foreach (Used_app uas in uselist)
                {
                    string sstr = uas.App_name;
                    ProcessList.Add(sstr);
                    string[] straray = new string[2];
                    straray[0] = uas.App_name;
                    straray[1] = uas.Used_time;
                    AllProcessList.Add(straray);
                }

            }

            foreach (string str in CurrentProcessList)
            {
                ProcessList.Add(str);

            }
            //--------------------------------------------------
           


            //---------------------------------------------------
            return uselist;

        }

        public List<Installedapp> GetInstalledApps()
        {
            InstalledList.Clear();
            using (RegistryKey rk = Registry.LocalMachine.OpenSubKey(uninstallKey))
            {
                foreach (string skName in rk.GetSubKeyNames())
                {
                    using (RegistryKey sk = rk.OpenSubKey(skName))
                    {
                        try
                        {
                            string InstallDate = sk.GetValue("InstallDate").ToString();
                            Apps = new Installedapp();
                            Apps.App_name = sk.GetValue("DisplayName").ToString();
                            Apps.App_publisher = sk.GetValue("Publisher").ToString();
                            //DateTime d = DateTime.ParseExact(InstallDate, "yyyyMdd", null);
                            Apps.App_installdate = DateTime.ParseExact(InstallDate, "yyyyMdd", null);

                            //Installedapp tempApps = (Installedapp)Apps.Clone();
                            InstalledList.Add(Apps);
                        }
                        catch (Exception ex)
                        { }
                    }
                }
            }

            for (int i = 0; i < InstalledList.Count; i++)
            {
                if (InstalledList[i].App_name.EndsWith("Google Chrome"))
                {
                    InstalledList[i].App_name = "Google Chrome";
                }
                if (InstalledList[i].App_name.EndsWith("Mozila Firefox"))
                {
                    InstalledList[i].App_name = "Mozilla Firefox";
                }
                if (InstalledList[i].App_name.StartsWith("Skype"))
                {
                    InstalledList[i].App_name = "Skype";
                }
            }


            List<Installedapp> noduplicatesList = InstalledList.Distinct(new InstalledappComparer()).ToList<Installedapp>();


            return noduplicatesList;
        }

       

        
    }
}
