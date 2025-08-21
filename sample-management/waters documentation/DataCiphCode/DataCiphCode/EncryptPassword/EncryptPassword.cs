#region Copyright (C) 2020, DataCiph AB | All rights reserved.

//=================================================================================
//   Product:    STF
//   Project:    EncryptPassword
//        NS:    EncryptPassword
//   Version:    0.1.1.1                                                                
//   Company:    DataCiph AB                                                             
//      File:    EncryptPassword.cs                                                          
//    Author:    Vedran Jašarević |                                        
//   Created:    23.11.2020 1:45 PM
//   Updated:    1.12.2020 12:59 PM                                                        
// Copyright:    DataCiph AB  | All rights reserved.    
//   Contact:    vedran.jasarevic@dataciph.com                                  
//==================================================================================

#endregion
using CommonSTF.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;


namespace EncryptPassword
{
    public partial class EncryptPassword : Form
    {
        private System.Timers.Timer aTimer;

        public EncryptPassword()
        {
            InitializeComponent();
        }

        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            txtPasswordEncrypted.Text = string.Empty;
            if (string.IsNullOrEmpty(txtPasswordPlain.Text))
            {
                MessageBox.Show("Please enter password!", "Alert", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            else
            { 
                txtPasswordEncrypted.Text = Crypto.EncryptStringAES(txtPasswordPlain.Text, Crypto.SharedSecret);
                System.Windows.Forms.Clipboard.SetText(txtPasswordEncrypted.Text);
                SetTimer();
            }
        }
        private void SetTimer()
        {
            lblMessage.Visible = true;
            // Create a timer with a 3 second interval.
            aTimer = new System.Timers.Timer(3000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = false;
            aTimer.Enabled = true;
        }

        private delegate void InvokeDelegate();

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            // To avoid error use delegate (Cross-thread operation not valid: Control 'lblMessage' 
            // accessed from a thread other than the thread it was created on).
            lblMessage.BeginInvoke(new InvokeDelegate(InvokeMethod));
        }

        public void InvokeMethod()
        {
            lblMessage.Visible = false;
        }
    }
}
