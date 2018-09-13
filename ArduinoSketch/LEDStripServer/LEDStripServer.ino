
#include <SPI.h>
#include <Adafruit_NeoPixel.h>
#include <avr/power.h>
#include<stdlib.h>

#define PIN    6
#define LEDNUM 50
#define SET_PIXELS_COLOR 0
#define SET_STRIP_COLOR  1
#define UPDATE_STRIP     2

// Parameter 1 = number of pixels in strip
// Parameter 2 = Arduino pin number (most are valid)
// Parameter 3 = pixel type flags, add together as needed:
//   NEO_KHZ800  800 KHz bitstream (most NeoPixel products w/WS2812 LEDs)
//   NEO_KHZ400  400 KHz (classic 'v1' (not v2) FLORA pixels, WS2811 drivers)
//   NEO_GRB     Pixels are wired for GRB bitstream (most NeoPixel products)
//   NEO_RGB     Pixels are wired for RGB bitstream (v1 FLORA pixels, not v2)
// IMPORTANT: To reduce NeoPixel burnout risk, add 1000 uF capacitor across
// pixel power leads, add 300 - 500 Ohm resistor on first pixel's data input
// and minimize distance between Arduino and first pixel.  Avoid connecting
// on a live circuit...if you must, connect GND first.
Adafruit_NeoPixel strip = Adafruit_NeoPixel(LEDNUM, PIN, NEO_GRB + NEO_KHZ800);

unsigned char mode;
unsigned char index;
unsigned char red;
unsigned char green;
unsigned char blue;

void setup() 
{
  #if defined (__AVR_ATtiny85__)
    if (F_CPU == 16000000) clock_prescale_set(clock_div_1);
  #endif
  strip.begin();
  //strip.setBrightness(255);
  strip.show(); // Initialize all pixels to 'off'
  
  Serial.begin(57600); 
  //Serial.begin(115200); 
  
  while (!Serial) 
  {
    ; // wait for serial port to connect. Needed for Leonardo only
  }
}

void loop() 
{
  if(Serial.available() >= 5) 
  {
    mode = Serial.read();
    index = Serial.read();
    red = Serial.read();
    green = Serial.read();
    blue = Serial.read();
    
    if(mode == SET_PIXELS_COLOR)
    {
      setColor(index, red, green, blue);
    }
    else if(mode == SET_STRIP_COLOR)
    {
      for(uint16_t indexLED=0; indexLED<strip.numPixels(); indexLED++) 
      {
         setColor(indexLED, red, green, blue);
      }
      updateStrip();
    }
    else if(mode == UPDATE_STRIP)
    {
      updateStrip();
      delay(10);
    }
  }
}

void setColor(unsigned char index, unsigned char red, unsigned char green, unsigned char blue) 
{
      if(index >= strip.numPixels()) return;
    setColor(index, strip.Color(red, green, blue));
}

void setColor(unsigned char index, uint32_t color) 
{
      if(index >= strip.numPixels()) return;
    strip.setPixelColor(index, color);
}

void updateStrip() 
{
    strip.show();
    //delay(wait);
}
