FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["src/HetznerWhatsApp.Api/HetznerWhatsApp.Api.csproj", "HetznerWhatsApp.Api/"]
RUN dotnet restore "HetznerWhatsApp.Api/HetznerWhatsApp.Api.csproj"

COPY src/HetznerWhatsApp.Api/ HetznerWhatsApp.Api/
WORKDIR "/src/HetznerWhatsApp.Api"
RUN dotnet build "HetznerWhatsApp.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "HetznerWhatsApp.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HetznerWhatsApp.Api.dll"]
