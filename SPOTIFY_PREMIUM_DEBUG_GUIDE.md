# 🔧 DEBUGGING SPOTIFY PREMIUM INTEGRATION

## 🚨 PROBLEMA REPORTADO
- ✅ Usuario tiene cuenta Spotify Premium
- ❌ Al intentar reproducir música → Error
- ❌ Al acceder a playlists → Error
- ✅ Backend API calls funcionan (status 200/204)

## 🔍 DEBUGGING STEPS

### 1. Verificar Console del Navegador
Abre las DevTools (F12) y busca:
```
🎵 Spotify Web Playback SDK Ready
👤 Producto de usuario: premium
🔍 Verificación Premium: {isPremium: true, product: "premium"}
✅ Usuario Premium verificado: premium
✅ Ready with Device ID [device_id]
✅ Connected to Spotify Web Playback SDK
```

### 2. Si NO aparece "Ready with Device ID":
**PROBLEMA**: El Web Playback SDK no se está inicializando

**POSIBLES CAUSAS**:
- Navegador no compatible (requiere Chrome/Edge/Firefox reciente)
- Spotify ya está reproduciendo en otro dispositivo
- Configuración de Spotify no permite Web SDK

**SOLUCIÓN**:
1. Cierra todas las apps de Spotify
2. Ve a Spotify → Settings → Devices
3. Desconecta otros dispositivos activos
4. Recarga la página de Melodix

### 3. Si SÍ aparece el Device ID pero da error al reproducir:
**PROBLEMA**: SDK inicializado pero playback falla

**DEBUGGING**: Busca en Console errores como:
```
❌ Toggle playback error: [error details]
❌ Error playing track: [error details]
```

### 4. Verificación Manual Premium Status
En Console del navegador ejecuta:
```javascript
fetch('/Spotify/CheckPremiumStatus')
  .then(r => r.json())
  .then(d => console.log('Premium Status:', d));
```

### 5. Test Device Connectivity
```javascript
if (player) {
  player.getCurrentState().then(state => {
    console.log('Player State:', state);
    if (state) {
      console.log('✅ Player is ready');
    } else {
      console.log('❌ Player not ready');
    }
  });
}
```

## 🛠️ SOLUCIONES POR TIPO DE ERROR

### ERROR: "Se requiere Spotify Premium"
```javascript
// En console, verificar:
console.log('userProduct:', userProduct);
console.log('isPremium:', isPremium);
// Debe mostrar: premium, true
```

### ERROR: "Reproductor no disponible"
```javascript
// En console, verificar:
console.log('device_id:', device_id);
console.log('player:', player);
// device_id debe tener valor, player debe ser object
```

### ERROR: "Error al reproducir la pista"
- Verificar que la canción esté disponible en tu región
- Intentar con otra canción
- Verificar permisos de Spotify

## ⚡ QUICK FIXES

### Fix 1: Reinicializar Player
```javascript
if (window.player) {
    window.player.disconnect();
    window.onSpotifyWebPlaybackSDKReady();
}
```

### Fix 2: Transfer Playback Manually
1. Abre Spotify Desktop/Mobile
2. Ve a Connect to a Device
3. Selecciona "Melodix Web Player"
4. Regresa a la web y prueba reproducir

### Fix 3: Clear Browser Cache
- Ctrl+Shift+Del → Clear all data
- Recarga la página
- Re-conecta Spotify

## 📊 EXPECTED VALUES

### Variables que DEBEN estar definidas:
```javascript
token: "BQC..." // Access token largo
userProduct: "premium" 
isPremium: true
device_id: "4e93a51..." // Device ID del SDK
player: SpotifyPlayer {...} // Object del player
```

### API Calls que DEBEN funcionar:
```
✅ GET /Spotify/CheckPremiumStatus → {isPremium: true}
✅ GET /Spotify/GetAccessToken → {access_token: "BQC..."}
✅ POST /Spotify/Play → 204 No Content (success)
```

## 🚨 EMERGENCY FALLBACK

Si nada funciona, revisar:

### 1. Spotify App Permissions
- spotify.com → Account → Privacy Settings
- Verificar que Melodix tenga permisos completos

### 2. Browser Compatibility
- Chrome 76+ ✅
- Firefox 68+ ✅  
- Edge 79+ ✅
- Safari NO SOPORTADO ❌

### 3. Network Issues
- Verificar que no haya proxy/VPN bloqueando
- Probar en red diferente

---

## 📞 DEBUGGING EN TIEMPO REAL

**Pasos para debugging en tiempo real:**

1. **Abre DevTools** (F12)
2. **Ve a Console**
3. **Recarga la página**
4. **Busca los mensajes de logging**
5. **Intenta reproducir una canción**
6. **Copia todos los errores que aparezcan**

**¡Los logs de Console nos dirán exactamente qué está fallando!** 🔍
