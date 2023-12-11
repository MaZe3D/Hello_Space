#version 430 core

out vec4 FragColor;
in vec4 gl_FragCoord;

layout(location = 0) uniform float timestamp; //time in seconds
layout(location = 1) uniform ivec2 resolution; //window resolution
layout(location = 2) uniform float lowSample; //audo sample
layout(location = 3) uniform float midSample; //audo sample
layout(location = 4) uniform float highSample; //audo sample


void main()
{
	float x = gl_FragCoord.x;
    float y = gl_FragCoord.y;
    vec4 color = vec4(1.0,1.0,1.0,1.0);

    float desiredX = resolution.x / 2.0;
    float desiredY = resolution.y / 2.0;
    bool whiteBG = true;

    if( abs(x - desiredX) < 0.6 || abs(y - desiredY) < 0.6 ) {
        color = vec4(0.0,0.0,0.0,0.0);
        whiteBG = false;
    }

    float desiredWaveWidth = 128.0 * lowSample * 100.0;
    float wavesOnScreen = resolution.x / desiredWaveWidth;
    float gradientWidth = 5.0;
    float sinWaveHeight = 130.0;
    float sinWaveSpeedMultiplier = 2.5;
    float sinWaveWidth = resolution.x / (3.141592 * wavesOnScreen);

    float xInWidth = x / (sinWaveWidth);
    float sinWaveMovementModifier = 0.0;
    // uncommenting the sinWaveMovementModifier below will cause
    // the sin wave to move horizontally across the screen.
    sinWaveMovementModifier = timestamp * sinWaveSpeedMultiplier;

    float sinx = sin(sinWaveMovementModifier + xInWidth);
    float sinxPositive = (sinx + 1.0) / 2.0;

   	float yInHeight = sinWaveHeight * sinxPositive;
    float centerScreenOffset = (resolution.y / 2.0) - sinWaveHeight / 2.0;

    float siny = yInHeight + centerScreenOffset;

    float distance = abs(siny - y);

    if(distance < gradientWidth){
        if(whiteBG == true){
        	color.g = (distance / gradientWidth);
        	color.b = (distance / gradientWidth);
        } else {
            color.r = 1.0 - (distance / gradientWidth);
        }
    }

	FragColor = color;
}

/* void main()
{
	vec2 uv = gl_FragCoord.xy / resolution.xy;
	FragColor = vec4(uv.x * lowSample * 5 + 0.5, 0.0, uv.y * midSample * 5 + 0.5, 1.0);
} */


/*
#define RADIANS 0.017453292519943295

const int zoom = 40;
const float brightness = 0.975;
float fScale = 1.25;

float cosRange(float degrees, float range, float minimum) {
	return (((1.0 + cos(degrees * RADIANS)) * 0.5) * range) + minimum;
}

void main()
{
	float time = timestamp * 1.25 + audioSample * 3;
	vec2 uv = gl_FragCoord.xy / resolution.xy;
	vec2 p  = (2.0*gl_FragCoord.xy-resolution.xy)/max(resolution.x,resolution.y);
	float ct = cosRange(time*5.0, 3.0, 1.1);
	float xBoost = cosRange(time*0.2, 5.0, 5.0);
	float yBoost = cosRange(time*0.1, 10.0, 5.0);
	
	fScale = cosRange(time * 15.5, 1.25, 0.5);
	
	for(int i=1;i<zoom;i++) {
		float _i = float(i);
		vec2 newp=p;
		newp.x+=0.25/_i*sin(_i*p.y+time*cos(ct)*0.5/20.0+0.005*_i)*fScale+xBoost;		
		newp.y+=0.25/_i*sin(_i*p.x+time*ct*0.3/40.0+0.03*float(i+15))*fScale+yBoost;
		p=newp;
	}
	
	vec3 col=vec3(0.5*sin(3.0*p.x)+0.5,0.5*sin(3.0*p.y)+0.5,sin(p.x+p.y));
	col *= brightness;
    
    // Add border
    float vigAmt = 5.0;
    float vignette = (1.-vigAmt*(uv.y-.5)*(uv.y-.5))*(1.-vigAmt*(uv.x-.5)*(uv.x-.5));
	float extrusion = (col.x + col.y + col.z) / 4.0;
    extrusion *= 1.5;
    extrusion *= vignette;
    
	FragColor = vec4(col, extrusion);
}*/