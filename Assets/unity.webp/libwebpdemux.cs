using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace WebP.Extern
{
    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct WebPData
    {
        public IntPtr bytes;
        public UIntPtr size;
    };

    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct WebPAnimInfo
    {
        public int canvas_width;
        public int canvas_height;
        public int loop_count;
        public int bgcolor;
        public int frame_count;
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.U4)]
        public uint[] pad;       // Padding for later use.
    };

    public enum WebPFormatFeature
    {
        WEBP_FF_FORMAT_FLAGS,      // bit-wise combination of WebPFeatureFlags
                                   // corresponding to the 'VP8X' chunk (if present).
        WEBP_FF_CANVAS_WIDTH,
        WEBP_FF_CANVAS_HEIGHT,
        WEBP_FF_LOOP_COUNT,        // only relevant for animated file
        WEBP_FF_BACKGROUND_COLOR,  // idem.
        WEBP_FF_FRAME_COUNT        // Number of frames present in the demux object.
                                   // In case of a partial demux, this is the number
                                   // of frames seen so far, with the last frame
                                   // possibly being partial.
    };

    [StructLayoutAttribute(LayoutKind.Sequential)]
    public struct WebPAnimDecoderOptions
    {
        // Output colorspace. Only the following modes are supported:
        // MODE_RGBA, MODE_BGRA, MODE_rgbA and MODE_bgrA.
        public WEBP_CSP_MODE color_mode;
        public int use_threads;           // If true, use multi-threaded decoding.
        /// uint32_t[7]
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 7, ArraySubType = UnmanagedType.U4)]
        public uint[] padding;       // Padding for later use.
    };

    public class libwebpdemux
    {
        const int WEBP_DEMUX_ABI_VERSION = 0x0107;
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        const string DLL_NAME = "libwebpdemux";
#elif UNITY_EDITOR || UNITY_STANDALONE_OSX
        const string DLL_NAME = "webpdemux";
#elif UNITY_ANDROID
		const string DLL_NAME = "webpdemux";
#elif UNITY_IOS
		const string DLL_NAME = "__Internal";
#endif

        // WEBP_EXTERN int WebPAnimDecoderOptionsInitInternal(WebPAnimDecoderOptions*, int);
        [DllImport(DLL_NAME, EntryPoint = "WebPAnimDecoderOptionsInitInternal")]
        public static extern void WebPAnimDecoderOptionsInitInternal(ref WebPAnimDecoderOptions data, int version);
        public static void WebPAnimDecoderOptionsInit(ref WebPAnimDecoderOptions data)
        {
            WebPAnimDecoderOptionsInitInternal(ref data, WEBP_DEMUX_ABI_VERSION);
        }

        // WEBP_EXTERN WebPAnimDecoder* WebPAnimDecoderNewInternal(const WebPData*, const WebPAnimDecoderOptions*, int);
        [DllImport(DLL_NAME, EntryPoint = "WebPAnimDecoderNewInternal")]
        public static extern IntPtr WebPAnimDecoderNewInternal(ref WebPData data, ref WebPAnimDecoderOptions option, int version);
        public static IntPtr WebPAnimDecoderNew(ref WebPData data, ref WebPAnimDecoderOptions option)
        {
            return WebPAnimDecoderNewInternal(ref data, ref option, WEBP_DEMUX_ABI_VERSION);
        }

        [DllImportAttribute(DLL_NAME, EntryPoint = "WebPAnimDecoderGetNext")]
        public static extern int WebPAnimDecoderGetNext(IntPtr dec, ref IntPtr p, ref int timestamp);

        //         void WebPDemuxReleaseIterator(WebPIterator* iter);
        [DllImport(DLL_NAME, EntryPoint = "WebPDemuxReleaseIterator")]
        public static extern void WebPDemuxReleaseIterator(IntPtr dec);

        // WEBP_EXTERN void WebPDemuxReleaseChunkIterator(WebPChunkIterator* iter);
        [DllImport(DLL_NAME, EntryPoint = "WebPDemuxReleaseChunkIterator")]
        public static extern void WebPDemuxReleaseChunkIterator(IntPtr webpChunkIterator);

        // WEBP_EXTERN int WebPDemuxPrevFrame(WebPIterator* iter);
        [DllImport(DLL_NAME, EntryPoint = "WebPDemuxPrevFrame")]
        public static extern int WebPDemuxPrevFrame(IntPtr webpIterator);

        //         WEBP_EXTERN int WebPDemuxPrevChunk(WebPChunkIterator* iter);
        [DllImport(DLL_NAME, EntryPoint = "WebPDemuxPrevChunk")]
        public static extern int WebPDemuxPrevChunk(IntPtr webpChunkIterator);

        // WEBP_EXTERN int WebPDemuxNextFrame(WebPIterator* iter);
        [DllImport(DLL_NAME, EntryPoint = "WebPDemuxNextFrame")]
        public static extern int WebPDemuxNextFrame(IntPtr webpIterator);

        // WEBP_EXTERN int WebPDemuxNextChunk(WebPChunkIterator* iter);
        [DllImport(DLL_NAME, EntryPoint = "WebPDemuxNextChunk")]
        public static extern int WebPDemuxNextChunk(IntPtr webpChunkIterator);

        // WEBP_EXTERN WebPDemuxer* WebPDemuxInternal(const WebPData*, int, WebPDemuxState*, int);
        [DllImport(DLL_NAME, EntryPoint = "WebPDemuxInternal")]
        public static extern int WebPDemuxInternal(IntPtr webpdata, int a, IntPtr state, int version);

        // WEBP_EXTERN uint32_t WebPDemuxGetI(const WebPDemuxer* dmux, WebPFormatFeature feature);
        [DllImport(DLL_NAME, EntryPoint = "WebPDemuxGetI")]
        public static extern uint WebPDemuxGetI(IntPtr demux, WebPFormatFeature feature);

        // WEBP_EXTERN int WebPDemuxGetFrame(const WebPDemuxer* dmux, int frame_number, WebPIterator* iter);
        [DllImport(DLL_NAME, EntryPoint = "WebPDemuxGetFrame")]
        public static extern int WebPDemuxGetFrame(IntPtr demux, int frame_number, IntPtr iter);

        // WEBP_EXTERN int WebPDemuxGetChunk(const WebPDemuxer* dmux, const char fourcc[4], int chunk_number, WebPChunkIterator* iter);
        [DllImport(DLL_NAME, EntryPoint = "WebPDemuxGetChunk")]
        public static extern int WebPDemuxGetChunk(IntPtr dmux, char[] fourcc, int chunk_number, IntPtr iter);

        // WEBP_EXTERN void WebPDemuxDelete(WebPDemuxer* dmux);
        [DllImport(DLL_NAME, EntryPoint = "WebPDemuxDelete")]
        public static extern void WebPDemuxDelete(IntPtr demux);

        // WEBP_EXTERN void WebPAnimDecoderReset(WebPAnimDecoder* dec);
        [DllImport(DLL_NAME, EntryPoint = "WebPAnimDecoderReset")]
        public static extern void WebPAnimDecoderReset(IntPtr dec);

        // WEBP_EXTERN int WebPAnimDecoderHasMoreFrames(const WebPAnimDecoder* dec);
        [DllImport(DLL_NAME, EntryPoint = "WebPAnimDecoderHasMoreFrames")]
        public static extern int WebPAnimDecoderHasMoreFrames(IntPtr dec);

        // WEBP_EXTERN int WebPAnimDecoderGetNext(WebPAnimDecoder* dec, uint8_t** buf, int* timestamp);
        [DllImport(DLL_NAME, EntryPoint = "WebPAnimDecoderGetNext")]
        public static extern int WebPAnimDecoderGetNext(IntPtr dec, IntPtr buf, ref int timestamp);

        // WEBP_EXTERN int WebPAnimDecoderGetInfo(const WebPAnimDecoder* dec, WebPAnimInfo* info);
        [DllImport(DLL_NAME, EntryPoint = "WebPAnimDecoderGetInfo")]
        public static extern int WebPAnimDecoderGetInfo(IntPtr dec, ref WebPAnimInfo info);

        // WEBP_EXTERN const WebPDemuxer* WebPAnimDecoderGetDemuxer(const WebPAnimDecoder* dec);
        [DllImport(DLL_NAME, EntryPoint = "WebPAnimDecoderGetDemuxer")]
        public static extern IntPtr WebPAnimDecoderGetDemuxer(IntPtr dec);

        //WEBP_EXTERN void WebPAnimDecoderDelete(WebPAnimDecoder* dec);
        [DllImport(DLL_NAME, EntryPoint = "WebPAnimDecoderDelete")]
        public static extern void WebPAnimDecoderDelete(IntPtr dec);
    }
}