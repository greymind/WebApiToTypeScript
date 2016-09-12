# Web API To TypeScript
A tool for code generating TypeScript endpoints for your ASP.NET Web API controllers and their actions

## Getting the tool from NuGet
[https://www.nuget.org/packages/WebApiToTypeScript/](https://www.nuget.org/packages/WebApiToTypeScript/)
```
Install-Package WebApiToTypeScript
```

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

## Using Angular Endpoints Service
You'll need to register the Endpoints service to your app and inject it as is typical in Angular
```
angular.module('framework').service('AngularEndpointsService', Framework.Endpoints.AngularEndpointsService);
```

## Config
```
{
  "WebApiModuleFileName": "string",
  
  "EndpointsOutputDirectory": "string",
  "EndpointsFileName": "string",
  "EndpointsNamespace": "string",

  "GenerateService": "boolean",
  "ServiceOutputDirectory": "string",
  "ServiceFileName": "string",
  "ServiceNamespace": "string",
  "ServicesName": "string",

  "GenerateEnums": "boolean",
  "EnumsOutputDirectory": "string",
  "EnumsFileName": "string",
  "EnumsNamespace": "string",

  "GenerateInterfaces": "boolean",
  "InterfacesOutputDirectory": "string",
  "InterfacesFileName": "string",
  "InterfacesNamespace": "string",

  "ScanOtherModules": "boolean",
  "WriteNamespaceAsModule": "boolean",

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
* Darko Sperac

## License
MIT