
#define THREADGROUPX 64
#define THREADGROUPY 1

inline uint DispatchThreadToIndex(uint3 id){
    return id.x + id.y * dispatchSize.x * THREADGROUPX + id.z * dispatchSize.x * THREADGROUPX * dispatchSize.y * THREADGROUPY;
}


struct MeshProperties {
<<<<<<< HEAD
    float3 position;
    float yRot;
    float2 size;
    float3 color;
    int chunkID;
=======
    float3 position;  //12 bytes
    float yRot; //4 bytes - 16
    float2 size; //8 bytes - 24
    float3 color; //12 bytes - 36
    int chunkID; //4 bytes - 40
>>>>>>> 50588ef6582aa230e035005b7444fbed58347fd2
};


