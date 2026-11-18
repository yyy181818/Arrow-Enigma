#ifndef VECTOR_DATA_INCLUDED
#define VECTOR_DATA_INCLUDED

struct VectorsData
{
    half3 geomNormalWS;
    half3 normalWS;
    half3 coatNormalWS;
    half3 bentNormalWS;
    half3 viewDirectionWS;
    half4 tangentWS;
};

VectorsData CreateVectorsData(half3 normalWS, half3 geomNormalWS, half3 coatNormalWS, half3 bentNormalWS, half3 viewDirectionWS, half4 tangentWS)
{
    VectorsData vectorsData;

    vectorsData.geomNormalWS     = geomNormalWS;
    vectorsData.normalWS         = normalWS;
    vectorsData.coatNormalWS     = coatNormalWS;
    vectorsData.bentNormalWS     = bentNormalWS;
    vectorsData.viewDirectionWS  = viewDirectionWS;
    vectorsData.tangentWS        = tangentWS;

    return vectorsData;
}

#endif