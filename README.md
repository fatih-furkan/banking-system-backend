appsettings.Developement.json file must be created locally for each microservice with the context:

```
{
  "ConnectionStrings": {
    "OracleDb": "User Id=...;Password=...;Data Source=...;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```
