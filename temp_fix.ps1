# Script para arreglar todas las referencias a campos de imagen eliminados

# Arreglar ArtistaController
$content = Get-Content "Melodix.MVC\Controllers\ArtistaController.cs" -Raw

# Reemplazar todas las referencias problemáticas
$content = $content -replace 'model\.ArchivoImagen', 'null'
$content = $content -replace 'model\.NuevaImagen', 'null'
$content = $content -replace 'pista\.RutaImagen', 'null'
$content = $content -replace 'album\.RutaImagen', 'null'
$content = $content -replace 'RutaImagenActual.*?pista\.RutaImagen', 'null'

Set-Content "Melodix.MVC\Controllers\ArtistaController.cs" $content

# Arreglar MusicaController
$content = Get-Content "Melodix.MVC\Controllers\MusicaController.cs" -Raw

$content = $content -replace 'model\.ArchivoImagen', 'null'
$content = $content -replace 'model\.NuevaImagen', 'null'
$content = $content -replace 'pista\.RutaImagen', 'null'
$content = $content -replace 'album\.RutaImagen', 'null'
$content = $content -replace 'RutaImagenActual.*?pista\.RutaImagen', 'null'

Set-Content "Melodix.MVC\Controllers\MusicaController.cs" $content

# Arreglar BibliotecaController
$content = Get-Content "Melodix.MVC\Controllers\BibliotecaController.cs" -Raw

$content = $content -replace 'model\.ImagenLista', 'null'
$content = $content -replace 'lista\.RutaImagen', 'null'

Set-Content "Melodix.MVC\Controllers\BibliotecaController.cs" $content

Write-Host "Script completado"
