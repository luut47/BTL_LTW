# ---------- Build stage ----------
# Dùng SDK phù hợp (thay 8.0 bằng 7.0 nếu project target .NET7)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Nếu bạn có solution file, copy nó trước để tận dụng cache docker restore
COPY *.sln ./
COPY *.csproj ./
# Nếu csproj nằm trong thư mục con, thay line trên bằng:
# COPY BTL_LTW/BTL_LTW.csproj ./BTL_LTW/

RUN dotnet restore

# copy toàn bộ source và publish
COPY . .
RUN dotnet publish BTL_LTW.csproj -c Release -o /app/publish

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# copy từ stage build
COPY --from=build /app/publish ./

# cho app lắng nghe trên PORT do platform cấp
ENV ASPNETCORE_URLS=http://0.0.0.0:$PORT

# expose port 80 (chỉ để clear)
EXPOSE 80

# entrypoint
ENTRYPOINT ["dotnet", "BTL_LTW.dll"]
