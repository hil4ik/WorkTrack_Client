﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace WorkTrack_WindowsService
{
    public partial class WorkTrack : ServiceBase
    {
        System.Timers.Timer CheckprocTimer;
        List<string> ProcessList = new List<string>();
        Process[] pr;
        bool doit;
        DateTime t1, t2;
        TimeSpan ts;
        GetInformation getinf = new GetInformation();
        ConnectionCheck connectCheck = new ConnectionCheck();
        string currentdate;
        FTP ftp = new FTP();
        
        public WorkTrack()
        {
            InitializeComponent();
            
        }
      

        private void CheckprocTimer_elapsed(object sender, EventArgs e)
        {

            getinf.CheckProcess(ProcessList, doit);
            doit = false;
            currentdate = DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString();
           // ftp.Upload(currentdate + "/" + System.Environment.MachineName + "/", System.Environment.MachineName + "_ProcessTime.txt");
           // ftp.Upload(currentdate + "/" + System.Environment.MachineName + "/", System.Environment.MachineName + "_InstalledApp.txt");

        }



        protected override void OnStart(string[] args)
        {
            getinf.GetInstalledApps();
            pr = Process.GetProcesses();

            foreach (Process poc in pr)
            {
                if (!string.IsNullOrEmpty(poc.MainWindowTitle))
                {
                    ProcessList.Add(poc.MainWindowTitle.ToString());

                }

            }
            t1 = DateTime.Now;
                       
            //currentdate = DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString();
            //ftp.checkFolderFtp(currentdate);
            //ftp.checkFolderFtp(currentdate + "/" + System.Environment.MachineName);

            doit = true;


            CheckprocTimer = new System.Timers.Timer();
            CheckprocTimer.Elapsed += CheckprocTimer_elapsed;
            CheckprocTimer.Enabled = true;
            CheckprocTimer.Interval = 10000;
            CheckprocTimer.Start();
        }

        protected override void OnStop()
        {
            t2 = DateTime.Now;
            ts = t2 - t1;
            getinf.getWorkedTime(ts);
           // ftp.Upload(currentdate + "/" + System.Environment.MachineName + "/", System.Environment.MachineName + "_WorkedTime.txt");
        }
    }
}
