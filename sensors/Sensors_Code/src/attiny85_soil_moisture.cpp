#include <sensor-device.h>

#define INPUT_PIN PIN_PB3

int moisture();
void dataReceived(CAN_Message msg);

void setup()
{
    init_sensor(DEVICE_TYPE_SOIL_MOISTURE_SENSOR, dataReceived);
}

void loop()
{
    delay(100);
}

void dataReceived(CAN_Message msg)
{
    if (msg.messageType == MSG_TYPE_DATA)
    {
        send_data(MSG_TYPE_DATA, moisture());
    }
}

int moisture() 
{
    return analogRead(INPUT_PIN);
}