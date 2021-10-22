
const uint2 neighbors[] = {
    uint2(-1,0),
    uint2(1,0),
    uint2(0,1),
    uint2(0,-1),
    uint2(-1,1),
    uint2(1,1),
    uint2(-1,1),
    uint2(-1,-1)
};
void SampleNeighbors(uint2 pixel, out float4 col)
{
    col = 0;
    for(int i = 0; i < 4; i++)
    {
        const uint2 neighborPixel = pixel + neighbors[i];
        const float4 neighborColor = Result[neighborPixel*3];
        // const float thres = 0.001;
        // if(neighborColor.r > thres || neighborColor.g > thres || neighborColor.b > thres)
        {
            col += neighborColor;
        }
    }
}