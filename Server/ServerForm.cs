using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Threading;

namespace ChatServer
{
    public partial class ServerForm : Form
    {
        //Biến toàn cục
        TcpListener listener = null;
        Socket sock = null;
        NetworkStream stream = null;
        Boolean isRunning = false;
        string newMsg = "";

        List<Thread> clientHandlerList = new List<Thread>();
        List<StreamReader> readerList = new List<StreamReader>();
        List<NetworkStream> streamList = new List<NetworkStream>();
        List<int> clientID = new List<int>();
        int count = 0;

        // AddTextCallBack
        private delegate void AddTextCallback(string input);

        public ServerForm()
        {
            InitializeComponent();
        }

        //----[Start Server]------
        //Khi nhất nút Start Server
        private void btnStartServer_Click(object sender, EventArgs args)
        {
            // Disable nút connect và textBox
            textBoxip.Enabled = false;
            textBoxport.Enabled = false;
            btnStartServer.Enabled = false;
            buttonStop.Enabled = true;

            isRunning = true;
            AddText("Server started...");

            listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 10000);
            Thread a = new Thread(new ThreadStart(serverHandler));
            a.Start();
        }

        //Thread dùng để lắng nghe kết nối mới từ client
        private void serverHandler()
        {
            while (true)
            {
                try
                {
                    listener.Start();

                    sock = listener.AcceptSocket();
                    AddText("New client connected from: " + sock.RemoteEndPoint.ToString());

                    clientHandlerList.Add(new Thread(new ThreadStart(clientHandler)));
                    clientHandlerList[clientHandlerList.Count - 1].Start();
                }
                catch
                {
                    break;
                }
            }
            AddText("Server stopped.");
        }

        //Xử lí việc lắng nghe tin nhắn từ Client
        private void clientHandler()
        {
            int id = count;
            count++;

            streamList.Add(new NetworkStream(sock));
            readerList.Add(new StreamReader(streamList[id]));

            string output = "";
            while (output != null)
            {
                try
                {
                    output = readerList[id].ReadLine();
                    newMsg = output;
                    if (output == null) break;
                    AddText(output);
                }
                catch
                {
                    break;
                }
            }
            AddText("Client disconnected!");
        }

        //----[Stop Server]------
        //Khi nhất nút Stop Server
        private void buttonStop_Click(object sender, EventArgs e)
        {
            // Enable nút connect và textBox
            textBoxip.Enabled = true;
            textBoxport.Enabled = true;
            btnStartServer.Enabled = true;
            buttonStop.Enabled = false;

            sock = null;
            listener.Stop();

            isRunning = false;
            // Đóng tất cả các Stream và ngừng listener
            for (int i = 0; i < streamList.Count; i++)
            {
                streamList[i].Dispose();
                streamList[i].Close();
            }
        }

        //Hàm xử lí việc gọi listView từ Thread khác
        private void AddText(string text)
        {
            // InvokeRequired required compares the thread ID of the 
            // calling thread to the thread ID of the creating thread. 
            // If these threads are different, it returns true. 
            if (this.richTextBox1.InvokeRequired)
            {
                AddTextCallback d = new AddTextCallback(AddText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.richTextBox1.AppendText(text + '\n');
            }
        }

        //Nếu có nội dung mới từ Client, richTextBox sẽ cập nhật nội dung
        //Sau đó gửi lại 
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < streamList.Count; i++)
            {
                try
                {
                    StreamWriter sw = new StreamWriter(streamList[i]);
                    sw.WriteLine(newMsg);
                    sw.Flush();
                }
                catch
                {
                    continue;
                }
            }    
        }
    }
}
