# BuscarRegistroSanitarioCR

**BuscarRegistroSanitarioCR** es una herramienta diseñada para realizar scraping al formulario de consulta de registros sanitarios del Ministerio de Salud de Costa Rica disponible en [registrelo.go.cr](https://v2.registrelo.go.cr/reports/12). Este proyecto permite extraer información de manera automatizada y estructurada, facilitando la integración con otros sistemas.

## Tabla de Contenidos

- [Requisitos Previos](#requisitos-previos)
- [Instalación](#instalación)
  - [En Windows](#en-windows)
  - [En Linux](#en-linux)
- [Uso](#uso)
- [Mantenimiento del Servicio](#mantenimiento-del-servicio)
  - [Comandos útiles en Windows](#comandos-útiles-en-windows)
  - [Comandos útiles en Linux](#comandos-útiles-en-linux)
- [Contribución](#contribución)
- [Licencia](#licencia)

---

## Requisitos Previos

Antes de instalar **BuscarRegistroSanitarioCR**, asegúrate de contar con:

1. .NET 8 SDK instalado en el sistema.
2. Permisos de administrador para registrar servicios (si lo instalas como un servicio).
3. Sistema operativo:
   - **Windows 10/11 o Windows Server 2019/2022**
   - **Linux**
---

## Instalación

### En Windows

1. Publica tu aplicación utilizando el siguiente comando desde el directorio del proyecto:

   ```shell
   dotnet publish -c Release -o C:\ruta\de\la\aplicacion
   ```

2. Registra el servicio usando el comando `sc create`:

   ```shell
   sc create BuscarRegistroSanitarioService binPath= "C:\ruta\de\la\aplicacion\BuscarRegistroSanitarioService.exe"
   ```

3. Inicia el servicio:

   ```shell
   sc start BuscarRegistroSanitarioService
   ```

### En Linux

1. Publica tu aplicación utilizando el siguiente comando desde el directorio del proyecto:

   ```shell
   dotnet publish -c Release -o /home/usuario/registro-sanitario-cr
   ```
2. Crea un archivo de servicio para systemd:

   ```shell
   sudo nano /etc/systemd/system/registro-sanitario-cr.service
   ```

3. Copia y pega la siguiente configuración, ajustando las rutas y el usuario según corresponda:

   ```ini
   [Unit]
   Description=Scraper para el registro sanitario del Ministerio de Salud
   [Service]
   ExecStart=/home/usuario/registro-sanitario-cr/BuscarRegistroSanitarioService
   Restart=always
   User=usuario
   Group=usuario
   Environment=ASPNETCORE_ENVIRONMENT=Production
   WorkingDirectory=/home/usuario/registro-sanitario-cr

   [Install]
   WantedBy=multi-user.target
   ```

5. Guarda el archivo y recarga systemd para aplicar los cambios:

   ```shell
   sudo systemctl daemon-reload
   ```

6. Inicia el servicio:

   ```shell
   sudo systemctl start registro-sanitario-cr
   ```

7. Habilita el servicio para que se inicie automáticamente al arrancar el sistema:

   ```shell
   sudo systemctl enable registro-sanitario-cr
   ```

---

## Uso

Una vez instalado y ejecutado, el scraper interactúa con el formulario del sitio web [registrelo.go.cr](https://v2.registrelo.go.cr/reports/12) para extraer la información del registro sanitario interceptando la respuesta en formato JSON.

Consulta la documentación interna del proyecto para ejemplos específicos de cómo utilizar las funcionalidades.

---

## Mantenimiento del Servicio

### Comandos útiles en Windows

- **Verificar el estado del servicio:**

   ```shell
   sc query BuscarRegistroSanitarioService
   ```

- **Detener el servicio:**

   ```shell
   sc stop BuscarRegistroSanitarioService
   ```

- **Eliminar el servicio:**

   ```shell
   sc delete BuscarRegistroSanitarioService
   ```

### Comandos útiles en Linux

- **Verificar el estado del servicio:**

   ```shell
   sudo systemctl status registro-sanitario-cr
   ```

- **Detener el servicio:**

   ```shell
   sudo systemctl stop registro-sanitario-cr
   ```

- **Eliminar el servicio:**

   ```shell
   sudo systemctl disable registro-sanitario-cr
   sudo rm /etc/systemd/system/registro-sanitario-cr.service
   sudo systemctl daemon-reload
   ```

---

## Contribución

¡Las contribuciones son bienvenidas! Por favor, abre un *issue* o envía un *pull request* para cualquier mejora o corrección.

---

## Licencia

Este proyecto está licenciado bajo la [MIT License](./LICENSE). Consulta el archivo para más detalles.

---

## Licencias de Terceros

Este proyecto incluye componentes de terceros licenciados bajo:  

- **Apache 2.0** (ver [`licenses/Apache-2.0.txt`](./licenses/Apache-2.0.txt))  
- **MIT** (ver [`licenses/MIT.txt`](./licenses/MIT.txt))  

Para obtener una lista completa de las atribuciones de terceros, consulta el archivo [`licenses/THIRD_PARTY_NOTICES.txt`](./licenses/THIRD_PARTY_NOTICES.txt).
