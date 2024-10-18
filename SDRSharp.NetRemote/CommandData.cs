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

using System.Text.Json.Serialization;

namespace SDRSharp.NetRemote;

public class CommandData
{
	[JsonPropertyName("command")] public string Command { get; set; }

	[JsonPropertyName("method")] public string Method { get; set; }

	[JsonPropertyName("value")] public int? Value { get; set; }
}