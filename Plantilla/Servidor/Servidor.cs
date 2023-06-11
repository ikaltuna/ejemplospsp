using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

class MyTcpListener
{

    TcpListener server = null;
    TcpClient client = null;
    NetworkStream str = null;
    StreamReader sr = null;
    StreamWriter sw = null;

    //Funcionalidad: Constructor de la clase.
    public MyTcpListener()
    {

    }

    //Funcionalidad: Función principal del servidor. Conecta con el cliente, recibe datos, trata los datos y devuelve el resultado.
    public static int Main(String[] args)
    {

        Process p;
        Thread.Sleep(4000);
        StartCliente(out p);

        static void StartCliente(out Process p1)
        {

            ProcessStartInfo info = new ProcessStartInfo(@"..\..\..\..\Cliente\Cliente.exe");
            p1 = Process.Start(info);

        }



        MyTcpListener appservidor = new MyTcpListener();
        Int32 port = 13000;
        IPAddress localAddr = IPAddress.Parse("127.0.0.1");
        appservidor.Conectar(localAddr, port);



        while (true)
        {
            string municipio = appservidor.RecepcionDatosMunicipio();

            if (municipio != string.Empty)
            {
                municipio = municipio.Trim();
                municipio = municipio.Replace("<EOF>", "");
                appservidor.ConsultaTempEnviar(municipio);
            }
            else
            {
                appservidor.Cerrar();
                break;
            }

        }

        return 0;
    }
    //Funcionalidad: Crea el socket y los buffers de lectura y escritura. Se queda a la espera de que se conecte algún cliente.
    //               Lanza el proceso cliente.
    //Entradas: IP del servidor y puerto.

    private void Conectar(IPAddress server, Int32 port)
    {
        //FALTA POR DESARROLLAR
        try
        {

            this.server = new TcpListener(server, port);

            // Start listening for client requests.
            this.server.Start();
            this.client = this.server.AcceptTcpClient(); //linea bloqueante (espera hasta que se conecta algún cliente).

            this.str = this.client.GetStream();
            this.sr = new StreamReader(this.str);
            this.sw = new StreamWriter(this.str);

        }
        catch (Exception e)
        {
            Console.WriteLine("Excepción creación de socket o buffer: {0}", e);
        }
    }

    //Funcionalidad: Recibe el nombre del municipio del cliente.
    //Salida: String con el nombre del municipio.

    private string RecepcionDatosMunicipio()
    {
        string data = string.Empty;
        try
        {
            while (true)
            {
                data += sr.ReadLine();
                if (data.Contains("<EOF>"))
                {
                    return data;
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Cerrar();
            Console.WriteLine("Error envio de datos, cerrando aplicación: {0}", e);
            return string.Empty;
        }
    }

    //Funcionalidad: Consulta la temperatura de un municipio y la envía al cliente. 
    //Entradas: String con el nombre del municipio.
    //Salidas: String con la temperatura del municipio.


    private async void ConsultaTempEnviar(string municipio)
    {
        //FALTA POR DESARROLLAR
        string apikey = "a285be9e83300f50a8a42ffbd0da297a";
        string url = $"https://api.openweathermap.org/data/2.5/weather?q=" + municipio + "&appid=" + apikey + "&units=metric";
        Console.WriteLine("Datos {0}, consultado. \n", municipio);
        Console.WriteLine(url);

        using (HttpClient client = new HttpClient())

        using (HttpResponseMessage response = await client.GetAsync(url))
        {
            string datociudad = await response.Content.ReadAsStringAsync();
            dynamic objDatosCiudad = JValue.Parse(datociudad);
            string temperatura = objDatosCiudad.main.temp;
            temperatura = EncriptarTexto(temperatura);
            temperatura = temperatura + "<EOF>";

            sw.WriteLine(temperatura);
            sw.Flush();
            Console.WriteLine("Respuesta {0} temperatura enviada al cliente.", temperatura);
        }
    }

    //Funcionalidad: Encripta el texto que se le pasa como parámetro. Algoritmo utilizado para la ecriptación simétrica AES.
    //Entradas: String con el texto a encriptar.
    //Salidas: String con el texto encriptado.Para convertir el string a bytes se usa la función Convert.ToBase64String.
    //Restricciones: El texto a encriptar no puede ser nulo.
    //Salidas: Fichero clave.txt y fichero vector.txt que contiene en bytes la clave y el vector para que el cliente pueda desencriptar el texto. 
    //         Los ficheros se guardarán con ruta relativa en directorio Plantilla.
    static string EncriptarTexto(String Data)
    {

        //FALTA POR DESARROLLAR
        byte[] encriptado_bytes;
        using (Aes Aesalg = Aes.Create())
        {

            File.WriteAllBytes("../../../../Clave/clave.txt", Aesalg.Key);
            File.WriteAllBytes("../../../../Clave/vector.txt", Aesalg.IV);

            ICryptoTransform encryptor = Aesalg.CreateEncryptor(Aesalg.Key, Aesalg.IV);


            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream cStream = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(cStream))
                    {
                        swEncrypt.Write(Data);
                    }
                    encriptado_bytes = msEncrypt.ToArray();
                }
            }
            string encriptado_string = Convert.ToBase64String(encriptado_bytes);
            return encriptado_string;
        }
    }

    //Funcionalidad: Cierra las conexiones del servidor.
    private void Cerrar()
    {
        try
        {
            this.sr.Close();
            this.sw.Close();
            this.str.Close();
            this.client.Close();
            Console.WriteLine("Todas las conexiones cerradas");
        }
        catch (Exception e)
        {
            Console.WriteLine("Error en el cierre de conexión: {0}", e);
        }
    }


}
