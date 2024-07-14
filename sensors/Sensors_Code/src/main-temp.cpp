#include <sensor-device.h>

#define THERMISTORPIN PIN_PA3
//3

#define SERIESRESISTOR 10000
#define NUMSAMPLES 10
#define TEMPERATURENOMINAL 25
#define BCOEFFICIENT 3950
#define THERMISTORNOMINAL 10000

int samples[NUMSAMPLES];
int tempValue;

float temp();
void dataReceived(uint8_t msgType);


void setup()
{
    init_sensor(DEVICE_TYPE_TEMP_SENSOR, dataReceived);
}

void loop()
{
    // tempValue = (int)(temp() * 100.0f);
    
    delay(500);
}

void dataReceived(uint8_t msgType)
{
    if(msgType == MSG_TYPE_DATA) {
        send_data(MSG_TYPE_DATA, temp());
    }
}

float temp()
{
    uint8_t i;
    float average;

    // take N samples in a row, with a slight delay
    for (i = 0; i < NUMSAMPLES; i++)
    {
        samples[i] = analogRead(analogInputToDigitalPin(THERMISTORPIN));
        delay(20);
    }

    // average all the samples out
    average = 0;
    for (i = 0; i < NUMSAMPLES; i++)
    {
        average += samples[i];
    }
    average /= NUMSAMPLES;

    // convert the value to resistance
    average = 1023 / average - 1;
    average = SERIESRESISTOR / average;

    float steinhart;
    steinhart = average / THERMISTORNOMINAL;          // (R/Ro)
    steinhart = log(steinhart);                       // ln(R/Ro)
    steinhart /= BCOEFFICIENT;                        // 1/B * ln(R/Ro)
    steinhart += 1.0 / (TEMPERATURENOMINAL + 273.15); // + (1/To)
    steinhart = 1.0 / steinhart;                      // Invert
    steinhart -= 273.15;                              // convert absolute temp to C

    return steinhart;
}