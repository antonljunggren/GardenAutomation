# Garden Automation System

A complete modular garden watering/monitoring system giving hobbyists and regular users a simple way to monitor and handle their gardens.

! STILL WIP !

## Overview
### Goals
In short, a complete system to handle everything from the sensors to the "cloud" automations

- Let users connect sensors in a simple way with a automated pairing process
- Be able to monitor the sensors on a dashboard
- Create automations or flows to let the system control sensors and outputs

## System Architecture
### Hardware

- Sensors will be simple MCUs like Attiny85 or similar (by using arduino supported MCUs a lot of problems will already been solved)
- A central hub consisting of a Raspberry Pi (hopefully a smaller Zero 2 will be sufficient enough)
- A CAN bus will be used for the communication between the devices and hub
- A perimiter wire that runs along the circumferense of the greenhouse that has: 24V power, CAN H & L and GND (24V for voltage drop, 12V, 5V & 3.3V are the target voltages on the devices/relays)


## Setup
To set up the raspberry pi we need to enable SPI and open a CAN bus interface in the boot config file. <br>
`dtparam=spi=on`<br>
`dtoverlay=mcp2515-can0,oscillator=16000000,interrupt=25,spimaxfrequency=1000000` <br>
`dtoverlay=spi-bcm2835-overlay`<br>

Then we install dotnet on the pi
`wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh`<br>
`chmod +x dotnet-install.sh` <br>
`./dotnet-install.sh --channel 8.0 --install-dir /usr/share/dotnet` <br>
`export DOTNET_ROOT="/usr/share/dotnet"` <br>
`export PATH="$PATH:$DOTNET_ROOT"` <br>
`echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc` <br>
`echo 'export PATH=$PATH:$HOME/.dotnet' >> ~/.bashrc` <br>
`source ~/.bashrc` <br>
