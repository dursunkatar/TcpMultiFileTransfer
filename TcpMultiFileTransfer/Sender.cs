using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TcpMultiFileTransfer
{
    public class Sender
    {
        private static readonly byte[] WAIT = new byte[] { 0 };
        public delegate void ListViewItemEvent(ListViewItem listViewItem);
        public static ListViewItemEvent AddListViewItem { get; set; }
        private static readonly List<Socket> sockets = new List<Socket>();

        public static void Send(IPAddress iPAddress, int port, string filePath)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            sockets.Add(socket);
            var localEndPoint = new IPEndPoint(iPAddress, port);
            try
            {
                socket.Connect(localEndPoint);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            var fileInfo = new FileInfo(filePath);
            byte[] bytesFileInfo = Encoding.UTF8.GetBytes(fileInfo.Name + "|" + fileInfo.Length);

            var lvi = listViewItem(fileInfo);
            AddListViewItem(lvi);

            socket.Send(bytesFileInfo);
            socket.Receive(WAIT);

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            int bytesRead;
            byte[] buffer = new byte[332800]; // 325 KB
            try
            {
                using (var networkStream = new NetworkStream(socket))
                using (var binaryWriter = new BinaryWriter(networkStream))
                using (var read = File.OpenRead(filePath))
                {
                    while ((bytesRead = read.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        binaryWriter.Write(buffer, 0, bytesRead);
                        binaryWriter.Flush();
                    }

                    stopwatch.Stop();
                    lvi.SubItems[3].Text = totalTime(stopwatch);
                    lvi.SubItems[2].Text = "OK";
                }
            }
            catch
            {
                lvi.SubItems[2].Text = "Failed!";
            }
            finally
            {
                try
                {
                    if (socket.Connected)
                        socket.Disconnect(false);
                    socket.Close();
                    socket.Dispose();
                    socket = null;
                }
                catch { }
            }
        }
        private static string totalTime(Stopwatch stopwatch)
        {
            double totalT = stopwatch.ElapsedMilliseconds / 1000.0;
            return totalT > 60 ? totalT / 60.0 + " min" : totalT + " sec";
        }
        private static ListViewItem listViewItem(FileInfo fileInfo)
        {
            var lvi = new ListViewItem();
            lvi.Text = fileInfo.Name;
            string strSize = String.Format("{0:0.##}", fileInfo.Length / 1024.0) + " KB";
            if (fileInfo.Length / 1024.0 > 1024)
            {
                strSize = String.Format("{0:0.##}", fileInfo.Length / 1024.0 / 1024.0) + " MB";
            }
            lvi.SubItems.Add(strSize);
            lvi.SubItems.Add("Sending...");
            lvi.SubItems.Add("");
            return lvi;
        }

        public static void Close()
        {
            foreach (Socket item in sockets)
            {
                if (item != null)
                {
                    try
                    {
                        if (item.Connected)
                            item.Disconnect(false);

                        item.Close();
                        item.Dispose();
                    }
                    catch { }
                }
            }
        }
    }
}
