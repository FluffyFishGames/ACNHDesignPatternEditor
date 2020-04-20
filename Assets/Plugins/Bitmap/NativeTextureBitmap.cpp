#include <cstdint>
#include <map>
#include "IUnityRenderingExtensions.h"

namespace
{
    std::map<uint32_t, void*> registered_textures;

    void InternalRegisterTexture(uint32_t id, void* ptr)
    {
        registered_textures[id] = ptr;
    }

    void InternalUnregisterTexture(uint32_t id)
    {
        registered_textures.erase(id);
    }
    
    void TextureBitmapUpdateCallback(int eventID, void* data)
     {
         auto event = static_cast<UnityRenderingExtEventType>(eventID);

         if (event == kUnityRenderingExtEventUpdateTextureBeginV2)
         {
             auto params = reinterpret_cast<UnityRenderingExtTextureUpdateParamsV2*>(data);
             uint32_t id = params->userData;
             auto texture = registered_textures.find(id);
             if (texture != registered_textures.end())
                params->texData = texture->second;
         }
         else if (event == kUnityRenderingExtEventUpdateTextureEndV2)
         {
             // no need to free. We do this in c#
         }
     }
}

extern "C" __declspec(dllexport) void __stdcall RegisterTexture(uint32_t id, uint32_t* ptr)
{
    InternalRegisterTexture(id, ptr);
}

extern "C" __declspec(dllexport) void __stdcall UnregisterTexture(uint32_t id)
{
    InternalUnregisterTexture(id);
}

extern "C" UnityRenderingEventAndData UNITY_INTERFACE_EXPORT GetTextureBitmapUpdateCallback()
{
    return TextureBitmapUpdateCallback;
}