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
using System.Text.Json;
using System.Text.RegularExpressions;
using SDRSharp.Common;
using SDRSharp.Radio;

namespace SDRSharp.NetRemote;

public class Parser
{
	private static readonly string[] Commands = { "get", "set", "exe" };
	private static readonly Dictionary<string, Func<bool, object, string>> Methods = new();

	private readonly ISharpControl _control;

	public Parser(ISharpControl control)
	{
		_control = control;

		Methods.Add("audiogain", CmdAudioGain);
		Methods.Add("audioismuted", CmdAudioIsMuted);

		Methods.Add("centrefrequency", CmdCentreFrequency);
		Methods.Add("frequency", CmdFrequency);

		Methods.Add("detectortype", CmdDetectorType);

		Methods.Add("isplaying", CmdIsPlaying);

		Methods.Add("sourceistunable", CmdSourceIsTunable);

		Methods.Add("squelchenabled", CmdSquelchEnabled);
		Methods.Add("squelchthreshold", CmdSquelchThreshold);

		Methods.Add("fmstereo", CmdFmStereo);

		Methods.Add("filtertype", CmdFilterType);
		Methods.Add("filterbandwidth", CmdFilterBandwidth);
		Methods.Add("filterorder", CmdFilterOrder);

		Methods.Add("start", CmdAudioGain);
		Methods.Add("stop", CmdAudioGain);
		Methods.Add("close", CmdAudioGain);
	}

	public string Parse(string data)
	{
		string result;

		data = Regex.Replace(data, @"[^\u0020-\u007F]", string.Empty);
		data = data.ToLower();

		try
		{
			var commandData = JsonSerializer.Deserialize<CommandData>(data);

			if (commandData != null)
			{
				if (string.IsNullOrEmpty(commandData.Command))
					throw new CommandException("Command key not found");
				if (Array.IndexOf(Commands, commandData.Command) == -1)
					throw new CommandException($"Unknown command: {commandData.Command}");

				if (string.IsNullOrEmpty(commandData.Method))
					throw new MethodException("Method key not found");
				if (!Methods.ContainsKey(commandData.Method))
					throw new MethodException($"Unknown method: {commandData.Method}");

				if (string.Equals(commandData.Command, "set") && !commandData.Value.HasValue)
					throw new ValueException("Value missing");

				result = Command(commandData.Command, commandData.Method, commandData.Value);
			}
			else
			{
				result = null;
			}
		}
		catch (Exception ex)
		{
			switch (ex)
			{
				case ArgumentOutOfRangeException:
					result = Error("Set error", "Could not set value");
					break;
				case ArgumentException:
				case InvalidOperationException:
					result = Error("Syntax error", data);
					break;
				case CommandException:
					result = Error("Command error", ex.Message);
					break;
				case MethodException:
					result = Error("Method error", ex.Message);
					break;
				case ValueException:
					result = Error("Value error", ex.Message);
					break;
				case SourceException:
					result = Error("Source error", ex.Message);
					break;
				default:
					throw;
			}
		}

		return result;
	}

	private string Command(string command, string method, object value)
	{
		string result;

		if (string.Equals(command, "exe"))
		{
			switch (method)
			{
				case "start":
					_control.StartRadio();
					break;
				case "stop":
					_control.StopRadio();
					break;
				case "close":
					throw new ClientException();
				default:
					throw new MethodException($"Unknown Exe method: {method}");
			}

			result = Response<object>(null, null);
		}
		else
		{
			var set = string.Equals(command, "set");
			result = Methods[method].Invoke(set, value);
		}

		return result;
	}

	private static object CheckValue<T>(object value)
	{
		var typeExpected = typeof(T);
		var typePassed = value.GetType();

		if (typeExpected == typeof(long))
			if (typePassed == typeof(long) || typePassed == typeof(int))
				return value;

		if (typePassed == typeExpected) return value;

		if (typeExpected == typeof(bool))
			throw new ValueException("Expected a boolean");
		if (typeExpected == typeof(int) || typeExpected == typeof(long))
			throw new ValueException("Expected an integer");
		if (typeExpected == typeof(string))
			throw new ValueException("Expected a string");

		return value;
	}

	private static void CheckRange(long value, long start, long end)
	{
		if (value < start)
			throw new ValueException($"Smaller than {start}");
		if (value > end)
			throw new ValueException($"Greater than {end}");
	}

	private static object CheckEnum(string value, Type type)
	{
		try
		{
			return Enum.Parse(type, value, true);
		}
		catch (ArgumentException)
		{
			var error = "Expected one of ";
			error += string.Join(", ", Enum.GetNames(type));
			throw new ValueException(error);
		}
	}

	private string CmdAudioGain(bool set, object value)
	{
		string result;

		if (set)
		{
			var gain = (int)CheckValue<int>(value);
			CheckRange(gain, 25, 60);
			_control.AudioGain = gain;
			result = Response<object>(null, null);
		}
		else
		{
			result = Response<int>("AudioGain", _control.AudioGain);
		}

		return result;
	}


	private string CmdAudioIsMuted(bool set, object value)
	{
		string result;

		if (set)
		{
			_control.AudioIsMuted = (bool)CheckValue<bool>(value);
			result = Response<object>(null, null);
		}
		else
		{
			result = Response<bool>("AudioIsMuted",
				_control.AudioIsMuted);
		}

		return result;
	}

	private string CmdCentreFrequency(bool set, object value)
	{
		string result;

		if (set)
		{
			if (!_control.SourceIsTunable)
				throw new SourceException("Not tunable");
			var freq = Convert.ToInt64(CheckValue<long>(value));
			CheckRange(freq, 1, 999999999999);
			_control.CenterFrequency = freq;
			result = Response<object>(null, null);
		}
		else
		{
			result = Response<long>("CenterFrequency",
				_control.CenterFrequency);
		}

		return result;
	}

	private string CmdFrequency(bool set, object value)
	{
		string result;

		if (set)
		{
			if (!_control.SourceIsTunable)
				throw new SourceException("Not tunable");
			var freq = Convert.ToInt64(CheckValue<long>(value));
			CheckRange(freq, 1, 999999999999);
			_control.Frequency = freq;
			result = Response<object>(null, null);
		}
		else
		{
			result = Response<long>("Frequency", _control.Frequency);
		}

		return result;
	}

	private string CmdDetectorType(bool set, object value)
	{
		string result;

		if (set)
		{
			var det = (string)CheckValue<string>(value);
			_control.DetectorType =
				(DetectorType)CheckEnum(det, typeof(DetectorType));
			result = Response<object>(null, null);
		}
		else
		{
			result = Response<string>("DetectorType",
				_control.DetectorType.ToString());
		}

		return result;
	}

	private string CmdIsPlaying(bool set, object value)
	{
		if (set)
			throw new MethodException("Read only");
		var result = Response<bool>("IsPlaying", _control.IsPlaying);

		return result;
	}

	private string CmdSourceIsTunable(bool set, object value)
	{
		if (set)
			throw new MethodException("Read only");
		var result = Response<bool>("SourceIsTunable",
			_control.SourceIsTunable);

		return result;
	}

	private string CmdSquelchEnabled(bool set, object value)
	{
		string result;

		if (set)
		{
			_control.SquelchEnabled = (bool)CheckValue<bool>(value);
			result = Response<object>(null, null);
		}
		else
		{
			result = Response<bool>("SquelchEnabled",
				_control.SquelchEnabled);
		}

		return result;
	}

	private string CmdSquelchThreshold(bool set, object value)
	{
		string result;

		if (set)
		{
			var thresh = (int)CheckValue<int>(value);
			CheckRange(thresh, 0, 100);
			_control.SquelchThreshold = thresh;
			result = Response<object>(null, null);
		}
		else
		{
			result = Response<int>("SquelchThreshold",
				_control.SquelchThreshold);
		}

		return result;
	}

	private string CmdFmStereo(bool set, object value)
	{
		string result;

		if (set)
		{
			_control.FmStereo = (bool)CheckValue<bool>(value);
			result = Response<object>(null, null);
		}
		else
		{
			result = Response<bool>("FmStereo", _control.FmStereo);
		}

		return result;
	}

	private string CmdFilterType(bool set, object value)
	{
		string result;

		if (set)
		{
			var type = (int)CheckValue<int>(value);
			CheckRange(type, 1, Enum.GetNames(typeof(WindowType)).Length);
			_control.FilterType = (WindowType)type;
			result = Response<object>(null, null);
		}
		else
		{
			result = Response<int>("FilterType", _control.FilterType);
		}

		return result;
	}

	private string CmdFilterBandwidth(bool set, object value)
	{
		string result;

		if (set)
		{
			var bw = (int)CheckValue<int>(value);
			CheckRange(bw, 10, 250000);
			_control.FilterBandwidth = bw;
			result = Response<object>(null, null);
		}
		else
		{
			result = Response<int>("FilterBandwidth",
				_control.FilterBandwidth);
		}

		return result;
	}

	private string CmdFilterOrder(bool set, object value)
	{
		string result;

		if (set)
		{
			var bw = (int)CheckValue<int>(value);
			CheckRange(bw, 10, 9999);
			_control.FilterOrder = bw;
			result = Response<object>(null, null);
		}
		else
		{
			result = Response<int>("FilterOrder",
				_control.FilterOrder);
		}

		return result;
	}

	public string Motd()
	{
		var version = new Dictionary<string, string>
		{
			{ "Name", AssemblyHelper.Title() },
			{ "Version", AssemblyHelper.Version() }
		};

		return JsonSerializer.Serialize(version) + "\r\n";
	}

	private string Error(string type, string message)
	{
		var version = new Dictionary<string, string>
		{
			{ "Result", "Error" },
			{ "Type", type },
			{ "Message", message }
		};

		return JsonSerializer.Serialize(version) + "\r\n";
	}

	private string Response<T>(string key, object value)
	{
		var resp = new Dictionary<string, object>
		{
			{ "Result", "OK" }
		};

		if (key != null)
		{
			resp.Add("Method", key);
			resp.Add("Value", (T)value);
		}

		return JsonSerializer.Serialize(resp) + "\r\n";
	}
}