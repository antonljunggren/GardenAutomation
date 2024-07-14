#ifndef SENSOR_CAN_H
#define SENSOR_CAN_H

#include <SPI.h>
#include <avr/interrupt.h>
#include <avr/wdt.h>
#include <mcp_can.h>
#ifndef CAN_CS
#define CAN_CS PIN_PB4
#endif
#ifndef CAN_INT
#define CAN_INT PIN_PB5
#endif
#ifndef UNIQUE_ID
#define UNIQUE_ID 24402
#endif

#define DEVICE_TYPE_TEMP_SENSOR 0
#define DEVICE_TYPE_SOIL_MOISTURE_SENSOR 1
#define DEVICE_TYPE_SOIL_TEMP_SENSOR 2
#define DEVICE_TYPE_WATER_PUMP 3

#define MSG_TYPE_REGISTER_REQ 0
#define MSG_TYPE_REGISTER_RESP 1
#define MSG_TYPE_DATA 2
#define MSG_TYPE_PING 3
#define MSG_TYPE_CMD 4
#define MSG_TYPE_DEVICE_STATE 5

// first 2 bits are filler as 29 bit CAN ID < 32bit integer
#define MSG_TYPE_MASK 0b00000011110000000000000000000000
#define MSG_ID_MASK 0b00000000001111111100000000000000

struct CAN_Message
{
    uint8_t messageType;
    uint8_t dataLegth;
    uint8_t data[8];
};

// device data type is a device scecific identifier if device has multiple data sources
void init_sensor(uint8_t deviceType, void (*receiveCallback)(CAN_Message msg));
void send_data(uint8_t msg_type, int value, uint8_t device_data_source);
void send_data(uint8_t msg_type, int value);
void send_data(uint8_t msg_type, float value, uint8_t device_data_source);
void send_data(uint8_t msg_type, float value);

#endif