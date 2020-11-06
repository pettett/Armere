
#define THREADGROUPX 64
#define THREADGROUPY 1

inline uint DispatchThreadToIndex(uint3 id, uint2 dispatchSize){
    return id.x + id.y * dispatchSize.x * THREADGROUPX + id.z * dispatchSize.x * THREADGROUPX * dispatchSize.y * THREADGROUPY;
}

struct MeshProperties {
    float3 position;
    float3 rotation;
    float2 size;
    float4 color;
};


