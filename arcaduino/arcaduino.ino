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
  Serial.write(0b10000000);
  Serial.write(xValue);
  Serial.write(yValue);
  delay(50);
}
