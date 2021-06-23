
//struct that will be outputted from the grass mover and sent to the shader
struct MatrixStruct {
    float3x4 worldTransform;
    float3 color;
	float unused;
	//uint chunkID; //If chunkID == 0, grass does not exist + makes size better?
};
