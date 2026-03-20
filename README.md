<p align="center">
  <img src="https://raw.githubusercontent.com/jellyfin/jellyfin-ux/master/branding/SVG/icon-transparent.svg" alt="Jellyfin" width="80"/>
</p>

<h1 align="center">TopJellyfin</h1>

<p align="center">
  Plugin para <a href="https://jellyfin.org">Jellyfin</a> que añade secciones personalizadas a la página de inicio con contenido <b>recientemente estrenado</b> y <b>rankings de Trakt.tv</b>.
</p>

<p align="center">
  <a href="https://github.com/pincharo/TopJellyfin/releases"><img src="https://img.shields.io/github/v/release/pincharo/TopJellyfin?style=flat-square&color=blue" alt="Release"/></a>
  <a href="https://github.com/pincharo/TopJellyfin/actions"><img src="https://img.shields.io/github/actions/workflow/status/pincharo/TopJellyfin/build-release.yml?style=flat-square" alt="Build"/></a>
  <img src="https://img.shields.io/badge/Jellyfin-10.10+-purple?style=flat-square" alt="Jellyfin 10.10+"/>
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square" alt=".NET 8"/>
</p>

---

## Secciones que añade

| Sección | Fuente de datos |
|---|---|
| **Recientemente estrenado en Películas** | Biblioteca local — películas estrenadas en los últimos X días |
| **Recientemente estrenado en Series** | Biblioteca local — series estrenadas en los últimos X días |
| **Top Películas** | Trakt.tv — trending o popular (configurable) |
| **Top Series** | Trakt.tv — trending o popular (configurable) |

> Solo se muestran títulos que ya existan en tu biblioteca de Jellyfin. El plugin cruza los IDs de IMDB/TMDB de Trakt con tu librería local.

---

## Instalación

### Opción 1 — Repositorio de plugins (recomendada)

1. Ve a **Panel de control → Plugins → Repositorios**
2. Añade un nuevo repositorio:

   ```
   Nombre: TopJellyfin
   URL:    https://raw.githubusercontent.com/pincharo/TopJellyfin/main/manifest.json
   ```

3. Ve a **Catálogo**, busca **TopJellyfin** e instálalo
4. Reinicia Jellyfin

### Opción 2 — Instalación manual

1. Descarga el `.zip` de la última [release](https://github.com/pincharo/TopJellyfin/releases)
2. Extrae `TopJellyfin.dll` en la carpeta de plugins de Jellyfin:
   - Linux: `/var/lib/jellyfin/plugins/TopJellyfin/`
   - Docker: `/config/plugins/TopJellyfin/`
   - Windows: `C:\ProgramData\Jellyfin\Server\plugins\TopJellyfin\`
3. Reinicia Jellyfin

---

## Configuración

Tras instalar, ve a **Panel de control → Plugins → TopJellyfin**:

| Opción | Descripción | Por defecto |
|---|---|---|
| **Trakt Client ID** | API Key de Trakt.tv ([crear app](https://trakt.tv/oauth/applications/new)) | *obligatorio para Top* |
| **Tipo de ranking** | `trending` (tendencia actual) o `popular` (más popular global) | `trending` |
| **Días para "reciente"** | Ventana de días para considerar un estreno como reciente | `90` |
| **Máx. elementos** | Número máximo de títulos por sección | `20` |
| **Caché (minutos)** | Tiempo de caché de las peticiones a Trakt | `60` |
| **Activar/desactivar** | Interruptores individuales para cada una de las 4 secciones | Todas activas |

### Obtener API Key de Trakt.tv

1. Entra en [trakt.tv/oauth/applications/new](https://trakt.tv/oauth/applications/new)
2. Crea una aplicación (el nombre y la redirect URI no importan)
3. Copia el **Client ID** y pégalo en la configuración del plugin

---

## Arquitectura

```
TopJellyfin/
├── Api/
│   └── TopJellyfinController.cs    # Endpoints REST
├── ClientScript/
│   └── topjellyfin.js              # JS inyectado en el frontend
├── Configuration/
│   ├── PluginConfiguration.cs      # Modelo de configuración
│   └── configPage.html             # UI de configuración
├── Models/
│   ├── SectionItemDto.cs           # DTO para las tarjetas
│   ├── TraktMovieResult.cs         # Modelo respuesta Trakt (películas)
│   └── TraktShowResult.cs          # Modelo respuesta Trakt (series)
├── Services/
│   ├── TraktService.cs             # Cliente API de Trakt.tv con caché
│   ├── LibraryMatchingService.cs   # Matching Trakt ↔ biblioteca local
│   └── RecentlyReleasedService.cs  # Consultas de estrenos recientes
├── ClientInjector.cs               # Inyecta el script en index.html
├── Plugin.cs                       # Punto de entrada del plugin
└── ServiceRegistrator.cs           # Registro de dependencias
```

### Flujo de datos

```
Jellyfin Home → topjellyfin.js → API Controller
                                      │
                    ┌─────────────────┼─────────────────┐
                    ▼                 ▼                  ▼
          RecentlyReleased      TraktService     LibraryMatching
          Service               (caché en RAM)    Service
                    │                 │                  │
                    ▼                 ▼                  ▼
              Biblioteca         api.trakt.tv      Cruce por
              local Jellyfin                       IMDB/TMDB ID
```

---

## Compilar desde código fuente

```bash
git clone https://github.com/pincharo/TopJellyfin.git
cd TopJellyfin
dotnet build --configuration Release
```

La DLL resultante estará en `TopJellyfin/bin/Release/net8.0/TopJellyfin.dll`.

---

## Requisitos

- **Jellyfin** 10.10+
- **.NET** 8.0 (solo para compilar)
- **Trakt.tv Client ID** (solo si activas las secciones Top)

---

## Licencia

MIT — haz lo que quieras con él.

---

<p align="center">
  Hecho con ☕ por <a href="https://github.com/pincharo">pincharo</a>
</p>
