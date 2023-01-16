# Unity-Grass

Grass rendering experiment in Unity. Uses an HLSL compute shader and graphics shader to draw procedurally generated blades on the GPU.  

Essentially an implementation of the approach used in Ghost of Tsushima, detailed in this incredible talk: 
[Procedural Grass in 'Ghost of Tsushima'
](https://www.youtube.com/watch?v=Ibe1JBF5i5Y).

The grass is quite performant (although there are crucial optimisations that should be added, like LODing). The look and movement of the grass is highly customisable and can be changed using various parameters. 

![Screenshot (4)](https://user-images.githubusercontent.com/52975691/211556726-e6e77c88-584e-4e81-9985-92c911e1d8d5.png)


## Key Features:

- **Shape of blades** determined by cubic Bezier curves
- **Wind animation** driven by scrolling 2D perlin noise inputted to a sin-based function that modulates various parameters of the grass
- **Clumping**: Grass can be grouped into Voronoi clumps that share the same parameters, for a less uniform look
- **Lighting**: Phong shading, with gloss map, and fake ambient occlusion based on length of blade
- **Grass color**: combination of color gradient along length, clump color, and albedo texture
- **Heightmap terrain**: blades placed on the surface of a heightmap terrain
- **GPU instancing**, allowing for fast rendering of millions of blades
- **Frustum culling**, Blades outside of the viewing frustum are not rendered
- **Distance culling**, Fewer blades are rendered at distance, with a smooth transition between near and far

## Videos

Some videos showcasing different styles of grass in motion (unfortunately very low quality due to Github's limit on video size).

https://user-images.githubusercontent.com/52975691/211547463-1073548f-4bba-4a1e-b0dc-1690bdc2d1cd.mp4


https://user-images.githubusercontent.com/52975691/211549861-9bd80d5b-2317-4518-937f-d0aba6c5a52d.mp4



https://user-images.githubusercontent.com/52975691/211554795-c2036cdd-9bba-4053-8bfd-650d8facf023.mp4




https://user-images.githubusercontent.com/52975691/211555848-5ef4aecc-e227-4d08-ab03-b912c04775af.mp4



https://user-images.githubusercontent.com/52975691/211556323-045d8a49-0474-4d55-8208-5969c4ea98db.mp4

## Overview

A compute shader is run: each thread of the compute shader computes a single blade of grass. First, a position is computed: the blades are evenly spread across the terrain and slightly jittered. We check if the grass blade should be rendered by doing frustum and distance culling on the position. If the blade should be rendered, we continue, else we drop out. Each blade belongs to a particular clump. Each clump type has its own set of artist-authored parameters that determine things like the height, bend, and color of the blade. The computed parameters for the blade are packed into a GrassBlade struct and added to an AppendBuffer. 

> **_NOTE:_**
It is most convenient to use an AppendBuffer as opposed to a RWStructuredBuffer because the number of blades rendered varies per frame due to frustum and distance culling. It is possible to use a RWStructuredBuffer though as demonstrated in [Acerolas video about grass rendering](https://www.youtube.com/watch?v=jw00MbIJcrk).

The vertex shader is then told to render many blades of grass using Graphics.DrawProceduralIndirect(). The blades of grass were modelled in Blender and have data packed into the vertex colors, such as how far along the blade the vertex is, and which side of the blade its on. 



> **_NOTE:_**
The vertex shader needs to know how many blades to draw. This is achieved by copying the size of the AppendBuffer into the indirectArgsBuffer of DrawProceduralIndirect() using ComputeBuffer.CopyCount(). 


In the vertex shader, we can index into the GrassBlades buffer (that lives on the GPU) to get the parameters for our current blade. The vertex is placed based on a Bezier curve determined by the GrassBlade parameters. Since we are using Bezier curves it is also easy to get the normal for the vertex, crossing the tangent of the curve (easily computable), with the side facing vector. We also animate the blade in the vertex shader by moving points of the Bezier curve based on the windForce. 

In the fragment shader, we do lighting (Phong Shading) and coloring of our procedurally generated geometry.

## Details:

### Shape of blades

The shape of the blades is determined by evaluating cubic Bezier curves. Each blade is 15 vertices which are placed along a Bezier curve in the vertex shader. The Bezier curve is defined by its 4 Bezier control points, which are determined based on parameters such as height, width, tilt, and bend, with random variation between blades. 

The parameters of a grassblade are contained in the GrassBlade struct:

```
struct GrassBlade {

    float3 position; 
    float rotAngle; 
    float hash; 
    float height; 
    float width;
    float tilt; 
    float bend; 
    float3 surfaceNorm;
    float3 color; 
    float windForce;
    float sideBend;
    float clumpColorDistanceFade;
};
```

The blades are also tapered down along the length. 

### Wind animation

The wind animation is driven by scrolling 2D perlin noise. The noise is inputted to a sin-based function that modulates various parameters of the grass such as its Bezier control points and its facing direction. 

### Grass clumping: 

The way the grass clumps together in the field can be controlled, for a less uniform, more organic look. This is meant to mimic the way grass grows in patches in nature.

The grass is divided into cells generated using a procedural Voronoi algorithm. Each cell is assigned a clump id indexing into the list of user defined clumps. 

Each clump type has its own set of artist-authored parameters. All grass that belongs to that clump will use these parameters.

```
struct ClumpParametersStruct {

      float pullToCentre;
      float pointInSameDirection;
      float baseHeight;
      float heightRandom;
      float baseWidth;
      float widthRandom;
      float baseTilt;
      float tiltRandom;
      float baseBend;
      float bendRandom;
      
};
```

These parameters can be used to achieve various effects, like pulling grass towards the center point of its clump, or controlling how much the grass in a clump points in the same direction.


### Clever tricks (mostly from the Tsushima grass talk)

#### Redistributing vertices of grass towards tip

Often most of the bend in grass blades is in the tip. If the verts are evenly distributed, this results in wasted verts used to represent straight geometry at the bottom of the blade. Verts can be distributed more towards the tip of the blade (where they are needed) by tweaking a parameter. 

#### Curved normals

The normals of the blade can be tilted outwards to give the appearance of curvature. This helps the blades look more 3D, and fuller, without adding extra verts. 

#### Blending normals to surface normal at distance

Even with temporal anti-aliasing the grass can be very grainy and aliased at distance, due to the constant movement of the blades, and the glossy specular highlights. To alleviate this, the normals of the blades can be lerped towards the normal of the underlying terrain at distance. This results in less noisiness and graininess because the normals vary less in screen space. 

Some other tricks that I used for distant grass: 
- Blending the color of distant terrain to match the top color of the grass. This creates the illusion of the grass density at distance being the same, even though it is heavily culled.
- Fading out the ambient occlusion at the base of the blade at distance. Seeing spots of darkness/shadow in distant regions looked unnatural.

#### Realigning blades verts when viewed side-on

Often the player will be looking at the grass exactly side on - rendering it very thin, or even invisible. In such cases the verts can be tilted to more face the player's view. This is achieved by slightly shifting the verts when the blade's facing is roughly orthogonal to the view vector. Each vert is rotated about the tangent to the Bezier curve at that point. 

## To-do
- LODing
- Do transparency fade of distance culled grass
- Painting grass positions with Splatmap
- Receive shadows from shadow map
- Cast shadows onto terrain (do not actually include grass in shadow pass, but maybe fake it using Tsushima's method)
- Have grass deform based on player movement
- Occlusion culling
- Spend verts for single blade on multiple blades when grass is short (Tsushima does this)

## Resources

I used a lot of online resources to learn the techniques used. I don't remember everything that I referenced but I'll list the main ones: 

- [Procedural Grass in 'Ghost of Tsushima](https://www.youtube.com/watch?v=Ibe1JBF5i5Y)
- [A coder's guide to spline-based procedural geometry (Freya Holmer)](https://www.youtube.com/watch?v=o9RK6O2kOKo)
- [Wikipedia - Phong Shading](https://en.wikipedia.org/wiki/Phong_reflection_model)
- [CatlikeCoding - Compute Shaders](https://catlikecoding.com/unity/tutorials/basics/compute-shaders/)
- [Ned Makes Games - Intro to Compute Shaders](https://www.youtube.com/watch?v=EB5HiqDl7VE)
- [Ronja Tutorials - Graphics.DrawProcedural](https://www.ronja-tutorials.com/post/051-draw-procedural/)
- [Acerola - How Do Games Render So Much Grass?](https://www.youtube.com/watch?v=Y0Ko0kvwfgA)


