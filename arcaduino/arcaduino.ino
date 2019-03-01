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
//  int xValue = analogRead(A0);
//  int yValue = analogRead(A1);
//  Serial.print("X: "); Serial.print(xValue); Serial.print("  ");
//  Serial.print("Y: "); Serial.print(yValue); Serial.print("  ");
//  Serial.println("uT");

  for (byte i=0 ;i<buttonCount; i++) {
    bool button = digitalRead(i);
    if (button != digButtons[i]) {
      digButtons[i] = button;
      byte data = (button << 6) | i;
      Serial.write(data);
    }
  }
  
  delay(100);
}
