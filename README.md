# Windows-backgroundService-demo
This is windows service with background service.
To interop with native Windows Services from .NET IHostedService implementations, you'll need to install the **_Microsoft.Extensions.Hosting.WindowsServices_** NuGet package.

# To install as windows service by using **powershell** run the following command:
## Open the powershell with administrative
```
sc.exe create "Sql Database Backup" binpath="[Build files directory]\WorkerServiceDemo.exe"
```
