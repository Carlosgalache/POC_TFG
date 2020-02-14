using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using Leap;
using POCLeapMotion;

#region EXTERNAL DEVICES

/// <summary>
/// Define las funciones y eventos para interactuar con Leap Motion
/// Define las funciones para interactuar con el puerto serie (Arduino)
/// </summary>
class LeapMotionListener
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    //variable de volumen a los puntos de la lista (Cajas sensoriales)
    private static int MULTIPLIER = 50;
    
    //Lista de puntos (x,y,z,'orden')
    private List<Point> pointList = new List<Point> 
    {
        new Point(-150,150,0,'F'), // Fuego
        new Point(150,150,0,'A'), // Aire
        new Point(-150,300,0,'T'), // Tierra 
        new Point(150,300,0,'W') // Agua (Water)
    };

    //Variable para la conexion por puerto serie (ARDUINO)
    private SerialPort port = new SerialPort("COM3",
      38400, Parity.None, 8, StopBits.One);

    #region LEAPMOTION LISTENERS
    /// <summary>
    /// Función que se ejecuta al iniciar el listener (solo por información)
    /// </summary>
    /// <param name="controller">Controlador de LeapMotion que se ha iniciado</param>
    public void OnInit(Controller controller)
    {
        log.Info(string.Format("Initialized"));
    }

    /// <summary>
    /// Función que se ejecuta al conectar con LeapMotion y que abre conexión con el puerto serie (ARDUINO)
    /// </summary>
    /// <param name="sender">No se usa</param>
    /// <param name="args">No se usa</param>
    public void OnConnect(object sender, DeviceEventArgs args)
    {
        log.Info(string.Format("Connected"));
        port.WriteTimeout = 500;
        port.Open();
    }

    public void OnDisconnect(object sender, DeviceEventArgs args)
    {
        log.Info(string.Format("Disconnected"));
    }

    public void OnFrame(object sender, FrameEventArgs args)
    {
        try
        {
            // Coger la información del último frame
            Frame frame = args.frame;

            // Procesar sólo la mano derecha (prueba de LINQ)
            foreach (Hand hand in frame.Hands.Where(x => x.IsRight))
            {
                processHand(hand);

            }
            // Si Leap motio no detecta manos enviar CCCCC (Frio)
            if (frame.Hands.Count != 0)
            {
                sendCold();
                log.Info(string.Format(""));
            }
        }
        catch (Exception ex)
        {
            log.Error(ex);
        }
    }

    public void OnServiceConnect(object sender, ConnectionEventArgs args)
    {
        log.Info(string.Format("Service Connected"));
    }

    public void OnServiceDisconnect(object sender, ConnectionLostEventArgs args)
    {
        log.Info(string.Format("Service Disconnected"));
    }

    public void OnServiceChange(Controller controller)
    {
        log.Info(string.Format("Service Changed"));
    }

    public void OnDeviceFailure(object sender, DeviceFailureEventArgs args)
    {
        log.Info(string.Format("Device Error"));
        log.Info(string.Format("  PNP ID:" + args.DeviceSerialNumber));
        log.Info(string.Format("  Failure message:" + args.ErrorMessage));
    }

    public void OnLogMessage(object sender, LogEventArgs args)
    {
        switch (args.severity)
        {
            case Leap.MessageSeverity.MESSAGE_CRITICAL:
                log.Info(string.Format("[Critical]"));
                break;
            case Leap.MessageSeverity.MESSAGE_WARNING:
                log.Info(string.Format("[Warning]"));
                break;
            case Leap.MessageSeverity.MESSAGE_INFORMATION:
                log.Info(string.Format("[Info]"));
                break;
            case Leap.MessageSeverity.MESSAGE_UNKNOWN:
                log.Info(string.Format("[Unknown]"));
                break;
        }
        log.Info(string.Format("[{0}] {1}", args.timestamp, args.message));
    }
    #endregion

    #region LEAP MOTION FUNCTIONS

    /// <summary>
    /// Funcion que procesa la mano llamando a los dedos y escribe en cosnsola
    /// </summary>
    /// <param name="hand"></param>
    private void processHand(Hand hand)
    {
        //Crear el array de 5 caracteres
        char[] dataToSend = new char[5];

        //Recorro todos los dedos de la mano grabando el valor en el array
        foreach (Finger finger in hand.Fingers)
        {
            dataToSend[(int)finger.Type] = processFinger(finger);
        }

        //Mando los datos al puerto serie
        Console.WriteLine(new string(dataToSend));
        sendData(dataToSend);
    }

    /// <summary>
    /// Procesa los dedos y restringe el envío de datos a sólo la posición de las falanges
    /// </summary>
    /// <param name="finger"></param>
    /// <returns></returns>
    private char processFinger(Finger finger)
    {
        // Recorrer los huesos de la mano y devuelve sólo los distales
        Bone bone;
        for (int b = 0; b < 4; b++)
        {
            bone = finger.Bone((Bone.BoneType)b);
            if (bone.Type == Bone.BoneType.TYPE_DISTAL)
            {
                return processPhalange(bone);
            }
        }
        return 'C';
    }
   /// <summary>
   /// Aplicación de volumen a la lista de puntos, si el dedo choca con el punto + el multiplicador envía orden de caja sensorial si no envía C (Frío)
   /// </summary>
   /// <param name="bone"></param>
   /// <returns></returns>
    private char processPhalange(Bone bone)
    {
        foreach (var point in pointList)
        {
            if ((bone.PrevJoint.x > -MULTIPLIER + point.X && bone.PrevJoint.x < MULTIPLIER + point.X) && (bone.PrevJoint.z > -MULTIPLIER + point.Z && bone.PrevJoint.z < point.Z + MULTIPLIER) && (bone.PrevJoint.y > -MULTIPLIER + point.Y && bone.PrevJoint.y < MULTIPLIER + point.Y))
            {
                return point.Name;
            }
        }

        return 'C';
    }

    #endregion

    #region SERIAL PORT(ARDUINO)
    /// <summary>
    /// Manda C (Frío) a todos los actuadores para apagarlos (Arduino)
    /// </summary>
    private void sendCold()
    {
        port.Write("CCCCC");
    }

    /// <summary>
    /// ENviar datos procesados a puerto serie (Arduino)
    /// </summary>
    /// <param name="dataToSend">Byte array to send to Arduino</param>
    private void sendData(char[] dataToSend)
    {
        port.Write(new string(dataToSend));
    }
    #endregion
}

#endregion

#region MAIN PROGRAM
namespace POCLeapMotion
{
    class Program
    {
        /// <summary>
        /// Función que inicializa el listener del Arduino
        /// </summary>
        /// <param name="args">No arguments needed</param>
        static void Main(string[] args)
        {
            using (Leap.IController controller = new Leap.Controller())
            {
                controller.SetPolicy(Leap.Controller.PolicyFlag.POLICY_ALLOW_PAUSE_RESUME);

                // Iniciar el listener
                LeapMotionListener listener = new LeapMotionListener();
                controller.Connect += listener.OnServiceConnect;
                controller.Disconnect += listener.OnServiceDisconnect;
                controller.FrameReady += listener.OnFrame;
                controller.Device += listener.OnConnect;
                controller.DeviceLost += listener.OnDisconnect;
                controller.DeviceFailure += listener.OnDeviceFailure;
                controller.LogMessage += listener.OnLogMessage;

                // Mantener el programa hasta que se pulse una tecla
                Console.WriteLine(string.Format("Press any key to quit..."));
                Console.ReadLine();
            }
        }
    }
}
#endregion