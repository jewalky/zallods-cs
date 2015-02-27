uniform float main_width;
uniform float main_height;

varying vec4 vColor;
varying vec2 vTexCoord;
void main()
{
	// typical ortho with reversed Y coordinate
	float width2 = main_width / 2.0;
	float height2 = main_height / 2.0;
	vec4 p = gl_Vertex + vec4(-width2, -height2, 0.0, 0.0);
	p = p * vec4(1.0, -1.0, 1.0, 1.0) / vec4(width2, height2, 1.0, 1.0);
	gl_Position = p;
	vColor = gl_Color;
	vTexCoord = gl_MultiTexCoord0;
}