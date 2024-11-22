Aquí tienes un archivo `README.md` para documentar el uso de tu API con base en el controlador proporcionado:

# API de Búsqueda de Registro Sanitario

Esta API permite realizar búsquedas de registros sanitarios y gestionar paginación, tipos de productos, y obtener información sobre el estado del servicio de scraping.

## Endpoints

### 1. Buscar Registro Sanitario
**Endpoint:** `/api/buscar`  
**Método:** `GET`

**Descripción:** Busca registros sanitarios de un producto dado.

#### Parámetros de consulta:
- `nombreProducto` (requerido): Nombre del producto a buscar.

#### Ejemplo de solicitud:

GET http://localhost:5000/api/buscar?nombreProducto=panadol

#### Respuesta exitosa:
- **Código:** `200 OK`
- **Cuerpo:**

```json
{
  "statusCode": 200,
  "status": "success",
  "errors": null,
  "data": [
    {
      "lineNumber": 1,
      "registerNumber": "2108-GZ-187",
      "type": 7,
      "typeName": "Producto",
      "subType": 53,
      "subTypeName": "Medicamentos",
      "productName": "Panadol Sinus",
      "description": null,
      "countrySourceId": 194,
      "countrySource": "Panamá",
      "createdAt": "2015-09-07T22:21:44Z",
      "expiredAt": "2028-03-01T06:00:00Z",
      "status": 1,
      "statusName": "vigente",
      ...
    }
  ]
}
```

#### Errores comunes:
- **Código:** `400 Bad Request`  
  **Mensaje:** "El nombre del producto no puede estar vacío."
- **Código:** `404 Not Found`  
  **Mensaje:** "No se encontraron resultados para el producto especificado."

---

### 2. Paginar Resultados
**Endpoint:** `/api/paginacion`  
**Método:** `GET`

**Descripción:** Navega entre páginas de resultados.

#### Parámetros de consulta:
- `comando` (opcional): Valores posibles:
  - `siguiente` (por defecto)
  - `anterior`

#### Ejemplo de solicitud:
```
GET http://localhost:5000/api/paginacion?comando=siguiente
```

#### Respuestas:
- **Código:** `204 No Content`  
  **Descripción:** No hay más páginas disponibles.
- **Código:** `200 OK`  
  **Cuerpo:** Devuelve los resultados de la página solicitada.

---

### 3. Cambiar Tipo de Producto
**Endpoint:** `/api/cambiarTipo`  
**Método:** `GET`

**Descripción:** Cambia el tipo de producto a buscar.

#### Parámetros de consulta:
- `tipoProducto` (requerido): Código del tipo de producto.

#### Ejemplo de solicitud:
```
GET http://localhost:5000/api/cambiarTipo?tipoProducto=7
```

#### Respuesta exitosa:
- **Código:** `200 OK`  
  **Cuerpo:** Información actualizada del tipo de producto.

#### Errores comunes:
- **Código:** `500 Internal Server Error`  
  **Mensaje:** Error en el servicio de scraping.

---

### 4. Obtener Tipos de Producto
**Endpoint:** `/api/tiposProducto`  
**Método:** `GET`

**Descripción:** Lista los tipos de productos disponibles.

#### Ejemplo de solicitud:
```
GET http://localhost:5000/api/tiposProducto
```

#### Respuesta exitosa:
- **Código:** `200 OK`  
  **Cuerpo:** Lista de tipos de productos con sus códigos.

---

### 5. Obtener Estado del Servicio
**Endpoint:** `/api/status`  
**Método:** `GET`

**Descripción:** Verifica si el servicio de scraping está listo para usarse.

#### Ejemplo de solicitud:
```
GET http://localhost:5000/api/status
```

#### Respuestas:
- **Código:** `200 OK`  
  **Mensaje:** "El servicio está listo."
- **Código:** `503 Service Unavailable`  
  **Mensaje:** "El servicio está inicializando."

---

## Notas
- Asegúrate de que el servicio de scraping esté completamente inicializado antes de realizar consultas.
- Los resultados pueden variar según la disponibilidad de datos del sitio web objetivo.

