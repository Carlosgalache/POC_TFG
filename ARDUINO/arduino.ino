//Variables motor de vibración por dedo 
//Responde al número de pin de la placa a la que está conectado el polo positivo del motor
int motorIndice = 5;
int motorPulgar = 3;
int motorCorazon = 6;
int motorAnular = 9;
int motorMenique = 10;

//Inicio de motores en estado apagado
int motorstate = LOW;

//Variable intesidad para pulsos PWM
int intensidad;

//Funcion: Setup
void setup() 
{
  //Indicamos a arduino que los pines de los motores serán utilizados como salidas de voltaje
  pinMode(motorIndice, OUTPUT);  
  pinMode(motorPulgar, OUTPUT); 
  pinMode(motorCorazon, OUTPUT); 
  pinMode(motorMenique, OUTPUT); 
  pinMode(motorAnular, OUTPUT); 
  
  //Indicamos la velocidad de transmisión del puerto 
  //para que sea igual a la de envío de datos del ordenador generados por el software de interconexión 38,400.
  Serial.begin(38400);
}

//Función que devuelve el pin de salida de arduino asociado a un dedo
int GetMotor(int finger)
{
  if (finger == 0)
    return motorPulgar;
  else if (finger == 1)
    return motorIndice;
  else if (finger == 2)
    return motorCorazon;
  else if (finger == 3)
    return motorAnular;
  else if (finger == 4)
    return motorMenique;
}

//Función que gestiona el estímulo Fuego, Agua, Tierra, Aire y Frio
void processFire(int finger) {
  int motor = GetMotor(finger);
  analogWrite(motor,255);
}
void processEarth(int finger) {
  int motor = GetMotor(finger);
  analogWrite(motor,150);
}
void processWater(int finger) {
  int motor = GetMotor(finger);
  analogWrite(motor,100);
}
void processWind(int finger) {
  int motor = GetMotor(finger);
  analogWrite(motor,200);
}
void processCold(int finger) {
  int motor = GetMotor(finger);
  analogWrite(motor,0);
}

//Comienzo del LOOP 
void loop() 
{ 
  
  //Indicamos que la variable tiempo debe medirse en milisegundos.
  unsigned long currentTime = millis();

  //Arduino debe comprobar si el puerto serie está enviando información
  if (Serial.available())
  {
    //Esperar 20 milisegundos para volver a leer el puerto.
    delay(20);
    
    //Our data.
    String data = "";
    
    //Indicamos a Arduino mientras lleguen señales por el puerto serie 
    //se deben leer como conjuntos de 5 caracteres.
    while (Serial.available())
      data += (char) (Serial.read());
    //Crea un array de 5 caracteres
    char c[5];
    data.toCharArray(c, 6);
    
    //Procesa los 5 caracteres de entrada asociados a cada dedo
    for(int i = 0;i<5;i++)
    {
      if (c[i] == 'F')
      {
        processFire(i);
      } else if (c[i] == 'T') {
        processEarth(i);
      } else if (c[i] == 'W') {
        processWater(i);
      } else if (c[i] == 'A') {
        processWind(i);
      } else if (c[i] == 'C') {
        processCold(i);
      }
    }
  }
}