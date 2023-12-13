#version 430 core

out vec4 FragColor;
in vec4 gl_FragCoord;

layout(location = 0) uniform float timestamp; //time in seconds
layout(location = 1) uniform ivec2 resolution; //window resolution
layout(location = 2) uniform float left_lowSample; //audo sample
layout(location = 3) uniform float left_midSample; //audo sample
layout(location = 4) uniform float left_highSample; //audo sample
layout(location = 5) uniform float right_lowSample; //audo sample
layout(location = 6) uniform float right_midSample; //audo sample
layout(location = 7) uniform float right_highSample; //audo sample
layout(location = 8) uniform float audioDuration; //audo sample
layout(location = 9) uniform vec2 mousePos; //audo sample
layout(location = 10) uniform float mouseBloom; //radius of the mouseBloom

const float PI = radians(180);
vec2  UV = gl_FragCoord.xy / resolution.xy;
float ASPECT_RATIO = float(resolution.x) / float(resolution.y);
vec2  ACORD = vec2(UV.x * ASPECT_RATIO, UV.y);
vec2  MAX_DIMENSIONS = vec2(ASPECT_RATIO, 1.0);
vec2  CENTER = vec2(MAX_DIMENSIONS.x / 2, MAX_DIMENSIONS.y/2);
float audioProgress = timestamp / audioDuration;
float audioProgressLeft = 1.0 - audioProgress;
vec2 mousePosA = vec2(mousePos.x * MAX_DIMENSIONS.x, mousePos.y * MAX_DIMENSIONS.y);

// Fade Color with Background Color
vec4 FadeColor(vec4 color, vec4 backgroundColor)
{
    return mix(backgroundColor, color, color.a);
}

// Generate a square on the screen
vec4 GenerateSqare(vec2 position, vec2 dimentions, vec4 color, vec4 backgroundColor)
{
    vec4 result = backgroundColor;

    // if dimensions are negative, shift the position and make them positive
    if (dimentions.x < 0.0)
    {
        position.x += dimentions.x;
        dimentions.x = -dimentions.x;
    }
    if (dimentions.y < 0.0)
    {
        position.y += dimentions.y;
        dimentions.y = -dimentions.y;
    }
    // Colorise the pixel if it is inside the square
    if (ACORD.x > position.x && ACORD.x < position.x + dimentions.x &&
        ACORD.y > position.y && ACORD.y < position.y + dimentions.y)
    {
        result = color;
    }

    // blend with the background color
    result = FadeColor(result, backgroundColor);

    return result;
}

// Generate a sin wave on the screen
vec4 GenerateSinWave(vec2 position, float amplitude, float frequency, float phase, float borderSize, vec4 color, vec4 backgroundColor)
{
    vec4 result = backgroundColor;

    // Colorise the pixel if it is inside the sin wave
    float sinValue = sin(2*PI*(ACORD.x - position.x) * frequency + phase) * amplitude + position.y;

    float dist = abs(ACORD.y - sinValue);

    if (dist < borderSize)
    {
        result = color;
    }

    result = FadeColor(result, backgroundColor);

    return result;
}

// Mask Sin Wave inside a square
vec4 GenerateMaskSinWave(vec2 position, vec2 dimensions, float frequency, float phase, float sinOffset, float borderSize, vec4 color, vec4 backgroundColor)
{
    return GenerateSqare(position, dimensions , GenerateSinWave(vec2(position.x + sinOffset, position.y + dimensions.y / 2), (dimensions.y / 2) - borderSize, frequency, phase, borderSize, color, backgroundColor), backgroundColor);
}

// Generate a circle on the screen
vec4 GenerateCircle(vec2 position, float radius, vec4 color, vec4 backgroundColor)
{
    vec4 result = backgroundColor;

    // blend with the background color

    // Colorise the pixel if it is inside the circle
    float dist = distance(ACORD, position);

    if (dist < radius)
    {
        result = color;
    }

    result = FadeColor(result, backgroundColor);

    return result;
}

// Generate a rounded square on the screen. It uses the Generate Circle function and generates two squares to fill it
vec4 GenerateRoundedSquare(vec2 position, vec2 dimensions, float radius, vec4 color, vec4 backgroundColor)
{
    vec4 result = backgroundColor;

    // if dimensions are negative, shift the position and make them positive
    if (dimensions.x < 0.0)
    {
        position.x += dimensions.x;
        dimensions.x = -dimensions.x;
    }
    if (dimensions.y < 0.0)
    {
        position.y += dimensions.y;
        dimensions.y = -dimensions.y;
    }

    float diameter = 2 * radius;

    // if dimensions are smaller than the radius, make them equal to the diameter
    if (dimensions.x < radius)
    {
        dimensions.x = diameter;
    }
    if (dimensions.y < radius)
    {
        dimensions.y = diameter;
    }

    vec2 circlePosition = position + radius;
    vec2 circleDimensions = dimensions - radius;

    // Generate circles on the corners. The origin of the sqare is outside the rounded corners
    result = GenerateCircle(vec2(circlePosition.x, circlePosition.y), radius, color, result);
    result = GenerateCircle(vec2(position.x + circleDimensions.x, circlePosition.y), radius, color, result);
    result = GenerateCircle(vec2(circlePosition.x, position.y + circleDimensions.y), radius, color, result);
    result = GenerateCircle(vec2(position.x + circleDimensions.x, position.y + circleDimensions.y), radius, color, result);

    // two sqares to fill the space between the circles
    result = GenerateSqare(vec2(circlePosition.x, position.y), vec2(dimensions.x - diameter, dimensions.y), color, result);
    result = GenerateSqare(vec2(position.x, circlePosition.y), vec2(dimensions.x, dimensions.y - diameter), color, result);

    return result;
}

// Generate a grid of points, normalized to the aspect ratio of the screen.
vec4 GeneratePointGrid(vec2 position, vec2 dist, vec2 size, float radius, vec4 color, vec4 backgroundColor)
{
    vec4 result = backgroundColor;

    vec2 currentPos = position;
    float numberY = size.y / dist.y;
    float numberX = size.x / dist.x;

    for(int i = 0; i < numberY; i++)
    {
        for(int j = 0; j < numberX; j++)
        {
            result = GenerateCircle(currentPos, radius, color, result);
            if (result != color)
            currentPos.x += ASPECT_RATIO / numberX;
        }
        currentPos.y += 1.0 / numberY;
        currentPos.x = position.x;
    }

    return result;
}

// Generate a glowing circle which fades from a minimum radius up to a maximum radius. Blend with the background color
vec4 GenerateGlowingCircle(vec2 position, float minimumRadius, float maximumRadius, vec4 color, vec4 backgroundColor)
{
    vec4 result = backgroundColor;

    // Colorise the pixel if it is inside the circle
    result = GenerateCircle(position, minimumRadius, color, result);

    float dist = distance(ACORD, position);

    if (dist > minimumRadius && dist < maximumRadius)
    {
        float alpha = 1 - (dist - minimumRadius) / (maximumRadius - minimumRadius);
        result = mix(result, color, alpha * backgroundColor.a);
    }
    return result;
}

void main()
{
    float left_lowSample2 = left_lowSample * 2;
    float left_midSample2 = left_midSample * 3;
    float left_highSample2 = left_highSample * 4;
    float right_lowSample2 = right_lowSample * 2;
    float right_midSample2 = right_midSample * 3;
    float right_highSample2 = right_highSample * 4;

    FragColor = vec4(0.0, 0.0, 0.0, 0.0);

    //display grid of points
    float pointGridModulation = 1.;//0.04 * (sin(2*PI*timestamp*0.1)+2)/2;
    FragColor = GeneratePointGrid(vec2(0.025, 0.025), vec2(0.03, 0.03), MAX_DIMENSIONS, 0.003, vec4(0.25, 0.0, 0.25, 1.0), FragColor);

    // Colors for the Boxes
    vec4 bassColor = vec4(0.855, 0.251, 1.0, 0.7);
    vec4 midColor = vec4(0.251, 0.659, 1.0, 0.7);
    vec4 highColor = vec4(0.251, 1.0, 0.855, 0.7);

    //position
    float lowHeight = CENTER.y / 2;
    float midHeight = CENTER.y;
    float highHeight = CENTER.y + CENTER.y / 2;

    float left = 0.25;
    float right = MAX_DIMENSIONS.x - left;

    float lowRadius = 0.9;
    float midRadius = 0.8;
    float highRadius = 0.6;

    float zero = 0.0;

    // Colorise the pixels in the background
    FragColor = GenerateGlowingCircle(vec2(left,  lowHeight),  zero, lowRadius  * left_lowSample2,  bassColor, FragColor);
    FragColor = GenerateGlowingCircle(vec2(right, lowHeight),  zero, lowRadius  * right_lowSample2, bassColor, FragColor);
    FragColor = GenerateGlowingCircle(vec2(left,  midHeight),  zero, midRadius  * left_midSample2 , midColor,  FragColor);
    FragColor = GenerateGlowingCircle(vec2(right, midHeight),  zero, midRadius  * left_midSample2 , midColor,  FragColor);
    FragColor = GenerateGlowingCircle(vec2(left,  highHeight), zero, highRadius * left_highSample2, highColor, FragColor);
    FragColor = GenerateGlowingCircle(vec2(right, highHeight), zero, highRadius * left_highSample2, highColor, FragColor);

    //Glow around the mouse
    FragColor = GenerateGlowingCircle(mousePosA, 0.0, mouseBloom, vec4(0.8, 0.8, 0.8, 0.5), FragColor);

    const float roundingsize = 0.015;
    const float barHeight = 0.1;
    float yOffset = -barHeight / 2;
    float xOffset = roundingsize / 2;

    FragColor = GenerateRoundedSquare(vec2(CENTER.x - xOffset - left_lowSample2,  lowHeight  + yOffset), vec2((left_lowSample2  + right_lowSample2 + xOffset) , barHeight), roundingsize, vec4(bassColor.xyz * 0.9, 1), FragColor);
    FragColor = GenerateRoundedSquare(vec2(CENTER.x - xOffset - left_midSample2,  midHeight  + yOffset), vec2((left_midSample2  + right_midSample2 + xOffset) , barHeight), roundingsize, vec4(midColor .xyz * 0.9, 1), FragColor);
    FragColor = GenerateRoundedSquare(vec2(CENTER.x - xOffset - left_highSample2, highHeight + yOffset), vec2((left_highSample2 + right_highSample2 + xOffset), barHeight), roundingsize, vec4(highColor.xyz * 0.9, 1), FragColor);

    //display a thin rectangle in the center
    FragColor = GenerateSqare(vec2(CENTER.x, 0), vec2(0.005, MAX_DIMENSIONS.y), vec4(0.0, 0.0, 0.0, 1.0), FragColor);


    //display a black rectangle on the bottom
    vec2 progressSectionlocation = vec2(0, 0);
    vec2 progressSectionDimemtions = vec2(MAX_DIMENSIONS.x, 0.07);
    vec2 progressSectionDimemtionsCenter = progressSectionDimemtions / 2.0;
    vec4 progressSectionBackgroundColor = vec4(0.0, 0.0, 0.0, 1.0);
    vec2 progressSectionBorderSize = vec2(0.0, 0.01);
    vec2 progressIndicatorDimentions = vec2(0.02, progressSectionDimemtions.y - progressSectionBorderSize.y * 2);
    vec4 progressIndicatorColor = vec4(0.8, 0.0, 0.6, 1.0);
    vec2 progressIndicatorPosition = vec2(audioProgress * MAX_DIMENSIONS.x, progressSectionBorderSize.y);
    float progressIndicatorRadius = 0.005;
    vec4 progressTimeLeftLineColor = vec4(0.4, 0.0, 0.8, 1.0);
    float progressIndicatorTimeLeftLineWidth = 0.003;
    vec2 progressSinWaveDimentions = vec2(progressIndicatorPosition.x, progressIndicatorDimentions.y-0.02);
    float progressSinWaveFrequency = (1 / progressSinWaveDimentions.y)*0.75;
    float progressSinWavePhase = timestamp * 2;
    float progressSinWaveSinOffset = 0.0;
    vec4 progressSinWaveColor = vec4(0.4, 0.0, 1.0, 1.0);
    float progressSinWaveWidth = 0.005;
    // vec4 GenerateMaskSinWave(vec2 position, vec2 dimensions, float frequency, float phase, float sinOffset, float borderSize, vec4 color, vec4 backgroundColor)
    FragColor = GenerateSqare(vec2(0, 0), progressSectionDimemtions, vec4(0.0, 0.0, 0.0, 1.0), FragColor);
    FragColor = GenerateSqare(vec2(progressIndicatorPosition.x, progressSectionDimemtionsCenter.y), vec2(MAX_DIMENSIONS.x * audioProgressLeft, progressIndicatorTimeLeftLineWidth), progressTimeLeftLineColor, FragColor);

    FragColor = GenerateMaskSinWave(vec2(0, progressSectionBorderSize.y + 0.01), progressSinWaveDimentions, progressSinWaveFrequency, progressSinWavePhase, progressSinWaveSinOffset, progressSinWaveWidth, progressSinWaveColor, FragColor);

    FragColor = GenerateRoundedSquare(vec2(audioProgress * MAX_DIMENSIONS.x, progressSectionBorderSize.y), progressIndicatorDimentions, progressIndicatorRadius, progressIndicatorColor, FragColor);

    return;
}