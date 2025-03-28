# trackerping_service
A tracker that make n pings in a server, register when it makes a ping in your database, the user don't know this, because is a hidden service for Windows.

## How to use
1. Create your own database.

In this database, you can view the activity in any computer, whe frequency will depend on how long the ping is scheduled.

This is an example database for SQL Server:

```sql
CREATE TABLE EquipoRemoto (
    EquipoRemoto INT IDENTITY(1,1) PRIMARY KEY,
    NombreEquipo VARCHAR(50) NOT NULL,
    FechaHora DATETIME DEFAULT GETDATE(),
    IpEquipo VARCHAR(50) NOT NULL
)
```

You need make a API RESTFUL that make inserts in this TABLE.

Is important that the API RESTFUL return the response `IsError`, `Message`, `Data` and `Data1`.

2. Programming

This project use C# and .NET RUNTIME (https://dotnet.microsoft.com/es-es/download), and Visual Studio 2022.

You need make someone configurations, with a click in your project -> Properties:

![image](https://hackmd.io/_uploads/HkA5W57a1x.png)

In Assembly Name, you can change this text for the name that you need for you service:

![image](https://hackmd.io/_uploads/H14JG5mTye.png)

And now, in Program.cs, you can search the line 27, you need set your server where you need make ping's:
```csharp

string ipAddress = "you.api.com";`

```

Now, you need search execution of `EnviarDatosEquipo` in the line 40:
```csharp

await EnviarDatosEquipo(equipoInfo, "tu.api.com/EquipoRemotoPing");

```

equipoInfo is the model with your PC Data.

You can modify the update frecuency in the line 49:
```csharp

Thread.Sleep(600000); // Wait 10 minutes
```
