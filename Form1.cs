using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MapNetDrive
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public CmdHelp Cmd;

        private void Form1_Load(object sender, EventArgs e)
        {
            this.labelConnecting.Visible = false;

            this.Cmd = new CmdHelp();

            // check map net info list from mapinfo.txt
            if (this.Cmd.MapInfoList.Count <= 0)
            {
                MessageBox.Show("No Department in MapInfo.txt");
                return;
            }

            // SelectBox add mapinfo list
            foreach (var s in this.Cmd.MapInfoList)
            {
                this.selectBoxDept.Items.Add(s.Department);
            }

            if (this.selectBoxDept.Items.Count > 0)
            {
                this.selectBoxDept.SelectedItem = this.selectBoxDept.Items[0];
            }
        }

        /// <summary>
        /// Set the SelectedMapInfo when the selectBox was changed
        /// </summary>
        private void selectBoxDept_SelectedIndexChanged(object sender, EventArgs e)
        {
            var depart = this.selectBoxDept.SelectedItem.ToString();
            foreach (var m in this.Cmd.MapInfoList)
            {
                if (depart == m.Department)
                {
                    this.Cmd.SelectedMapInfo = m;
                }
            }
        }


        private void FormLoading()
        {
            this.button1.Enabled = false;
            this.labelConnecting.Visible = true;
        }

        private void FormLoadingDone()
        {
            this.button1.Enabled = true;
            this.labelConnecting.Visible = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.BeginInvoke((Action)delegate() {
                ConnectNetDriveTask();              
            });
        }

        private void ConnectNetDriveTask()
        {
            // 開始執行鏈接 cmd
            Task.Factory.StartNew(() => {

                // UserName and Password
                var name = this.textBoxName.Text.Trim();
                var pass = this.textBoxPassword.Text.Trim();


                if (string.IsNullOrEmpty(name))
                {
                    MessageBox.Show("UserName Null.");
                    return;
                }

                // 鎖 button1 ，顯示 Connecting... lable
                this.BeginInvoke((Action)delegate {
                    FormLoading();
                });

                // Net Use drive: /d /y
                this.Cmd.RunNetUseDeleteCmd();
                Thread.Sleep(2000);

                // execute "Net Use drive: \\sharepath " cmd, return error message if it has.
                var result = this.Cmd.RunNetUseCmd(name, pass).Trim();

                if (!string.IsNullOrEmpty(result))
                {
                    MessageBox.Show(result);
                }
                else
                {
                    // open the net drive explorer
                    this.Cmd.RunOpenDrive();
                    Thread.Sleep(2000);

                    // colse the winform
                    this.Invoke((Action)delegate { this.Close(); });
                }

                // 開翻 button1 ， Connecting... hidden
                this.BeginInvoke((Action)delegate {
                    FormLoadingDone();
                });
            });            
        }


        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.BeginInvoke((Action)delegate () {
                    ConnectNetDriveTask();
                });
            }
        }
    }
}
