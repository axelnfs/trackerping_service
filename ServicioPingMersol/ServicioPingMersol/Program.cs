using System;
using System.Management;
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

        string ipAddress = "https://localhost:7001";
        Ping pingSender = new();

        while (true)
        {
            try
            {
                EquipoInfo equipoInfo = ObtenerEquipoInfo();

                // Verificar si el equipo está activo
                if (EquipoEstaActivo())
                {
                    PingReply reply = pingSender.Send("google.com.mx"); // Aquí deberíamos hacer ping a nuestro propio servidor
                    if (reply.Status == IPStatus.Success)
                    {
                        Console.WriteLine($"Ping a {ipAddress} - Estado: {reply.Status}");
                        await EnviarDatosEquipo(equipoInfo, "https://localhost:7001/api/Tracking");
                    }
                }
                else
                {
                    Console.WriteLine("El equipo no está activo. No se enviarán datos.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            Thread.Sleep(1000);
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
            Console.WriteLine($"Excepción al enviar datos del equipo: {ex.Message}");
        }
    }

    /// <summary>
    /// Verifica si el equipo está activo (no bloqueado, hibernando o suspendido).
    /// </summary>
    private static bool EquipoEstaActivo()
    {
        try
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_ComputerSystem"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    string userName = obj["UserName"] as string;
                    if (!string.IsNullOrEmpty(userName))
                    {
                        return true;
                    }
                }
            }

            // Verificar si el sistema está en modo de suspensión o hibernación
            using (ManagementObjectSearcher powerSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PowerManagementEvent"))
            {
                foreach (ManagementObject powerEvent in powerSearcher.Get())
                {
                    int eventType = Convert.ToInt32(powerEvent["EventType"]);
                    // EventType 4: Suspendido, EventType 7: Reanudado
                    if (eventType == 4) // Suspendido
                    {
                        return false;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al verificar el estado del equipo: {ex.Message}");
        }

        // Si no se detecta actividad, se considera inactivo
        return false;
    }

}

/// <summary>
/// Clase que representa la información del equipo.
/// </summary>
public class EquipoInfo
{
    required public string NombreEquipo { get; set; }
    required public string IpEquipo { get; set; }
}

/// <summary>
/// Clase que representa la respuesta de la API.
/// </summary>
public class ApiResponse
{
    public bool IsError { get; set; }
    required public string Message { get; set; }
    required public object Data { get; set; }
    required public object Data1 { get; set; }
}
