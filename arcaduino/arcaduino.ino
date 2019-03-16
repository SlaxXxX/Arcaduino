const int buttonCount = 54;
bool digButtons[buttonCount];
byte axis[2];

void setup() {
  axis[0] = 0;
  axis[1] = 0;
  Serial.begin(9600);
  for (byte i=0 ;i<buttonCount; i++) {
    pinMode(i, INPUT_PULLUP);
    digButtons[i] = 0;
  }
}

void loop() {
  byte xValue = analogRead(A0) >> 2;
  byte yValue = analogRead(A1) >> 2;

  for (byte i=0 ;i<buttonCount; i++) {
    bool button = digitalRead(i);
    if (button != digButtons[i]) {
      digButtons[i] = button;
      byte data = (button << 6) | i;
      Serial.write(data);
    }
  }

  if (xValue != axis[0] || yValue != axis[1]) {
    axis[0] = xValue;
    axis[1] = yValue;
    Serial.write(0b10000000);
    Serial.write(xValue);
    Serial.write(yValue);
  }
  delay(20);
}
