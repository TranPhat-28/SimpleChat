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

namespace ChatClient
{
    public partial class ClientForm : Form
    {
        string displayname = "";
        TcpClient client = null;
        NetworkStream stream = null;
        StreamWriter sw = null;

        //AddTextCallBack
        private delegate void AddTextCallback(string input);

        public ClientForm()
        {
            InitializeComponent();
            displayname = textBoxdisplayname.Text;
        }

        // ------------[Connect]------------------
        // Nhấn nút Connect tạo kết nối đến Server
        private void btnConnect_Click(object sender, EventArgs e)
        {
            // Xóa richTextBox
            richTextBox1.Clear();

            // Disable textBox ip port và nút connect, nút apply
            textBoxip.Enabled = false;
            textBoxport.Enabled = false;
            btnConnect.Enabled = false;
            buttonDis.Enabled = true;
            textBoxdisplayname.Enabled = false;
            buttonSetDisplayName.Enabled = false;

            AddText("Connected to server");

            client = new TcpClient();
            client.Connect(IPAddress.Parse(textBoxip.Text), int.Parse(textBoxport.Text));
            stream = client.GetStream();
            sw = new StreamWriter(stream);

            Thread l = new Thread(new ThreadStart(listen));
            l.Start();
        }

        //--------[Send]-----------
        //Nhấn nút Send để gửi tin
        private void btnSend_Click(object sender, EventArgs e)
        {
            string msg = displayname + ": " + textBox1.Text;
            sw.WriteLine(msg);
            sw.Flush();
            textBox1.Clear();
        }

        //Hàm xử lí việc gọi richTextBox từ Thread khác
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

        // Ngắt kết nối
        private void buttonDis_Click(object sender, EventArgs e)
        {
            // Enable textBox ip port và nút connect, nút apply
            textBoxip.Enabled = true;
            textBoxport.Enabled = true;
            btnConnect.Enabled = true;
            buttonDis.Enabled = false;
            textBoxdisplayname.Enabled = true;
            buttonSetDisplayName.Enabled = true;

            // Hủy stream và đóng client
            stream.Close();
            client.Close();
        }

        // Hàm nghe phản hồi lại từ Server
        private void listen()
        {
            StreamReader sr = new StreamReader(stream);
            string output = "";
            while (output != null)
            {
                try
                {
                    output = sr.ReadLine();
                    if (output == null) break;
                    AddText(output);
                }
                catch
                {
                    break;
                }
            }
            AddText("Disconnected from Server!");
        }

        // Đổi tên hiển thị
        private void buttonSetDisplayName_Click(object sender, EventArgs e)
        {
            displayname = textBoxdisplayname.Text;
            MessageBox.Show("Display name changed!", "Success");
        }
    }
}
