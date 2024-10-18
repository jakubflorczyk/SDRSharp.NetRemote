# SDRSharp.NetRemote

Copyright 
2014 - 2015 Al Brown
2024 Jakub Florczyk

This is updated version of https://github.com/EarToEarOak/SDRSharp-Net-Remote

Tested on:

- Windows 11

## Installation ##
Run the installer and point it to an installation of SDRSharp, this will copy the necessary files and register the plugin.

## Testing ##
Start SDRSharp and check the plugin control panel is shown on the left side of the main window and 'Network' is ticked.

Run:

    telnet localhost 3382

You should get a JSON response showing the plugin name and version, for example:

    {"Name":"Net Remote","Version":"2.1.0.0"}

If 'Serial' is enabled in the control panel commands will be read from the serial port, currently the port defaults to 115200 8N1.

## Commands ##
Commands are JSON formatted strings containing a *Command*, *Method* and  optional *Value* attributes. For example to set the current volume to 30:

    {"Command": "Set", "Method": "AudioGain", "Value": 30}


Or to test if SDRSharp is currently playing:

    {"Command": "Get", "Method": "IsPlaying"}

Which returns:

    {"IsPlaying":true}

All attributes are case insensitive.

### Command Attribute ###
The command attribute may be one of the following:

    Set
    Get
    Exe 

The value attribute only used with the Set command.

### Method Attribute ###
For the *Get* and *Set* commands the method can be one of the following:

    AudioGain			- Volume <25-60>  
    AudioIsMuted		- Mute <true|false>

    CenterFrequency		- Centre displayed frequency <0-999999999999>
	Frequency			- Tuned frequency <0-999999999999>

    DetectorType		- Demodulation <AM|CW|DSB|LSB|NFM|RAW|USB|WFM>

    IsPlaying			- Currently playing <true|false>

	SourceIsTunable		- Tunable device <true|false>

	SquelchEnabled		- Squelch <true|false>
	SquelchThreshold	- Squelch level <0-100>

	FmStereo			- FM stereo <true|false>

	FilterType			- Filter type <1-6>
	FilterBandwidth		- Filter bandwidth <10-250000>
	FilterOrder			- Filter order <10-9999>

For the *Exe* command these methods are available:

    Start				- Start playing
    Stop				- Stop playing
    Close				- Close network connection

### Response ###
The response of a command my be either be *OK* or *Error*.

*OK* is returned if a *Set* command completes:

	{"Result":"OK"}

For *Get* commands *method* and *value* attributes are added, e.g.:

	{"Result":"OK","Method":"AudioGain","Value":30}

An *Error* is returned if a problem occurred, e.g.:

	{"Result":"Error","Type":"Value error","Message":"Value missing"}

## License ##
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
