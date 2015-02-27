uniform sampler2D tex1;
uniform sampler2D tex2; // used for palettes

uniform float main_stencil;
uniform float main_paletted;

varying vec4 vColor;
varying vec2 vTexCoord;
void main()
{
	float stencil = clamp(main_stencil, 0.0, 1.0);

	// only one condition here
	if (main_paletted != 0.0)
	{
		vec4 cc = texture2D(tex1, vTexCoord);
		vec4 c;
		c.rgb = texture2D(tex2, vec2(cc.r, 0)).rgb;
		c.a = cc.a;
		c = (vec4(vColor.rgb, cc.a)*stencil)+((vColor*c)*(1.0-stencil));
		gl_FragColor = c;
	}
	else
	{
		vec4 c = texture2D(tex1, vTexCoord);
		c = (vColor*stencil)+((vColor*c)*(1.0-stencil));
		gl_FragColor = c;
	}                                                                   
}