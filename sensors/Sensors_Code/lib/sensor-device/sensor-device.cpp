#include <sensor-device.h>

#define TEN_MIN_DELAY 600000

uint8_t dataBuffer[8];
unsigned long idBuffer;
uint8_t dataLength;

uint8_t _deviceType;
MCP_CAN _CAN0(CAN_CS);
uint8_t _deviceId;

CAN_Message _receivedMessage = {0, 8, {0, 0, 0, 0, 0, 0, 0, 0}};

bool _recivedDeviceId = false;
void (*_receiveCallback)(CAN_Message msg);

void _handle_receive();

void _send_data(uint8_t msg_type, uint8_t optional_device_data_source)
{
    unsigned long priority = 1;
    idBuffer = 0;
    idBuffer = (priority << 26) | ((unsigned long)msg_type << 22) | ((unsigned long)_deviceId << 14) | ((unsigned long)optional_device_data_source << 13);

    _CAN0.sendMsgBuf(idBuffer, 0x1, dataLength, dataBuffer);
}

void _requestId()
{
    // get the unique id on the buffer
    for (uint8_t i = 0; i < 8; i++)
    {
        dataBuffer[i] = 0;
        dataBuffer[i] = (UNIQUE_ID >> (8 * i)) & 0xFF;
    }

    dataLength = sizeof(UNIQUE_ID);
    dataBuffer[dataLength++] = _deviceType;

    _send_data(MSG_TYPE_REGISTER_REQ, 0);
}

void init_sensor(uint8_t deviceType, void (*receiveCallback)(CAN_Message msg))
{
    srand(UNIQUE_ID);

    delay(random(800, 3000));

    byte stat = _CAN0.begin(MCP_ANY, CAN_500KBPS, MCP_16MHZ);
    if (stat == CAN_OK)
    {
        // GOOD
        _CAN0.setMode(MCP_NORMAL);
    }
    else
    {

        // BAD
        delay(TEN_MIN_DELAY);
        wdt_enable(WDTO_15MS);
        while (true)
        {
            delay(1000);
        }
    }

    pinMode(CAN_INT, INPUT_PULLUP);

#ifdef __AVR_ATtinyX5__
    if(digitalPinToInterrupt(CAN_INT) == NOT_AN_INTERRUPT) {
        // Enable Pin Change Interrupt
        GIMSK |= (1 << PCIE);  // Enable Pin Change Interrupts
        PCMSK |= 0;
        PCMSK |= (1 << CAN_INT);  // Enable Pin Change Interrupt on PB5
        sei();
    }
#endif
#ifndef __AVR_ATtinyX5__
attachInterrupt(digitalPinToInterrupt(CAN_INT), _handle_receive, FALLING);
#endif
    _deviceType = deviceType;
    _receiveCallback = receiveCallback;

    _requestId();
    uint32_t retries = 10 * 5;
    uint32_t retryCount = 0;

    while (!_recivedDeviceId)
    {
        if (retryCount > retries)
        {
            // after some time retry to get the Id
            retryCount = 0;
            _requestId();
        }

        delay(100);
        retryCount++;
    }
}

ISR(PCINT0_vect) {
    // Code to execute when the interrupt occurs
    if(digitalRead(CAN_INT) == LOW) {
        _handle_receive();
    }
    
}

void _handle_receive()
{
    _CAN0.readMsgBuf(&idBuffer, &dataLength, dataBuffer);
    if (!_recivedDeviceId)
    {
        if (((idBuffer & MSG_TYPE_MASK) >> 22) == MSG_TYPE_REGISTER_RESP)
        {
            uint32_t uniqueId =
                ((uint32_t)dataBuffer[3] << 24) | ((uint32_t)dataBuffer[2] << 16) | ((uint32_t)dataBuffer[1] << 8) | dataBuffer[0];

            if (uniqueId == UNIQUE_ID)
            {
                _deviceId = dataBuffer[dataLength - 1];

                _recivedDeviceId = true;
            }
        }
    }
    else
    {
        if (((idBuffer & MSG_ID_MASK) >> 14) == _deviceId)
        {
            for(uint8_t i = 0; i < dataLength; i++) {
                _receivedMessage.data[i] = 0;
                _receivedMessage.data[i] = dataBuffer[i];
            }

            _receivedMessage.dataLegth = dataLength;
            _receivedMessage.messageType = (idBuffer & MSG_TYPE_MASK) >> 22;

            _receiveCallback(_receivedMessage);
        }
        else if (((idBuffer & MSG_ID_MASK) >> 14) == 0 && ((idBuffer & MSG_TYPE_MASK) >> 22) == MSG_TYPE_PING)
        {
            //send device info as ping
            for (uint8_t i = 0; i < 8; i++)
            {
                dataBuffer[i] = 0;
                dataBuffer[i] = (UNIQUE_ID >> (8 * i)) & 0xFF;
            }

            dataLength = sizeof(UNIQUE_ID);
            dataBuffer[dataLength++] = _deviceType;

            _send_data(MSG_TYPE_PING, 0);
        }
    }
}

void send_data(uint8_t msg_type, int value, uint8_t data_source_type)
{
    for (uint8_t i = 0; i < 8; i++)
    {
        dataBuffer[i] = 0;
        dataBuffer[i] = (value >> (8 * i)) & 0xFF;
    }

    dataLength = sizeof(value);

    if(dataLength == 0 && value == 0) {
        dataLength = 1;
    }

    _send_data(msg_type, data_source_type);
}

void send_data(uint8_t msg_type, int value)
{
    send_data(msg_type, value, 0);
}

void send_data(uint8_t msg_type, float value)
{
    int data = (int)(value*100.0f);
    send_data(msg_type, data);
}

void send_data(uint8_t msg_type, float value, uint8_t data_source_type)
{
    int data = (int)(value*100.0f);
    send_data(msg_type, data, data_source_type);
}