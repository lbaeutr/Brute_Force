using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Fuerza_Bruta_I;

class Program
{
    static void Main()
    {
        string contrasena = @"C:\Users\lbaen\Desktop\Fuerza_Bruta_I\Fuerza_Bruta_I\passwords.txt";

        if (File.Exists(contrasena))
        {
            string[] contrasenasArchivo = File.ReadAllLines(contrasena);

            // Calculo del hash SHA-256 de la contraseña objetivo
            string hashToCrack = ComputeSha256Hash("!boshito!");
            Console.WriteLine($"Buscando la contraseña con hash: {hashToCrack}");

            var seTermino = new Wrapper<string>(""); // Variable compartida entre hilos
            var seHaEnterado = new Wrapper<bool>(false); // Notificación de finalización

            Action<int> finalizar = (hiloNumero) =>
            {
                Console.WriteLine();
                Console.WriteLine($"Fuerza bruta terminada en el Hilo: {hiloNumero}");
            };

            Console.WriteLine("Iniciando fuerza bruta con dos hilos");

            // Dividir la lista en dos partes para los dos hilos
            int mitad = contrasenasArchivo.Length / 2;

            // Lanzamiento de los dos hilos
            Thread t1 = new Thread(() =>
                FuerzaBruta(contrasenasArchivo, 0, mitad, hashToCrack, seTermino, seHaEnterado, finalizar, 1));
            Thread t2 = new Thread(() => FuerzaBruta(contrasenasArchivo, mitad, contrasenasArchivo.Length, hashToCrack,
                seTermino, seHaEnterado, finalizar, 2));

            t1.Start();
            t2.Start();

            // Esperar hasta que se encuentre la contraseña
            while (string.IsNullOrEmpty(seTermino.Value))
            {
                // Esperando que algún hilo termine
            }

            // Notificar a los hilos que la contraseña fue encontrada
            seHaEnterado.Value = true;

            t1.Join();
            t2.Join();

            if (!string.IsNullOrEmpty(seTermino.Value))
            {
                Console.WriteLine($"\n Contraseña encontrada: {seTermino.Value}");
            }
            else
            {
                Console.WriteLine("No se encontro la contraseña.");
            }
        }
        else
        {
            Console.WriteLine("El archivo no existe");
        }
    }

    // Método para calcular el hash SHA-256
    static string ComputeSha256Hash(string data)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(data));
            StringBuilder
                builder = new StringBuilder(); // Crea un objeto StringBuilder para almacenar los bytes calculados.
            foreach (byte b in bytes)
            {
                builder.Append(
                    b.ToString("x2")); // Convierte el byte a un string hexadecimal de dos dígitos parte del proceso de hash de bytes en representacion hexadecimal.
            }

            return builder.ToString(); // Devuelve el hash en formato hexadecimal.
        }
    }

    // Método de fuerza bruta con rangos para múltiples hilos
    static void FuerzaBruta(string[] passwords, int start, int end, string hash, Wrapper<string> seTermino,
        Wrapper<bool> seHaEnterado, Action<int> finalizar, int hiloNumero)
    {
        string ultimaProbada = "";

        for (int i = start; i < end && string.IsNullOrEmpty(seTermino.Value); i++)
        {
            ultimaProbada = passwords[i];
            Console.WriteLine($"[Hilo {hiloNumero}] Procesando: {passwords[i]}");

            if (ComputeSha256Hash(passwords[i]) == hash)
            {
                seTermino.Value = passwords[i]; // Guardamos la contraseña
                seHaEnterado.Value = true; // Decimos  a los demás hilos que se encontró la contraseña
               // Console.WriteLine($"Hilo {hiloNumero} terminado, última contraseña probada: {passwords[i - 1]}");
               Console.WriteLine($"El hilo {hiloNumero} ha encontrado la contrasena: {passwords[i]}");
               break;
            }
        }

        Console.WriteLine($"[Hilo {hiloNumero}] terminado, ultima contrasena : {ultimaProbada}");

        while (!seHaEnterado.Value)
        {
            // Esperar a que se termine la fuerza bruta
        }

        finalizar?.Invoke(hiloNumero);
    }
}