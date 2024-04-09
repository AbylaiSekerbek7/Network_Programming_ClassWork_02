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
                // Stop server
            }
        }

        private void OutLog(string msg)
        {
            if (txtJournal.InvokeRequired)
            {
                txtJournal.Invoke(new Action(() => { }));
            }
            else
            {
                txtJournal.Text += msg + "\r\n";
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
                OutLog($"Server is Started by address {form.socServer.LocalEndPoint}");
                form.Invoke(new Action(() =>
                {
                    btnStart.Enabled = true;
                    btnStart.Text = "Stop";
                }));
                while (true)
                {
                    OutLog("Waiting client connection...");
                    Socket client = form.socServer.Accept();
                    OutLog("Client is connected");
                    // Start Thread for clients
                    ThreadPool.QueueUserWorkItem(
                      new WaitCallback((par) =>
                      {
                          ClientRoutine(par, client);
                      }
                    ), this);
                }

            }
            catch (Exception ex)
            {
                OutLog("Error - " + ex.Message);
            }
            finally
            {
                form.Invoke(new Action(() =>
                {
                    btnStart.Enabled = true;
                    btnStart.Text = "Start";
                }));
                if (socServer != null)
                {
                    socServer.Close();
                    socServer = null;
                }
            }
        }

        private void ClientRoutine(object param, Socket client)
        {
            Form1 form = param as Form1;
            byte[] buffer = new byte[4*1024];
            string msg, command, nickName;
            try
            {
                OutLog($"Client : {client.RemoteEndPoint}");
                // Протокол взаимодействия сервера с клиентом:
                
                // 1) ==> "Hello!"
                client.Send(Encoding.UTF8.GetBytes("Hello!"));
                
                // 2) NickName <== from Client
                int size = client.Receive(buffer);
                msg = Encoding.UTF8.GetString(buffer, 0, size);
                nickName = msg;
                OutLog("NickName : " + msg);
                lstClients.Add(new Client{
                    client = client,
                    cntMsg = 0,
                    NickName = msg
                });
                // UpdateClientList(); обновить список клиентов на экране
                
                while(true)
                {
                    // 3 - ждем команду от клиента
                    size = client.Receive(buffer);
                    command = Encoding.UTF8.GetString(buffer, 0, size);
                    OutLog("From client get command: " + command);
                    switch (command)
                    {
                        case "GetUserList":
                            msg = "";
                            foreach (var cl in lstClients)
                            {
                                msg += cl.NickName + ";";
                            }
                            client.Send(Encoding.UTF8.GetBytes(msg));
                            OutLog("Sent a msg: " + msg);
                            break;

                        case "SendAllUsers":
                            size = client.Receive(buffer);
                            msg = Encoding.UTF8.GetString(buffer, 0, size);
                            OutLog(msg);
                            int cnt = 0;
                            foreach (var cl in lstClients)
                            {
                                if (cl.NickName == nickName) continue;
                                //cl.client.Send(Encoding.UTF8.GetBytes(msg));
                                cl.client.Send(buffer, 0, size, SocketFlags.None);
                                cnt++;
                            }
                            client.Send(Encoding.UTF8.GetBytes(cnt.ToString()));
                            OutLog($"Sent {cnt} msgs to clients");
                            break;

                        case "SendUser":
                            break;

                        case "ChangeNickName":
                            break;

                        case "SendFileAll":
                            break;
                    } // switch(command)
                }
            }
            catch (Exception ex)
            {
                OutLog("Error - " + ex.Message);
            }
            finally
            {

            }
        }

        // Список подключенных клиентов 
        public List<Client> lstClients = new List<Client>();
        // Class описания клиента 
        public class Client
        {
            public Socket client; // client socket
            public string NickName; // client name
            public int cntMsg;

            public Client() { }
            public Client(Socket client, string nickName, int cntMsg)
            {
                this.client = client;
                NickName = nickName;
                this.cntMsg = cntMsg;
            }
        }
    }
}