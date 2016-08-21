# Web API To TypeScript
A tool for code generating TypeScript endpoints for your ASP.NET Web API controllers and their actions

## Using as standalone tool
Just run the executable with arguments
```
watts.exe "bin\WebApiTestApplication.dll" "src\Scripts\Framework\Endpoints" "src\TypeMappings.json"
```

## Using as MSBuild Target
Ensure the DLL is part of the build process, so you have access to it. Easiest way is to reference it in your project.
```
<UsingTask TaskName="WebApiToTypeScript.WebApiToTypeScript" AssemblyFile="$(ProjectDir)..\WebApiToTypeScript\bin\Debug\WebApiToTypeScript.dll" />
<Target Name="AfterBuild">
    <WebApiToTypeScript WebApiApplicationAssembly="$(TargetDir)WebApiTestApplication.dll" OutputDirectory="$(ProjectDir)Scripts\Endpoints" TypeMappingsFileName="${ProjectDir)TypeMappings.json" />
</Target>
```

## Type Mappings
You can provide an optional JSON config for type mappings. This is used in cases where you want to replace the generic QueryParam for complex types to a class you extend from QueryParam.
```
[
    {
        "WebApiTypeName": "EncryptedIntAttribute",
        "TypeScriptTypeName": "string",
        "TreatAsAttribute": true
    }
]
```

## Team
* Balakrishnan (Balki) Ranganathan

## License
MIT