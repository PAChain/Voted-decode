using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace VotedDecode
{
    public partial class FrmMain : Form
    {
        private DecodeProcess _DecodeProcess = new DecodeProcess();

        public FrmMain()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void EnableForm(bool enable)
        {
            txtServer.Enabled = enable;
            txtToken.Enabled = enable;
            txtKey.Enabled = enable;
            btnLoadToken.Enabled = enable;
            btnLoadKey.Enabled = enable;
            btnSaveKey.Enabled = enable;
            btnGenerateKey.Enabled = enable;
            btnDecode.Enabled = enable;
            btnList.Enabled = enable;
            numInterval.Enabled = enable;
            dtpEnd.Enabled = enable;
        }

        private void btnLoadToken_Click(object sender, EventArgs e)
        {
            OFD.FileName = txtToken.Text;
            if (OFD.ShowDialog() == DialogResult.OK)
            {
                txtToken.Text = File.ReadAllText(OFD.FileName);
            }
        }
        private void btnSaveToken_Click(object sender, EventArgs e)
        {
            SFD.Filter = "*.txt|*.txt";
            if (SFD.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(SFD.FileName, txtToken.Text, Encoding.UTF8);
            }
        }
        private void btnLoadKey_Click(object sender, EventArgs e)
        {
            OFD.FileName = txtKey.Text;
            if (OFD.ShowDialog() == DialogResult.OK)
            {
                String key = File.ReadAllText(OFD.FileName);
                key = Regex.Replace(key, "[-]{2,}[^-]*[-]{2,}", "");
                txtKey.Text = key.Replace("\n", "").Replace("\r", "").Trim();
            }
        }

        private void btnSaveKey_Click(object sender, EventArgs e)
        {
            SFD.Filter = "*.txt|*.txt";
            if (SFD.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(SFD.FileName, txtKey.Text, Encoding.UTF8);
            }
        }

        private void btnGenerateKey_Click(object sender, EventArgs e)
        {
            _DecodeProcess.Server = txtServer.Text.Trim();
            _DecodeProcess.Token = txtToken.Text.Trim();
            string token = "";
            string key = "";
            if (_DecodeProcess.GetNewKey(out token, out key))
            {
                txtToken.Text = token;
                txtKey.Text = key;
                MessageBox.Show("Generate key Success");
            }
            else
            {
                MessageBox.Show("Generate key failed");
            }
        }

        private void btnDecode_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtServer.Text.Trim()))
            {
                MessageBox.Show("Missing Server");
                return;
            }
            if (string.IsNullOrEmpty(txtToken.Text.Trim()))
            {
                MessageBox.Show("Missing Token");
                return;
            }
            if (string.IsNullOrEmpty(txtKey.Text.Trim()))
            {
                MessageBox.Show("Missing Key");
                return;
            }
            _DecodeProcess.Server = txtServer.Text.Trim();
            _DecodeProcess.Token = txtToken.Text.Trim();
            _DecodeProcess.Key = txtKey.Text.Trim();
            _DecodeProcess.Start();
            EnableForm(false);
            timerStatus.Enabled = true;
        }

        private void btnList_Click(object sender, EventArgs e)
        {
            _DecodeProcess.Server = txtServer.Text.Trim();
            labStatus.Text = _DecodeProcess.GetList(txtToken.Text.Trim(), txtKey.Text.Trim());
            MessageBox.Show("List Success");
        }

        private void timerStatus_Tick(object sender, EventArgs e)
        {
            timerStatus.Enabled = false;
            try
            {
                labStatus.Text = _DecodeProcess.Status.Replace("&", "&&");
                if (_DecodeProcess.Completed)
                {
                    labStatus.Text = _DecodeProcess.Status.Replace("&", "&&");
                    if (numInterval.Value > 0 && (!dtpEnd.Checked ||  DateTime.Now<dtpEnd.Value))
                    {
                        timerInterval.Interval = (int)numInterval.Value * 60000;
                        timerInterval.Enabled = true;
                        return;
                    }
                    else
                    {
                        EnableForm(true);
                        return;
                    }
                }
            }
            catch { }
            timerStatus.Enabled = true;
        }

        private void timerInterval_Tick(object sender, EventArgs e)
        {
            timerInterval.Enabled = false;
            btnDecode_Click(null, null);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            numInterval.Value = 0;
        }

    }
}
