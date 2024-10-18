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
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace SDRSharp.NetRemote;
// Based on http://msdn.microsoft.com/en-us/library/fx6588te.aspx

public class Server(Parser parser, int port)
{
	private const int MaxClients = 4;
	private readonly List<Client> _clients = new();

	private readonly object _lockClients = new();

	private readonly ManualResetEvent _signal = new(false);
	private volatile bool _cancel;

	public event EventHandler ServerError;

	public void Start()
	{
		var localEndPoint = new IPEndPoint(IPAddress.Any, port);
		var socket = new Socket(AddressFamily.InterNetwork,
			SocketType.Stream, ProtocolType.Tcp);

		var timerAlive = new Timer();
		timerAlive.Elapsed += OnTimerAlive;
		timerAlive.Interval = 1000;
		timerAlive.Enabled = true;

		try
		{
			socket.Bind(localEndPoint);
			socket.Listen(100);

			while (!_cancel)
			{
				_signal.Reset();
				socket.BeginAccept(ConnectCallback,
					socket);
				_signal.WaitOne();
			}
		}
		catch (SocketException ex)
		{
			OnServerError();

			if (socket.IsBound)
				socket.Shutdown(SocketShutdown.Both);

			var msg = "Network Error:\n" + ex.Message;
			MessageBox.Show(msg, AssemblyHelper.Title(),
				MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
		finally
		{
			timerAlive.Close();
			socket.Close();
			foreach (var client in _clients.ToArray())
				ClientRemove(client);
		}
	}

	public void Stop()
	{
		_cancel = true;
		_signal.Set();
	}

	private static bool IsConnected(Socket socket)
	{
		try
		{
			return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
		}
		catch (SocketException)
		{
			return false;
		}
	}

	private void ClientAdd(Client client)
	{
		lock (_lockClients)
		{
			_clients.Add(client);
		}

		Send(client, parser.Motd());
		try
		{
			client.Socket.BeginReceive(client.Buffer, 0, Client.BufferSize, 0,
				ReadCallback,
				client);
		}
		catch (Exception)
		{
			ClientRemove(client);
		}
	}

	private void ClientRemove(Client client)
	{
		lock (_lockClients)
		{
			_clients.Remove(client);
		}

		try
		{
			client.Socket.Shutdown(SocketShutdown.Both);
			client.Socket.Close();
		}
		catch (SocketException)
		{
		}
		catch (ObjectDisposedException)
		{
		}
	}

	private void ConnectCallback(IAsyncResult ar)
	{
		_signal.Set();

		try
		{
			var socketServer = (Socket)ar.AsyncState;
			var socketClient = socketServer.EndAccept(ar);
			var client = new Client(socketClient);

			if (_clients.Count < MaxClients)
				ClientAdd(client);
			else
				ClientRemove(client);
		}
		catch (SocketException)
		{
		}
		catch (ObjectDisposedException)
		{
		}
	}

	private void ReadCallback(IAsyncResult ar)
	{
		var data = string.Empty;
		var client = (Client)ar.AsyncState;

		try
		{
			var read = client.Socket.EndReceive(ar);
			if (read > 0)
			{
				client.Data.Append(Encoding.ASCII.GetString(client.Buffer,
					0, read));
				data = client.Data.ToString();
				if (data.Split('{').Length == data.Split('}').Length)
				{
					try
					{
						var result = parser.Parse(client.Data.ToString());
						Send(client, result);
					}
					catch (CommandException)
					{
						ClientRemove(client);
					}
					catch (ClientException)
					{
						ClientRemove(client);
					}

					client.Data.Length = 0;
				}

				client.Socket.BeginReceive(client.Buffer, 0,
					Client.BufferSize, 0,
					ReadCallback,
					client);
			}
		}
		catch (Exception)
		{
			ClientRemove(client);
		}
	}

	private void Send(Client client, string data)
	{
		if (data == null)
			return;

		var byteData = Encoding.ASCII.GetBytes(data);
		try
		{
			client.Socket.BeginSend(byteData, 0, byteData.Length, 0,
				SendCallback, client);
		}
		catch (Exception)
		{
			ClientRemove(client);
		}
	}

	private void SendCallback(IAsyncResult ar)
	{
		var client = (Client)ar.AsyncState;
		try
		{
			client.Socket.EndSend(ar);
		}
		catch (Exception)
		{
			ClientRemove(client);
		}
	}

	private void OnTimerAlive(object source, ElapsedEventArgs e)
	{
		var disconnected = new List<Client>();

		lock (_lockClients)
		{
			foreach (var client in _clients)
				if (!IsConnected(client.Socket))
					disconnected.Add(client);
			foreach (var client in disconnected)
				ClientRemove(client);
		}
	}

	protected virtual void OnServerError()
	{
		var handler = ServerError;
		if (handler != null) handler(this, EventArgs.Empty);
	}
}