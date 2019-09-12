//www.elegoo.com

//     Left motor truth table
//Here are some handy tables to show the various modes of operation.
//  ENB         IN1               IN2         Description  
//  LOW   Not Applicable    Not Applicable    Motor is off
//  HIGH        LOW               LOW         Motor is stopped (brakes)
//  HIGH        HIGH              LOW         Motor is on and turning forwards
//  HIGH        LOW               HIGH        Motor is on and turning backwards
//  HIGH        HIGH              HIGH        Motor is stopped (brakes)

// define IO pin
#define ENB 5 
#define IN1 7
#define IN2 8

//init the car
void setup() {
  pinMode(IN1, OUTPUT);     //set IO pin mode OUTPUT
  pinMode(IN2, OUTPUT);
  pinMode(ENB, OUTPUT);
  digitalWrite(ENB, HIGH);  //Enable left motor
}

//mian loop
void loop() {
  digitalWrite(IN1, HIGH);
  digitalWrite(IN2, LOW);   //Right wheel turning forwards
  delay(1000);              //delay 500ms
  digitalWrite(IN1, LOW);
  digitalWrite(IN2, LOW);   //Right wheel stoped
  delay(1000);
  digitalWrite(IN1, LOW);
  digitalWrite(IN2, HIGH);  //Right wheel turning backwards
  delay(1000);
  digitalWrite(IN1, HIGH);
  digitalWrite(IN2, HIGH);  //Right wheel stoped
  delay(1000);
}
