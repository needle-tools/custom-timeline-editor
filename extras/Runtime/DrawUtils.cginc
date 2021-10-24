
void DrawLine(float2 start, float2 end, uint width, uint height, RWTexture2D<float4> tex)
{
    const uint steps = sqrt(width * height) * length(start.xy - end.xy);
    for (uint i = 0; i <= steps; i++)
    {
        float2 pos = lerp(start.xy, end.xy, i / (float)steps);
        const int2 pixel = (pos.xy + .5) * uint2(width, height);
        tex[pixel] = float4(1, 0, 0, 1);
    }
}

void DrawCircle(RWTexture2D<float4> tex, int2 pt)
{
    
}


// float GetDist(float ax, float ay, float bx, float by, float x, float y)
// {
//     if ((ax - bx) * (x - bx) + (ay - by) * (y - by) <= 0)
//         return sqrt((x - bx) * (x - bx) + (y - by) * (y - by));
//
//     if ((bx - ax) * (x - ax) + (by - ay) * (y - ay) <= 0)
//         return sqrt((x - ax) * (x - ax) + (y - ay) * (y - ay));
//
//     return abs((by - ay) * x - (bx - ax) * y + bx * ay - by * ax) /
//         sqrt((ay - by) * (ay - by) + (ax - bx) * (ax - bx));
// }
