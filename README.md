# Ambiculus
As promised years ago: The Ambiculus source code for the paper: https://dl.acm.org/citation.cfm?id=2927939

Ambiculus is a peripheral, low-resolution display extension for HMDs.
The Unity project transmits the pixel colors using a virtual serial port to an Arduino Nano.
The Arduino Nano is connected to LED Strips with an integrated WS2812B Controller. The sketch uses the Adafruit Neopixel library (https://github.com/adafruit/Adafruit_NeoPixel) .

Finally managed to find some time to clean up the code and used the opportunity to update it and make it faster. Now using async gpu readbacks.
There is an example Scene included which works without an HMD.
