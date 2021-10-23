
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
// void FindBrightestNeighborRed(RWTexture2D<float4> tex, uint2 pixel, int maxDistance, out float brightness, out int neighborDist)
// {
//     brightness = 0;
//     for(int k = 1; k <= maxDistance; k++)
//     {
//         for(int i = 0; i < 8; i++)
//         {
//             const uint2 neighborPixel = pixel + neighbors[i];
//             const float4 neighborColor = tex[neighborPixel*distance];
//             if(neighborColor.r > 0)
//             {
//             
//                 return;
//             }
//         }
//     }
// }