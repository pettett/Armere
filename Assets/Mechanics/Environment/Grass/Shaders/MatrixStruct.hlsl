
//struct that will be outputted from the grass mover and sent to the shader
struct MatrixStruct {
    float4x4 worldTransform;
    float3 color;
	float unused;
	//uint chunkID; //If chunkID == 0, grass does not exist + makes size better?
};



float4x4 PackMatrix(float4x4 mat){
	float4x4 packed = (float4x4)0;

	packed[0] = mat[0];
	packed[1] = mat[1];
	packed[2] = mat[2];
	packed[3] = mat[3];

	return mat;
}
float4x4 UnpackMatrix(float4x4 mat){
	float4x4 unPacked = (float4x4)0;

	unPacked[0] = mat[0];
	unPacked[1] = mat[1];
	unPacked[2] = mat[2];
	unPacked[3] = mat[3];

	unPacked[3][3] = 1;

	return mat;
}