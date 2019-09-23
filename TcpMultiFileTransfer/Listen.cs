using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TcpMultiFileTransfer
{
    public class Listen
    {
        private static Socket socket;
        private static bool IsStop;
        public delegate void ListViewItemEvent(ListViewItem listViewItem);
        public static ListViewItemEvent AddListViewItem { get; set; }

        public static void Start(int port)
        {
            if (socket != null)
                return;

            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Bind(new IPEndPoint(IPAddress.Any, port));
                socket.Listen(100);
            }
            catch (Exception ex)
            {
                Stop();
                throw ex;
            }

            IsStop = false;

            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        var client = socket.Accept();
                        new Thread(() =>
                        {
                            var fileInfo = ReceiverEngine.GetFileInfo(client);
                            var lvi = listViewItem(fileInfo);
                            AddListViewItem(lvi);
                            fileInfo.LoadData();

                            if (!IsStop)
                                lvi.SubItems[2].Text = "OK";
                        }).Start();
                    }
                    catch (Exception ex)
                    {
                        if (!IsStop)
                            MessageBox.Show(ex.Message);
                        break;
                    }
                }
            }).Start();
        }

        private static ListViewItem listViewItem(ReceiveFileInfo fileInfo)
        {
            var lvi = new ListViewItem();
            lvi.Text = fileInfo.FileName;
            string strSize = fileInfo.FileSize + " KB";
            if (fileInfo.FileSize > 1024)
            {
                strSize = String.Format("{0:0.##}", fileInfo.FileSize / 1024.0) + " MB";
            }
            lvi.SubItems.Add(strSize);
            lvi.SubItems.Add("Receiving...");
            return lvi;
        }
        public static void Stop()
        {
            IsStop = true;
            if (socket != null)
            {
                if (socket.Connected)
                    socket.Disconnect(false);

                socket.Close();
                socket.Dispose();
                socket = null;
            }
        }
    }
}
