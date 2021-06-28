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

        public Form1()
        {
            InitializeComponent();

            MouseDown += (o, e) => { if (e.Button == MouseButtons.Left) { On = true; Pos = e.Location; } };
            MouseMove += (o, e) => { if (On) Location = new Point(Location.X + (e.X - Pos.X), Location.Y + (e.Y - Pos.Y)); };
            MouseUp += (o, e) => { if (e.Button == MouseButtons.Left) { On = false; Pos = e.Location; } };

            this.BatteryChange();

            this.MonitorTimer.Interval = 10000; // 10초에 한번씩 상태를 체크
            this.MonitorTimer.Tick += new EventHandler(MonitorTimer_Tick);
            this.MonitorTimer.Start();
        }

        void MonitorTimer_Tick(object sender, EventArgs e) 
        { 
            this.BatteryChange();
        }

        public void BatteryChange()
        {
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
