# 🚨 SPOTIFY PREMIUM - DIAGNÓSTICO EN TIEMPO REAL

## 🔍 PASOS PARA DIAGNOSTICAR TU PROBLEMA

### 1. **ABRE LA CONSOLA DEL NAVEGADOR**
- Presiona **F12** 
- Ve a la pestaña **Console**
- Recarga la página (Ctrl+R)

### 2. **VERIFICA LA INICIALIZACIÓN**
Busca estos mensajes en orden:
```
🎵 Spotify Web Playback SDK Ready
👤 Producto de usuario: premium
🔍 Verificación Premium: {isPremium: true, product: "premium"}
✅ Usuario Premium verificado: premium
✅ Ready with Device ID [un ID largo]
🔍 Player State Test: ✅ Connected  
✅ Connected to Spotify Web Playback SDK
```

### 3. **SI NO VES "Ready with Device ID"**
El problema es que el Web Playback SDK no se está inicializando.

**CAUSA MÁS COMÚN**: Spotify está activo en otro dispositivo

**SOLUCIÓN INMEDIATA**:
1. Cierra **TODAS** las aplicaciones de Spotify (móvil, desktop, web)
2. Ve a **Spotify.com → Settings → Devices**
3. Desconecta todos los dispositivos activos
4. **Recarga la página de Melodix**

### 4. **SI VES EL DEVICE ID PERO FALLA AL REPRODUCIR**
Ejecuta esto en la consola:
```javascript
spotifyDebug.testPlayer()
```

Debe mostrar:
```
📱 Device ID: ✅ [ID válido]
🎵 Player Object: ✅ INICIALIZADO
🔑 Token: ✅ DISPONIBLE
👤 User Product: premium
💎 Is Premium: true
🎧 Player State: ✅ ACTIVO
```

### 5. **TEST DE REPRODUCCIÓN MANUAL**
En la consola, ejecuta:
```javascript
spotifyDebug.testPlay()
```

Esto intentará reproducir una canción de prueba y mostrará todos los logs detallados.

### 6. **SI AÚN NO FUNCIONA - REINICIO COMPLETO**
En la consola:
```javascript
spotifyDebug.reinit()
```

Esto reinicializará completamente el reproductor.

## 🚨 ERRORES COMUNES Y SOLUCIONES

### Error: "account_error"
```
❌ Spotify SDK Account Error: Premium required
```
**SOLUCIÓN**: Tu cuenta no es realmente Premium o hay un problema de sincronización
- Ve a **spotify.com/account** y verifica tu suscripción
- Espera unos minutos para que se sincronice
- Desconecta y reconecta la cuenta en Melodix

### Error: "initialization_error"
```
❌ Spotify SDK Initialization Error: [mensaje]
```
**SOLUCIÓN**: Problema de navegador o conexión
- Usa Chrome, Firefox o Edge (no Safari)
- Desactiva extensiones de bloqueo
- Prueba en ventana privada/incógnito

### Error: "authentication_error"
```
❌ Spotify SDK Authentication Error: [mensaje]  
```
**SOLUCIÓN**: Token expirado o inválido
- Ve a Configuración → Desconectar Spotify
- Vuelve a conectar la cuenta

### Error HTTP en requests
```
❌ Play request failed: HTTP 403
```
**SOLUCIÓN**: Permisos insuficientes
- Verifica que Melodix tenga todos los permisos en tu cuenta Spotify
- Reconecta la cuenta con todos los permisos

## 📋 CHECKLIST DE VERIFICACIÓN

- [ ] Cuenta Spotify Premium activa
- [ ] Navegador compatible (Chrome/Firefox/Edge)
- [ ] No hay otros dispositivos Spotify activos
- [ ] JavaScript habilitado
- [ ] Sin extensiones que bloqueen scripts
- [ ] Conexión a internet estable

## 🎯 DEBUGGING AVANZADO

Si nada de lo anterior funciona, ejecuta en la consola y copia el resultado:

```javascript
// Test completo
spotifyDebug.testPlayer();
await spotifyDebug.testPremium();

// Información del navegador
console.log('Browser:', navigator.userAgent);

// Estado de Spotify SDK
console.log('Spotify SDK loaded:', typeof Spotify !== 'undefined');
```

**¡Copia TODOS los mensajes de la consola y compártelos para diagnóstico avanzado!** 📋
