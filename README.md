# Laboratorio4 🛠️

Este laboratorio se enfoca en la implementación y análisis de seguridad en el almacenamiento y autenticación de usuarios, firma digital de archivos y cifrado de datos. Se utilizarán técnicas como JWT, ECC/RSA, AES-CBC, y SHA-256 para garantizar la seguridad de la información.

# Competencias a Desarrollar 💡

Desarrollar una aplicación donde los usuarios puedan:
* Registrarse y almacenar su contraseña de manera segura.
* Autenticarse con JWT.
* Firmar archivos digitalmente con ECC o RSA (ellos pueden escoger).
* Proteger la integridad de los archivos con SHA-256.
* Acceder a archivos cifrados usando su llave privada.

## Diagrama Del proyecto
![alt text](<Diagramalab4.png>)


## Frontend(Vie.js) 🧠
Funcionalidades:
* Formulario de Login/Register:
* Campos: Email y contraseña.
* Botón de "Iniciar Sesión" o "Registrar".
* JWT almacenado en el navegador.

Pantalla de Home:
* Botón para generar nuevas llaves privadas/públicas (se descarga la privada y la pública se almacena).
* Formulario para guardar un archivo (puede firmarse de un solo o solo guardarlo).
* Listado de archivos disponibles.
* Botón para descargar archivo.
* Opción para validar la firma de un archivo antes de descargar.

## Backend (C# ASP.NET 9) 📡
* /login, POST, Recibe email y password, valida credenciales y genera JWT.
* /register, POST, Recibe email y password (protegida con SHA-256).
* /archivos, GET, Devuelve lista de archivos.
* /archivos/{id}/descargar, GET, Devuelve un archivo y la llave pública del usuario que lo guardó.
* /guardar, POST, Guarda un archivo firmado con ECC/RSA (requiere la llave privada para firmar) o normal sin firmar.
* /verificar, POST, Recibe un archivo y la llave pública para verificar su autenticidad.

## 🔐 Seguridad y Cifrado
1️⃣ Autenticación JWT:
* Generación de JWT en el backend con la librería jwt.
* JWT debe incluir el ID del usuario y expirar en 1 hora.
* Se almacena en el navegador y se usa en peticiones protegidas.

2️⃣ Firma Digital:
* Cada usuario tiene un par de claves ECC o RSA (se deben generar después de iniciar sesión en la pantalla home).
* Los archivos son firmados con la clave privada del usuario.
* Se puede verificar la autenticidad con la clave pública.

3️⃣ Hashing SHA-256:
* Cada usuario almacenará de manera segura la contraseña en la base de datos.
* Cada archivo tendrá un hash generado con SHA-256.
* Al verificar un archivo, se compara su hash con el original.

4️⃣ Comunicación cifrada de Archivos:
* Los archivos se cifran con una llave simétrica (AES-CBC).

## Requisitos del lab

* Máquina virtual Ubuntu o contenedor Docker con las dependencias necesarias instaladas.
* C# 8 o superior para el backend.
* Vite.js para el frontend.
* Un IDE para programar.

## Sugerencias 💡

* Dividir el código en funciones para una mayor claridad y reutilización.
* Utilizar comentarios para explicar el funcionamiento de cada sección del código.
* Experimentar con diferentes parámetros y configuraciones para profundizar en la comprensión de los cifrados.

# Cómo Ejecutar el Código ⏳

Clonar el proyecto
```bash
git clone https://github.com/estebandonis/Asimetric-App.git
```

Frontend (Vue.js/JavaScript)

Ingresar al directorio
```bash
cd Asimetric-App/Backend
```

Instalar dependencias
```bash
dotnet restore
```

Ejecutar el proyecto
```bash
dotnet run
```

Ejecutar el proyecto en modo desarrollo
```bash
dotnet run
```


Backend (ASP.NET/C#)

Ingresar al directorio
```bash
cd Asimetric-App/Frontend
```

Instalar dependencias
```bash
npm install
```

Ejecutar el proyecto
```bash
npm run dev
```

# Equipo
* Abner Iván García -21285
* Oscar Esteban Donis -21610

# Contribuciones 🌟

¡Las contribuciones son bienvenidas! Si tienes ideas para mejorar este laboratorio o agregar nuevas funcionalidades, no dudes en enviar un pull request.

# Licencia 📝

Este laboratorio es de uso libre para fines educativos y personales. Por favor, da el crédito correspondiente si utilizas este código en tus proyectos u ejercicios.