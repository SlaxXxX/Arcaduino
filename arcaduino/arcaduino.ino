const int buttonCount = 54;
bool digButtons[buttonCount];

void setup() {
  Serial.begin(9600);
  for (byte i=0 ;i<buttonCount; i++) {
    pinMode(i, INPUT_PULLUP);
    digButtons[i] = 0;
  }
}

void loop() {
  byte x1Value = (analogRead(A0) >> 2) & 0b11111111;
  byte y1Value = (analogRead(A1) >> 2) & 0b11111111;

  for (byte i=0 ;i<buttonCount; i++) {
    bool button = digitalRead(i);
    if (button != digButtons[i]) {
      digButtons[i] = button;
      byte data = (button << 6) | i;
      Serial.write(data);
    }
  }
  Serial.write(0b10000001);
  Serial.write(x1Value);
  Serial.write(y1Value);
  delay(20);
}
