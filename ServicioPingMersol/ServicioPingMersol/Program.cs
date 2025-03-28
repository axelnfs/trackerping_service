using System;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    [DllImport("user32.dll")]
    private static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    private const int SW_HIDE = 0;

    static async Task Main()
    {
        // Ocultar la consola
        IntPtr handle = GetConsoleWindow();
        ShowWindow(handle, SW_HIDE);

        string ipAddress = "you.api.com";
        Ping pingSender = new Ping();

        while (true)
        {
            try
            {
                EquipoInfo equipoInfo = ObtenerEquipoInfo();

                PingReply reply = pingSender.Send(ipAddress);
                if (reply.Status == IPStatus.Success)
                {
                    Console.WriteLine($"Ping a {ipAddress} - Estado: {reply.Status}");
                    await EnviarDatosEquipo(equipoInfo, "tu.api.com/EquipoRemotoPing");
                }
            }
            catch (Exception ex)
            {
                // Manejar la excepción sin interrumpir la ejecución del programa
                Console.WriteLine($"Error: {ex.Message}");
            }

            Thread.Sleep(600000); // Esperar 10 MINUTOS
        }
    }

    private static EquipoInfo ObtenerEquipoInfo()
    {
        string nombreEquipo = Environment.MachineName;
        string ipEquipo = ObtenerIpEquipo();

        return new EquipoInfo
        {
            NombreEquipo = nombreEquipo,
            IpEquipo = ipEquipo
        };
    }

    private static string ObtenerIpEquipo()
    {
        string ipAddress = string.Empty;
        foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                ipAddress = ip.ToString();
                break;
            }
        }
        return ipAddress;
    }

    private static async Task EnviarDatosEquipo(EquipoInfo equipoInfo, string url)
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                string json = JsonSerializer.Serialize(equipoInfo);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(url, content);
                string responseContent = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = JsonSerializer.Deserialize<ApiResponse>(responseContent);
                    Console.WriteLine($"Datos del equipo enviados exitosamente. Mensaje: {apiResponse.Message}");
                }
                else
                {
                    Console.WriteLine($"Error al enviar datos del equipo: {response.StatusCode} - {responseContent}");
                }
            }
        }
        catch (Exception ex)
        {
            // Manejar la excepción sin interrumpir la ejecución del programa
            Console.WriteLine($"Excepción al enviar datos del equipo: {ex.Message}");
        }
    }
}

public class EquipoInfo
{
    public string NombreEquipo { get; set; }
    public string IpEquipo { get; set; }
}

public class ApiResponse
{
    public bool IsError { get; set; }
    public string Message { get; set; }
    public object Data { get; set; }
    public object Data1 { get; set; }
}


