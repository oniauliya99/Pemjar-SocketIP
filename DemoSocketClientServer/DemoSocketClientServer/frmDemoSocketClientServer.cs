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

namespace DemoSocketClientServer
{
	public partial class frmDemoSocketClientServer : Form
	{
		private void tabPage1_Click(object sender, EventArgs e)
		{

		}

		private void frmDemoSocketClientServer_Load(object sender, EventArgs e)
		{

		}
		public frmDemoSocketClientServer()
		{
			InitializeComponent();
		}

		private void btnListenClick(object sender, EventArgs e)
		{
			Thread thr = new Thread(listenSocket);
			if (txtPortServer.Text != "" && txtIPServer.Text != "")
			{
				thr.Start();
				btnListen.Enabled = false;
			}
			else
			{
				MessageBox.Show("Lengkapi port dan alamat IP terlebih dahulu !!!");
			}
		}
		private void listenSocket()
		{
			varGlobal.port = Int16.Parse(txtPortServer.Text);
			varGlobal.alamatIPServer = txtIPServer.Text;
			SocketTCP.StartListening();
		}
		private void btnKirimDataKeServer_Click(object sender, EventArgs e)
		{
			varGlobal.alamatIPServer = txtIPTujuan.Text;
			varGlobal.port = Int16.Parse(txtPortClient.Text);
			SocketTCP.StartClient(txtKirimData.Text);
		}
		private void timer1_Tick(object sender, EventArgs e)
		{
			txtTerimaData.Clear();
			txtTerimaData.Text = varGlobal.terimapesandiserver;
		}

	}
}
