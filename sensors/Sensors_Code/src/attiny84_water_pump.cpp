#include <sensor-device.h>
#include <EEPROM.h>

#define WATER_PUMP_PIN PIN_PA1
#define WATER_LEVEL PIN_PA3
#define WATER_LEVEL_STOP PIN_PA2

#define PUMP_CMD 0
#define SET_LOW_OFFSET_CMD 1
#define SET_MAX_OFFSET_CMD 2

#define DATA_WATER_LEVEL 0

#define NUMSAMPLES 10
int samples[NUMSAMPLES];

const int water_level_low_offset_address = 0;
const int water_level_max_offset_address = sizeof(int);

void dataReceived(CAN_Message msg);

int getWaterLevel();

void readFromEEPROM(int &low_offset, int &max_offset) {
  EEPROM.get(water_level_low_offset_address, low_offset);
  EEPROM.get(water_level_max_offset_address, max_offset);
}

bool is_pumping = false;

int lowOffset, maxOffset;

void setup()
{
    pinMode(WATER_PUMP_PIN, OUTPUT);
    pinMode(WATER_LEVEL, INPUT);
    pinMode(WATER_LEVEL_STOP, INPUT_PULLUP);

    digitalWrite(WATER_PUMP_PIN, LOW);
    delay(100);

    init_sensor(DEVICE_TYPE_WATER_PUMP, dataReceived);

    digitalWrite(WATER_PUMP_PIN, LOW);

    readFromEEPROM(lowOffset, maxOffset);
}

void loop()
{
    // tempValue = (int)(temp() * 100.0f);

    if(is_pumping && digitalRead(WATER_LEVEL_STOP) == LOW) {
        is_pumping = false;
        digitalWrite(WATER_PUMP_PIN, is_pumping);
        send_data(MSG_TYPE_DEVICE_STATE, 0);
        send_data(MSG_TYPE_DATA, 0.0f, DATA_WATER_LEVEL);
    }
    
    delay(500);
}

void dataReceived(CAN_Message msg)
{
    uint8_t commandType = msg.data[msg.dataLegth-2];
    uint8_t commandValue = msg.data[msg.dataLegth-1];

    if(msg.messageType == MSG_TYPE_CMD) {
        if(commandType == PUMP_CMD) {

            if(digitalRead(WATER_LEVEL_STOP) == LOW) {
                send_data(MSG_TYPE_DEVICE_STATE, 0);
                send_data(MSG_TYPE_DATA, 0, DATA_WATER_LEVEL);
                return;
            }

            digitalWrite(WATER_PUMP_PIN, commandValue);
            is_pumping = commandValue;
            send_data(MSG_TYPE_DEVICE_STATE, (int)commandValue);
            send_data(MSG_TYPE_DATA, getWaterLevel(), DATA_WATER_LEVEL);
        }
        else if(commandType == SET_LOW_OFFSET_CMD) {
            int currentLevel = analogRead(analogInputToDigitalPin(WATER_LEVEL));
            EEPROM.put(water_level_low_offset_address, currentLevel);
            lowOffset = currentLevel;
        }
        else if(commandType == SET_MAX_OFFSET_CMD) {
            int currentLevel = analogRead(analogInputToDigitalPin(WATER_LEVEL));
            EEPROM.put(water_level_max_offset_address, currentLevel);
            maxOffset = currentLevel;
        }
    }
    else if(msg.messageType == MSG_TYPE_DATA) {

        //default data, send water level
        if(digitalRead(WATER_LEVEL_STOP) == LOW) {
            send_data(MSG_TYPE_DATA, 0.0f, DATA_WATER_LEVEL);
        } else {
            send_data(MSG_TYPE_DATA, getWaterLevel(), DATA_WATER_LEVEL);
        }
    }
}

int getWaterLevel()
{
    uint8_t i;
    int average;

    for (i = 0; i < NUMSAMPLES; i++)
    {
        samples[i] = analogRead(analogInputToDigitalPin(WATER_LEVEL));
        delay(20);
    }

    average = 0;
    for (i = 0; i < NUMSAMPLES; i++)
    {
        average += samples[i];
    }
    average /= NUMSAMPLES;

    if(average < lowOffset) {
        return 0;
    }

    if(average > maxOffset) {
        return 1023;
    }

    return (average - lowOffset) * 1023L / (maxOffset - lowOffset);
}