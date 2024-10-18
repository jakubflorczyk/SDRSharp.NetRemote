/*
	SDRSharp Net Remote

	Copyright
	2014 - 2017 Al Brown
	2024 Jakub Florczyk

	A network remote control plugin for SDRSharp


	This program is free software: you can redistribute it and/or modify
	it under the terms of the GNU General Public License as published by
	the Free Software Foundation, or (at your option)
	any later version.

	This program is distributed in the hope that it will be useful,
	but WITHOUT ANY WARRANTY; without even the implied warranty of
	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
	GNU General Public License for more details.

	You should have received a copy of the GNU General Public License
	along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;

namespace SDRSharp.NetRemote;

public class Serial(Parser parser, string port)
{
	private readonly ManualResetEvent _signal = new(false);

	private string _data;
	public event EventHandler SerialError;

	public static string[] GetPorts()
	{
		var ports = SerialPort.GetPortNames();
		Array.Sort(ports);

		return ports;
	}

	public void Start()
	{
		var port1 = new SerialPort(port, 115200);
		port1.DataReceived += DataReceivedHandler;
		try
		{
			port1.Open();
		}
		catch (IOException ex)
		{
			OnSerialError();
			var msg = "Serial Port Error:\n" + ex.Message;
			MessageBox.Show(msg, AssemblyHelper.Title() + ": Serial Port Error",
				MessageBoxButtons.OK, MessageBoxIcon.Error);
			Close(port1);
			return;
		}
		catch (UnauthorizedAccessException ex)
		{
			OnSerialError();
			var msg = "Serial Port Error:\n" + ex.Message;
			MessageBox.Show(msg, AssemblyHelper.Title() + ": Serial Port Error",
				MessageBoxButtons.OK, MessageBoxIcon.Error);
			Close(port1);
			return;
		}

		Send(port1, parser.Motd());

		_signal.Reset();
		_signal.WaitOne();

		Close(port1);
	}

	public void Stop()
	{
		_signal.Set();
	}

	private void Send(SerialPort serialPort, string data)
	{
		if (serialPort.IsOpen && data != null)
			serialPort.Write(data);
	}

	private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
	{
		var serialPort = (SerialPort)sender;

		_data += serialPort.ReadExisting();
		if (_data.Split('{').Length != _data.Split('}').Length) return;

		try
		{
			var result = parser.Parse(_data);
			Send(serialPort, result);
		}
		catch (CommandException)
		{
		}

		_data = "";
	}

	private void Close(SerialPort serialPort)
	{
		try
		{
			serialPort.Close();
		}
		catch (IOException)
		{
		}
	}

	protected virtual void OnSerialError()
	{
		var handler = SerialError;
		handler?.Invoke(this, EventArgs.Empty);
	}
}