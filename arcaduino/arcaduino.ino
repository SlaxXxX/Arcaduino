const int buttonCount = 54;
bool digButtons[buttonCount];
byte axis[4];

void setup() {
  axis[0] = 0;
  axis[1] = 0;
  axis[2] = 0;
  axis[3] = 0;
  Serial.begin(9600);
  for (byte i=0 ;i<buttonCount; i++) {
    pinMode(i, INPUT_PULLUP);
    digButtons[i] = 0;
  }
}

void loop() {
  byte x1Value = (analogRead(A0) >> 2) & 0b11111100;
  byte y1Value = (analogRead(A1) >> 2) & 0b11111100;
  byte x2Value = (analogRead(A2) >> 2) & 0b11111100;
  byte y2Value = (analogRead(A3) >> 2) & 0b11111100;

  for (byte i=0 ;i<buttonCount; i++) {
    bool button = digitalRead(i);
    if (button != digButtons[i]) {
      digButtons[i] = button;
      byte data = (button << 6) | i;
      Serial.write(data);
    }
  }
  byte changedAxes = 0;
  if (x1Value != axis[0]){
    axis[0] = x1Value;
    changedAxes++;
    changedAxes |= 0b00000100;
  }
  if (y1Value != axis[1]){
    axis[1] = y1Value;
    changedAxes++;
    changedAxes |= 0b00001000;
  }
  if (x2Value != axis[2]){
    axis[2] = x2Value;
    changedAxes++;
    changedAxes |= 0b00010000;
  }
  if (y2Value != axis[3]){
    axis[3] = y2Value;
    changedAxes++;
    changedAxes |= 0b00100000;
  }

  if (changedAxes != 0) {
    Serial.write(0b10000000 | changedAxes);
    if (changedAxes >= 32 ) {
      Serial.write(y2Value);
      changedAxes -=32;
    }
    if (changedAxes >= 16 ) {
      Serial.write(x2Value);
      changedAxes -=16;
    }
    if (changedAxes >= 8 ) {
      Serial.write(y1Value);
      changedAxes -=8;
    }
    if (changedAxes >= 4 ) {
      Serial.write(x1Value);
      changedAxes -=4;
    }
  }
  delay(20);
}
