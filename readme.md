# MaZe Music Visualizer
Der MaZe Music Visualizer ist eine graphische Desktopanwendung zur Visualisierung von Musik und anderen Audiodateien.
Ziel des Projektes ist es, die Computergraphik mit Tonwiedergabe zu kombinieren. Die Anwendung soll die Musik in Echtzeit analysieren und die Ergebnisse in Form von Animationen darstellen.

## Bauen
Die Anwendung verwendet Betriebssystem-spezifische Bibliotheken, weshalb derzeit zur Ausführung Microsoft Windows benötigt wird.
Das Projekt basiert auf der [.NET 8.0 Plattform](https://dotnet.microsoft.com/en-us/download) und kann mit dem .NET SDK gebaut werden.
Die Anwendung kann mit dem Befehl `dotnet run` ausgeführt werden.
Bei dem Bauvorgang wird das Verzeichnis `res/` vollständig zur Binary kopiert. Die Dateien in diesem Verzeichnis werden zur Laufzeit benötigt.

## Verwendung
Das Programm benötigt eine Audiodatei im FLAC-Format. Diese muss im Verzeichnis `res/audio/` liegen und `audio.flac` heißen. Der `res/` Ordner muss sich im selben Verzeichnis wie die Binary befinden.

Nach Programmstart lädt die Anwendung die Audiodatei und bereitet die Visualisierung vor. Dieser Vorgang kann einige Sekunden dauern. Währenddessen wird ein schwarzer Bildschirm angezeigt. Sobald die Anwendung zur Wiedergabe bereit ist, ist im Titelbalken neben dem Anwendungstitel die Bildwiederholrate und die aktuelle Wiedergabezeit zu sehen. Die Wiedergabe kann mit der `Leertaste` gestartet und pausiert werden.
Am Ende der Audiowiedergabe wird die Wiedergabe von neuem gestartet. Die Anwendung kann mit der `Escape` Taste beendet werden.

### Tastenbelegung

| Taste | Funktion |
| --- | --- |
| `Escape` | Beendet die Anwendung. |
| `Leertaste` | Startet und Pausiert die Wiedergabe. |
| `M` | Schaltet den Leuchteffekt rund um den Mauszeiger aus und an. |
| `R` | Lädt alle Ressourcen (Shader und Audiodateien) neu und setzt die Wiedergabe auf den Beginn zurück. Kann genutzt werden um einen anderen, kompatiblen Shader während der Laufzeit zu laden oder die Audiodatei während der Laufzeit zu wechseln. |
| `S` | Gibt einen detaillierte Informationen zu den berechneten Frequenzen im Standard Debug Output aus. |

### Maussteuerung
Die Software verfügt über einige möglichkeiten 

## Funktionsweise
Der Programmablauf sieht vor dass

### Audio

### Shader
Die Anwendung basiert auf OpenGL 4.3 und verwendet die [OpenTK](https://opentk.net/) Bibliothek für die Interaktion mit OpenGL. Die Shader werden mit [GLSL](https://www.khronos.org/opengl/wiki/Core_Language_(GLSL)) geschrieben.