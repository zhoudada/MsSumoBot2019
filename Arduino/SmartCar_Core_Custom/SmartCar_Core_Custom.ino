#include <IRremote.h>
#include <Servo.h>  

#define f 16736925  // FORWARD
#define b 16754775  // BACK
#define l 16720605  // LEFT
#define r 16761405  // RIGHT
#define s 16712445  // STOP
#define KEY1 16738455 //Line Teacking mode
#define KEY2 16750695 //Obstacles Avoidance mode
#define KEY3 16756815
#define KEY4 16724175
#define KEY5 16718055
#define KEY6 16743045
#define KEY7 16716015
#define KEY8 16726215
#define KEY9 16734885
#define KEY0 16730805
#define KEY_STAR 16728765
#define KEY_HASH 16732845

#define RECV_PIN  12
#define ECHO_PIN  A4  
#define TRIG_PIN  A5 
#define ENA 5
#define ENB 6
#define IN1 7
#define IN2 8
#define IN3 9
#define IN4 11
#define LED_Pin 13
#define LineTeacking_Pin_Right  10
#define LineTeacking_Pin_Middle 4
#define LineTeacking_Pin_Left   2
#define LineTeacking_Read_Right   !digitalRead(10)
#define LineTeacking_Read_Middle  !digitalRead(4)
#define LineTeacking_Read_Left    !digitalRead(2)
#define HIGH_SPEED 250
#define MEDIUM_SPEED 180
#define LOW_SPEED 80

Servo servo;
IRrecv irrecv(RECV_PIN);
decode_results results;
unsigned long IR_PreMillis;
unsigned long LT_PreMillis;
int rightDistance = 0, leftDistance = 0, middleDistance = 0;
unsigned int carSpeed = HIGH_SPEED;
bool isDebug = false;

enum FUNCTIONMODE{
  IDLE,
  LineTeacking,
  ObstaclesAvoidance,
  Bluetooth,
  IRremote
} func_mode = IDLE;

enum MotionMode {
  Stop,
  Forward,
  Back,
  Left,
  Right,
  LeftForward,
  RightForward,
  LeftBackward,
  RightBackward,
} motionMode = Stop;

//void delays(unsigned long t) {
//  for(unsigned long i = 0; i < t; i++) {
//    getBTData();
//    delay(1);
//  }
//}

void forward(){ 
  analogWrite(ENA, carSpeed);
  analogWrite(ENB, carSpeed);
  digitalWrite(IN1,HIGH);
  digitalWrite(IN2,LOW);
  digitalWrite(IN3,LOW);
  digitalWrite(IN4,HIGH);
  if(isDebug)
  {
    Serial.print("Go forward! Speed: ");
    Serial.println(carSpeed);
  }
}

void back(){
  analogWrite(ENA, carSpeed);
  analogWrite(ENB, carSpeed);
  digitalWrite(IN1,LOW);
  digitalWrite(IN2,HIGH);
  digitalWrite(IN3,HIGH);
  digitalWrite(IN4,LOW);
  if(isDebug)
  {
    Serial.print("Go backward! Speed: ");
    Serial.println(carSpeed);
  }
}

void left(){
  analogWrite(ENA,carSpeed);
  analogWrite(ENB,carSpeed);
  digitalWrite(IN1,LOW);
  digitalWrite(IN2,HIGH);
  digitalWrite(IN3,LOW);
  digitalWrite(IN4,HIGH); 
  if(isDebug)
  {
    Serial.print("Go left! Speed: ");
    Serial.println(carSpeed);
  }
}

void right(){
  analogWrite(ENA,carSpeed);
  analogWrite(ENB,carSpeed);
  digitalWrite(IN1,HIGH);
  digitalWrite(IN2,LOW);
  digitalWrite(IN3,HIGH);
  digitalWrite(IN4,LOW);
  if(isDebug)
  {
    Serial.print("Go right! Speed: ");
    Serial.println(carSpeed);
  }
}

void leftForward(){
  analogWrite(ENA, LOW_SPEED);
  analogWrite(ENB, HIGH_SPEED);
  digitalWrite(IN1,LOW);
  digitalWrite(IN2,HIGH);
  digitalWrite(IN3,LOW);
  digitalWrite(IN4,HIGH); 
  if(isDebug) Serial.println("Go left forward!");
}

void rightForward(){
  analogWrite(ENA, HIGH_SPEED);
  analogWrite(ENB, LOW_SPEED);
  digitalWrite(IN1,HIGH);
  digitalWrite(IN2,LOW);
  digitalWrite(IN3,HIGH);
  digitalWrite(IN4,LOW);
  if(isDebug) Serial.println("Go right forward!");
}

void leftBackward()
{
  analogWrite(ENA, LOW_SPEED);
  analogWrite(ENB, HIGH_SPEED);
  digitalWrite(IN1,HIGH);
  digitalWrite(IN2,LOW);
  digitalWrite(IN3,HIGH);
  digitalWrite(IN4,LOW);
  if(isDebug) Serial.println("Go left backward!");
}

void rightBackward()
{
  analogWrite(ENA, HIGH_SPEED);
  analogWrite(ENB, LOW_SPEED);
  digitalWrite(IN1,LOW);
  digitalWrite(IN2,HIGH);
  digitalWrite(IN3,LOW);
  digitalWrite(IN4,HIGH); 
  if(isDebug) Serial.println("Go right backward!");
}

void setHighSpeed()
{
  carSpeed = HIGH_SPEED;
  if (isDebug)
  {
    Serial.println("Set high speed.");
  }
}

void setMediumSpeed()
{
  carSpeed = MEDIUM_SPEED;
  if (isDebug)
  {
    Serial.println("Set medium speed.");
  }
}

void setLowSpeed()
{
  carSpeed = LOW_SPEED;
  if (isDebug)
  {
    Serial.println("Set low speed.");
  }
}

void stop(){
  digitalWrite(ENA, LOW);
  digitalWrite(ENB, LOW);
  if(isDebug) Serial.println("Stop!");
}

void getBluetoothData() {
  if(Serial.available()) {
    switch(Serial.read()) {
      case 'f': motionMode = Forward; break;
      case 'b': motionMode = Back; break;
      case 'l': motionMode = Left; break;
      case 'r': motionMode = Right; break;
      case 's': motionMode = Stop; break;
      case 'q': motionMode = LeftForward; break;
      case 'e': motionMode = RightForward; break;
      case 'z': motionMode = LeftBackward; break;
      case 'c': motionMode = RightBackward; break;
      case 'u': setHighSpeed(); break;
      case 'i': setMediumSpeed(); break;
      case 'o': setLowSpeed(); break;
      default:  break;
    } 
  }
}

void bluetoothMotionUpdate()
{
  switch (motionMode)
  {
    case Forward:
      forward();
      break;

    case Back:
      back();
      break;

    case Left:
      left();
      break;

    case Right:
      right();
      break;

    case Stop:
      stop();
      break;

    case LeftForward:
      leftForward();
      break;

    case RightForward:
      rightForward();
      break;

    case LeftBackward:
      leftBackward();
      break;

    case RightBackward:
      rightBackward();
      break;

    default:
      break;
  }
}

void setup() {
  Serial.begin(9600);
  servo.attach(3,500,2400);// 500: 0 degree  2400: 180 degree
  servo.write(90);
  irrecv.enableIRIn();
  pinMode(ECHO_PIN, INPUT);
  pinMode(TRIG_PIN, OUTPUT);
  pinMode(IN1, OUTPUT);
  pinMode(IN2, OUTPUT);
  pinMode(IN3, OUTPUT);
  pinMode(IN4, OUTPUT);
  pinMode(ENA, OUTPUT);
  pinMode(ENB, OUTPUT);
}

void loop() {
//  leftForward();
  getBluetoothData();
  bluetoothMotionUpdate();
}
