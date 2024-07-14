#include <SPI.h>
#include <mcp_can.h>

MCP_CAN CAN0(4);

#define LED_PIN 3

void setup()
{
    // Serial.begin(115200);
    pinMode(LED_PIN, OUTPUT);
    digitalWrite(LED_PIN, LOW);

    // Initialize MCP2515 running at 16MHz with a baudrate of 500kb/s and the masks and filters disabled.
    if (CAN0.begin(MCP_ANY, CAN_500KBPS, MCP_16MHZ) == CAN_OK)
        Serial.println("MCP2515 Initialized Successfully!");
    else
    {
        digitalWrite(LED_PIN, HIGH);
        delay(500);
        digitalWrite(LED_PIN, LOW);
        delay(500);
    }

    CAN0.setMode(MCP_NORMAL); // Change to normal mode to allow messages to be transmitted
}

byte data[8] = {0x12, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x14};

void loop()
{
    // send data:  ID = 0x100, Standard CAN Frame, Data length = 8 bytes, 'data' = array of data bytes to send
    byte sndStat = CAN0.sendMsgBuf(0x100, 0, 8, data);
    if (sndStat == CAN_OK)
    {
        // Serial.println("Message Sent Successfully!");
        digitalWrite(LED_PIN, HIGH);
        delay(1000);
        digitalWrite(LED_PIN, LOW);
        delay(1000);
    }
    else
    {
        // Serial.println("Error Sending Message...");
    }
    delay(1000); // send data per 1000ms
}