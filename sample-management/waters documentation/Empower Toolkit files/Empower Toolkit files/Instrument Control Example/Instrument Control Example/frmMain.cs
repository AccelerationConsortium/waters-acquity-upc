using MillenniumToolkit;
using System;
using System.Windows.Forms;
using System.Threading;

namespace Waters.Empower.InstrumentControl.Example
{
	public partial class FrmMain
	{
		private Project _project = new Project();

		private Instrument _instrument;
		
		public FrmMain()
		{
			InitializeComponent();	
		}
		
		public void FrmMain_Load(System.Object sender, System.EventArgs e)
		{
			// TODO values are hard coded ....
			string username = "system";
			string pswd = "manager";
			string project = "Defaults";
			string db = "empower";

			// Perform the login.
			try
			{
				LogIntoToolkit(username, pswd, db, project);
				MessageBox.Show("Login Complete", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);

				// Info can be posted to the Empower message center also
				_project.MessageCenter($"{this.Text} Login Complete");

				_instrument = new Instrument();
				LoadSystemsForDisplay();
				LoadNodesForDisplay();
				LoadSampleSetMethodsForDisplay();
			}
			catch (System.Runtime.InteropServices.COMException ex)
			{
				MessageBox.Show($"Error: {_project.TkErrorDescription(ex.ErrorCode)}");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
		private void FrmMain_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (_instrument != null)
			{
				// when the form closes disconnect from the instrument
				_instrument.Disconnect();
			}
		}

        /// <summary>
        /// Log into the Empower Toolkit
        /// </summary>
        /// <param name="username">Empower username</param>
        /// <param name="pswd">Empower password</param>
        /// <param name="database">Empower database</param>
        /// <param name="project">Empower project</param>
        private void LogIntoToolkit(string username, string pswd, string database, string project)
        {
            _project = new Project();
            _project.Login(database, project, username, pswd);
        }

		/// <summary>
		/// Loads the Systems ComboBox
		/// </summary>
		private void LoadSystemsForDisplay()
        {
            cbSystem.Items.Clear();

            object obj = _instrument.Systems;
            if (obj is System.DBNull)
            {
                MessageBox.Show("There are no systems available", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                string[] systems = (string[])obj;
                cbSystem.Items.AddRange(systems);
            }
        }

		/// <summary>
		/// Load the Nodes ComboBox
		/// </summary>
		private void LoadNodesForDisplay()
        {
            cbNode.Items.Clear();

            // Obtain the available nodes/acq servers
            object obj = _instrument.AcqServers;
            if (obj is System.DBNull)
            {
                MessageBox.Show("There are no nodes available", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                string[] nodes = (string[])obj;
                cbNode.Items.AddRange(nodes);
            }
        }

        /// <summary>
        /// Loads the sample set methods ComboBox
        /// </summary>
        private void LoadSampleSetMethodsForDisplay()
        {
            cbSampleSetMethod.Items.Clear();

            SampleSetMethod ssm = new SampleSetMethod();

            object obj = ssm.SampleSetMethodNames;
            if (obj is System.DBNull)
            {
                MessageBox.Show("There are no sample set methods available", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                string[] nodes = (string[])obj;
                cbSampleSetMethod.Items.AddRange(nodes);
            }
        }

		/// <summary>
		/// Connect to the selected node and system
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void BtnConnect_Click(object sender, EventArgs e)
		{
			try
			{
				string nodeName = (string)cbNode.SelectedItem;
				string systemName = (string)cbSystem.SelectedItem;

				_instrument.Connect(nodeName, systemName);

				// while the connectionstatus says the connection is not 'Done' ...
				while (_instrument.ConnectionStatus.Done == false)
				{
					// Sleep for 1 second
					Thread.Sleep(1000);
				}

				// Declare and obtain a copy of the connectstatus object.
				ConnectionStatus connectionStatus = _instrument.ConnectionStatus;

				// If the connection status text is either
				//   "Successfully connected to instrument server"
				// OR an empty string
				// Then call connection succeeded, otherwise show the text to the user.
				if (connectionStatus.Text.Equals("Successfully connected to instrument server") || connectionStatus.Text.Length == 0)
				{
					ConnectionSucceeded();
				}
				else
				{
					MessageBox.Show($"Instrument connection failed with error : {connectionStatus.Text}", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
			catch (System.Runtime.InteropServices.COMException ex)
			{
				MessageBox.Show($"Error: {_project.TkErrorDescription(ex.ErrorCode)}", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		
		private void ConnectionSucceeded()
		{
			RefreshInstrumentStatusInformation();
		}
		
		private void RefreshInstrumentStatusInformation()
		{
			lbStatusInformation.Items.Clear();
			
			// Note casting should not need to be done, but there is an error in the IDL that means the return type is incorrect
			InstrumentStatus instrumentStatus = (InstrumentStatus) _instrument.Status;
					
			// Add each piece of instrument status to the list box, replace the empty string with the correct field value
			// for all of the fields below
			string instrumentState = string.Format("State : {0}", instrumentStatus.SystemStateDescription);
			lbStatusInformation.Items.Add(instrumentState);
			
			string systemState = string.Format("SystemState : {0}", instrumentStatus.SystemState);
			lbStatusInformation.Items.Add(systemState);
			
			string sampleSetLineNumber = string.Format("SampleSetLineNumber : {0}", instrumentStatus.SampleSetLineNumber);
			lbStatusInformation.Items.Add(sampleSetLineNumber);
			
			string vial = string.Format("Vial : {0}", instrumentStatus.Vial);
			lbStatusInformation.Items.Add(vial);
			
			string injection = string.Format("Injection : {0}", instrumentStatus.Injection);
			lbStatusInformation.Items.Add(injection);
			
			string runTime = string.Format("Run Time : {0}", instrumentStatus.RunTime);
			lbStatusInformation.Items.Add(runTime);
			
			string totalInjectionTime = string.Format("totalInjectionTime : {0}", instrumentStatus.TotalInjectionTime);
			lbStatusInformation.Items.Add(totalInjectionTime);
			
			string sampleSetMethodName = string.Format("SampleSetMethodName : {0}", instrumentStatus.SampleSetMethodName);
			lbStatusInformation.Items.Add(sampleSetMethodName);
			
			string sampleSetMethodID = string.Format("sampleSetMethodID : {0}", instrumentStatus.SampleSetMethodID);
			lbStatusInformation.Items.Add(sampleSetMethodID);
			
			string InstrumentMethodName = string.Format("InstrumentMethodName : {0}", instrumentStatus.InstrumentMethodName);
			lbStatusInformation.Items.Add(InstrumentMethodName);
			
			string sampleSetName = string.Format("Sample Set Name : {0}", instrumentStatus.SampleSetMethodName);
			lbStatusInformation.Items.Add(sampleSetName);
			
			string methodSetID = string.Format("Method Set ID : {0}", instrumentStatus.MethodSetID);
			lbStatusInformation.Items.Add(methodSetID);
			
			string methodSetName = string.Format("Method Set ID : {0}", instrumentStatus.MethodSetName);
			
			lbStatusInformation.Items.Add(methodSetName);
		}
		
		/// <summary>
		/// Starts a run
		/// </summary>
		public void BtnStartRun_Click(object sender, EventArgs e)
		{
			try
			{
				string sampleSetMethodName = (string)cbSampleSetMethod.SelectedItem;
				string newName = txtNewSSMName.Text;

				// Start the run with the selected instrument method and output sample set method name
				_instrument.Run(sampleSetMethodName, newName);

				RefreshInstrumentStatusInformation();
			}
			catch (System.Runtime.InteropServices.COMException ex)
			{
				MessageBox.Show($"Error: {_project.TkErrorDescription(ex.ErrorCode)}", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		public void BtnRefreshInfo_Click(object sender, EventArgs e)
		{
			try
			{
				RefreshInstrumentStatusInformation();
			}
			catch (System.Runtime.InteropServices.COMException ex)
			{
				MessageBox.Show($"Error: {_project.TkErrorDescription(ex.ErrorCode)}", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		/// <summary>
		/// Pause the instrument run at the end of the current injection
		/// </summary>
		public void BtnPauseRun_Click(object sender, EventArgs e)
		{
			try
			{
				_instrument.Pause(float.MaxValue);
			}
			catch (System.Runtime.InteropServices.COMException ex)
			{
				MessageBox.Show($"Error: {_project.TkErrorDescription(ex.ErrorCode)}", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		
		/// <summary>
		/// Modify the next line to be run on the sampleset
		/// </summary>
		public void BtnModifyNextLine_Click(object sender, EventArgs e)
		{
			try
			{
				// Obtain the instrument status
				InstrumentStatus instrumentStatus = (InstrumentStatus)_instrument.Status;

				// If the instrument status contains 'paused' ...
				if (instrumentStatus.SystemStateDescription.Contains("Paused"))
				{
					// Obtain the sample set method name
					string sampleSetMethodName = instrumentStatus.SampleSetMethodName;

					// Obtain the current vial
					int vialNumber = instrumentStatus.Vial;

					// Instantiate a sample set method object, set the name, and call 'fetch'
					SampleSetMethod ssm = new SampleSetMethod
					{
						Name = sampleSetMethodName
					};
					ssm.Fetch();

					// Obtain the sample set line at the current 'vial number'
					SampleSetLine ssl = ssm.SampleSetLines.Item(vialNumber);

					// Update the 'Runtime' property on this line to 1 minute
					ssl.Set("Runtime", "1");

					// Store the changes to the database
					ssm.Store();

					// Continue the run with the newly changed sample set method
					_instrument.Replace(ssm.Name);
				}
				else
				{
					// If the instrument status is not paused - show an error message
					MessageBox.Show("Wait for the instrument to be paused first", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
			catch (System.Runtime.InteropServices.COMException ex)
			{
				MessageBox.Show($"Error: {_project.TkErrorDescription(ex.ErrorCode)}", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		
		/// <summary>
		/// Add a line to the end of the sample set method, and restarts a paused run with the changes made
		/// </summary>
		public void BtnAddALine_Click(object sender, EventArgs e)
		{
			try
			{
				// Declare and obtain the instrument status object
				InstrumentStatus instrumentStatus = (InstrumentStatus)_instrument.Status;

				// If the instrument status contains 'paused'..
				if (instrumentStatus.SystemStateDescription.Contains("Paused"))
				{
					// Obtain the sample set method name
					string sampleSetMethodName = instrumentStatus.SampleSetMethodName;

					// Instantiate a sample set method object, set the name and call 'fetch'
					SampleSetMethod ssm = new SampleSetMethod
					{
						Name = sampleSetMethodName
					};
					ssm.Fetch();

					// Obtain the lst sample set line (SampleSetLines.Count - 1)
					SampleSetLine ssl = ssm.SampleSetLines.Item(ssm.SampleSetLines.Count - 1);

					// Call the returnLineWithNextVialPosition with the ssl and the ssl plate layout object
					ssl = ReturnLineWithNextVialPosition(ssl, ssm.PlateLayouts);

					// Add the newly created sample set to the end of the current sample set lines class
					ssm.SampleSetLines.Add(ssl, ssm.SampleSetLines.Count - 1, MillenniumToolkit.mtkConstants.mtkAfter);

					// Store the changes to the sample set
					ssm.Store();

					// Restart the run using the new sample set method
					_instrument.Replace(sampleSetMethodName);
				}
				else
				{
					// If the instrument status is not paused - show an error message
					MessageBox.Show("Wait for the instrument to be paused first", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
			catch (System.Runtime.InteropServices.COMException ex)
			{
				MessageBox.Show($"Error: {_project.TkErrorDescription(ex.ErrorCode)}", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
		
		/// <summary>
		/// Creates a new line, with the vial position being the one AFTER the vial position
		/// of the line that is passed in.
		/// </summary>
		/// <param name="ssl">The model sample set line</param>
		/// <param name="pl">the plate layout to use for the positions</param>
		/// <returns></returns>
		private SampleSetLine ReturnLineWithNextVialPosition(SampleSetLine ssl, PlateLayouts pl)
		{
			// Get the position of the vial for the passed in line (call get on the vial field)
			string lastKnownPos = (string) (ssl.Get("vial", true));
			
			// Create a plate position object from the plate layouts
			PlatePosition platePos = pl.CreatePlatePosition();
			
			// Move to the last possible vial of the current plate layout
			platePos.SetToLastVial();
			
			// Store the value of the last possible vial in the plate layout
			string maxPossiblePos = platePos.Position;
			
			// Move the first position in the plate position
			platePos.SetToFirstVial();
			
			// If the plate layout is not lastKnownPos or maxPossiblePos
			while (!platePos.Position.Equals(lastKnownPos) && !platePos.Position.Equals(maxPossiblePos))
			{
				// increment the vial position by 1
				platePos.IncrementVial(1);
			}
			
			// If we did not reach the last possible plate position, increment the vial position to get the next position
			//   -i.e Is the current position the same as the maxPossiblePos
			if (!platePos.Position.Equals(maxPossiblePos))
			{
				// increment the position of the plate pos
				platePos.IncrementVial(1);
			}
			
			// Update the vial position on the line to the new vial position
			ssl.Set("Vial", platePos.Position);
			
			// Return the now modified line
			return ssl;
		}
		
		/// <summary>
		/// Displays the current instrument configurations for this node
		/// 
		/// TODO this is a bit Ghetto ...
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void BtnShowInstConfig_Click(object sender, EventArgs e)
		{
			txtShowInstrumentConfig.Text = "";
			
			// Loop over each inst config in the instConfigs object
			foreach (InstConfig instConfig in _instrument.InstConfigs)
			{
				// Add a line divider
				txtShowInstrumentConfig.Text = $"{txtShowInstrumentConfig.Text}\r\nNew Instrument\r\n";
				
				// For field in the instConfig fields object
				foreach (string fieldname in instConfig.Fields)
				{
					// create the string description which shows the fieldname and its value
					string desc = $"{fieldname}:{instConfig.Get(fieldname, true)}\r\n";
					txtShowInstrumentConfig.Text += desc;
				}
			}
		}
	}	
}