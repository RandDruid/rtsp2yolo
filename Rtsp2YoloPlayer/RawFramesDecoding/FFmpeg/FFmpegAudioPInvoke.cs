﻿using System;
using System.Runtime.InteropServices;

namespace Rtsp2YoloPlayer.RawFramesDecoding.FFmpeg
{
    class FFmpegAudioPInvoke
    {
        private const string LibraryName = "libffmpeghelper.dll";

        [DllImport(LibraryName, EntryPoint = "create_audio_decoder", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CreateAudioDecoder(FFmpegAudioCodecId audioCodecId, int bitsPerCodedSample, out IntPtr handle);

        [DllImport(LibraryName, EntryPoint = "set_audio_decoder_extradata", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetAudioDecoderExtraData(IntPtr handle, IntPtr extradata, int extradataLength);

        [DllImport(LibraryName, EntryPoint = "remove_audio_decoder", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RemoveAudioDecoder(IntPtr handle);

        [DllImport(LibraryName, EntryPoint = "decode_audio_frame", CallingConvention = CallingConvention.Cdecl)]
        public static extern int DecodeFrame(IntPtr handle, IntPtr rawBuffer, int rawBufferLength, out int sampleRate, out int bitsPerSample, out int channels);

        [DllImport(LibraryName, EntryPoint = "get_decoded_audio_frame", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetDecodedFrame(IntPtr handle, out IntPtr outBuffer, out int outDataSize);

        [DllImport(LibraryName, EntryPoint = "create_audio_resampler", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CreateAudioResampler(IntPtr decoderHandle, int outSampleRate, int outBitsPerSample, int outChannels, out IntPtr handle);

        [DllImport(LibraryName, EntryPoint = "resample_decoded_audio_frame", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ResampleDecodedFrame(IntPtr decoderHandle, IntPtr resamplerHandle, out IntPtr outBuffer, out int outDataSize);

        [DllImport(LibraryName, EntryPoint = "remove_audio_resampler", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RemoveAudioResampler(IntPtr handle);
    }
}
