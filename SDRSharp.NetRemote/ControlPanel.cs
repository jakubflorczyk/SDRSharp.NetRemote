using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using SDRSharp.Common;
using SDRSharp.Radio;

namespace SDRSharp.NetRemote;

public partial class ControlPanel : UserControl
{
	private const string SettingServerEn = "netRemoteServerEnable";
	private const string SettingServerPort = "netRemoteServerPort";
	private const string SettingSerialEn = "netRemoteSerialEnable";
	private const string SettingSerialPort = "netRemoteSerialPort";

	private const int Port = 3382;

	private ISharpControl _control;

	private Thread _threadServer;
	private Thread _threadSerial;
	private readonly Parser _parser;
	private Server _server = null;
	private Serial _serial = null;

	public ControlPanel(ISharpControl control)
	{
		InitializeComponent();

		_parser = new Parser(control);
		_control = control;

		cbNetwork.Checked = Utils.GetBooleanSetting(SettingServerEn, true);
		nudPort.Value = Utils.GetIntSetting(SettingServerPort, Port);
		nudPort.Enabled = !cbNetwork.Checked;

		cbSerial.Checked = Utils.GetBooleanSetting(SettingSerialEn, false);
		var ports = Serial.GetPorts();
		if (ports.Length > 0)
		{
			cobSerial.Enabled = !cbSerial.Checked;
			cobSerial.Items.AddRange(Serial.GetPorts().Select(x => (object)x).ToArray());
			cobSerial.SelectedIndex = 0;
			cobSerial.SelectedItem = Utils.GetStringSetting(SettingSerialPort, "");
		}
		else
		{
			cbSerial.Checked = false;
			cobSerial.Enabled = false;
		}

		ServerControl();
		SerialControl();
	}

	public void Close()
	{
		Utils.SaveSetting(SettingServerEn, cbNetwork.Checked);
		Utils.SaveSetting(SettingServerPort, nudPort.Value);
		Utils.SaveSetting(SettingSerialEn, cbSerial.Checked);
		Utils.SaveSetting(SettingSerialPort, cobSerial.SelectedItem);

		cbNetwork.Checked = false;
		cbSerial.Checked = false;
		ServerControl();
		SerialControl();
	}

	private void ServerControl()
	{
		if (cbNetwork.Checked)
		{
			if (_threadServer != null) return;

			_server = new Server(_parser, (int)nudPort.Value);
			_server.ServerError += OnServerError;
			_threadServer = new Thread(new ThreadStart(_server.Start));
			_threadServer.Start();
		}
		else
		{
			if (_threadServer == null) return;

			_server.Stop();
			_threadServer.Join(1000);
			_threadServer = null;
		}
	}

	private void SerialControl()
	{
		if (cbSerial.Checked)
		{
			if (_threadSerial != null) return;
			if (cobSerial.SelectedItem == null) return;

			_serial = new Serial(_parser, cobSerial.SelectedItem.ToString());
			_serial.SerialError += OnSerialError;
			_threadSerial = new Thread(new ThreadStart(_serial.Start));
			_threadSerial.Start();
		}
		else
		{
			if (_threadSerial == null) return;

			_serial.Stop();
			_threadSerial.Join(1000);
			_threadSerial = null;
		}
	}

	private void cbNetwork_CheckedChanged(object sender, EventArgs e)
	{
		nudPort.Enabled = !cbNetwork.Checked;
		ServerControl();
	}

	private void cbSerial_CheckedChanged(object sender, EventArgs e)
	{
		cobSerial.Enabled = !cbSerial.Checked;
		SerialControl();
	}

	private void OnSerialError(object sender, EventArgs e)
	{
		_threadSerial = null;
		cbSerial.Checked = false;
	}

	private void OnServerError(object sender, EventArgs e)
	{
		_threadServer = null;
		cbNetwork.Checked = false;
	}
}