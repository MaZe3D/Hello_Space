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
Die Software verfügt über die Möglichkeit mithilfe eines Mausklicks oder -ziehens die Wiedergabeposition anzupassen. Hierfür muss auf dem Fortschrittsbalken auf dem unteren Bildschirmrand geklickt werden.

## Funktionsweise
Der Programmablauf sieht vor dass zunächst das Fenster erstellt wird. Im Konstruktor des Fensters wird ebenfalls die Audiobibliothek initialisiert. Darauf hin

### Audio
Zur Aufbereitung der Audiodaten bedarf es einiger schritte, sodass die Visualisierung ansprechend aussieht.
Be der Initialisierung der `Audio` Klasse wird zunächst die Audiodatei eingelesen. Daraufhin werden alle Samples aus dem Audiostream ausgelesen.

Da es sich um Stereoinhalte handelt befinden in dem Array aller Samples sowohl die Samples für den linken als auch den rechten Kanal. Dabei sind die Samples Interleaved (Verschränkt). Daher sind alle Samples des linken Kanal an der Position `2n` und alle Samples des rechten Kanals an der Position `2n + 1`. Die Samples werden auf zwei Arrays jeweils für den linken und rechten Audiokanal aufgeteilt und in einer Instanz der Struktur `Stereo Audio` gespeichert.

```cs
public struct StereoAudio
{
    public float[] Left;
    public float[] Right;
}
```

Um nun für den Shader die daten aufzubereiten werden beide Kanäle gefiltert. Einmal durch einen Tiefpass, Bandpass und einen Hochpass. Dies erfolgt mithilfe der `BiQuadFilter` welche Teil der `naudio.Dsp` Namespaces ist.
Der Tiefpass hat eine Grenzfrequenz von 200 Hz, der Bandpass arbeitet um ca. 600 Hz und der Hochpass hat eine Grenzfrequenz von 1000 Hz.

Nach der Filterung befinden sich die Samples in den respektiven `StereoAudio` Instanzen in der `Audio`-Klasse.
```cs
public StereoAudio bassData;
public StereoAudio midData;
public StereoAudio highData;
```

### Shader
Die Anwendung basiert auf OpenGL 4.3 und verwendet die [OpenTK](https://opentk.net/) Bibliothek für die Interaktion mit OpenGL. Die Shader werden mit [GLSL](https://www.khronos.org/opengl/wiki/Core_Language_(GLSL)) geschrieben.

Die OpenGL Shader Pipeline sieht mehrere unterschiedliche Shader vor um das Bild (abschließend) zu berechnen. In dieser Anwendung findet der Vertex- und Fragment-Shader anwendung.

#### Vertex Shader
Der Vertex Shader ist ein programmierbarer Shader, der die Verarbeitung von einzelnen Vertices im Rendering-Pipeline übernimmt. Vertex-Shader erhalten Vertex-Attribute-Daten, die von einem Zeichenbefehl aus einem Vertex-Array-Objekt spezifiziert werden. Ein Vertex-Shader erhält einen einzelnen Vertex aus dem Vertex-Stream und generiert einen einzelnen Vertex für den Ausgangs-Vertex-Stream 1. Es muss eine 1:1-Zuordnung von Eingangs- zu Ausgangs-Vertices geben. Vertex-Shader führen in der Regel Transformationen in den Post-Projektionsraum durch, um von der Vertex-Post-Processing-Phase verarbeitet zu werden. Sie können auch verwendet werden, um pro-Vertex-Beleuchtung durchzuführen oder Setup-Arbeiten für spätere Shader-Stufen durchzuführen.

Der für dieses Programm notwendige Shader genießt, da die eigentliche Bildberechnung erst im später folgenden Fragment-Shader erfolgt, eine untergeordnete Rolle. Das Hauptprogramm generiert eine kompakte liste an Koordinaten welche für die Generierung zweier Dreiecke verwendet werden.
```cs
List<Vector2> vertices = new()
{
    new Vector2(-1f, 1f),  // topleft vert
    new Vector2(1f,  1f),  // topright vert
    new Vector2(-1f, -1f), // bottomleft vert
    new Vector2(1f,  -1f)  // bottomright vert
};
```

Diese werden zu zwei Dreiecken zusammengesetzt um den gesamten Darstellungsbereich auszufüllen. Diese Dreiecke dienen sozusagen als Leinwand für den darauffolgenden Fragment-Shader welcher dann jedes Pixel individuell coloriert.

```glsl
#version 430 core

layout (location = 0) in vec2 aPosition; // vertex coordinates

void main()
{
    gl_Position = vec4(aPosition, 1., 1.); // coordinates
}
```
Der hier verwendete Vertex-Shader setzt die Eingangs-Vertex-Koordinaten und setzt `gl_Position` auf einen 4D-Vektor im homogenen Bildraum. Die Z-Koordinate, welche die Tiefeninformation im Raum darstellt, wird konstant auf 1.0 gesetzt, da es sich um ein zweidimensionales Bild auf der Basis des Fragment-Shaders handelt.

#### Fragment Shader
Der Fragment-Shader ist, in diesem Fall, gänzlich für die Darstellung aller Anzeigeelemente des Programms verantwortlich.

Der Fragment-Shader wird für jedes einzelne Pixel welches sich auf der Leinwand befindet aufgerufen. Für eine Anwendung mit eine Auflösung von 1920x1080 Pixeln entspricht das 2.073.600 Mal.

Der einzelnen Programmroutine wird standardmäßig die Koordinate des derzeit zu berechnenden Pixels übergeben.