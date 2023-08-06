# Windows-backgroundService-demo
This is windows service with background service.
To interop with native Windows Services from .NET IHostedService implementations, you'll need to install the **_Microsoft.Extensions.Hosting.WindowsServices_** NuGet package.

# To install as windows service by using **powershell** run the following command:
## Open the powershell with administrative
```
sc.exe create "Sql Database Backup" binpath="[Build files directory]\WorkerServiceDemo.exe"
```

# How to install windows Service using CMD

Run cmd with administrative mode.
For 32bit OS, Navigate to 
```
C:\Windows\Microsoft.NET\Framework\v4.0.30319
```
For 64bit OS,navigate to
```
C:\Windows\Microsoft.NET\Framework64\v4.0.30319
```
After navigating, Run the following command-
```
installutil "[Service directory]\WorkerServiceDemo.exe"
```

How to Uninstall windows Service using CMD
```
installutil /u WorkerServiceDemo.exe
```

