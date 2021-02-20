
#define THREADGROUPS 64



struct MeshProperties {
    float3 position;
    float yRot;
    float2 size;
    float3 color;
	//If chunkID = 0, grass does not exist
    uint chunkID;
};


