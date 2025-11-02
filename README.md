# StockCare

Sistema sencillo de control de inventario con ASP.NET Core MVC y Entity Framework Core (SQL Server).

Instrucciones rápidas:

1. Edita el `appsettings.json` y ajusta la cadena de conexión `DefaultConnection` a tu servidor SQL Server.

2. Desde la carpeta del proyecto:

```bash
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

3. Abre `https://localhost:5001` o la URL que muestre la salida.

Notas:
- No se usan datos seed. Registra productos y movimientos desde la interfaz.
- El panel principal muestra alertas de stock bajo y reportes básicos.
