//UNITY_SHADER_NO_UPGRADE
#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

void ConvertToOrganId_float(float4 color, out float organId)
{
    // reassemble the original uint. each byte of the uint is one color channel
    organId =
        (color.r * 256.0) +
        (color.g * 256.0) * (256.0) +
        (color.b * 256.0) * (256.0 * 256.0);// +
        //(color.a * 255.0) * (256.0 * 256.0 * 256.0);
}

#endif //MYHLSLINCLUDE_INCLUDED