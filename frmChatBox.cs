using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Text_Chat_IP
{
    public partial class frmChatBox : Form
    {
        Socket sck;
        IPEndPoint epMe;
        EndPoint epFr;
        byte[] buffer_data;
        public frmChatBox()
        {
            InitializeComponent();
        }

        private void frmChatBox_Load(object sender, EventArgs e)
        {
            InitializeChatBox();
        }
        #region Initialize Data

        private void InitializeChatBox()
        {
            //initialize socket
            sck = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sck.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            //get Self IP
            txtMyIP.Text = getMyIP();
            txtMyPort.Text = "1001";
            txtFriendPort.Text = "1001";
        }

        private string getMyIP()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            return "127.0.0.1";
        }

        #endregion

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {
                // binding with socket and port
                epMe = new IPEndPoint(IPAddress.Parse(txtMyIP.Text),
                Convert.ToInt32(txtMyPort.Text));
                sck.Bind(epMe);
                // connect to friend IP and port
                epFr = new IPEndPoint(IPAddress.Parse(txtFriendIP.Text),
                Convert.ToInt32(txtFriendPort.Text));
                sck.Connect(epFr);
                // connected to listen to an specified port
                buffer_data = new byte[1500];
                sck.BeginReceiveFrom(buffer_data, 0, buffer_data.Length, SocketFlags.None, ref epFr, new
                AsyncCallback(MessageCallBack), buffer_data);
                // enable send button to send message
                btnSend.Enabled = true;
                btnConnect.Text = "Connected";
                btnConnect.Enabled = false;
                txtChat.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void MessageCallBack(IAsyncResult ar)
        {
            try
            {
                int size = sck.EndReceiveFrom(ar, ref epFr);
                // check if there any information
                if (size > 0)
                {
                    // getting the data
                    byte[] receivedData = new byte[1464];
                    // finding the message data
                    receivedData = (byte[])ar.AsyncState;
                    // converts message data byte array to string
                    ASCIIEncoding eEncoding = new ASCIIEncoding();
                    string receivedMessage = eEncoding.GetString(receivedData);
                    // adding Message to the rich text box
                    Add_Chat_Log(receivedMessage, "Friend: ");
                }
                // connect to listen the socket again
                buffer_data = new byte[1500];
                sck.BeginReceiveFrom(buffer_data, 0, buffer_data.Length, SocketFlags.None, ref epFr, new AsyncCallback(MessageCallBack), buffer_data);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
            }
        }


        private void Add_Chat_Log(string Messages, string who)
        {
            txtChatLog.AppendText(who + Messages + " \n");
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                // encoding string to byte[]
                System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
                byte[] msg = new byte[1500];
                msg = enc.GetBytes(txtChat.Text);
                // sending the chat
                sck.Send(msg);
                // add to chatbox
                Add_Chat_Log(txtChat.Text, "Me: ");
                // clear chatbox
                txtChat.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
