FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /
COPY *.sln ./app/
COPY *.csproj ./app/

WORKDIR /app/
RUN dotnet restore

COPY . .
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish --no-restore -c $BUILD_CONFIGURATION -o /app/build


FROM base AS final
WORKDIR /app
COPY --from=publish /app/build .

RUN apt-get update \
    && apt-get install -y \
        libnss3 \
        libgdk-pixbuf2.0-0 \
        libxss1 \
        libgconf-2-4 \
        libgtk-3-0 \
        libasound2 \
        libx11-xcb1 \
        fonts-liberation \
        libu2f-udev \
        libnss3 \
        xdg-utils \
        libxrandr2 \
        libgbm1 \
        libatk1.0-0 \
        libatk-bridge2.0-0 \
        libxcomposite1 \
        libxdamage1 \
        libgl1-mesa-glx \
        ca-certificates \
        libdrm2 \
        libvulkan1 \
        libxkbcommon0 \
    && rm -rf /var/lib/apt/lists/*
ENTRYPOINT ["dotnet", "BuscarRegistroSanitarioService.dll"]
