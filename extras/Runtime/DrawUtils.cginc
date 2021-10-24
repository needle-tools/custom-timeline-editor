


void DrawLine(RWTexture2D<float4> tex, uint2 texSize, float2 start, float2 end, float4 color)
{
    const uint steps = sqrt(texSize.x*texSize.y) * length(start.xy - end.xy);
    for (uint i = 0; i <= steps; i++)
    {
        float2 pos = lerp(start.xy, end.xy, i / (float)steps);
        const int2 pixel = (pos.xy + .5) * texSize;
        tex[pixel] = color;
    }
}

void DrawCircle(RWTexture2D<float4> tex, uint2 texSize, float2 center, float radius, float4 color)
{
    float sq = radius * radius;
    const int2 pt = (center.xy + .5) * texSize;
    for (int x = pt.x - radius; x < pt.x + radius; x++)
    {
        for (int y = pt.y - radius; y < pt.y + radius; y++)
        {
            if ((pt.x - x) * (pt.x - x) + (pt.y - y) * (pt.y - y) < sq)
            {
                tex[uint2(x, y)] = color;
            }
        }
    }
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
