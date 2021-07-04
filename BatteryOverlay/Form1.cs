using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BatteryOverlay
{
    public partial class Form1 : Form
    {
        bool On;
        Point Pos;
        private Timer MonitorTimer = new Timer();
        private int beforeBattery = 0;
        private bool chargeSuccess = false;
        private bool chargeError = false;
        private bool isCharging = false;

        public Form1()
        {
            InitializeComponent();

            MouseDown += (o, e) => { if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right) { On = true; Pos = e.Location; } };
            MouseMove += (o, e) => { if (On) Location = new Point(Location.X + (e.X - Pos.X), Location.Y + (e.Y - Pos.Y)); };
            MouseUp += (o, e) => { if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right) { On = false; Pos = e.Location; } };

            this.BatteryChange();

            this.MonitorTimer.Interval = 1000; // 10초에 한번씩 상태를 체크
            this.MonitorTimer.Tick += new EventHandler(MonitorTimer_Tick);
            this.MonitorTimer.Start();
        }

        void MonitorTimer_Tick(object sender, EventArgs e) 
        { 
            this.BatteryChange();
        }

        public void checkCharge()
        {
            ObjectQuery query = new ObjectQuery("Select * FROM Win32_Battery");
            PowerStatus pwr = SystemInformation.PowerStatus;

            switch (pwr.PowerLineStatus)
            {
                case (PowerLineStatus.Offline):
                    this.beforeBattery = 0;
                    this.chargeSuccess = false;
                    this.chargeError = false;
                    this.isCharging = false;
                    break;

                case (PowerLineStatus.Online):
                    this.isCharging = true;
                    if (this.chargeSuccess) break;
                    if (this.beforeBattery <= 0)
                    {
                        this.beforeBattery = 0;
                        foreach (ManagementObject o in new ManagementObjectSearcher(query).Get())
                        {
                            this.beforeBattery += Convert.ToInt32(o.Properties["EstimatedChargeRemaining"].Value);
                        }
                    }
                    else
                    {
                        int curr_batt = 0;
                        foreach (ManagementObject o in new ManagementObjectSearcher(query).Get())
                        {
                            curr_batt += Convert.ToInt32(o.Properties["EstimatedChargeRemaining"].Value);
                        }

                        if (curr_batt < this.beforeBattery)
                        {
                            // Charging Error!
                            this.chargeError = true;
                        }
                        else if (curr_batt > this.beforeBattery)
                        {
                            this.chargeSuccess = true;
                        }
                    }
                    break;
            }
        }

        public void BatteryChange()
        {
            this.checkCharge();
            if (this.isCharging)
            {
                if (this.chargeError)
                {
                    this.BackColor = Color.FromArgb(245, 222, 179);
                }
                else if(this.chargeSuccess)
                {
                    this.BackColor = Color.FromArgb(0, 250, 154);
                }
                else
                {
                    this.BackColor = Color.FromArgb(175, 238, 238);
                }
            }
            else
            {
                this.BackColor = Color.FromArgb(240, 255, 255);
            }

            String fore_text = "";
            ObjectQuery query = new ObjectQuery("Select * FROM Win32_Battery");

            foreach (ManagementObject o in new ManagementObjectSearcher(query).Get())
            {
                //uint level = (uint)o.Properties["EstimatedChargeRemaining"].Value;
                fore_text += o.Properties["EstimatedChargeRemaining"].Value + "%  ";
            }

            fore_text += DateTime.Now.ToString("HH:mm");

            this.battery_status.Text = fore_text;
        }
    }
}
