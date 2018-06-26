# API Usage Rate Tool

Build

```
git clone https://github.com/idkiller/UsageRateTool.git
cd UsageRateTool
dotnet build
```

Get Markdown API usage table only

```
dotnet <UsageRateTool directory>/UsageRateTool.dll <API dlls>
```


Get Markdown API usage table from target dll

```
dotnet <UsageRateTool directory>/UsageRateTool.dll [-s] <API dlls> [-t] <target dlls>
```
