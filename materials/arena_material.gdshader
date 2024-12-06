shader_type spatial;

// Noise texture parameters
uniform float noise_scale : hint_range(0.0, 1.0) = 0.05;
uniform float noise_detail : hint_range(0.0, 50.0) = 15.0;
uniform float noise_roughness : hint_range(0.0, 1.0) = 0.5;
uniform float noise_lacunarity : hint_range(0.0, 5.0) = 2.0;
uniform float noise_distortion : hint_range(0.0, 1.0) = 0.0;

// Material parameters
uniform float roughness : hint_range(0.0, 1.0) = 1.0;
uniform float metallic : hint_range(0.0, 1.0) = 0.0;
uniform float alpha : hint_range(0.0, 1.0) = 1.0;

// Bump parameters
uniform float bump_strength : hint_range(0.0, 2.0) = 1.0;
uniform float bump_distance : hint_range(0.0, 10.0) = 5.0;

// Hash function for Perlin noise
vec4 permute(vec4 x) {
    return mod(((x * 34.0) + 1.0) * x, 289.0);
}

vec4 taylorInvSqrt(vec4 r) {
    return 1.79284291400159 - 0.85373472095314 * r;
}

// 3D Perlin noise
float perlin_noise(vec3 P) {
    vec3 Pi0 = floor(P); // Integer part
    vec3 Pi1 = Pi0 + vec3(1.0); // Integer part + 1
    Pi0 = mod(Pi0, 289.0);
    Pi1 = mod(Pi1, 289.0);
    vec3 Pf0 = fract(P); // Fractional part
    vec3 Pf1 = Pf0 - vec3(1.0); // Fractional part - 1.0
    vec4 ix = vec4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);
    vec4 iy = vec4(Pi0.yy, Pi1.yy);
    vec4 iz0 = vec4(Pi0.z);
    vec4 iz1 = vec4(Pi1.z);

    vec4 ixy = permute(permute(ix) + iy);
    vec4 ixy0 = permute(ixy + iz0);
    vec4 ixy1 = permute(ixy + iz1);

    vec4 gx0 = ixy0 / 7.0;
    vec4 gy0 = fract(floor(gx0) / 7.0) - 0.5;
    gx0 = fract(gx0);
    vec4 gz0 = vec4(0.5) - abs(gx0) - abs(gy0);
    vec4 sz0 = step(gz0, vec4(0.0));
    gx0 -= sz0 * (step(0.0, gx0) - 0.5);
    gy0 -= sz0 * (step(0.0, gy0) - 0.5);

    vec4 gx1 = ixy1 / 7.0;
    vec4 gy1 = fract(floor(gx1) / 7.0) - 0.5;
    gx1 = fract(gx1);
    vec4 gz1 = vec4(0.5) - abs(gx1) - abs(gy1);
    vec4 sz1 = step(gz1, vec4(0.0));
    gx1 -= sz1 * (step(0.0, gx1) - 0.5);
    gy1 -= sz1 * (step(0.0, gy1) - 0.5);

    vec3 g000 = vec3(gx0.x, gy0.x, gz0.x);
    vec3 g100 = vec3(gx0.y, gy0.y, gz0.y);
    vec3 g010 = vec3(gx0.z, gy0.z, gz0.z);
    vec3 g110 = vec3(gx0.w, gy0.w, gz0.w);
    vec3 g001 = vec3(gx1.x, gy1.x, gz1.x);
    vec3 g101 = vec3(gx1.y, gy1.y, gz1.y);
    vec3 g011 = vec3(gx1.z, gy1.z, gz1.z);
    vec3 g111 = vec3(gx1.w, gy1.w, gz1.w);

    vec4 norm0 = taylorInvSqrt(vec4(dot(g000, g000), dot(g010, g010), dot(g100, g100), dot(g110, g110)));
    g000 *= norm0.x;
    g010 *= norm0.y;
    g100 *= norm0.z;
    g110 *= norm0.w;
    vec4 norm1 = taylorInvSqrt(vec4(dot(g001, g001), dot(g011, g011), dot(g101, g101), dot(g111, g111)));
    g001 *= norm1.x;
    g011 *= norm1.y;
    g101 *= norm1.z;
    g111 *= norm1.w;

    float n000 = dot(g000, Pf0);
    float n100 = dot(g100, vec3(Pf1.x, Pf0.yz));
    float n010 = dot(g010, vec3(Pf0.x, Pf1.y, Pf0.z));
    float n110 = dot(g110, vec3(Pf1.xy, Pf0.z));
    float n001 = dot(g001, vec3(Pf0.xy, Pf1.z));
    float n101 = dot(g101, vec3(Pf1.x, Pf0.y, Pf1.z));
    float n011 = dot(g011, vec3(Pf0.x, Pf1.yz));
    float n111 = dot(g111, Pf1);

    vec3 fade_xyz = Pf0 * Pf0 * (3.0 - 2.0 * Pf0);
    vec4 n_z = mix(vec4(n000, n100, n010, n110), vec4(n001, n101, n011, n111), fade_xyz.z);
    vec2 n_yz = mix(n_z.xy, n_z.zw, fade_xyz.y);
    float n_xyz = mix(n_yz.x, n_yz.y, fade_xyz.x);
    return 2.2 * n_xyz;
}

// Get fractal noise by layering Perlin noise
float get_noise(vec3 p) {
    float value = 0.0;
    float amplitude = 1.0;
    float frequency = 1.0;
    vec3 pos = p * noise_scale;
    
    for (int i = 0; i < int(noise_detail); i++) {
        value += amplitude * perlin_noise(pos);
        pos *= noise_lacunarity;
        amplitude *= noise_roughness;
    }
    
    return value * 0.5 + 0.5;
}

void fragment() {
     // Start with view vector (already normalized, in view space)
        vec3 view_dir = VIEW;
        
        // Reconstruct position in view space
        // FRAGCOORD.z contains the depth
        // We need to multiply by view_dir to get the actual position in view space
        vec3 view_pos = view_dir * FRAGCOORD.z;
        
        // Transform to world space using INV_VIEW_MATRIX
        vec4 world_pos = INV_VIEW_MATRIX * vec4(view_pos, 1.0);
        
       // Calculate noise for color
    float noise_color = get_noise(world_pos.xyz);
    vec3 base_color = mix(vec3(0.0), vec3(1.0), noise_color);
    
    // Calculate noise for bump
    vec2 bump_uv = UV;
    float bump = get_noise(vec3(bump_uv * bump_distance, TIME * 0.1));
    
    // Apply material parameters
    ALBEDO = base_color;
    METALLIC = metallic;
    ROUGHNESS = roughness;
    ALPHA = alpha;
    
    // Apply normal mapping from bump
    NORMAL = normalize(mix(NORMAL, NORMAL + vec3(bump * bump_strength), bump_strength));
}