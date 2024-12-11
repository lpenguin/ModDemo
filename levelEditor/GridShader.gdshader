shader_type spatial;
render_mode unshaded, blend_mix;

uniform vec4 grid_color : source_color = vec4(0.2, 0.2, 0.2, 0.5);
uniform float grid_size = 1.0;
uniform float line_thickness = 0.02;

void fragment() {
    vec2 uv = UV * 20.0; // Scale UV to match the grid size
    vec2 grid = abs(fract(uv - 0.5) - 0.5) / fwidth(uv);
    float line = min(grid.x, grid.y);
    float alpha = 1.0 - min(line, 1.0);

    // Major grid lines (every 5 units)
    vec2 uv_5 = UV * 4.0;
    vec2 grid_5 = abs(fract(uv_5 - 0.5) - 0.5) / fwidth(uv_5);
    float line_5 = min(grid_5.x, grid_5.y);
    float alpha_5 = 1.0 - min(line_5, 1.0);

    alpha = max(alpha * 0.5, alpha_5 * 0.8); // Combine both grids

    ALBEDO = grid_color.rgb;
    ALPHA = alpha * grid_color.a;
}