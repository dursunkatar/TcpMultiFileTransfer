using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TcpMultiFileTransfer
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
            
            init();
        }

        private void init()
        {
            CheckForIllegalCrossThreadCalls = false;

            Listen.AddListViewItem = addlistViewReceiverItem;
            Sender.AddListViewItem = addlistViewSenderItem;

            if (!Directory.Exists("Downloads"))
            {
                Directory.CreateDirectory("Downloads");
            }
        }

        private void addlistViewReceiverItem(ListViewItem lvi)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    listViewReceiver.Items.Insert(0, lvi);
                });
            }
        }

        private void addlistViewSenderItem(ListViewItem lvi)
        {
            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    listViewSender.Items.Insert(0, lvi);
                });
            }
        }

        private void StatusStopped()
        {
            foreach (ListViewItem item in listViewReceiver.Items)
            {
                if (item.SubItems[2].Text == "Receiving...")
                {
                    item.SubItems[2].Text = "Stopped";
                }
            }
        }

        private void btnWaitForFiles_Click(object sender, EventArgs e)
        {
            if (btnWaitForFiles.Text == "Wait For Files")
            {
                try
                {
                    Listen.Start(int.Parse(txtPort.Text));
                    btnWaitForFiles.Text = "Cancel Waiting";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                Listen.Stop();
                btnWaitForFiles.Text = "Wait For Files";
                StatusStopped();
            }
        }

        private void btnSendFiles_Click(object sender, EventArgs e)
        {
            IPAddress ipAddr = IPAddress.Parse(txtIpAddress.Text);
            int port = int.Parse(txtPortConnect.Text);

            var file = new OpenFileDialog();
            file.Multiselect = true;
            file.Title = "Select Files";

            if (file.ShowDialog() == DialogResult.OK)
            {
                foreach (string fileName in file.FileNames)
                {
                    new Thread(() =>
                    {
                        Sender.Send(ipAddr, port, fileName);

                    }).Start();
                    Thread.Sleep(200);
                }
            }
            file.Dispose();
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Listen.Stop();
            Sender.Close();
        }
    }
}
