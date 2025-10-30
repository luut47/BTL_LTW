# Build stage - dùng SDK .NET 10
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy csproj / restore
COPY *.sln ./
COPY *.csproj ./
RUN dotnet restore

# copy tất cả source, publish
COPY . .
RUN dotnet publish BTL_LTW.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://0.0.0.0:$PORT
EXPOSE 80
ENTRYPOINT ["dotnet", "BTL_LTW.dll"]
