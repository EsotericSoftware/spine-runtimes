/* eslint-disable max-len */
export const darkTintBit = {
    name: 'color-bit',
    vertex: {
        header: /* wgsl */`
            @in aDarkColor: vec4<f32>;
            @out vDarkColor: vec4<f32>;
        `,
        main: /* wgsl */`
        vDarkColor = aDarkColor;
        `
    },
    fragment: {
        header: /* wgsl */`
            @in vDarkColor: vec4<f32>;
        `,
        end: /* wgsl */`
            
        let alpha = outColor.a * vColor.a;
        let rgb = ((outColor.a - 1.0) * vDarkColor.a + 1.0 - outColor.rgb) * vDarkColor.rgb + outColor.rgb * vColor.rgb;

        finalColor = vec4<f32>(rgb, alpha);

        `
    }
};

export const darkTintBitGl = {
    name: 'color-bit',
    vertex: {
        header: /* glsl */`
            in vec4 aDarkColor;
            out vec4 vDarkColor;
        `,
        main: /* glsl */`
            vDarkColor = aDarkColor;
        `
    },
    fragment: {
        header: /* glsl */`
            in vec4 vDarkColor;
        `,
        end: /* glsl */`
            
        finalColor.a = outColor.a * vColor.a;
        finalColor.rgb = ((outColor.a - 1.0) * vDarkColor.a + 1.0 - outColor.rgb) * vDarkColor.rgb + outColor.rgb * vColor.rgb;
        `
    }
};
