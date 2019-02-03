// the setup routine runs once when you press reset:
void setup() {
  // initialize serial communication at 9600 bits per second:
  Serial.begin(9600);
}

// the loop routine runs over and over again forever:
void loop() {
  // read the input on analog pin 0:
  int xValue = analogRead(A0);
  int yValue = analogRead(A1);

  Serial.print("X: "); Serial.print(xValue); Serial.print("  ");
  Serial.print("Y: "); Serial.print(yValue); Serial.print("  ");
  Serial.println("uT");
  
  delay(1);        // delay in between reads for stability
}
