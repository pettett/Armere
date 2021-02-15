
//struct that will be outputted from the grass mover and sent to the shader
struct MatrixStruct {
    float4x4 mat;
    float3 color;
	int chunkID; //If chunkID == 0, grass does not exist + makes size better?
};
