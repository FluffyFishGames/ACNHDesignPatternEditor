#include <map>
#include "IUnityInterface.h"
#include "IUnityRenderingExtensions.h"

std::map<uint32_t, void*> registered_textures;

namespace {
    void InternalRegisterTexture(uint32_t id, void* ptr)
    {
        registered_textures[id] = ptr;
    }

    void InternalUnregisterTexture(uint32_t id)
    {
        registered_textures.erase(id);
    }

    void UNITY_INTERFACE_API TextureBitmapUpdateCallback(int eventID, void* data)
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
    }
}

#if defined(WIN32) || defined(_WIN32) || defined(__WIN32__) || defined(_WIN64) || defined(WINAPI_FAMILY)
extern "C" void __declspec(dllexport) __stdcall RegisterTexture(uint32_t id, void* ptr)
{
    InternalRegisterTexture(id, ptr);
}

extern "C" void __declspec(dllexport) __stdcall UnregisterTexture(uint32_t id)
{
    InternalUnregisterTexture(id);
}

extern "C" UnityRenderingEventAndData __declspec(dllexport) __stdcall GetTextureBitmapUpdateCallback()
{
    return TextureBitmapUpdateCallback;
}
#elif defined(__MACH__) || defined(__ANDROID__) || defined(__linux__) || defined(LUMIN)
extern "C" void __attribute__((visibility("default"))) RegisterTexture(uint32_t id, uint32_t * ptr)
{
    InternalRegisterTexture(id, ptr);
}

extern "C" void __attribute__((visibility("default"))) UnregisterTexture(uint32_t id)
{
    InternalUnregisterTexture(id);
}

extern "C" UnityRenderingEventAndData __attribute__((visibility("default"))) GetTextureBitmapUpdateCallback()
{
    return reinterpret_cast<UnityRenderingEventAndData>(TextureBitmapUpdateCallback);
}
#else
extern "C" void RegisterTexture(uint32_t id, uint32_t * ptr)
{
    InternalRegisterTexture(id, ptr);
}

extern "C" void UnregisterTexture(uint32_t id)
{
    InternalUnregisterTexture(id);
}

extern "C" UnityRenderingEventAndData GetTextureBitmapUpdateCallback()
{
    return reinterpret_cast<UnityRenderingEventAndData>(TextureBitmapUpdateCallback);
}
#endif