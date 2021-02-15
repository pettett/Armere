
#define THREADGROUPX 64
#define THREADGROUPY 1

inline uint DispatchThreadToIndex(uint3 id){
    return id.x + id.y * dispatchSize.x * THREADGROUPX + id.z * dispatchSize.x * THREADGROUPX * dispatchSize.y * THREADGROUPY;
}


struct MeshProperties {
    float3 position;
    float yRot;
    float2 size;
    float3 color;
	//If chunkID = 0, grass does not exist
    int chunkID;
};


