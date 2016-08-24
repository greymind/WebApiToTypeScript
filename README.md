# Web API To TypeScript
A tool for code generating TypeScript endpoints for your ASP.NET Web API controllers and their actions

## Using as standalone tool
Just run the executable with the config file as the argument
```
watts.exe "Path/To/Config/File.json"
```

## Using as MSBuild Target
Ensure the DLL is part of the build process, so you have access to it. Easiest way is to reference it in your project.
```
<UsingTask TaskName="WebApiToTypeScript.WebApiToTypeScript" AssemblyFile="$(ProjectDir)..\WebApiToTypeScript\bin\Debug\WebApiToTypeScript.dll" />
<Target Name="AfterBuild">
    <WebApiToTypeScript ConfigFilePath="$(ProjectDir)Watts.config.json" />
</Target>
```

## Config
```
{
  "WebApiModuleFileName": "string",
  "EndpointsOutputDirectory": "string",
  "EndpointsFileName": "string",
  "EndpointsNamespace": "string",
  "WriteNamespaceAsModule": "boolean",
  "GenerateInterfaces": "boolean",
  "ScanOtherModules": "boolean",
  "GenerateEnums": "boolean",
  "EnumsOutputDirectory": "string",
  "EnumsFileName": "string",
  "EnumsNamespace": "string",
  "TypeMappings": [
    {
      "WebApiTypeName": "string",
      "TypeScriptTypeName": "string",
      "AutoInitialize": "boolean",
      "TreatAsAttribute": "boolean"
    }
  ]
}
```

## Team
* [Balakrishnan (Balki) Ranganathan](https://github.com/greymind)
* [Augustin Juricic](https://github.com/omittones)

## License
MIT