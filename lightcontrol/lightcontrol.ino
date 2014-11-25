int redPin = 3;
int greenPin = 5;
int bluePin = 6;

// the setup routine runs once when you press reset:
void setup() {
  // initialize serial communication at 9600 bits per second:
  Serial.begin(9600);
  // debug pin
  pinMode(13, OUTPUT);
  // RGB Out
  pinMode(3, OUTPUT);
  pinMode(5, OUTPUT);
  pinMode(6, OUTPUT);
}

char buf[3];

// the loop routine runs over and over again forever:
void loop() {
  while (Serial.available()) {
    int r = Serial.parseInt();
    Serial.read();
    int g = Serial.parseInt();
    Serial.read();
    int b = Serial.parseInt();
    Serial.read();
    digitalWrite(13, LOW);
    analogWrite(redPin, r);
    analogWrite(greenPin, g);
    analogWrite(bluePin, b);
  }
  delay(1);        // delay in between reads for stability
}
