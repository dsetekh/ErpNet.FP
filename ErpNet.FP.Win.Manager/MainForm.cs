﻿using ErpNet.FP.Server.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace ErpNet.FP.Win.Manager
{
    public interface IMainForm { }
    public class MainForm : Form, IMainForm
    {
        //private const string ServiceFileName = @"..\..\..\..\ErpNet.FP.Server\Published\win-x86\ErpNet.FP.Server.exe";
        private const string ServiceFileName = @"ErpNet.FP.Server.exe";
        private readonly NotifyIcon managerNotifyIcon;
        private readonly ContextMenu managerContextMenu;
        private readonly MenuItem menuItemExit;
        private readonly MenuItem menuItemShowConsole;
        private readonly MenuItem menuItemShowAdminPage;
        private readonly MenuItem menuItemRestartService;
        private readonly TextBox logBox;
        private readonly IContainer components;
        private Process? serviceProcess;
        private bool cancelClose = true;
        private readonly Size consoleSize = new Size(120, 30);
        private readonly IWritableOptions<ErpNetFPConfigOptions> options;
        private ErpNetFPConfigOptions configOptions;

        public MainForm(
            IWritableOptions<ErpNetFPConfigOptions> options,
            IOptionsMonitor<ErpNetFPConfigOptions> monitor)
        {
            this.options = options;
            this.configOptions = options.Value;

            this.Text = "ErpNet.FP.Win Manager";
            this.Icon = new Icon("ErpNet.FP.ico");
            this.FormClosing += MainForm_FormClosing;

            this.components = new Container();

            this.logBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Parent = this,
                Multiline = true,
                BackColor = Color.Black,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                WordWrap = true,
                ScrollBars = ScrollBars.Vertical
            };

            this.logBox.Font = new Font(FontFamily.GenericMonospace, logBox.Font.Size);
            this.Width = (int)Math.Round(this.logBox.Font.Size * consoleSize.Width);
            this.Height = (int)Math.Round(this.logBox.Font.GetHeight() * consoleSize.Height);

            this.components.Add(this.logBox);

            this.menuItemShowConsole = new MenuItem
            {
                Text = "Show &console"
            };
            this.menuItemShowConsole.Click += MenuItemShowConsole_Click;

            this.menuItemShowAdminPage = new MenuItem
            {
                Text = "Show &Admin Page"
            };
            this.menuItemShowAdminPage.Click += MenuItemShowAdminPage_Click;

            this.menuItemRestartService = new MenuItem
            {
                Text = "&Restart and Detect"
            };
            this.menuItemRestartService.Click += menuItemRestartService_Click;

            this.menuItemExit = new MenuItem
            {
                Text = "E&xit"
            };
            this.menuItemExit.Click += MenuItemExit_Click;

            this.managerContextMenu = new ContextMenu();
            this.managerContextMenu.MenuItems.AddRange(
                new MenuItem[] {
                    this.menuItemShowConsole,
                    this.menuItemShowAdminPage,
                    new MenuItem("-"),
                    this.menuItemRestartService,
                    new MenuItem("-"),
                    this.menuItemExit
                });

            this.managerNotifyIcon = new NotifyIcon(this.components)
            {
                Icon = new Icon("ErpNet.FP.ico"),
                ContextMenu = this.managerContextMenu,
                Text = "ErpNet.FP.Win.Manager",
                Visible = true,
            };
            managerNotifyIcon.DoubleClick += ManagerNotifyIcon_DoubleClick;

            monitor.OnChange((opt, str) =>
            {
                this.configOptions = opt;
            });

            this.managerNotifyIcon.BalloonTipText = "Starting ErpNet.FP.Server...";
            this.managerNotifyIcon.ShowBalloonTip(3000);
            StartService();
        }

        private void menuItemRestartService_Click(object sender, EventArgs e)
        {
            options.Update(updatedConfigOptions =>
            {
                updatedConfigOptions.AutoDetect = true;
            });
            if (configOptions.AutoDetect)
            {
                this.managerNotifyIcon.BalloonTipText = "Restarting ErpNet.FP.Server...";
                this.managerNotifyIcon.ShowBalloonTip(5000);
                StopService();
                StartService();
            }
        }

        private void MenuItemShowConsole_Click(object sender, EventArgs e)
        {
            this.CenterToScreen();
            this.Show();
        }

        private void MenuItemShowAdminPage_Click(object sender, EventArgs e)
        {
            ProcessStartInfo sInfo = new ProcessStartInfo
            {
                FileName = "http://localhost:8001",
                UseShellExecute = true
            };
            Process.Start(sInfo);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = cancelClose;
            this.Hide();
        }

        protected override void SetVisibleCore(bool value)
        {
            if (!this.IsHandleCreated)
            {
                // Starting form invisible, show notify icon only in tray area
                CreateHandle();
                value = false;
            }
            base.SetVisibleCore(value);
        }

        protected override void Dispose(bool disposing)
        {
            // Clean up any components being used.
            if (disposing)
                if (components != null)
                    components.Dispose();

            base.Dispose(disposing);
        }

        private void ManagerNotifyIcon_DoubleClick(object Sender, EventArgs e)
        {
            // Show the form when the user double clicks on the notify icon.

            // Set the WindowState to normal if the form is minimized.
            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;

            // Activate the form.
            this.Activate();
        }

        private void MenuItemExit_Click(object Sender, EventArgs e)
        {
            this.managerNotifyIcon.BalloonTipText = "Stopping ErpNet.FP.Server...";
            this.managerNotifyIcon.ShowBalloonTip(2000);
            StopService();
            cancelClose = false;
            this.Close();
        }

        private void StopService()
        {
            if (serviceProcess != null)
            {
                serviceProcess.CloseMainWindow();
                Thread.Sleep(2000);
                serviceProcess.Kill();
            }
        }

        private void StartService()
        {
            // Creating the service process
            serviceProcess = new Process();
            serviceProcess.StartInfo.FileName = ServiceFileName;
            serviceProcess.StartInfo.UseShellExecute = false;
            serviceProcess.StartInfo.RedirectStandardOutput = true;
            serviceProcess.StartInfo.RedirectStandardError = true;
            serviceProcess.StartInfo.CreateNoWindow = true;

            // Setting output and error (asynchronous) handlers
            serviceProcess.OutputDataReceived += new DataReceivedEventHandler(OutputHandler);
            serviceProcess.ErrorDataReceived += new DataReceivedEventHandler(OutputHandler);

            // Starting process and handlers
            ThreadStart processThreadStarter = new ThreadStart(() =>
            {
                serviceProcess.Start();
                serviceProcess.BeginOutputReadLine();
                serviceProcess.BeginErrorReadLine();
                serviceProcess.WaitForExit();
            });
            Thread processThread = new Thread(processThreadStarter);
            processThread.Start();
        }

        private void OutputHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            this.BeginInvoke(new MethodInvoker(() =>
            {
                this.logBox.AppendText(outLine.Data ?? string.Empty);
                this.logBox.AppendText(Environment.NewLine);
            }));
        }
    }
}