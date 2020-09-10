

######################################################
# How to generate a .NET CORE executable: https://secanablog.wordpress.com/2018/06/08/compile-a-net-core-app-to-a-single-native-binary/
#
#Add to your nuget.config
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <packageSources>
        <clear />
        <add key="dotnet-core" value="https://dotnet.myget.org/F/dotnet-core/api/v3/index.json" />
        <add key="nuget.org" value="https://api.nuget.org/v3/index.json" protocolVersion="3" />
    </packageSources>
</configuration>

# Restore all the packages
dotnet restore

# Ahead of time compile 
dotnet publish -c Release -r win-x64