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

#define Rotate(value, amount) ((value << amount) | (value >> (32 - amount)))

void GetBitShuffledOrganId_float(float4 color, out float4 shuffledOrganColor)
{
    uint organId =
        (color.r * 256.0) +
        (color.g * 256.0) * (256.0) +
        (color.b * 256.0) * (256.0 * 256.0);// +


    uint onePer4 = 0x88888888;
    uint b1 = organId & onePer4;
    uint b2 = Rotate(organId & (onePer4 >> 1), 8 + 2);
    uint b3 = Rotate(organId & (onePer4 >> 2), 16);
    uint b4 = Rotate(organId & (onePer4 >> 3), 24 - 2);

    uint shuffledOrganId = b1 | b2 | b3 | b4;

    shuffledOrganColor = float4(
        (shuffledOrganId & 0xFF) / 256.0,
        ((shuffledOrganId >> 8) & 0xFF) / 256.0,
        ((shuffledOrganId >> 16) & 0xFF) / 256.0,
        ((shuffledOrganId >> 24) & 0xFF) / 256.0);
}

//
//uint Rotate(uint value, int amount) {
//    return (value << amount) | (value >> (32 - amount));
//}

#endif //MYHLSLINCLUDE_INCLUDED