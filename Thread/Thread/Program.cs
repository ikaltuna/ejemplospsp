using System;
using System.Threading;

class Program
{

    static object bloqueaCalculo = new object();
    public static bool final = false;
    static int ganador = -1;
    static Random rand = new Random();

    static void Main(string[] args)
    {
        while (!final)
        {
            Thread empezarPartida = new Thread(partida);
            empezarPartida.Start();
            empezarPartida.Join();
        }
        


    }
    public static void partida()
    {
        
        Thread jugador1 = new Thread(AvanceJugadores);
        Thread jugador2 = new Thread(AvanceJugadores);

        jugador1.Name = "1";
        jugador2.Name = "2";

        
       
         jugador1.Start(1);
         jugador1.Join(1);
         jugador2.Start(2);
         jugador2.Join(2);


    }
    public static void AvanceJugadores(object id)
    {

        int contador = 0;
        

        
        lock (bloqueaCalculo)
        {

            contador = 0;
            for (int i = 0; i < 10; i++)
            {
                contador = contador + rand.Next(0, 20);
            }

            if ((contador > 100) && !final)
            {
                final = true;
                ganador = (int)id;
                Console.WriteLine("El jugador {0} ha sacado {1} puntos", id, contador);
                Console.WriteLine("El juego ha terminado, el ganador es {0}. Con {1} puntos", id, contador);
            }
            else
            {
                if (final)
                {
                    Console.WriteLine("El juego ha terminado, no soy el ganador y tengo el id {0}. Puntos que tenia {1} ", id, contador);
                }
                else
                {
                    Console.WriteLine("El jugador {0} ha sacado {1} puntos", id, contador);
                }
            }
        }
    }
}