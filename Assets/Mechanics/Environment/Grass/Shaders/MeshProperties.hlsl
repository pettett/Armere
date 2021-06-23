
#define THREADGROUPS 64



struct MeshProperties { 
    float3 position; // 12
	//12
    float yRot;		//4
	//16
    float2 size;	//8
	//24
    float3 color;	//12
	//36
	//If chunkID = 0, grass does not exist
    uint chunkID;	//4
	//40
	//Space for 6 more floats
	float shrinkDistance;
};


