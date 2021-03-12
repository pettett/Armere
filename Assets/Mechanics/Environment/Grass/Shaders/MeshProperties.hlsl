
#define THREADGROUPS 64



struct MeshProperties { //total 64
    float3 position; // 12
    float yRot;		//4
    float2 size;	//8
    float3 color;	//12
	//If chunkID = 0, grass does not exist
    uint chunkID;	//4

	//float3 blank0;
	//float3 blank1;
};


