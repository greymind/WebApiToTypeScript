# Web API To TypeScript
A tool for code generating TypeScript endpoints for your ASP.NET Web API controllers and their actions

## Using as standalone tool
Just run the executable with arguments
```
watts.exe "bin\WebApiTestApplication.dll" "src\Scripts\Framework\Endpoints"
```

## Using as MSBuild Target
Ensure the DLL is part of the build process, so you have access to it. Easiest way is to reference it in your project.
```
<UsingTask TaskName="WebApiToTypeScript.WebApiToTypeScript" AssemblyFile="$(ProjectDir)..\WebApiToTypeScript\bin\Debug\WebApiToTypeScript.dll" />
<Target Name="AfterBuild">
    <WebApiToTypeScript WebApiApplicationAssembly="$(TargetDir)WebApiTestApplication.dll" OutputDirectory="$(ProjectDir)Scripts\Endpoints" />
</Target>
```

## Team
* Balakrishnan (Balki) Ranganathan

## License
MIT