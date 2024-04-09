using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Net.WebSockets;

namespace ClassWork_02
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            txtPort.Text = "12345"; // port by default 12345
            // client list 
            lvClients.View = View.Details; // view by table
            lvClients.Columns.Clear();
            lvClients.Columns.Add("N");
            lvClients.Columns.Add("NickName");
            lvClients.Columns.Add("Remote IP:Port");
            lvClients.Columns.Add("Message count");
            // IP-address server
            string ThisHostName = Dns.GetHostName();
            this.Text = "Server programm : " + ThisHostName;
            IPAddress[] ipAddr = Dns.GetHostAddresses(ThisHostName);

            cbIPAddr.Items.Add(IPAddress.Any.ToString()); // ==> cbIPAddr.Items.Add("0.0.0.0");
            foreach (IPAddress ip in ipAddr)
            {
                cbIPAddr.Items.Add(ip.ToString());
            }
            cbIPAddr.SelectedIndex = 0;
        }

        private Thread thServer = null;
        private Socket socServer = null;
        private IPAddress ipServer = null;
        private int portServer = 0;

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (btnStart.Text == "Start")
            {
                try
                {
                    ipServer = IPAddress.Parse(cbIPAddr.Text);
                    portServer = int.Parse(txtPort.Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error - " + ex.Message, "Error");
                    return;
                }
                btnStart.Enabled = false;
                thServer = new Thread(ServerRoutine);
                thServer.IsBackground = true;
                thServer.Start(this);
                btnStart.Text = "Stop";
            }
            else
            {
                btnStart.Text = "Start";
            }
        }

        private void ServerRoutine(object param)
        {
            Form1 form = param as Form1;
            try
            {
                form.socServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipep = new IPEndPoint(form.ipServer, form.portServer);
                form.socServer.Bind(ipep);
                form.socServer.Listen(100);
                while (true)
                {
                    Socket client = form.socServer.Accept();
                    // Start Thread for clients
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error - " + ex.Message, "Error");
                return;
            }
            finally
            {

            }
        }
    }
}
