KMP382-driver
=============

Driver for talking to a serially connected Kamstrup 685-382 electricity meter

Serial driver to Kamstrup type 685-382 electricity meters using the legacy but open
IEC61107 protocol as well as the native but propriatary "Kamstrup Meter
Protocol", hence forth referred to as the KMP protocol.

The IEC71107 protocol (http://kamstrup.com/media/2105/IEC61107_comprotocol.pdf) allows 
for easy low-speed retrieval of some basic meter properties and might work for other 
legacy meters from Kamstrup and possible other manufacturers as well. An example output 
from a Kamstrup 685-382 using this IEC61107 protocol is:

	MAKE: KAM				// Make
	MODEL: 685-382-OK-10	// Model
	0.0: 10865272			// Customer no.
	1.20: 0009032*kWh		// Energy register
	1.20.1: 0009031*kWh		// Tarif 1
	1.20.2: 0000001*kWh		// Tarif 2
	1.31: 0057392*h			// Hour counter
	1.26: 0000000			// Pulse counter
	1.6: 000001,5*kW		// Actual peak
	1.6*1: 000002,8			// Last months peak

IEC61107 uses a very simple ascii protocol, where each line (except the first 3) are
on the form <KEY>:<VALUE> where the key is apparently a standardzed OBIS code
(http://www.dlms.com/documentation/listofstandardobiscodesandmaintenanceproces/index.html).

The KMP protocol is more complex and allows not just retrieval but also
programming of the meter. However, the current version of the library deals
exclusibely with retrieval of data, albeit in greater detail than what the 
IEC1107 protocol offers. The protocol has been reverse engineered by carefully 
inspecting and analyzing request/response data between the meter and officially 
available programming software. This is in spite of Kamstrup claiming their
protocol and software is open... lamers. Example of output by using this
protocol:

	METER_TYPE: 685382OK10
	METER_ID: 123456
	CUSTOMER_NO: 12345678
	TOTAL_ENERGY: 9032
	ENERGY_LOAD_WH: 0
	OPERATING_HOURS: 57377
	RESET_COUNTER: 903233
	PEAK_POWER: 1559
	CUSTOMER_NO_2: 12345678
	PRODUCTION_NO: 168339478
	ENERGY_TOTAL2: 9032
	ENERGY_TARIF1: 9031
	ENERGY_TARIF2: 1

The KMP is an odd one; while physically binary, (mostly) relying on ASCII characters 
underneath. Everything but the first frame byte (0x40) is dealth with in pairs, such 
that two separate physical bytes, interpreted as ASCII characters, unite to form one 
logical byte. The first ASCII character contain the high-order nibble and the second 
ASCII character contains the low-order nibble, utilizing only the range 0x30 to 0x46 
while skipping over 0x40 (ASCII characters 0-1, A-F). Furthermore, a logical 8-bit 
LRC checksum of everything following the frame type, is appended in the same manner, 
taking up two physical bytes.

Feel free to contribute any new findings for the 685-382 meter. Happy hacking :)
