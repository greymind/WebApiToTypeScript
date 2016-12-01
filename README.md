# Web API To TypeScript

[![Join the chat at https://gitter.im/WebApiToTypeScript/Lobby](https://badges.gitter.im/WebApiToTypeScript/Lobby.svg)](https://gitter.im/WebApiToTypeScript/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

A tool for code generating TypeScript endpoints for your ASP.NET Web API controllers and their actions. No more need for dumb repositories!
* Generates typed endpoints with required and optional parameters for GET, PUT, POST and DELETE verbs
* Generates enumerations and interfaces for DTOs used in all controller actions
* Generates Angular service to expose typed `call()` function to fetch response from server
* Generates typed views to use on front-end
* Generates resources files based on .resx files

## Examples
Take a look at the generated files and sample test files [here](https://github.com/greymind/WebApiToTypeScript/tree/master/src/WebApiTestApplication/Scripts)

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
> I haven't tested this in a while, so if you find any inconsistencies with working directory and such, feel free to file an issue or better yet, submit a pull request!

Ensure the DLL is part of the build process, so you have access to it. Easiest way is to reference it in your project.
```
<UsingTask TaskName="WebApiToTypeScript.WebApiToTypeScript" AssemblyFile="$(ProjectDir)..\WebApiToTypeScript\bin\Debug\WebApiToTypeScript.dll" />
<Target Name="AfterBuild">
    <WebApiToTypeScript ConfigFilePath="$(ProjectDir)Watts.config.json" />
</Target>
```

## T4 Template
If you wish to integrate the executable as part of your workflow from within Visual Studio, you may consider wiring it up to a T4 template. [Here](https://github.com/greymind/WebApiToTypeScript/tree/master/src/WebApiTestApplication/Scripts/Watts.tt) is a sample.

## Protip
You can build just the C# parts of the solution by adding a condition to the TypeScript target in the `csproj` file. This way you can ensure that before you run WebApiToTypeScript, you have a hassle-free build of the backend code.
```
  <PropertyGroup>
    <SkipTypeScript Condition="'$(SkipTypeScript)'==''">False</SkipTypeScript>
  </PropertyGroup>
  <Import Project="$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)\TypeScript\Microsoft.TypeScript.targets" Condition="Exists('$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)\TypeScript\Microsoft.TypeScript.targets') AND '$(SkipTypeScript)'=='False'" />
```
```
Path\To\msbuild.exe /t:build /p:SkipTypeScript:True Path\To\Project.csproj
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
  
  "GenerateEndpoints": "boolean",  
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
  "InterfaceMembersInCamelCase": "boolean",
  "InterfaceMatches": [
    "Match": "string",
    "ExcludeMatch": "string",
    "BaseTypeName": "string"
  ],

  "GenerateViews": "boolean",
  "ViewsSourceDirectory": "string",
  "ViewsPattern": "string",
  "ViewsOutputDirectory": "string",
  "ViewsFileName": "string",
  "ViewsNamespace": "string",
  "UseViewsGroupingNamespace": "boolean",

  "ScanOtherModules": "boolean",
  "WriteNamespaceAsModule": "boolean",

  "TypeMappings": [
    {
      "WebApiTypeName": "string",
      "TypeScriptTypeName": "string",
      "AutoInitialize": "boolean",
      "TreatAsAttribute": "boolean"
      "TreatAsConstraint": "boolean",
      "Match": "string"
    }
  ]
}
```

## Team
* [Balakrishnan (Balki) Ranganathan](https://github.com/greymind)
* [Augustin Juricic](https://github.com/omittones)
* [Darko Sperac](https://github.com/dsperac)

## License
MIT