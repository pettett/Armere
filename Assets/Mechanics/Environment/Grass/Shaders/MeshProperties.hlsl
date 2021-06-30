
#define THREADGROUPS 64



struct MeshProperties { 
    float3 position; // 12
	//12
    float yRot;		//4
	//16
    float size;	//4
	//20
    float3 color;	//12
	//32
	//If chunkID = 0, grass does not exist
    uint chunkID;	//4
	//36
	//Space for 6 more floats
	float shrinkDistance;
};


