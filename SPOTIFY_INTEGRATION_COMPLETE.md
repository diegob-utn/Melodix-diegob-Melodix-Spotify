# Integración Completa de Spotify APIs en Melodix

## Resumen de Mejoras Implementadas

Basándonos en la guía completa de la API Web de Spotify y Web Playback SDK en ASP.NET MVC, hemos expandido significativamente la funcionalidad de Spotify en la aplicación Melodix. Las mejoras incluyen funciones avanzadas de gestión de biblioteca, control de reproducción, análisis de audio, recomendaciones y más.

## Funcionalidades Agregadas

### 1. Gestión de Biblioteca del Usuario ("Tu Biblioteca")

**Endpoints implementados:**

#### `GET /Spotify/GetSavedTracks`
- **Descripción**: Obtiene las pistas guardadas del usuario
- **Parámetros**: `limit` (default: 20), `offset` (default: 0)
- **Scopes requeridos**: `user-library-read`
- **Ejemplo de uso**: 
```javascript
fetch('/Spotify/GetSavedTracks?limit=50&offset=0')
```

#### `POST /Spotify/SaveTracks`
- **Descripción**: Guarda pistas en la biblioteca del usuario
- **Body**: `{ "trackIds": ["track_id1", "track_id2"] }`
- **Scopes requeridos**: `user-library-modify`

#### `DELETE /Spotify/RemoveSavedTracks`
- **Descripción**: Elimina pistas guardadas de la biblioteca del usuario
- **Body**: `{ "trackIds": ["track_id1", "track_id2"] }`
- **Scopes requeridos**: `user-library-modify`

### 2. Gestión Avanzada de Playlists

#### `POST /Spotify/CreatePlaylist`
- **Descripción**: Crea una nueva playlist
- **Body**: `{ "name": "Mi Playlist", "description": "Descripción", "isPublic": true }`
- **Scopes requeridos**: `playlist-modify-public` o `playlist-modify-private`

#### `POST /Spotify/AddTracksToPlaylist`
- **Descripción**: Agrega pistas a una playlist
- **Body**: `{ "playlistId": "playlist_id", "trackUris": ["spotify:track:..."] }`
- **Scopes requeridos**: `playlist-modify-public` o `playlist-modify-private`

#### `DELETE /Spotify/RemoveTracksFromPlaylist`
- **Descripción**: Elimina pistas de una playlist
- **Body**: `{ "playlistId": "playlist_id", "trackUris": ["spotify:track:..."] }`
- **Scopes requeridos**: `playlist-modify-public` o `playlist-modify-private`

### 3. Sistema de Seguimiento (Follow)

#### `POST /Spotify/FollowArtists`
- **Descripción**: Sigue artistas
- **Body**: `{ "artistIds": ["artist_id1", "artist_id2"] }`
- **Scopes requeridos**: `user-follow-modify`

#### `DELETE /Spotify/UnfollowArtists`
- **Descripción**: Deja de seguir artistas
- **Body**: `{ "artistIds": ["artist_id1", "artist_id2"] }`
- **Scopes requeridos**: `user-follow-modify`

#### `GET /Spotify/GetFollowedArtists`
- **Descripción**: Obtiene artistas seguidos
- **Parámetros**: `limit` (default: 20), `after` (cursor)
- **Scopes requeridos**: `user-follow-read`

### 4. Elementos Más Escuchados del Usuario

#### `GET /Spotify/GetTopTracks`
- **Descripción**: Obtiene las pistas más escuchadas del usuario
- **Parámetros**: `timeRange` (`short_term`, `medium_term`, `long_term`), `limit` (default: 20)
- **Scopes requeridos**: `user-top-read`

#### `GET /Spotify/GetTopArtists`
- **Descripción**: Obtiene los artistas más escuchados del usuario
- **Parámetros**: `timeRange` (`short_term`, `medium_term`, `long_term`), `limit` (default: 20)
- **Scopes requeridos**: `user-top-read`

### 5. Recomendaciones

#### `GET /Spotify/GetRecommendations`
- **Descripción**: Obtiene recomendaciones basadas en semillas
- **Parámetros**: 
  - `seedArtists`: IDs de artistas separados por comas
  - `seedTracks`: IDs de pistas separados por comas
  - `seedGenres`: Géneros separados por comas
  - `limit` (default: 20)
- **Ejemplo**: `/Spotify/GetRecommendations?seedArtists=4NHQUGzhtTLFvgF5SZesLK&limit=10`

### 6. Análisis de Audio

#### `GET /Spotify/GetAudioFeatures/{trackId}`
- **Descripción**: Obtiene características de audio de una pista
- **Respuesta incluye**: 
  - `danceability`, `energy`, `loudness`, `speechiness`
  - `acousticness`, `instrumentalness`, `liveness`, `valence`
  - `tempo`, `key`, `timeSignature`

### 7. Control Avanzado de Reproducción

#### `POST /Spotify/NextTrack`
- **Descripción**: Salta a la siguiente pista
- **Scopes requeridos**: `user-modify-playback-state`

#### `POST /Spotify/PreviousTrack`
- **Descripción**: Salta a la pista anterior
- **Scopes requeridos**: `user-modify-playback-state`

#### `POST /Spotify/SetVolume`
- **Descripción**: Establece el volumen
- **Body**: `{ "volumePercent": 50, "deviceId": "device_id" }`
- **Scopes requeridos**: `user-modify-playback-state`

#### `POST /Spotify/Seek`
- **Descripción**: Salta a una posición específica en la pista
- **Body**: `{ "positionMs": 30000, "deviceId": "device_id" }`
- **Scopes requeridos**: `user-modify-playback-state`

#### `GET /Spotify/GetPlaybackState`
- **Descripción**: Obtiene el estado actual de reproducción
- **Scopes requeridos**: `user-read-playback-state`

#### `GET /Spotify/GetDevices`
- **Descripción**: Obtiene dispositivos disponibles
- **Scopes requeridos**: `user-read-playback-state`

#### `POST /Spotify/TransferPlayback`
- **Descripción**: Transfiere reproducción a otro dispositivo
- **Body**: `{ "deviceIds": ["device_id"], "play": false }`
- **Scopes requeridos**: `user-modify-playback-state`

### 8. Información Detallada de Artistas

#### `GET /Spotify/GetArtist/{id}`
- **Descripción**: Obtiene información detallada de un artista

#### `GET /Spotify/GetArtistTopTracks/{id}`
- **Descripción**: Obtiene las pistas más populares de un artista
- **Parámetros**: `market` (default: "US")

#### `GET /Spotify/GetRelatedArtists/{id}`
- **Descripción**: Obtiene artistas relacionados

### 9. Historial de Reproducciones

#### `GET /Spotify/GetRecentlyPlayed`
- **Descripción**: Obtiene pistas reproducidas recientemente
- **Parámetros**: `limit` (default: 20)
- **Scopes requeridos**: `user-read-recently-played`

## Configuración de Scopes Actualizada

Los scopes de Spotify han sido actualizados para incluir todos los permisos necesarios:

```json
"Scopes": "user-read-private user-read-email playlist-read-private playlist-read-collaborative playlist-modify-public playlist-modify-private user-library-read user-library-modify user-follow-read user-follow-modify user-read-recently-played user-read-playback-state user-modify-playback-state streaming user-top-read"
```

## Seguridad y Mejores Prácticas Implementadas

### 1. Gestión Automática de Tokens
- **Método auxiliar `RefreshTokenIfNeeded`**: Verifica automáticamente si el token está próximo a expirar (5 minutos) y lo renueva si es necesario.
- **Almacenamiento seguro**: Los tokens se almacenan en la base de datos cifrada y se manejan de forma segura.

### 2. Validación de Tokens
- **Endpoint `SetToken`**: Permite validar tokens manualmente mediante una llamada de prueba a la API.

### 3. Manejo de Errores
- **Logging completo**: Todos los errores se registran con detalles específicos.
- **Respuestas consistentes**: Todas las respuestas de error siguen un formato estándar.

## Ejemplos de Uso

### Ejemplo 1: Crear Playlist y Agregar Pistas
```javascript
// Crear playlist
const playlistResponse = await fetch('/Spotify/CreatePlaylist', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
        name: 'Mi Playlist Rock',
        description: 'Las mejores canciones de rock',
        isPublic: false
    })
});

const playlist = await playlistResponse.json();

// Agregar pistas a la playlist
await fetch('/Spotify/AddTracksToPlaylist', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
        playlistId: playlist.id,
        trackUris: [
            'spotify:track:4uLU6hMCjMI75M1A2tKUQC',
            'spotify:track:7qiZfU4dY1lWllzX7mPBI3'
        ]
    })
});
```

### Ejemplo 2: Obtener Recomendaciones Basadas en Top Artists
```javascript
// Obtener artistas más escuchados
const topArtists = await fetch('/Spotify/GetTopArtists?timeRange=short_term&limit=5')
    .then(r => r.json());

const seedArtists = topArtists.items.map(artist => artist.id).join(',');

// Obtener recomendaciones
const recommendations = await fetch(`/Spotify/GetRecommendations?seedArtists=${seedArtists}&limit=20`)
    .then(r => r.json());
```

### Ejemplo 3: Control de Reproducción Avanzado
```javascript
// Obtener dispositivos disponibles
const devices = await fetch('/Spotify/GetDevices').then(r => r.json());

// Transferir reproducción al primer dispositivo disponible
if (devices.devices.length > 0) {
    await fetch('/Spotify/TransferPlayback', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            deviceIds: [devices.devices[0].id],
            play: true
        })
    });
}

// Establecer volumen al 70%
await fetch('/Spotify/SetVolume', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
        volumePercent: 70
    })
});
```

## Estructura de Respuesta de APIs

Todas las respuestas siguen la estructura de la API de Spotify, manteniendo compatibilidad total con la documentación oficial. Los objetos principales incluyen:

- **SpotifyTrack**: Información completa de pista
- **SpotifyArtist**: Información de artista
- **SpotifyAlbum**: Información de álbum
- **SpotifyPlaylist**: Información de playlist
- **SpotifyDevice**: Información de dispositivo
- **SpotifyAudioFeatures**: Características de audio
- **SpotifyPlaybackState**: Estado de reproducción

## Web Playback SDK Integration

La integración del Web Playback SDK permite que el navegador actúe como un dispositivo Spotify Connect. Las funcionalidades incluyen:

1. **Control total de reproducción** desde el navegador
2. **Sincronización en tiempo real** con otros dispositivos
3. **Acceso al token de usuario** para interacciones con la Web API
4. **Eventos de estado** para actualización de UI en tiempo real

## Conclusión

Esta implementación proporciona una integración completa y robusta con las APIs de Spotify, siguiendo todas las mejores prácticas de seguridad y funcionalidad recomendadas en la guía oficial. La aplicación ahora puede ofrecer una experiencia similar a la aplicación oficial de Spotify, con capacidades completas de:

- Gestión de biblioteca musical
- Creación y modificación de playlists
- Sistema de seguimiento de artistas
- Recomendaciones personalizadas
- Análisis de audio avanzado
- Control completo de reproducción
- Historial de escuchas

Todos los endpoints están protegidos con autorización adecuada y manejan automáticamente la renovación de tokens para una experiencia de usuario fluida.
