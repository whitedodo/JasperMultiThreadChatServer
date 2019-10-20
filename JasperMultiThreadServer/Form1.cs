using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JasperMultiThreadServer
{
    public partial class Form1 : Form
    {
        int state;
        Thread thConnect;
        TcpClient thClientSocket;
        TcpListener serverSocket;
        TcpClient clientSocket;
        string clNo;
        int port;

        public Form1()
        {
            InitializeComponent();
            init();
        }

        public void init()
        {
            this.port = 8888;
            state = 0;
            txtLog.Text = DateTime.Now.ToString() + " 환영합니다." ;
            serverSocket = null;
            clientSocket = null;
        }

        [Obsolete]
        private void btnOnOff_Click(object sender, EventArgs e)
        {
            if (state == 0)
            {
                thConnect = new Thread(serviceOn);
                thConnect.Start();
                state = 1;
                btnOnOff.Text = "종료(Stop)";
            }
            else
            {
                if( serverSocket != null )
                    serverSocket = null;

                if( clientSocket != null )
                    clientSocket.Close();

                if ( thClientSocket != null )
                    thClientSocket.Close();


                MessageBox.Show("서버를 종료합니다.(Shutdown the server.)", "메시지(Messages)",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                Application.ExitThread();
                Environment.Exit(0);

                thConnect.Abort();
                state = 0;
                btnOnOff.Text = "시작(Start)";
            }
            
        }

        [Obsolete]
        public void serviceOn()
        {
            serverSocket = new TcpListener(port);
            clientSocket = default(TcpClient);
            int counter;

            try
            {
                serverSocket.Start();   // 서버 시작
            }
            catch(Exception ex)
            {
                this.Invoke(new EventHandler(delegate {
                    txtLog.Text = txtLog.Text + Environment.NewLine +
                                  ex.ToString();
                }));
            }

            counter = 0;

            this.Invoke(new EventHandler(delegate {
                txtLog.Text = txtLog.Text + Environment.NewLine +
                              ">> 서버 시작되었음.(Server Started)";
            }));

            while (true)
            {
                counter += 1;
                clientSocket = serverSocket.AcceptTcpClient();

                this.Invoke(new EventHandler(delegate {
                    txtLog.Text = txtLog.Text +
                              Environment.NewLine +
                              ">> 클라이언트 번호(Client No):" +
                              Convert.ToString(counter);
                }));
              

                startClient(clientSocket, Convert.ToString(counter));

            }

            clientSocket.Close();
            serverSocket.Stop();

            this.Invoke(new EventHandler(delegate {
                txtLog.Text = txtLog.Text + Environment.NewLine + ">> 서버 탈출(Server Exit)";
            }));


        }

        public void startClient(TcpClient thClientSocket, string clineNo)
        {
            this.thClientSocket = thClientSocket;
            this.clNo = clineNo;
            Thread ctThread = new Thread(doChat);
            ctThread.Start();
        }

        private void doChat()
        {
            int requestCount = 0;
            byte[] bytesFrom = new byte[65536];
            string dataFromClient = null;
            string dataFromClientIP = null;
            string dataFromClientNickname = null;
            string dataFromClientMsg = null;
            Byte[] sendBytes = null;
            string serverResponse = null;
            string rCount = null;
            requestCount = 0;
            NetworkStream networkStream = null;

            while (true)
            {
                requestCount = requestCount + 1;

                try
                {
                    
                    networkStream = thClientSocket.GetStream();
                    networkStream.Read(bytesFrom, 0, (int)thClientSocket.ReceiveBufferSize);
                    
                    dataFromClient = System.Text.Encoding.UTF8.GetString(bytesFrom);
                    dataFromClientIP = dataFromClient.Substring(0, dataFromClient.IndexOf(";"));
                    dataFromClientNickname = dataFromClient.Replace(dataFromClientIP + ";", "");
                    dataFromClientNickname = dataFromClientNickname.Substring(0, dataFromClientNickname.IndexOf(";"));
                    dataFromClientMsg = dataFromClient.Replace(dataFromClientIP + ";", "");
                    dataFromClientMsg = dataFromClientMsg.Replace(dataFromClientNickname + ";", "");

                    dataFromClientMsg = dataFromClientMsg.Substring(0, dataFromClientMsg.IndexOf("\0"));

                    /*
                    this.Invoke(new EventHandler(delegate {
                        txtLog.Text = txtLog.Text +
                                  Environment.NewLine +
                                  ">> 클라이언트로 부터(From Client):" +
                                  clNo + " " + dataFromClientMsg;
                    }));
                    */
                    this.Invoke(new EventHandler(delegate {
                        txtLog.Text = txtLog.Text +
                                  Environment.NewLine +
                                  ">> :" + clNo + " " + dataFromClientMsg;
                    }));


                    rCount = Convert.ToString(requestCount);
                    serverResponse = "서버 to 클라이언트(Server to client) (" + clNo + ") " + rCount;
                    //sendBytes = Encoding.UTF8.GetBytes(serverResponse);

                    sendBytes = Encoding.UTF8.GetBytes(txtLog.Text);

                    networkStream.Write(sendBytes, 0, sendBytes.Length);
                    networkStream.FlushAsync();

                    /*
                    this.Invoke(new EventHandler(delegate {
                        txtLog.Text = txtLog.Text + Environment.NewLine +
                                  ">>" + serverResponse;
                    }));
                    */

                }
                catch (Exception ex)
                {
                    //this.Invoke(new EventHandler(delegate {
                    //    txtLog.Text = txtLog.Text + Environment.NewLine + ">>" + ex.ToString();
                    //}));

                    //requestCount = requestCount - 1;
                }

            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            MessageBox.Show("서버를 종료합니다.(Shutdown the server.)", "메시지(Messages)", 
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
            Application.ExitThread();
            Environment.Exit(0);
        }
    }


}
